using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces.Notifications;

namespace GitUI.UserControls
{
    // "stashes"
    public partial class RepoObjectsTree
    {
        static readonly string stashKey = Guid.NewGuid().ToString();
        static readonly string stashesKey = Guid.NewGuid().ToString();

        /*
        /// <summary>Reloads the stashes.</summary>
        static void OnReloadStashes(ICollection<StashNode> stashes, Tree<StashNode> stashesNode)
        {
            stashesNode.TreeNode.Text = string.Format("{0} ({1})", Strings.stashes, stashes.Count);
        }

        /// <summary>Adds the specified <paramref name="stashNode"/> to the stashes tree.</summary>
        TreeNode OnAddStash(TreeNodeCollection nodes, StashNode stashNode)
        {
            GitStash stash = stashNode.Value;
            TreeNode treeNode = new TreeNode(stash.Name)
            {
                Name = string.Format("stash{0}", stash.Index),
                ToolTipText = stash.Message,
                ContextMenuStrip = menuStash,
                ImageKey = stashKey,
                SelectedImageKey = stashKey,
            };
            nodes.Add(treeNode);

            return treeNode;
        }

        /// <summary>Stash node.</summary>
        sealed class StashNode : Node<StashNode>
        {
            public readonly GitStash Stash;

            public StashNode(GitStash aStash, Tree<StashNode> aTree, StashNode aParentNode)
                : base(aTree, aParentNode)
            {
                Stash = aStash;
                IsDraggable = true;
            }

            internal override void OnSelected()
            {
                base.OnSelected();
                UICommands.BrowseRepo.GoToRef(Stash.Name, true);
            }

            public void Pop()
            {
                UICommands.StashPop(ParentWindow());
            }

            public void Apply()
            {
                UICommands.StashApply(ParentWindow(), Stash.Name);
            }

            public void Delete()
            {
                UICommands.StashDrop(ParentWindow(), Stash.Name);
            }
        }
         *          */

    }
}
