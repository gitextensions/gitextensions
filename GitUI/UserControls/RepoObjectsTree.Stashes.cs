using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls
{
    // "stashes"
    public partial class RepoObjectsTree
    {
        /// <summary>Reloads the stashes.</summary>
        void ReloadStashes(ICollection<GitStash> stashes)
        {
            nodeStashes.Text = string.Format("{0} ({1})", Strings.Instance.stashes, stashes.Count);
        }

        /// <summary>Adds the specified <paramref name="stash"/> to the stashes tree.</summary>
        TreeNode AddStash(TreeNodeCollection nodes, GitStash stash)
        {
            TreeNode treeNode = nodes.Add(stash.Index.ToString(), stash.Name);
            treeNode.Tag = stash;
            treeNode.ToolTipText = stash.Message;
            ApplyStashStyle(treeNode);
            return treeNode;
        }

        void ApplyStashStyle(TreeNode treeNode)
        {
            // style
        }

        class StashNode : Node<GitStash, RootNode<StashNode>>
        {
            public StashNode(GitStash stash, RootNode<StashNode> parent, GitUICommands uiCommands)
                : base(stash, parent, uiCommands) { }
        }
    }
}
