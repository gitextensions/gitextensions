using System;
using System.Windows.Forms;
using GitUI.CommandsDialogs;

namespace GitUI.BranchTreePanel
{
    partial class RepoObjectsTree
    {
        private TreeNode _lastRightClickedNode;

        private void OnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            _lastRightClickedNode = e.Button == MouseButtons.Right ? e.Node : null;
        }

        private static void RegisterClick(ToolStripItem item, Action onClick)
        {
            item.Click += (o, e) => onClick();
        }

        private void RegisterClick<T>(ToolStripItem item, Action<T> onClick) where T : Node
        {
            item.Click += (o, e) => Node.OnNode(_lastRightClickedNode, onClick);
        }

        private void RegisterContextActions()
        {
            RegisterClick(mnubtnCollapseAll, () => treeMain.CollapseAll());
            RegisterClick(mnubtnExpandAll, () => treeMain.ExpandAll());
            RegisterClick(mnubtnReload, () => ThreadHelper.JoinableTaskFactory.RunAsync(() => ReloadAsync()).FileAndForget());

            treeMain.NodeMouseClick += OnNodeMouseClick;

            RegisterClick<LocalBranchNode>(mnuBtnCheckoutLocal, branch => branch.Checkout());
            RegisterClick<LocalBranchNode>(mnubtnBranchDelete, branch => branch.Delete());
            RegisterClick<LocalBranchNode>(mnubtnBranchDeleteForce, branch => branch.DeleteForce());
            RegisterClick<LocalBranchNode>(mnubtnFilterLocalBranchInRevisionGrid, FilterInRevisionGrid);
            Node.RegisterContextMenu(typeof(LocalBranchNode), menuBranch);

            RegisterClick<BranchPathNode>(mnubtnDeleteAllBranches, branchPath => branchPath.DeleteAll());
            RegisterClick<BranchPathNode>(mnubtnDeleteAllBranchesForce, branchPath => branchPath.DeleteAllForce());
            Node.RegisterContextMenu(typeof(BranchPathNode), menuBranchPath);

            RegisterClick<RemoteBranchNode>(mnubtnDeleteRemoteBranch, remoteBranch => remoteBranch.Delete());
            RegisterClick<RemoteBranchNode>(mnubtnBranchCheckout, branch => branch.Checkout());
            RegisterClick<RemoteBranchNode>(mnubtnFetchOneBranch, remoteBranch => remoteBranch.Fetch());
            RegisterClick<RemoteBranchNode>(mnubtnPullFromRemoteBranch, remoteBranch =>
            {
                remoteBranch.Fetch();
                remoteBranch.Merge();
            });
            RegisterClick<RemoteBranchNode>(mnubtnCreateBranchBasedOnRemoteBranch, remoteBranch => remoteBranch.CreateBranch());
            RegisterClick<RemoteBranchNode>(mnubtnMergeBranch, remoteBranch => remoteBranch.Merge());
            RegisterClick<RemoteBranchNode>(mnubtnRebase, remoteBranch => remoteBranch.Rebase());
            RegisterClick<RemoteBranchNode>(mnubtnReset, remoteBranch => remoteBranch.Reset());
            RegisterClick<RemoteBranchNode>(mnubtnFilterRemoteBranchInRevisionGrid, FilterInRevisionGrid);
            RegisterClick<RemoteBranchNode>(mnubtnRemoteBranchFetchAndCheckout, b =>
            {
                b.Fetch();
                b.Checkout();
            });
            RegisterClick<RemoteBranchNode>(mnubtnFetchCreateBranch, b =>
            {
                b.Fetch();
                b.CreateBranch();
            });
            RegisterClick<RemoteBranchNode>(mnubtnFetchRebase, b =>
            {
                b.Fetch();
                b.Rebase();
            });
            Node.RegisterContextMenu(typeof(RemoteBranchNode), menuRemote);

            RegisterClick<RemoteRepoNode>(mnubtnFetchAllBranchesFromARemote, remote => remote.Fetch());
            RegisterClick<RemoteRepoNode>(mnubtnManageRemotes, remoteBranch => PopupManageRemotesForm());
            Node.RegisterContextMenu(typeof(RemoteRepoNode), menuRemoteRepoNode);

            RegisterClick<TagNode>(mnubtnCreateBranchForTag, tag => tag.CreateBranch());
            RegisterClick<TagNode>(mnubtnDeleteTag, tag => tag.Delete());
            RegisterClick<TagNode>(mnuBtnCheckoutTag, tag => tag.Checkout());
            Node.RegisterContextMenu(typeof(TagNode), menuTag);

            RegisterClick(mnuBtnManageRemotesFromRootNode, PopupManageRemotesForm);
        }

        private void FilterInRevisionGrid(BaseBranchNode branch)
        {
            FilterBranchHelper.SetBranchFilter(branch.FullPath, refresh: true);
        }

        private void PopupManageRemotesForm()
        {
            using (var form = new FormRemotes(UICommands))
            {
                form.ShowDialog(this);
            }
        }

        private void OnRemoteAdded(object sender, RemoteChangedEventArgs args)
        {
            _remoteTree?.AddRemote(args.RemoteName);
        }

        private void OnRemoteRenamed(object sender, RemoteRenamedEventArgs args)
        {
            _remoteTree?.RenameRemote(args.OriginalName, args.NewName);
        }

        private void OnRemoteDeleted(object sender, RemoteChangedEventArgs args)
        {
            _remoteTree?.DeleteRemote(args.RemoteName);
        }
    }
}
