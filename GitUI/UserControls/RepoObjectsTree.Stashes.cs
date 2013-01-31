using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls
{
    // "stashes"
    public partial class RepoObjectsTree
    {
        /// <summary>Reloads the stashes.</summary>
        static void OnReloadStashes(ICollection<StashNode> stashes,RootNode<StashNode> stashesNode)
        {
            stashesNode.TreeNode.Text = string.Format("{0} ({1})", Strings.Instance.stashes, stashes.Count);
        }

        /// <summary>Adds the specified <paramref name="stashNode"/> to the stashes tree.</summary>
        static TreeNode OnAddStash(TreeNodeCollection nodes, StashNode stashNode)
        {
            GitStash stash = stashNode.Value;
            TreeNode treeNode = nodes.Add(stash.Index.ToString(), stash.Name);
            treeNode.Tag = stash;
            treeNode.ToolTipText = stash.Message;
            return treeNode;
        }

        class StashNode : Node<GitStash, RootNode<StashNode>>
        {
            public StashNode(GitStash stash, GitUICommands uiCommands)
                : base(stash, null, uiCommands) { }
        }
    }
}
