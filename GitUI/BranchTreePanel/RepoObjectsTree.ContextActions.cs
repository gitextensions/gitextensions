using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.BranchTreePanel.ContextMenu;
using GitUI.BranchTreePanel.Interfaces;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.BranchTreePanel
{
    partial class RepoObjectsTree : IMenuItemFactory
    {
        private GitRefsSortOrderContextMenuItem _sortOrderContextMenuItem;
        private GitRefsSortByContextMenuItem _sortByContextMenuItem;

        /// <summary>
        /// Local branch context menu [git ref / rename / delete] actions.
        /// </summary>
        private LocalBranchMenuItems<LocalBranchNode> _localBranchMenuItems;

        /// <summary>
        /// Remote branch context menu [git ref / rename / delete] actions.
        /// </summary>
        private MenuItemsGenerator<RemoteBranchNode> _remoteBranchMenuItems;

        /// <summary>
        /// Tags context menu [git ref] actions.
        /// </summary>
        private MenuItemsGenerator<TagNode> _tagNodeMenuItems;

        private void EnableMenuItems(bool enabled, params ToolStripItem[] items)
        {
            foreach (var item in items)
            {
                item.Enable(enabled);
            }
        }

        private void EnableMenuItems<TNode>(MenuItemsGenerator<TNode> generator, Func<ToolStripItemWithKey, bool> isEnabled) where TNode : class, INode
        {
            foreach (var item in generator)
            {
                item.Item.Enable(isEnabled(item));
            }
        }

        private static void RegisterClick(ToolStripItem item, Action onClick)
        {
            item.Click += (o, e) => onClick();
        }

        private void RegisterClick<T>(ToolStripItem item, Action<T> onClick) where T : class, INode
        {
            item.Click += (o, e) => Node.OnNode(treeMain.SelectedNode, onClick);
        }

        private void RegisterContextActions()
        {
            copyContextMenuItem.SetRevisionFunc(() => _scriptHost.GetSelectedRevisions());

            filterForSelectedRefsMenuItem.ToolTipText = "Filter the revision grid to show selected (underlined) refs (branches and tags) only." +
                "\nHold CTRL while clicking to de/select multiple and include descendant tree nodes by additionally holding SHIFT." +
                "\nReset the filter via View > Show all branches.";

            RegisterClick(filterForSelectedRefsMenuItem, () =>
            {
                var refPaths = GetMultiSelection().OfType<IGitRefActions>().Select(b => b.FullPath);
                _filterRevisionGridBySpaceSeparatedRefs(refPaths.Join(" "));
            });

            // git refs (tag, local & remote branch) menu items (rename, delete, merge, etc)
            _tagNodeMenuItems = new TagMenuItems<TagNode>(this);
            _remoteBranchMenuItems = new RemoteBranchMenuItems<RemoteBranchNode>(this);
            _localBranchMenuItems = new LocalBranchMenuItems<LocalBranchNode>(this);
            menuMain.InsertItems(_tagNodeMenuItems.Select(s => s.Item).Prepend(new ToolStripSeparator()), after: filterForSelectedRefsMenuItem);
            menuMain.InsertItems(_remoteBranchMenuItems.Select(s => s.Item).Prepend(new ToolStripSeparator()), after: filterForSelectedRefsMenuItem);
            menuMain.InsertItems(_localBranchMenuItems.Select(s => s.Item).Prepend(new ToolStripSeparator()), after: filterForSelectedRefsMenuItem);

            // Remotes Tree
            RegisterClick(mnuBtnManageRemotesFromRootNode, () => _remotesTree.PopupManageRemotesForm(remoteName: null));
            RegisterClick(mnuBtnFetchAllRemotes, () => _remotesTree.FetchAll());
            RegisterClick(mnuBtnPruneAllRemotes, () => _remotesTree.FetchPruneAll());

            // RemoteRepoNode
            RegisterClick<RemoteRepoNode>(mnubtnManageRemotes, remoteBranch => remoteBranch.PopupManageRemotesForm());
            RegisterClick<RemoteRepoNode>(mnubtnFetchAllBranchesFromARemote, remote => remote.Fetch());
            RegisterClick<RemoteRepoNode>(mnuBtnPruneAllBranchesFromARemote, remote => remote.Prune());
            RegisterClick<RemoteRepoNode>(mnuBtnOpenRemoteUrlInBrowser, remote => remote.OpenRemoteUrlInBrowser());
            RegisterClick<RemoteRepoNode>(mnubtnEnableRemote, remote => remote.Enable(fetch: false));
            RegisterClick<RemoteRepoNode>(mnubtnEnableRemoteAndFetch, remote => remote.Enable(fetch: true));
            RegisterClick<RemoteRepoNode>(mnubtnDisableRemote, remote => remote.Disable());

            // SubmoduleNode
            RegisterClick<SubmoduleNode>(mnubtnManageSubmodules, _ => _submoduleTree.ManageSubmodules(this));
            RegisterClick<SubmoduleNode>(mnubtnSynchronizeSubmodules, _ => _submoduleTree.SynchronizeSubmodules(this));
            RegisterClick<SubmoduleNode>(mnubtnOpenSubmodule, node => _submoduleTree.OpenSubmodule(this, node));
            RegisterClick<SubmoduleNode>(mnubtnOpenGESubmodule, node => _submoduleTree.OpenSubmoduleInGitExtensions(this, node));
            RegisterClick<SubmoduleNode>(mnubtnUpdateSubmodule, node => _submoduleTree.UpdateSubmodule(this, node));
            RegisterClick<SubmoduleNode>(mnubtnResetSubmodule, node => _submoduleTree.ResetSubmodule(this, node));
            RegisterClick<SubmoduleNode>(mnubtnStashSubmodule, node => _submoduleTree.StashSubmodule(this, node));
            RegisterClick<SubmoduleNode>(mnubtnCommitSubmodule, node => _submoduleTree.CommitSubmodule(this, node));

            // RemoteBranchNode
            RegisterClick<RemoteBranchNode>(mnubtnFetchOneBranch, remoteBranch => remoteBranch.Fetch());
            RegisterClick<RemoteBranchNode>(mnubtnPullFromRemoteBranch, remoteBranch => remoteBranch.FetchAndMerge());
            RegisterClick<RemoteBranchNode>(mnubtnRemoteBranchFetchAndCheckout, remoteBranch => remoteBranch.FetchAndCheckout());
            RegisterClick<RemoteBranchNode>(mnubtnFetchCreateBranch, remoteBranch => remoteBranch.FetchAndCreateBranch());
            RegisterClick<RemoteBranchNode>(mnubtnFetchRebase, remoteBranch => remoteBranch.FetchAndRebase());

            // BranchPathNode (folder)
            RegisterClick<BranchPathNode>(mnubtnDeleteAllBranches, branchPath => branchPath.DeleteAll());
            RegisterClick<BranchPathNode>(mnubtnCreateBranch, branchPath => branchPath.CreateBranch());

            // Expand / Collapse
            RegisterClick(mnubtnCollapse, () => GetMultiSelection().HavingChildren().Collapsible().ForEach(parent => parent.TreeViewNode.Collapse()));
            RegisterClick(mnubtnExpand, () => GetMultiSelection().HavingChildren().Expandable().ForEach(parent => parent.TreeViewNode.ExpandAll()));

            // Move up / down (for top level Trees)
            RegisterClick(mnubtnMoveUp, () => ReorderTreeNode(treeMain.SelectedNode, up: true));
            RegisterClick(mnubtnMoveDown, () => ReorderTreeNode(treeMain.SelectedNode, up: false));

            // Sort by / order
            _sortByContextMenuItem = new GitRefsSortByContextMenuItem(() => Refresh(new FilteredGitRefsProvider(UICommands.GitModule).GetRefs));
            _sortOrderContextMenuItem = new GitRefsSortOrderContextMenuItem(() => Refresh(new FilteredGitRefsProvider(UICommands.GitModule).GetRefs));
            menuMain.InsertItems(new ToolStripItem[] { new ToolStripSeparator(), _sortByContextMenuItem, _sortOrderContextMenuItem }, after: mnubtnMoveDown);
        }

        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (sender is not ContextMenuStrip contextMenu)
            {
                return;
            }

            var selectedNodes = GetMultiSelection().ToArray();
            var hasSingleSelection = selectedNodes.Length <= 1;
            var selectedNode = treeMain.SelectedNode.Tag as NodeBase;

            EnableMenuItems(hasSingleSelection && selectedNode is BaseBranchLeafNode branch && branch.Visible, copyContextMenuItem);
            EnableMenuItems(selectedNodes.OfType<IGitRefActions>().Any(), filterForSelectedRefsMenuItem);

            EnableMenuItems(_localBranchMenuItems, t => selectedNode is LocalBranchNode localBranch
                && hasSingleSelection // only display for single-selected branch
                && (!localBranch.IsCurrent // with all items for non-current branches
                    || LocalBranchMenuItems<LocalBranchNode>.CurrentBranchItemKeys.Contains(t.Key))); // or only those applying to the current branch

            // remote branch
            var isSingleRemoteBranchSelected = hasSingleSelection && selectedNode is RemoteBranchNode;
            EnableMenuItems(_remoteBranchMenuItems, _ => isSingleRemoteBranchSelected);
            EnableMenuItems(isSingleRemoteBranchSelected, mnubtnFetchOneBranch, mnubtnPullFromRemoteBranch,
                mnubtnRemoteBranchFetchAndCheckout, mnubtnFetchCreateBranch, mnubtnFetchRebase);

            EnableMenuItems(_tagNodeMenuItems, _ => hasSingleSelection && selectedNode is TagNode);
            EnableMenuItems(hasSingleSelection && selectedNode is RemoteBranchTree, mnuBtnManageRemotesFromRootNode, mnuBtnFetchAllRemotes, mnuBtnPruneAllRemotes);

            // remote repo
            var isSingleRemoteRepoSelected = hasSingleSelection && selectedNode is RemoteRepoNode;
            var remoteRepo = selectedNode as RemoteRepoNode;
            EnableMenuItems(isSingleRemoteRepoSelected, mnubtnManageRemotes);
            EnableMenuItems(isSingleRemoteRepoSelected && remoteRepo.Enabled, mnubtnFetchAllBranchesFromARemote, mnubtnDisableRemote, mnuBtnPruneAllBranchesFromARemote);
            EnableMenuItems(isSingleRemoteRepoSelected && remoteRepo.IsRemoteUrlUsingHttp, mnuBtnOpenRemoteUrlInBrowser);
            EnableMenuItems(isSingleRemoteRepoSelected && !remoteRepo.Enabled, mnubtnEnableRemote, mnubtnEnableRemoteAndFetch);

            // submodule
            var isSingleSubmoduleSelected = hasSingleSelection && selectedNode is SubmoduleNode;
            var submoduleNode = selectedNode as SubmoduleNode;
            var bareRepository = Module.IsBareRepository();
            EnableMenuItems(isSingleSubmoduleSelected && submoduleNode.CanOpen, mnubtnOpenSubmodule, mnubtnOpenGESubmodule);
            EnableMenuItems(isSingleSubmoduleSelected, mnubtnUpdateSubmodule);
            EnableMenuItems(isSingleSubmoduleSelected && !bareRepository && submoduleNode.IsCurrent, mnubtnManageSubmodules, mnubtnSynchronizeSubmodules);
            EnableMenuItems(isSingleSubmoduleSelected && !bareRepository, mnubtnResetSubmodule, mnubtnStashSubmodule, mnubtnCommitSubmodule);

            EnableMenuItems(hasSingleSelection && selectedNode is BranchPathNode, mnubtnCreateBranch, mnubtnDeleteAllBranches);

            // expand / collapse
            var multiSelectedParents = selectedNodes.HavingChildren().ToArray();
            EnableMenuItems(multiSelectedParents.Expandable().Any(), mnubtnExpand);
            EnableMenuItems(multiSelectedParents.Collapsible().Any(), mnubtnCollapse);

            // move up / down (for top-level trees)
            var isSingleTreeSelected = hasSingleSelection && selectedNode is Tree;
            var treeNode = (selectedNode as Tree)?.TreeViewNode;
            EnableMenuItems(isSingleTreeSelected && treeNode.PrevNode is not null, mnubtnMoveUp);
            EnableMenuItems(isSingleTreeSelected && treeNode.NextNode is not null, mnubtnMoveDown);

            var isSingleRefSelected = hasSingleSelection && selectedNode is IGitRefActions;
            EnableMenuItems(isSingleRefSelected, _sortByContextMenuItem);

            // If refs are sorted by git (GitRefsSortBy = Default) don't show sort order options
            var showSortOrder = AppSettings.RefsSortBy != GitRefsSortBy.Default;
            EnableMenuItems(isSingleRefSelected && showSortOrder, _sortOrderContextMenuItem);

            if (hasSingleSelection && selectedNode is LocalBranchNode localBranch && localBranch.Visible)
            {
                contextMenu.AddUserScripts(runScriptToolStripMenuItem, _scriptRunner.Execute);
            }
            else
            {
                contextMenu.RemoveUserScripts(runScriptToolStripMenuItem);
            }

            contextMenu.ToggleSeparators();

            /* Cancel context menu if no items are enabled.
             * This relies on the items' Enabled flag being toggled (e.g. by EnableMenuItems) and BEFORE this line. */
            e.Cancel = !contextMenu.Items.Cast<ToolStripItem>().Any(i => i.Enabled);
        }

        /// <inheritdoc />
        public TMenuItem CreateMenuItem<TMenuItem, TNode>(Action<TNode> onClick, TranslationString text, TranslationString toolTip, Bitmap? icon = null)
            where TMenuItem : ToolStripItem, new()
            where TNode : class, INode
        {
            TMenuItem result = new();
            result.Image = icon;
            result.Text = text.Text;
            result.ToolTipText = toolTip.Text;
            RegisterClick(result, onClick);
            return result;
        }
    }
}
