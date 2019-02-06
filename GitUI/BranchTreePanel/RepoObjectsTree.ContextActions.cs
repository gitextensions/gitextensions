using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace GitUI.BranchTreePanel
{
    partial class RepoObjectsTree
    {
        private TreeNode _lastRightClickedNode;

        private void ContextMenuAddExpandCollapseTree(ContextMenuStrip contextMenu)
        {
            // Add the following to the every participating context menu:
            //
            //    ---------
            //    Collapse All
            //    Expand All

            if (!contextMenu.Items.Contains(tsmiSpacer1))
            {
                contextMenu.Items.Add(tsmiSpacer1);
            }

            if (!contextMenu.Items.Contains(mnubtnCollapseAll))
            {
                contextMenu.Items.Add(mnubtnCollapseAll);
            }

            if (!contextMenu.Items.Contains(mnubtnExpandAll))
            {
                contextMenu.Items.Add(mnubtnExpandAll);
            }
        }

        private void ContextMenuBranchSpecific(ContextMenuStrip contextMenu)
        {
            if (contextMenu != menuBranch)
            {
                return;
            }

            var node = (contextMenu.SourceControl as TreeView)?.SelectedNode;
            if (node == null)
            {
                return;
            }

            var isNotActiveBranch = !((node.Tag as LocalBranchNode)?.IsActive ?? false);
            mnuBtnCheckoutLocal.Visible = isNotActiveBranch;
            tsmiSpacer2.Visible = isNotActiveBranch;
            mnubtnBranchDelete.Visible = isNotActiveBranch;
        }

        private void ContextMenuRemoteRepoSpecific(ContextMenuStrip contextMenu)
        {
            if (contextMenu != menuRemoteRepoNode)
            {
                return;
            }

            var node = (contextMenu.SourceControl as TreeView)?.SelectedNode?.Tag as RemoteRepoNode;
            if (node == null)
            {
                return;
            }

            // Actions on enabled remotes
            mnubtnFetchAllBranchesFromARemote.Visible = node.Enabled;
            mnubtnDisableRemote.Visible = node.Enabled;

            // Actions on disabled remotes
            mnubtnEnableRemote.Visible = !node.Enabled;
            mnubtnEnableRemoteAndFetch.Visible = !node.Enabled;
        }

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

            RegisterClick(mnubtnReload, () => RefreshTree());

            treeMain.NodeMouseClick += OnNodeMouseClick;

            RegisterClick<LocalBranchNode>(mnuBtnCheckoutLocal, branch => branch.Checkout());
            RegisterClick<LocalBranchNode>(mnubtnBranchDelete, branch => branch.Delete());
            RegisterClick<LocalBranchNode>(mnubtnFilterLocalBranchInRevisionGrid, FilterInRevisionGrid);
            Node.RegisterContextMenu(typeof(LocalBranchNode), menuBranch);

            RegisterClick<BranchPathNode>(mnubtnDeleteAllBranches, branchPath => branchPath.DeleteAll());
            Node.RegisterContextMenu(typeof(BranchPathNode), menuBranchPath);

            RegisterClick<RemoteBranchNode>(mnubtnDeleteRemoteBranch, remoteBranch => remoteBranch.Delete());
            RegisterClick<RemoteBranchNode>(mnubtnBranchCheckout, branch => branch.Checkout());
            RegisterClick<RemoteBranchNode>(mnubtnFetchOneBranch, remoteBranch => remoteBranch.Fetch());
            RegisterClick<RemoteBranchNode>(mnubtnPullFromRemoteBranch, remoteBranch => remoteBranch.FetchAndMerge());
            RegisterClick<RemoteBranchNode>(mnubtnCreateBranchBasedOnRemoteBranch, remoteBranch => remoteBranch.CreateBranch());
            RegisterClick<RemoteBranchNode>(mnubtnMergeBranch, remoteBranch => remoteBranch.Merge());
            RegisterClick<RemoteBranchNode>(mnubtnRebase, remoteBranch => remoteBranch.Rebase());
            RegisterClick<RemoteBranchNode>(mnubtnReset, remoteBranch => remoteBranch.Reset());
            RegisterClick<RemoteBranchNode>(mnubtnFilterRemoteBranchInRevisionGrid, FilterInRevisionGrid);
            RegisterClick<RemoteBranchNode>(mnubtnRemoteBranchFetchAndCheckout, remoteBranch => remoteBranch.FetchAndCheckout());
            RegisterClick<RemoteBranchNode>(mnubtnFetchCreateBranch, remoteBranch => remoteBranch.FetchAndCreateBranch());
            RegisterClick<RemoteBranchNode>(mnubtnFetchRebase, remoteBranch => remoteBranch.FetchAndRebase());
            Node.RegisterContextMenu(typeof(RemoteBranchNode), menuRemote);

            RegisterClick<RemoteRepoNode>(mnubtnManageRemotes, remoteBranch => remoteBranch.PopupManageRemotesForm());
            RegisterClick<RemoteRepoNode>(mnubtnFetchAllBranchesFromARemote, remote => remote.Fetch());
            RegisterClick<RemoteRepoNode>(mnubtnEnableRemote, remote => remote.Enable(fetch: false));
            RegisterClick<RemoteRepoNode>(mnubtnEnableRemoteAndFetch, remote => remote.Enable(fetch: true));
            RegisterClick<RemoteRepoNode>(mnubtnDisableRemote, remote => remote.Disable());
            Node.RegisterContextMenu(typeof(RemoteRepoNode), menuRemoteRepoNode);

            RegisterClick<TagNode>(mnubtnCreateBranchForTag, tag => tag.CreateBranch());
            RegisterClick<TagNode>(mnubtnDeleteTag, tag => tag.Delete());
            RegisterClick<TagNode>(mnuBtnCheckoutTag, tag => tag.Checkout());
            Node.RegisterContextMenu(typeof(TagNode), menuTag);

            RegisterClick(mnuBtnManageRemotesFromRootNode, () => _remotesTree.PopupManageRemotesForm(remoteName: null));
        }

        private void FilterInRevisionGrid(BaseBranchNode branch)
        {
            _filterBranchHelper?.SetBranchFilter(branch.FullPath, refresh: true);
        }

        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            var contextMenu = sender as ContextMenuStrip;
            if (contextMenu == null)
            {
                return;
            }

            ContextMenuAddExpandCollapseTree(contextMenu);
            ContextMenuBranchSpecific(contextMenu);
            ContextMenuRemoteRepoSpecific(contextMenu);
        }
    }
}
