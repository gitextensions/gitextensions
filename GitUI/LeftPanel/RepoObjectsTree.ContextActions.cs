using System.ComponentModel;
using GitCommands;
using GitUI.CommandsDialogs;
using GitUI.LeftPanel.ContextMenu;
using GitUI.LeftPanel.Interfaces;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.LeftPanel
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

        private static void EnableMenuItems(bool enabled, params ToolStripItem[] items)
        {
            foreach (ToolStripItem item in items)
            {
                item.Enable(enabled);
            }
        }

        private static void EnableMenuItems<TNode>(MenuItemsGenerator<TNode> generator, Func<ToolStripItemWithKey, bool> isEnabled) where TNode : class, INode
        {
            foreach (ToolStripItemWithKey item in generator)
            {
                item.Item.Enable(isEnabled(item));
            }
        }

        /* add Expand All / Collapse All menu entry
         * depending on whether node is expanded or collapsed and has child nodes at all */
        private void EnableExpandCollapseContextMenu(NodeBase[] selectedNodes)
        {
            NodeBase[] multiSelectedParents = selectedNodes.HavingChildren().ToArray();
            mnubtnExpand.Visible = mnubtnCollapse.Visible = multiSelectedParents.Length > 0;
            mnubtnExpand.Enabled = multiSelectedParents.Expandable().Any();
            mnubtnCollapse.Enabled = multiSelectedParents.Collapsible().Any();
        }

        private void EnableMoveTreeUpDownContexMenu(bool hasSingleSelection, NodeBase? selectedNode)
        {
            bool isSingleTreeSelected = hasSingleSelection && selectedNode is Tree;
            TreeNode? treeNode = (selectedNode as Tree)?.TreeViewNode;
            mnubtnMoveUp.Visible = mnubtnMoveDown.Visible = isSingleTreeSelected;
            mnubtnMoveUp.Enabled = isSingleTreeSelected && treeNode?.PrevNode is not null;
            mnubtnMoveDown.Enabled = isSingleTreeSelected && treeNode?.NextNode is not null;
        }

        private void EnableRemoteBranchContextMenu(bool hasSingleSelection, NodeBase? selectedNode)
        {
            bool isSingleRemoteBranchSelected = hasSingleSelection && selectedNode is RemoteBranchNode;
            EnableMenuItems(_remoteBranchMenuItems, _ => isSingleRemoteBranchSelected);

            EnableMenuItems(isSingleRemoteBranchSelected, mnubtnFetchOneBranch, mnubtnPullFromRemoteBranch,
                mnubtnRemoteBranchFetchAndCheckout, mnubtnFetchCreateBranch, mnubtnFetchRebase);
        }

        private void EnableRemoteRepoContextMenu(bool hasSingleSelection, NodeBase? selectedNode)
        {
            bool isSingleRemoteRepoSelected = hasSingleSelection && selectedNode is RemoteRepoNode;
            RemoteRepoNode? remoteRepo = selectedNode as RemoteRepoNode;
            mnubtnManageRemotes.Enable(isSingleRemoteRepoSelected);
            EnableMenuItems(isSingleRemoteRepoSelected && remoteRepo?.Enabled is true, mnubtnFetchAllBranchesFromARemote, mnubtnDisableRemote, mnuBtnPruneAllBranchesFromARemote);
            mnuBtnOpenRemoteUrlInBrowser.Enable(isSingleRemoteRepoSelected && remoteRepo?.IsRemoteUrlUsingHttp is true);
            EnableMenuItems(isSingleRemoteRepoSelected && remoteRepo?.Enabled is false, mnubtnEnableRemote, mnubtnEnableRemoteAndFetch);
        }

        private void EnableSortContextMenu(bool hasSingleSelection, NodeBase? selectedNode)
        {
            bool isSingleRefSelected = hasSingleSelection && selectedNode is IGitRefActions;
            _sortByContextMenuItem.Enable(isSingleRefSelected);

            // If refs are sorted by git (GitRefsSortBy = Default) don't show sort order options
            bool showSortOrder = AppSettings.RefsSortBy != GitRefsSortBy.Default;
            _sortOrderContextMenuItem.Enable(isSingleRefSelected && showSortOrder);
        }

        private void EnableStashContextMenu(bool hasSingleSelection, NodeBase? selectedNode)
        {
            bool isSingleStashSelected = hasSingleSelection && selectedNode is StashNode;
            bool isBareRepository = Module.IsBareRepository();
            EnableMenuItems(isSingleStashSelected && !isBareRepository, mnubtnOpenStash, mnubtnApplyStash, mnubtnPopStash, mnubtnDropStash);
        }

        private void EnableSubmoduleContextMenu(bool hasSingleSelection, NodeBase? selectedNode)
        {
            bool isSingleSubmoduleSelected = hasSingleSelection && selectedNode is SubmoduleNode;
            SubmoduleNode? submoduleNode = selectedNode as SubmoduleNode;
            bool isBareRepository = Module.IsBareRepository();
            mnubtnOpenSubmodule.Enable(isSingleSubmoduleSelected && submoduleNode?.IsCurrent is false);
            EnableMenuItems(isSingleSubmoduleSelected, mnubtnOpenGESubmodule, mnubtnUpdateSubmodule);
            EnableMenuItems(isSingleSubmoduleSelected && !isBareRepository && submoduleNode?.IsCurrent is true, mnubtnManageSubmodules, mnubtnSynchronizeSubmodules);
            EnableMenuItems(isSingleSubmoduleSelected && !isBareRepository, mnubtnResetSubmodule, mnubtnStashSubmodule, mnubtnCommitSubmodule);
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
            copyContextMenuItem.SetRevisionFunc(() => _revisionGridInfo.GetSelectedRevisions());

            // Filter for selected
            filterForSelectedRefsMenuItem.ToolTipText = "Filter the revision grid to show selected (underlined) refs (branches and tags) only." +
                "\nHold CTRL while clicking to de/select multiple and include descendant tree nodes by additionally holding SHIFT." +
                "\nReset the filter via View > Show all branches.";

            RegisterClick(filterForSelectedRefsMenuItem, () =>
            {
                var refPaths = GetSelectedNodes().OfType<IGitRefActions>().Select(b => b.FullPath);
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
            RegisterClick<SubmoduleNode>(mnubtnOpenSubmodule, node => _submoduleTree.OpenSubmodule(this, node));
            RegisterClick<SubmoduleNode>(mnubtnOpenGESubmodule, node => _submoduleTree.OpenSubmoduleInGitExtensions(this, node));
            RegisterClick<SubmoduleNode>(mnubtnManageSubmodules, _ => _submoduleTree.ManageSubmodules(this));
            RegisterClick<SubmoduleNode>(mnubtnSynchronizeSubmodules, _ => _submoduleTree.SynchronizeSubmodules(this));
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

            // Stash
            RegisterClick(mnubtnStashAllFromRootNode, () => _stashTree.StashAll(this));
            RegisterClick(mnubtnStashStagedFromRootNode, () => _stashTree.StashStaged(this));
            RegisterClick(mnubtnManageStashFromRootNode, () => _stashTree.OpenStash(this));
            RegisterClick<StashNode>(mnubtnOpenStash, node => node.OpenStash(this));
            RegisterClick<StashNode>(mnubtnApplyStash, node => node.ApplyStash(this));
            RegisterClick<StashNode>(mnubtnPopStash, node => node.PopStash(this));
            RegisterClick<StashNode>(mnubtnDropStash, node => node.DropStash(this));

            // Expand / Collapse
            RegisterClick(mnubtnCollapse, () => GetSelectedNodes().HavingChildren().Collapsible().ForEach(node => node.TreeViewNode.Collapse()));
            RegisterClick(mnubtnExpand, () => GetSelectedNodes().HavingChildren().Expandable().ForEach(node => node.TreeViewNode.ExpandAll()));

            // Move up / down (for top level Trees)
            RegisterClick(mnubtnMoveUp, () => ReorderTreeNode(treeMain.SelectedNode, up: true));
            RegisterClick(mnubtnMoveDown, () => ReorderTreeNode(treeMain.SelectedNode, up: false));

            // Sort by / order
            _sortByContextMenuItem = new GitRefsSortByContextMenuItem(() => ResortRefs(new FilteredGitRefsProvider(UICommands.GitModule).GetRefs));
            _sortOrderContextMenuItem = new GitRefsSortOrderContextMenuItem(() => ResortRefs(new FilteredGitRefsProvider(UICommands.GitModule).GetRefs));
            menuMain.InsertItems(new ToolStripItem[] { new ToolStripSeparator(), _sortByContextMenuItem, _sortOrderContextMenuItem }, after: mnubtnMoveDown);
        }

        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (sender is not ContextMenuStrip contextMenu)
            {
                return;
            }

            NodeBase[] selectedNodes = GetSelectedNodes().ToArray();
            bool hasSingleSelection = selectedNodes.Length == 1;
            NodeBase? selectedNode = treeMain.SelectedNode?.Tag as NodeBase;

            copyContextMenuItem.Enable(hasSingleSelection && (selectedNode is BaseBranchLeafNode or StashNode) && selectedNode.Visible);
            filterForSelectedRefsMenuItem.Enable(selectedNodes.OfType<IGitRefActions>().Any()); // enable if selection contains refs

            var selectedLocalBranch = selectedNode as LocalBranchNode;

            foreach (ToolStripItemWithKey item in _localBranchMenuItems)
            {
                bool visible = hasSingleSelection && selectedLocalBranch != null;
                item.Item.Visible = visible; // only display for single-selected branch

                /* Enabled items must also be visible; cancellation of menu opening below relies on it.
                 * Read from local variable because ToolStripItem.Visible will always returns false
                 * because the ContextMenuStrip as the visual parent is not Visible on Opening. */
                item.Item.Enabled = visible

                    // enable all items for non-current branches or only those applying to the current branch
                    && (selectedLocalBranch?.IsCurrent == false || LocalBranchMenuItems<LocalBranchNode>.CurrentBranchItemKeys.Contains(item.Key));
            }

            EnableRemoteBranchContextMenu(hasSingleSelection, selectedNode);
            EnableMenuItems(_tagNodeMenuItems, _ => hasSingleSelection && selectedNode is TagNode);
            EnableMenuItems(hasSingleSelection && selectedNode is RemoteBranchTree, mnuBtnManageRemotesFromRootNode, mnuBtnFetchAllRemotes, mnuBtnPruneAllRemotes);
            EnableRemoteRepoContextMenu(hasSingleSelection, selectedNode);
            EnableMenuItems(hasSingleSelection && selectedNode is StashTree, mnubtnStashAllFromRootNode, mnubtnStashStagedFromRootNode, mnubtnManageStashFromRootNode);
            EnableStashContextMenu(hasSingleSelection, selectedNode);
            EnableSubmoduleContextMenu(hasSingleSelection, selectedNode);
            EnableMenuItems(hasSingleSelection && selectedNode is BranchPathNode, mnubtnCreateBranch, mnubtnDeleteAllBranches);
            EnableExpandCollapseContextMenu(selectedNodes);
            EnableMoveTreeUpDownContexMenu(hasSingleSelection, selectedNode);
            EnableSortContextMenu(hasSingleSelection, selectedNode);

            if (hasSingleSelection && selectedLocalBranch?.Visible == true)
            {
                contextMenu.AddUserScripts(runScriptToolStripMenuItem, _scriptRunner.Execute);
            }
            else
            {
                contextMenu.RemoveUserScripts(runScriptToolStripMenuItem);
            }

            /* Cancel context menu opening if no items are Enabled.
             * This relies on that flag being set correctly on all menu items above. */
            e.Cancel = !contextMenu.Items.OfType<ToolStripMenuItem>().Any(i => i.Enabled);
        }

        private void contextMenu_Opened(object sender, EventArgs e)
        {
            if (sender is not ContextMenuStrip contextMenu)
            {
                return;
            }

            // Waiting for the ContextMenuStrip (as the visual parent of its menu items) to be visible to
            // toggle (depending on ToolStripItem.Visible) existing separators in between item groups as required.
            contextMenu.ToggleSeparators();

            // Working around the context menu strip being positioned incorrectly on first open - which may be a Windows Forms bug,
            // see https://stackoverflow.com/q/15841863/2338036.
            if (contextMenu.Top != Cursor.Position.Y)
            {
                contextMenu.Top = Cursor.Position.Y;
            }
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
