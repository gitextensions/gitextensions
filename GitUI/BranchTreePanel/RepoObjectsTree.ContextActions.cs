using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.BranchTreePanel.ContextMenu;
using GitUI.BranchTreePanel.Interfaces;
using GitUI.CommandsDialogs;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.BranchTreePanel
{
    partial class RepoObjectsTree : IMenuItemFactory
    {
        private GitRefsSortOrderContextMenuItem _sortOrderContextMenuItem;
        private GitRefsSortByContextMenuItem _sortByContextMenuItem;
        private ToolStripSeparator _tsmiSortMenuSpacer = new() { Name = "tsmiSortMenuSpacer" };
        private ToolStripMenuItem _runScriptToolStripMenuItem = new("Run script", Properties.Images.Console);
        private ToolStripItem[] _menuBranchCopyContextMenuItems = Array.Empty<ToolStripItem>();
        private ToolStripItem[] _menuRemoteCopyContextMenuItems = Array.Empty<ToolStripItem>();

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

        /* add Expand All / Collapse All menu entry
         * depending on whether node is expanded or collapsed and has child nodes at all */
        private void EnableExpandCollapseContextMenu(ContextMenuStrip contextMenu, NodeBase[] selectedNodes)
        {
            var multiSelectedParents = selectedNodes.HavingChildren().ToArray();

            // selection contains nodes with children
            if (multiSelectedParents.Length > 0)
            {
                if (contextMenu == menuMain)
                {
                    contextMenu.Items.Clear();
                }
                else if (contextMenu.Items.Count > 0)
                {
                    contextMenu.SetLastItem(tsmiMainMenuSpacer1); // add a separator if any items exist already

                    /* Display separator if any preceding items are enabled.
                     * This relies on the items' Enabled flag being toggled (e.g. by EnableMenuItems) and BEFORE this method. */
                    var precedingItems = contextMenu.Items.Cast<ToolStripItem>().TakeWhile(item => item != tsmiMainMenuSpacer1);
                    EnableMenuItems(precedingItems.Any(item => item.Enabled), tsmiMainMenuSpacer1);
                }

                contextMenu.SetLastItem(mnubtnCollapse);
                contextMenu.SetLastItem(mnubtnExpand);
                EnableMenuItems(multiSelectedParents.Expandable().Any(), mnubtnExpand);
                EnableMenuItems(multiSelectedParents.Collapsible().Any(), mnubtnCollapse);
            }

            // no expandable or collapsible nodes selected
            else
            {
                EnableMenuItems(false, tsmiMainMenuSpacer1, mnubtnExpand, mnubtnCollapse);
            }
        }

        // adds Move Up / Move Down menu entries for re-arranging top level tree nodes
        private void EnableMoveTreeUpDownContexMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu.GetSelectedNode() is not Tree tree)
            {
                return;
            }

            contextMenu.SetLastItem(tsmiMainMenuSpacer2); // add another separator
            contextMenu.SetLastItem(mnubtnMoveUp);
            contextMenu.SetLastItem(mnubtnMoveDown);

            var treeNode = tree.TreeViewNode;
            EnableMenuItems(hasSingleSelection, tsmiMainMenuSpacer2);
            EnableMenuItems(hasSingleSelection && treeNode.PrevNode is not null, mnubtnMoveUp);
            EnableMenuItems(hasSingleSelection && treeNode.NextNode is not null, mnubtnMoveDown);
        }

        private void EnableLocalBranchContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu != menuBranch || contextMenu.GetSelectedNode() is not LocalBranchNode localBranch)
            {
                return;
            }

            EnableMenuItems(_localBranchMenuItems,
                t => hasSingleSelection // only display for single-selected branch
                 && (!localBranch.IsCurrent // with all items for non-current branches
                     || LocalBranchMenuItems<LocalBranchNode>.CurrentBranchItemKeys.Contains(t.Key))); // or only those applying to the current branch

            EnableMenuItems(localBranch.Visible && hasSingleSelection, _menuBranchCopyContextMenuItems);
        }

        private void EnableRunScriptContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu != menuBranch || contextMenu.GetSelectedNode() is not LocalBranchNode localBranch)
            {
                return;
            }

            contextMenu.SetLastItem(_runScriptToolStripMenuItem);

            if (localBranch.Visible && hasSingleSelection)
            {
                contextMenu.AddUserScripts(_runScriptToolStripMenuItem, _scriptRunner.Execute);
            }
            else
            {
                contextMenu.RemoveUserScripts(_runScriptToolStripMenuItem);
            }
        }

        private void EnableBranchPathContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu != menuBranchPath || contextMenu.GetSelectedNode() is not BranchPathNode)
            {
                return;
            }

            // only display items in single-selection context
            EnableMenuItems(hasSingleSelection, mnubtnCreateBranch, mnubtnDeleteAllBranches);
        }

        private void EnableRemoteBranchContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu != menuRemote || contextMenu.GetSelectedNode() is not RemoteBranchNode node)
            {
                return;
            }

            EnableMenuItems(_remoteBranchMenuItems, _ => hasSingleSelection);

            // toggle remote branch menu items operating on a single branch
            EnableMenuItems(hasSingleSelection, mnubtnFetchOneBranch, mnubtnPullFromRemoteBranch,
                mnubtnRemoteBranchFetchAndCheckout, mnubtnFetchCreateBranch, mnubtnFetchRebase, toolStripSeparator1);

            EnableMenuItems(node.Visible && hasSingleSelection, _menuRemoteCopyContextMenuItems);
        }

        private void EnableRemoteRepoContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu != menuRemoteRepoNode || contextMenu.GetSelectedNode() is not RemoteRepoNode node)
            {
                return;
            }

            EnableMenuItems(hasSingleSelection, mnubtnManageRemotes, tsmiSpacer3);

            // Actions on enabled remotes
            EnableMenuItems(hasSingleSelection && node.Enabled, mnubtnFetchAllBranchesFromARemote, mnubtnDisableRemote, mnuBtnPruneAllBranchesFromARemote);

            EnableMenuItems(hasSingleSelection && node.IsRemoteUrlUsingHttp, mnuBtnOpenRemoteUrlInBrowser);

            // Actions on disabled remotes
            EnableMenuItems(hasSingleSelection && !node.Enabled, mnubtnEnableRemote, mnubtnEnableRemoteAndFetch);
        }

        private void EnableRemotesTreeContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu != menuRemotes || contextMenu.GetSelectedNode() is not Tree)
            {
                return;
            }

            EnableMenuItems(hasSingleSelection, mnuBtnManageRemotesFromRootNode, mnuBtnFetchAllRemotes, mnuBtnPruneAllRemotes);
        }

        private void EnableTagContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu != menuTag || contextMenu.GetSelectedNode() is not TagNode node)
            {
                return;
            }

            EnableMenuItems(_tagNodeMenuItems, _ => hasSingleSelection);
        }

        private void EnableSortContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            // We can only sort refs, i.e. branches and tags
            if (contextMenu != menuBranch && contextMenu != menuRemote && contextMenu != menuTag)
            {
                return;
            }

            // Add the following to the every participating context menu:
            //
            //    ---------
            //    Sort By...
            //    Sort Order...

            contextMenu.SetLastItem(_tsmiSortMenuSpacer);
            contextMenu.SetLastItem(_sortByContextMenuItem);
            contextMenu.SetLastItem(_sortOrderContextMenuItem);

            // sorting doesn't make a lot of sense in a multi-selection context
            EnableMenuItems(hasSingleSelection, _tsmiSortMenuSpacer, _sortByContextMenuItem);

            // If refs are sorted by git (GitRefsSortBy = Default) don't show sort order options
            var showSortOrder = AppSettings.RefsSortBy != GitUIPluginInterfaces.GitRefsSortBy.Default;
            EnableMenuItems(hasSingleSelection && showSortOrder, _sortOrderContextMenuItem);
        }

        private void EnableSubmoduleContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu != menuSubmodule || contextMenu.GetSelectedNode() is not SubmoduleNode submoduleNode)
            {
                return;
            }

            var bareRepository = Module.IsBareRepository();

            EnableMenuItems(hasSingleSelection && submoduleNode.CanOpen, mnubtnOpenSubmodule, mnubtnOpenGESubmodule);
            EnableMenuItems(hasSingleSelection, mnubtnUpdateSubmodule);
            EnableMenuItems(hasSingleSelection && !bareRepository && submoduleNode.IsCurrent, mnubtnManageSubmodules, mnubtnSynchronizeSubmodules);
            EnableMenuItems(hasSingleSelection && !bareRepository, mnubtnResetSubmodule, mnubtnStashSubmodule, mnubtnCommitSubmodule);
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
            CreateFilterSelectedRefsContextMenuItem();

            #region Sort by / order
            _sortOrderContextMenuItem = new GitRefsSortOrderContextMenuItem(() => Refresh(new FilteredGitRefsProvider(UICommands.GitModule).GetRefs));
            _sortByContextMenuItem = new GitRefsSortByContextMenuItem(() => Refresh(new FilteredGitRefsProvider(UICommands.GitModule).GetRefs));
            #endregion

            #region Expand / Collapse
            RegisterClick(mnubtnCollapse, () => GetMultiSelection().HavingChildren().Collapsible().ForEach(parent => parent.TreeViewNode.Collapse()));
            RegisterClick(mnubtnExpand, () => GetMultiSelection().HavingChildren().Expandable().ForEach(parent => parent.TreeViewNode.ExpandAll()));
            #endregion

            #region Move up / down (for top level Trees)
            RegisterClick(mnubtnMoveUp, () => ReorderTreeNode(treeMain.SelectedNode, up: true));
            RegisterClick(mnubtnMoveDown, () => ReorderTreeNode(treeMain.SelectedNode, up: false));
            #endregion

            #region LocalBranchNode
            _menuBranchCopyContextMenuItems = CreateCopyContextMenuItems();
            menuBranch.Items.AddRange(_menuBranchCopyContextMenuItems);
            _localBranchMenuItems = new LocalBranchMenuItems<LocalBranchNode>(this);
            menuBranch.Items.AddRange(_localBranchMenuItems.Select(s => s.Item).ToArray());
            Node.RegisterContextMenu(typeof(LocalBranchNode), menuBranch);
            #endregion

            #region BranchPathNode (folder)
            Node.RegisterContextMenu(typeof(BranchPathNode), menuBranchPath);
            RegisterClick<BranchPathNode>(mnubtnDeleteAllBranches, branchPath => branchPath.DeleteAll());
            RegisterClick<BranchPathNode>(mnubtnCreateBranch, branchPath => branchPath.CreateBranch());
            #endregion

            #region RemoteBranchNode
            _menuRemoteCopyContextMenuItems = CreateCopyContextMenuItems();
            menuRemote.InsertItems(_menuRemoteCopyContextMenuItems);
            _remoteBranchMenuItems = new RemoteBranchMenuItems<RemoteBranchNode>(this);
            menuRemote.InsertItems(_remoteBranchMenuItems.Select(s => s.Item), after: toolStripSeparator1);

            RegisterClick<RemoteBranchNode>(mnubtnFetchOneBranch, remoteBranch => remoteBranch.Fetch());
            RegisterClick<RemoteBranchNode>(mnubtnPullFromRemoteBranch, remoteBranch => remoteBranch.FetchAndMerge());
            RegisterClick<RemoteBranchNode>(mnubtnRemoteBranchFetchAndCheckout, remoteBranch => remoteBranch.FetchAndCheckout());
            RegisterClick<RemoteBranchNode>(mnubtnFetchCreateBranch, remoteBranch => remoteBranch.FetchAndCreateBranch());
            RegisterClick<RemoteBranchNode>(mnubtnFetchRebase, remoteBranch => remoteBranch.FetchAndRebase());
            Node.RegisterContextMenu(typeof(RemoteBranchNode), menuRemote);
            #endregion

            #region RemoteRepoNode
            RegisterClick<RemoteRepoNode>(mnubtnManageRemotes, remoteBranch => remoteBranch.PopupManageRemotesForm());
            RegisterClick<RemoteRepoNode>(mnubtnFetchAllBranchesFromARemote, remote => remote.Fetch());
            RegisterClick<RemoteRepoNode>(mnuBtnPruneAllBranchesFromARemote, remote => remote.Prune());
            RegisterClick<RemoteRepoNode>(mnuBtnOpenRemoteUrlInBrowser, remote => remote.OpenRemoteUrlInBrowser());
            RegisterClick<RemoteRepoNode>(mnubtnEnableRemote, remote => remote.Enable(fetch: false));
            RegisterClick<RemoteRepoNode>(mnubtnEnableRemoteAndFetch, remote => remote.Enable(fetch: true));
            RegisterClick<RemoteRepoNode>(mnubtnDisableRemote, remote => remote.Disable());
            Node.RegisterContextMenu(typeof(RemoteRepoNode), menuRemoteRepoNode);
            #endregion

            #region TagNode
            _tagNodeMenuItems = new TagMenuItems<TagNode>(this);
            menuTag.InsertItems(_tagNodeMenuItems.Select(s => s.Item));
            Node.RegisterContextMenu(typeof(TagNode), menuTag);
            #endregion

            #region Remotes Tree
            RegisterClick(mnuBtnManageRemotesFromRootNode, () => _remotesTree.PopupManageRemotesForm(remoteName: null));
            RegisterClick(mnuBtnFetchAllRemotes, () => _remotesTree.FetchAll());
            RegisterClick(mnuBtnPruneAllRemotes, () => _remotesTree.FetchPruneAll());
            #endregion

            #region SubmoduleNode
            RegisterClick<SubmoduleNode>(mnubtnManageSubmodules, _ => _submoduleTree.ManageSubmodules(this));
            RegisterClick<SubmoduleNode>(mnubtnSynchronizeSubmodules, _ => _submoduleTree.SynchronizeSubmodules(this));
            RegisterClick<SubmoduleNode>(mnubtnOpenSubmodule, node => _submoduleTree.OpenSubmodule(this, node));
            RegisterClick<SubmoduleNode>(mnubtnOpenGESubmodule, node => _submoduleTree.OpenSubmoduleInGitExtensions(this, node));
            RegisterClick<SubmoduleNode>(mnubtnUpdateSubmodule, node => _submoduleTree.UpdateSubmodule(this, node));
            RegisterClick<SubmoduleNode>(mnubtnResetSubmodule, node => _submoduleTree.ResetSubmodule(this, node));
            RegisterClick<SubmoduleNode>(mnubtnStashSubmodule, node => _submoduleTree.StashSubmodule(this, node));
            RegisterClick<SubmoduleNode>(mnubtnCommitSubmodule, node => _submoduleTree.CommitSubmodule(this, node));
            Node.RegisterContextMenu(typeof(SubmoduleNode), menuSubmodule);
            #endregion
        }

        private ToolStripItem[] CreateCopyContextMenuItems()
        {
            CopyContextMenuItem copyContextMenuItem = new();

            copyContextMenuItem.SetRevisionFunc(() => _scriptHost.GetSelectedRevisions());

            return new ToolStripItem[]
            {
                copyContextMenuItem,
                new ToolStripSeparator()
            };
        }

        #region Filter for selected refs
        private Action<string?> _filterRevisionGridBySpaceSeparatedRefs;
        private ToolStripMenuItem _filterForSelectedRefsMenuItem;

        private void CreateFilterSelectedRefsContextMenuItem()
        {
            _filterForSelectedRefsMenuItem = new("&Filter for selected", Properties.Images.ShowThisBranchOnly)
            {
                ToolTipText = "Filter the revision grid to show selected (underlined) refs (branches and tags) only." +
                    "\nHold CTRL while clicking to de/select multiple and include descendant tree nodes by additionally holding SHIFT." +
                    "\nReset the filter via View > Show all branches."
            };

            RegisterClick(_filterForSelectedRefsMenuItem, () =>
            {
                var refPaths = GetMultiSelection().OfType<IGitRefActions>().Select(b => b.FullPath);
                _filterRevisionGridBySpaceSeparatedRefs(refPaths.Join(" "));
            });
        }

        private void EnableFilterSelectedRefsContextMenu(ContextMenuStrip contextMenu, NodeBase[] selectedNodes)
        {
            var selectionContainsRefs = selectedNodes.OfType<IGitRefActions>().Any();
            contextMenu.SetLastItem(_filterForSelectedRefsMenuItem);
            EnableMenuItems(selectionContainsRefs, _filterForSelectedRefsMenuItem);
        }
        #endregion

        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (sender is not ContextMenuStrip contextMenu)
            {
                return;
            }

            var selectedNodes = GetMultiSelection().ToArray();
            var hasSingleSelection = selectedNodes.Length <= 1;

            EnableLocalBranchContextMenu(contextMenu, hasSingleSelection);
            EnableBranchPathContextMenu(contextMenu, hasSingleSelection);
            EnableRemoteBranchContextMenu(contextMenu, hasSingleSelection);
            EnableRemoteRepoContextMenu(contextMenu, hasSingleSelection);
            EnableRemotesTreeContextMenu(contextMenu, hasSingleSelection);
            EnableTagContextMenu(contextMenu, hasSingleSelection);
            EnableSubmoduleContextMenu(contextMenu, hasSingleSelection);
            EnableFilterSelectedRefsContextMenu(contextMenu, selectedNodes);
            EnableSortContextMenu(contextMenu, hasSingleSelection);
            EnableExpandCollapseContextMenu(contextMenu, selectedNodes);
            EnableRunScriptContextMenu(contextMenu, hasSingleSelection);
            EnableMoveTreeUpDownContexMenu(contextMenu, hasSingleSelection);

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
