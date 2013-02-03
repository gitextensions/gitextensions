using System;
using System.Windows.Forms;

namespace GitUI.UserControls
{
    partial class RepoObjectsTree
    {
        /// <summary>most recently right-clicked <see cref="TreeNode"/></summary>
        TreeNode rightClickNode;

        /// <summary>Occurs when a <see cref="TreeNode"/> is clicked.</summary>
        void OnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                e.Node.TreeView.SelectedNode = e.Node;
                rightClickNode = e.Node;
            }
        }

        /// <summary>Hooks an action onto the Click event of a <see cref="ToolStripMenuItem"/>.</summary>
        void RegisterClick<TNode>(ToolStripMenuItem item, Action<TNode> onClick)
            where TNode : Node
        {
            item.Click += (o, e) => Node.OnNode(rightClickNode, onClick);
        }

        /// <summary>Registers the context menu actions.</summary>
        void RegisterContextActions()
        {
            treeMain.NodeMouseClick += OnNodeMouseClick;

            RegisterClick<BranchesNode>(mnubtnNewBranch, branches => branches.CreateBranch());


            RegisterClick<BranchNode>(mnubtnBranchCheckout, branch => branch.Checkout());
            RegisterClick<BranchNode>(mnubtnBranchCreateFrom, branch => branch.CreateBranch());
            RegisterClick<BranchNode>(mnubtnBranchDelete, branch => branch.Delete());
            RegisterClick<BranchNode>(mnubtnBranchDeleteForce, branch => branch.DeleteForce());

            RegisterClick<BranchPathNode>(mnubtnCreateBranchWithin, branchPath => branchPath.CreateWithin());
            RegisterClick<BranchPathNode>(mnubtnDeleteAllBranches, branchPath => branchPath.DeleteAll());
            RegisterClick<BranchPathNode>(mnubtnDeleteAllBranchesForce, branchPath => branchPath.DeleteAllForce());

            RegisterClick<RootNode>(mnubtnStashSave, stashes => stashes.UiCommands.StartStashDialog());
            //RegisterClick<RootNode>(mnubtnClearStashes, stashes => stashes.UiCommands.Module);

            RegisterClick<StashNode>(mnubtnStashPop, stash => stash.Pop());
            RegisterClick<StashNode>(mnubtnStashApply, stash => stash.Apply());
            RegisterClick<StashNode>(mnubtnStashShowDiff, stash => stash.ShowDiff());
            RegisterClick<StashNode>(mnubtnStashDrop, stash => stash.Delete());
            RegisterClick<StashNode>(mnubtnStashBranch, stash => stash.CreateBranch());
        }


    }
}
