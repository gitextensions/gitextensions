using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls
{
    // "stashes"
    public partial class RepoObjectsTree
    {
        static readonly string stashKey = "stash";
        static readonly string stashesKey = "stashes";

        /// <summary>Reloads the stashes.</summary>
        static void OnReloadStashes(ICollection<StashNode> stashes, RootNode<StashNode> stashesNode)
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
                Tag = stashNode,
                ToolTipText = stash.Message,
                ContextMenuStrip = menuStash,
                ImageKey = stashKey,
                SelectedImageKey = stashKey,
            };
            nodes.Add(treeNode);

            return treeNode;
        }

        /// <summary>Stash node.</summary>
        sealed class StashNode : Node<GitStash, RootNode<StashNode>>
        {
            public StashNode(GitStash stash, GitUICommands uiCommands)
                : base(stash, null, uiCommands)
            {
                IsDraggable = true;
            }

            public void Pop()
            {
                throw new System.NotImplementedException();
            }

            public void Apply()
            {
                throw new System.NotImplementedException();
            }

            public void Delete()
            {
                throw new System.NotImplementedException();
            }

            public void ShowDiff()
            {
                throw new System.NotImplementedException();
            }

            public void CreateBranch()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
