using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs
{
    internal interface IRevisionFileTreeController
    {
        /// <summary>
        /// Locates the node by the label.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="label"></param>
        /// <returns>The first node matching the label, if one found; otherwise <see langword="null"/>.</returns>
        TreeNode Find(TreeNodeCollection nodes, string label);

        /// <summary>
        /// Loads the provided items in the specified nodes.
        /// For file type items it also loads icons associated with these types at the OS level.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="node"></param>
        /// <param name="imageCollection"></param>
        void LoadItemsInTreeView(IEnumerable<IGitItem> items, TreeNodeCollection node, ImageList.ImageCollection imageCollection);
    }

    internal sealed class RevisionFileTreeController : IRevisionFileTreeController
    {
        internal static class TreeNodeImages
        {
            public const int Folder = 1;
            public const int Submodule = 2;
        }

        private readonly IGitModule _module;
        private readonly IFileAssociatedIconProvider _iconProvider;


        public RevisionFileTreeController(IGitModule module, IFileAssociatedIconProvider iconProvider)
        {
            _module = module;
            _iconProvider = iconProvider;
        }


        /// <summary>
        /// Locates the node by the label.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="label"></param>
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
        /// Loads the provided items in the specified nodes.
        /// For file type items it also loads icons associated with these types at the OS level.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="node"></param>
        /// <param name="imageCollection"></param>
        /// <remarks>The method DOES NOT check any input parameters for performance reasons.</remarks>
        public void LoadItemsInTreeView(IEnumerable<IGitItem> items, TreeNodeCollection node, ImageList.ImageCollection imageCollection)
        {
            var sortedItems = items.OrderBy(gi => gi, new GitFileTreeComparer());

            foreach (var item in sortedItems)
            {
                var subNode = node.Add(item.Name);
                subNode.Tag = item;

                var gitItem = item as GitItem;
                if (gitItem == null)
                {
                    subNode.Nodes.Add(new TreeNode());
                    continue;
                }

                if (gitItem.IsTree)
                {
                    subNode.ImageIndex = subNode.SelectedImageIndex = TreeNodeImages.Folder;
                    subNode.Nodes.Add(new TreeNode());
                    continue;
                }

                if (gitItem.IsCommit)
                {
                    subNode.ImageIndex = subNode.SelectedImageIndex = TreeNodeImages.Submodule;
                    subNode.Text = $@"{item.Name} (Submodule)";
                    continue;
                }

                if (gitItem.IsBlob)
                {
                    var extension = Path.GetExtension(gitItem.FileName);
                    if (string.IsNullOrWhiteSpace(extension))
                    {
                        continue;
                    }
                    if (!imageCollection.ContainsKey(extension))
                    {
                        var fileIcon = _iconProvider.Get(_module.WorkingDir, gitItem.FileName);
                        if (fileIcon == null)
                        {
                            continue;
                        }
                        imageCollection.Add(extension, fileIcon);
                    }
                    subNode.ImageKey = subNode.SelectedImageKey = extension;
                }
            }
        }
    }
}
