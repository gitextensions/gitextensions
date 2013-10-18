using System;
using System.Windows.Forms;

namespace GitUI.UserControls
{
    partial class RepoObjectsTree
    {
        /// <summary>most recently right-clicked <see cref="TreeNode"/></summary>
        TreeNode rightClickNode;

        /// <summary>Captures the right-clicked <see cref="TreeNode"/>.</summary>
        void OnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {// right-click -> set right-click TreeNode
                e.Node.TreeView.SelectedNode = e.Node;
                rightClickNode = e.Node;
            }
            else
            {// (NOT right-click) -> set right-click TreeNode to null
                rightClickNode = null;
            }
        }


        /// <summary>Hooks an action onto the Click event of a <see cref="ToolStripMenuItem"/>.</summary>
        void RegisterClick(ToolStripMenuItem item, Action onClick)
        {
            item.Click += (o, e) => onClick();
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
            RegisterClick(mnubtnCollapseAll, () => treeMain.CollapseAll());
            RegisterClick(mnubtnExpandAll, () => treeMain.ExpandAll());
            RegisterClick(mnubtnReload, Reload);

            treeMain.NodeMouseClick += OnNodeMouseClick;

            RegisterClick<BranchesNode>(mnubtnNewBranch, branches => branches.CreateBranch());


            RegisterClick<BranchNode>(mnubtnBranchCheckout, branch => branch.Checkout());
            RegisterClick<BranchNode>(mnubtnBranchCreateFrom, branch => branch.CreateBranch());
            RegisterClick<BranchNode>(mnubtnBranchDelete, branch => branch.Delete());
            RegisterClick<BranchNode>(mnubtnBranchDeleteForce, branch => branch.DeleteForce());

            RegisterClick<BranchPathNode>(mnubtnCreateBranchWithin, branchPath => branchPath.CreateWithin());
            RegisterClick<BranchPathNode>(mnubtnDeleteAllBranches, branchPath => branchPath.DeleteAll());
            RegisterClick<BranchPathNode>(mnubtnDeleteAllBranchesForce, branchPath => branchPath.DeleteAllForce());

            RegisterClick<RootNode>(mnubtnStashSave, stashes => stashes.UICommands.StartStashDialog());

            RegisterClick<StashNode>(mnubtnStashPop, stash => stash.Pop());
            RegisterClick<StashNode>(mnubtnStashApply, stash => stash.Apply());
            RegisterClick<StashNode>(mnubtnStashShowDiff, stash => stash.ShowDiff());
            RegisterClick<StashNode>(mnubtnStashDrop, stash => stash.Delete());
            RegisterClick<StashNode>(mnubtnStashBranch, stash => stash.CreateBranch());

            RegisterClick<RemoteBranchNode>(mnubtnTrackedFetch, remoteBranch => remoteBranch.Fetch());
            RegisterClick<RemoteBranchNode>(mnubtnTrackedPull, remoteBranch => remoteBranch.Pull());
            RegisterClick<RemoteBranchNode>(mnubtnTrackedCreateBranch, remoteBranch => remoteBranch.CreateBranch());
            RegisterClick<RemoteBranchNode>(mnubtnTrackedUnTrack, remoteBranch => remoteBranch.UnTrack());
            RegisterClick<RemoteBranchNode>(mnubtnTrackedDelete, remoteBranch => remoteBranch.Delete());

            // TODO: context actions for RemoteBranchNode depending on its current state 
            // can either create additional remote branch Node classes OR use method overloads

            //RegisterClick<RemoteBranchUnTrackedNode>(mnubtnUntrackedFetch, remoteBranch => remoteBranch.Fetch());
            //RegisterClick<RemoteBranchUnTrackedNode>(mnubtnUntrackedTrack, remoteBranch => remoteBranch.Track());

            //RegisterClick<RemoteBranchStaleNode>(mnubtnStaleRemove, remoteBranch => remoteBranch.Remove());

            //RegisterClick<RemoteBranchNode>(mnubtnNewFetch, remoteBranch => remoteBranch.Fetch());
            //RegisterClick<RemoteBranchNode>(mnubtnNewCreateBranch, remoteBranch => remoteBranch.CreateBranch());
     
        }


    }
}
