﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitUI.CommandsDialogs
{
    internal interface IRevisionFileTreeController
    {
        /// <summary>
        /// Locates the node by the label.
        /// </summary>
        /// <returns>The first node matching the label, if one found; otherwise <see langword="null"/>.</returns>
        TreeNode? Find(TreeNodeCollection nodes, string label);

        /// <summary>
        /// Loads children items for the provided item in to the specified nodes.
        /// For file type items it also loads icons associated with these types at the OS level.
        /// </summary>
        void LoadChildren(IGitItem item, TreeNodeCollection nodes, ImageList.ImageCollection imageCollection);

        /// <summary>
        /// Select the file or directory node corresponding to the sub path provided.
        /// </summary>
        bool SelectFileOrFolder(NativeTreeView tree, string fileSubPath);

        /// <summary>
        /// Clears the cache of the current revision's loaded children items.
        /// </summary>
        void ResetCache();
    }

    internal sealed class RevisionFileTreeController : IRevisionFileTreeController
    {
        internal static class TreeNodeImages
        {
            public const int Folder = 1;
            public const int Submodule = 2;
        }

        private readonly IFileAssociatedIconProvider _iconProvider;
        private readonly Func<string> _getWorkingDir;
        private readonly IGitRevisionInfoProvider _revisionInfoProvider;
        private readonly ConcurrentDictionary<string, IEnumerable<INamedGitItem>> _cachedItems = new();

        public RevisionFileTreeController(Func<string> getWorkingDir, IGitRevisionInfoProvider revisionInfoProvider, IFileAssociatedIconProvider iconProvider)
        {
            _getWorkingDir = getWorkingDir;
            _revisionInfoProvider = revisionInfoProvider;
            _iconProvider = iconProvider;
        }

        /// <summary>
        /// Locates the node by the label.
        /// </summary>
        /// <returns>The first node matching the label, if one found; otherwise <see langword="null"/>.</returns>
        public TreeNode? Find(TreeNodeCollection nodes, string label)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Text == label)
                {
                    return nodes[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Loads children items for the provided item in to the specified nodes.
        /// Loaded children are cached until <see cref="ResetCache"/> method is called.
        /// For file type items it also loads icons associated with these types at the OS level.
        /// </summary>
        /// <remarks>The method DOES NOT check any input parameters for performance reasons.</remarks>
        public void LoadChildren(IGitItem item, TreeNodeCollection nodes, ImageList.ImageCollection imageCollection)
        {
            Validates.NotNull(item.Guid);

            var childrenItems = _cachedItems.GetOrAdd(item.Guid, _revisionInfoProvider.LoadChildren(item));
            if (childrenItems is null)
            {
                return;
            }

            string? workingDir = null;
            foreach (var childItem in childrenItems.OrderBy(gi => gi, new GitFileTreeComparer()))
            {
                var subNode = nodes.Add(childItem.Name);
                subNode.Tag = childItem;

                if (!(childItem is GitItem gitItem))
                {
                    subNode.Nodes.Add(new TreeNode());
                    continue;
                }

                switch (gitItem.ObjectType)
                {
                    case GitObjectType.Tree:
                        {
                            subNode.ImageIndex = subNode.SelectedImageIndex = TreeNodeImages.Folder;
                            subNode.Nodes.Add(new TreeNode());
                            break;
                        }

                    case GitObjectType.Commit:
                        {
                            subNode.ImageIndex = subNode.SelectedImageIndex = TreeNodeImages.Submodule;
                            subNode.Text = $@"{childItem.Name} (Submodule)";
                            break;
                        }

                    case GitObjectType.Blob:
                        {
                            var extension = Path.GetExtension(gitItem.FileName);

                            if (string.IsNullOrWhiteSpace(extension))
                            {
                                continue;
                            }

                            if (!imageCollection.ContainsKey(extension))
                            {
                                // lazy - initialise the first time used
                                workingDir ??= _getWorkingDir();

                                var fileIcon = _iconProvider.Get(workingDir, gitItem.FileName);
                                if (fileIcon is null)
                                {
                                    continue;
                                }

                                imageCollection.Add(extension, fileIcon);
                            }

                            subNode.ImageKey = subNode.SelectedImageKey = extension;
                            break;
                        }
                }
            }
        }

        public bool SelectFileOrFolder(NativeTreeView tree, string fileSubPath)
        {
            Queue<string> pathParts = new(fileSubPath.Split(Path.DirectorySeparatorChar));
            var foundNode = FindSubNode(tree.Nodes, pathParts);
            if (foundNode is null)
            {
                return false;
            }

            tree.SelectedNode = foundNode;
            return true;
        }

        private TreeNode? FindSubNode(TreeNodeCollection nodes, Queue<string> pathParts)
        {
            while (true)
            {
                var treeToFind = pathParts.Dequeue();
                if (pathParts.Count == 0)
                {
                    return nodes.Cast<TreeNode>().SingleOrDefault(n => n?.Tag is GitItem item && item.Name == treeToFind);
                }

                var node = nodes.Cast<TreeNode>().SingleOrDefault(n =>
                    n?.Tag is GitItem { ObjectType: GitObjectType.Tree } item && item.Name == treeToFind);

                if (node is null)
                {
                    return null;
                }

                node.Expand(); // load the sub nodes (otherwise, the search find nothing)
                nodes = node.Nodes;
            }
        }

        /// <summary>
        /// Clears the cache of the current revision's loaded children items.
        /// </summary>
        public void ResetCache()
        {
            _cachedItems.Clear();
        }
    }
}
