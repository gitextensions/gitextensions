using System;
using System.Windows.Forms;
using GitUI.CommandsDialogs;

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
                //e.Node.TreeView.SelectedNode = e.Node;
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
        void RegisterClick<T>(ToolStripMenuItem item, Action<T> onClick) where T: Node
        {
            item.Click += (o, e) => Node.OnNode<T>(rightClickNode, onClick);
        }

        /// <summary>Registers the context menu actions.</summary>
        private void RegisterContextActions()
        {
            RegisterClick(mnubtnCollapseAll, () => treeMain.CollapseAll());
            RegisterClick(mnubtnExpandAll, () => treeMain.ExpandAll());
            RegisterClick(mnubtnReload, Reload);

            treeMain.NodeMouseClick += OnNodeMouseClick;

            //RegisterClick<BranchesNode>(mnubtnNewBranch, branches => branches.CreateBranch());


            RegisterClick<BranchNode>(mnuBtnCheckoutLocal, branch => branch.Checkout());
            RegisterClick<BranchNode>(mnubtnBranchDelete, branch => branch.Delete());
            RegisterClick<BranchNode>(mnubtnBranchDeleteForce, branch => branch.DeleteForce());
            Node.RegisterContextMenu(typeof (BranchNode), menuBranch);

            RegisterClick<BranchPathNode>(mnubtnDeleteAllBranches, branchPath => branchPath.DeleteAll());
            RegisterClick<BranchPathNode>(mnubtnDeleteAllBranchesForce, branchPath => branchPath.DeleteAllForce());
            Node.RegisterContextMenu(typeof (BranchPathNode), menuBranchPath);

            //RegisterClick<RootNode>(mnubtnStashSave, stashes => stashes.UICommands.StartStashDialog());
            /*

RegisterClick<StashNode>(mnubtnStashPop, stash => stash.Pop());
RegisterClick<StashNode>(mnubtnStashApply, stash => stash.Apply());
RegisterClick<StashNode>(mnubtnStashDrop, stash => stash.Delete());

            RegisterClick<RemoteBranchNode>(mnubtnTrackedFetch, remoteBranch => remoteBranch.Fetch());
            RegisterClick<RemoteBranchNode>(mnubtnTrackedPull, remoteBranch => remoteBranch.Pull());
            RegisterClick<RemoteBranchNode>(mnubtnTrackedCreateBranch, remoteBranch => remoteBranch.CreateBranch());
            RegisterClick<RemoteBranchNode>(mnubtnTrackedUnTrack, remoteBranch => remoteBranch.UnTrack());
            RegisterClick<RemoteBranchNode>(mnubtnTrackedDelete, remoteBranch => remoteBranch.Delete());
            */
            // TODO: context actions for RemoteBranchNode depending on its current state
            // can either create additional remote branch Node classes OR use method overloads

            //RegisterClick<RemoteBranchUnTrackedNode>(mnubtnUntrackedFetch, remoteBranch => remoteBranch.Fetch());
            //RegisterClick<RemoteBranchUnTrackedNode>(mnubtnUntrackedTrack, remoteBranch => remoteBranch.Track());

            RegisterClick<RemoteBranchNode>(mnubtnRemoteRemove, remoteBranch => remoteBranch.Delete());
            RegisterClick<RemoteBranchNode>(mnubtnBranchCheckout, branch => branch.Checkout());
            RegisterClick<RemoteBranchNode>(mnubtnNewFetch, remoteBranch => remoteBranch.Fetch());
            RegisterClick<RemoteBranchNode>(toolbtnRemotePull, remoteBranch => remoteBranch.Pull());
            RegisterClick<RemoteBranchNode>(mnubtnNewCreateBranch, remoteBranch => remoteBranch.CreateBranch());
            RegisterClick<RemoteBranchNode>(mnubtnMergeBranch, remoteBranch => remoteBranch.Merge());
            RegisterClick<RemoteBranchNode>(mnubtnRebase, remoteBranch => remoteBranch.Rebase());
            RegisterClick<RemoteBranchNode>(mnubtnReset, remoteBranch => remoteBranch.Reset());
            RegisterClick<RemoteBranchNode>(mnubtnRemoteBranchFetchAndCheckout, b =>
            {
                b.Fetch();
                b.Checkout();
            });
            Node.RegisterContextMenu(typeof(RemoteBranchNode), menuRemote);

            RegisterClick<RemoteRepoNode>(mnubtnRemoteFetch, remoteBranch => remoteBranch.Fetch());
            RegisterClick<RemoteRepoNode>(mnubtnManageRemotes, remoteBranch => PopupManageRemotesForm());
            Node.RegisterContextMenu(typeof(RemoteRepoNode), menuRemoteRepoNode);

            RegisterClick<TagNode>(mnubtnCreateBranchForTag, tag => tag.CreateBranch());
            RegisterClick<TagNode>(mnubtnDeleteTag, tag => tag.Delete());
            Node.RegisterContextMenu(typeof(TagNode), menuTag);

            RegisterClick(mnuBtnManageRemotesFromRootNode, PopupManageRemotesForm);
        }

        private void PopupManageRemotesForm()
        {
            using (var form = new FormRemotes(UICommands))
            {
                form.OnRemoteDeleted += OnRemoteDeleted;
                form.OnRemoteRenamedOrAdded += OnRemoteRenamedOrAdded;
                form.ShowDialog(this);
            }
        }

        private void OnRemoteRenamedOrAdded(string orgName, string newName)
        {
            if (_remoteTree == null)
            {
                return;
            }
            _remoteTree.RenameOrAddRemote(orgName, newName);
        }

        private void OnRemoteDeleted(string remoteName)
        {
            if (_remoteTree == null)
            {
                return;
            }
            _remoteTree.DeleteRemote(remoteName);
        }
    }
}
