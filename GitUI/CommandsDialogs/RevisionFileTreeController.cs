using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    internal interface IRevisionFileTreeController
    {
        /// <summary>
        /// Locates the node by the label.
        /// </summary>
        /// <returns>The first node matching the label, if one found; otherwise <see langword="null"/>.</returns>
        [CanBeNull]
        TreeNode Find(TreeNodeCollection nodes, string label);

        /// <summary>
        /// Loads children items for the provided item in to the specified nodes.
        /// For file type items it also loads icons associated with these types at the OS level.
        /// </summary>
        void LoadChildren(IGitItem item, TreeNodeCollection nodes, ImageList.ImageCollection imageCollection);

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
        private readonly ConcurrentDictionary<string, IEnumerable<IGitItem>> _cachedItems = new ConcurrentDictionary<string, IEnumerable<IGitItem>>();

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
        public TreeNode Find(TreeNodeCollection nodes, string label)
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
            var childrenItems = _cachedItems.GetOrAdd(item.Guid, _revisionInfoProvider.LoadChildren(item));
            if (childrenItems == null)
            {
                return;
            }

            string workingDir = null;
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
                                // a little optimisation - initialise the first time it is required
                                workingDir = workingDir ?? _getWorkingDir();

                                var fileIcon = _iconProvider.Get(workingDir, gitItem.FileName);
                                if (fileIcon == null)
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

        /// <summary>
        /// Clears the cache of the current revision's loaded children items.
        /// </summary>
        public void ResetCache()
        {
            _cachedItems.Clear();
        }
    }
}
