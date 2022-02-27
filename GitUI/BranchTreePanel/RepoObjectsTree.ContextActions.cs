using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

        private void ToggleMenuItems(bool enabled, params ToolStripItem[] items)
        {
            foreach (var item in items)
            {
                item.Toggle(enabled);
            }
        }

        private void ToggleMenuItems<TNode>(MenuItemsGenerator<TNode> generator, Func<ToolStripItemWithKey, bool> isEnabled) where TNode : class, INode
            => generator.ForEach(i => ToggleMenuItems(isEnabled(i), i.Item));

        /* add Expand All / Collapse All menu entry
         * depending on whether node is expanded or collapsed and has child nodes at all */
        private void ToggleExpandCollapseContextMenu(ContextMenuStrip contextMenu, NodeBase[] selectedNodes)
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
                    contextMenu.AddOnce(tsmiMainMenuSpacer1); // add a separator if any items exist already

                    /* Display separator if any preceding items are enabled.
                     * This relies on the items' Enabled flag being toggled (e.g. by ToggleMenuItems) and BEFORE this method. */
                    var precedingItems = contextMenu.Items.Cast<ToolStripItem>().TakeWhile(item => item != tsmiMainMenuSpacer1);
                    ToggleMenuItems(precedingItems.Any(item => item.Enabled), tsmiMainMenuSpacer1);
                }

                contextMenu.AddOnce(mnubtnCollapse);
                contextMenu.AddOnce(mnubtnExpand);
                ToggleMenuItems(multiSelectedParents.Expandable().Any(), mnubtnExpand);
                ToggleMenuItems(multiSelectedParents.Collapsible().Any(), mnubtnCollapse);
            }

            // no expandable or collapsible nodes selected
            else
            {
                ToggleMenuItems(false, tsmiMainMenuSpacer1, mnubtnExpand, mnubtnCollapse);
            }
        }

        // adds Move Up / Move Down menu entries for re-arranging top level tree nodes
        private void ToggleMoveTreeUpDownContexMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu.GetSelectedNode() is not Tree tree)
            {
                return;
            }

            contextMenu.AddOnce(tsmiMainMenuSpacer2); // add another separator
            contextMenu.AddOnce(mnubtnMoveUp);
            contextMenu.AddOnce(mnubtnMoveDown);

            var treeNode = tree.TreeViewNode;
            ToggleMenuItems(hasSingleSelection, tsmiMainMenuSpacer2);
            ToggleMenuItems(hasSingleSelection && treeNode.PrevNode is not null, mnubtnMoveUp);
            ToggleMenuItems(hasSingleSelection && treeNode.NextNode is not null, mnubtnMoveDown);
        }

        private void ToggleLocalBranchContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu != menuBranch || contextMenu.GetSelectedNode() is not LocalBranchNode localBranch)
            {
                return;
            }

            ToggleMenuItems(_localBranchMenuItems,
                t => hasSingleSelection // only display for single-selected branch
                 && (!localBranch.IsCurrent // with all items for non-current branches
                     || LocalBranchMenuItems<LocalBranchNode>.CurrentBranchItemKeys.Contains(t.Key))); // or only those applying to the current branch

            ToggleMenuItems(localBranch.Visible && hasSingleSelection, _menuBranchCopyContextMenuItems);

            if (localBranch.Visible && hasSingleSelection)
            {
                contextMenu.AddUserScripts(runScriptToolStripMenuItem, _scriptRunner.Execute);
            }
            else
            {
                contextMenu.RemoveUserScripts(runScriptToolStripMenuItem);
            }
        }

        private void ToggleBranchPathContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu != menuBranchPath || contextMenu.GetSelectedNode() is not BranchPathNode)
            {
                return;
            }

            // only display items in single-selection context
            ToggleMenuItems(hasSingleSelection, mnubtnCreateBranch, mnubtnDeleteAllBranches);
        }

        private void ToggleRemoteBranchContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu != menuRemote || contextMenu.GetSelectedNode() is not RemoteBranchNode node)
            {
                return;
            }

            ToggleMenuItems(_remoteBranchMenuItems, _ => hasSingleSelection);

            // toggle remote branch menu items operating on a single branch
            ToggleMenuItems(hasSingleSelection, mnubtnFetchOneBranch, mnubtnPullFromRemoteBranch,
                mnubtnRemoteBranchFetchAndCheckout, mnubtnFetchCreateBranch, mnubtnFetchRebase, toolStripSeparator1);

            ToggleMenuItems(node.Visible && hasSingleSelection, _menuRemoteCopyContextMenuItems);
        }

        private void ToggleRemoteRepoContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu != menuRemoteRepoNode || contextMenu.GetSelectedNode() is not RemoteRepoNode node)
            {
                return;
            }

            ToggleMenuItems(hasSingleSelection, mnubtnManageRemotes, tsmiSpacer3);

            // Actions on enabled remotes
            ToggleMenuItems(hasSingleSelection && node.Enabled, mnubtnFetchAllBranchesFromARemote, mnubtnDisableRemote, mnuBtnPruneAllBranchesFromARemote);

            ToggleMenuItems(hasSingleSelection && node.IsRemoteUrlUsingHttp, mnuBtnOpenRemoteUrlInBrowser);

            // Actions on disabled remotes
            ToggleMenuItems(hasSingleSelection && !node.Enabled, mnubtnEnableRemote, mnubtnEnableRemoteAndFetch);
        }

        private void ToggleRemotesTreeContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu != menuRemotes || contextMenu.GetSelectedNode() is not Tree)
            {
                return;
            }

            ToggleMenuItems(hasSingleSelection, mnuBtnManageRemotesFromRootNode, mnuBtnFetchAllRemotes, mnuBtnPruneAllRemotes);
        }

        private void ToggleTagContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu != menuTag || contextMenu.GetSelectedNode() is not TagNode node)
            {
                return;
            }

            ToggleMenuItems(_tagNodeMenuItems, _ => hasSingleSelection);
        }

        private void ToggleSortContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
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

            if (!contextMenu.Items.Contains(_sortOrderContextMenuItem))
            {
                contextMenu.Items.AddRange(new ToolStripItem[] { _tsmiSortMenuSpacer, _sortByContextMenuItem, _sortOrderContextMenuItem });
            }

            // sorting doesn't make a lot of sense in a multi-selection context
            ToggleMenuItems(hasSingleSelection, _tsmiSortMenuSpacer, _sortByContextMenuItem);

            // If refs are sorted by git (GitRefsSortBy = Default) don't show sort order options
            var showSortOrder = AppSettings.RefsSortBy != GitUIPluginInterfaces.GitRefsSortBy.Default;
            ToggleMenuItems(hasSingleSelection && showSortOrder, _sortOrderContextMenuItem);
        }

        private void ToggleSubmoduleContextMenu(ContextMenuStrip contextMenu, bool hasSingleSelection)
        {
            if (contextMenu != menuSubmodule || contextMenu.GetSelectedNode() is not SubmoduleNode submoduleNode)
            {
                return;
            }

            var bareRepository = Module.IsBareRepository();

            ToggleMenuItems(hasSingleSelection && submoduleNode.CanOpen, mnubtnOpenSubmodule, mnubtnOpenGESubmodule);
            ToggleMenuItems(hasSingleSelection, mnubtnUpdateSubmodule);
            ToggleMenuItems(hasSingleSelection && !bareRepository && submoduleNode.IsCurrent, mnubtnManageSubmodules, mnubtnSynchronizeSubmodules);
            ToggleMenuItems(hasSingleSelection && !bareRepository, mnubtnResetSubmodule, mnubtnStashSubmodule, mnubtnCommitSubmodule);
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
            menuBranch.Items.Insert(0, CreateFilterSelectedRefsContextMenuItem());
            _menuBranchCopyContextMenuItems = CreateCopyContextMenuItems();
            menuBranch.InsertItems(_menuBranchCopyContextMenuItems);
            _localBranchMenuItems = new LocalBranchMenuItems<LocalBranchNode>(this);
            menuBranch.InsertItems(_localBranchMenuItems.Select(s => s.Item), after: _menuBranchCopyContextMenuItems.Last());
            Node.RegisterContextMenu(typeof(LocalBranchNode), menuBranch);
            #endregion

            #region BranchPathNode (folder)
            Node.RegisterContextMenu(typeof(BranchPathNode), menuBranchPath);
            RegisterClick<BranchPathNode>(mnubtnDeleteAllBranches, branchPath => branchPath.DeleteAll());
            RegisterClick<BranchPathNode>(mnubtnCreateBranch, branchPath => branchPath.CreateBranch());
            #endregion

            #region RemoteBranchNode
            menuRemote.Items.Add(CreateFilterSelectedRefsContextMenuItem());
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
            menuTag.Items.Add(CreateFilterSelectedRefsContextMenuItem());
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
        private readonly TranslationString _filterForSelected = new("&Filter for selected");

        private readonly TranslationString _filterForSelectedToolTip = new(
            "Filter the revision grid to show selected (underlined) refs (branches and tags) only." +
            "\nHold Ctrl while clicking to de/select multiple and include descendant tree nodes by additionally holding Alt." +
            "\nTo reset the filter, right click the revision grid, select 'View' and then 'Show all branches'.");

        private ToolStripMenuItem CreateFilterSelectedRefsContextMenuItem()
        {
            ToolStripMenuItem menuItem = new(_filterForSelected.Text, Properties.Images.ShowThisBranchOnly) { ToolTipText = _filterForSelectedToolTip.Text };
            RegisterClick(menuItem, () => _filterRevisionGridBySpaceSeparatedRefs(GetMultiSelection().ReduceToRefs().Select(b => b.FullPath).Join(" ")));
            return menuItem;
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

            ToggleLocalBranchContextMenu(contextMenu, hasSingleSelection);
            ToggleBranchPathContextMenu(contextMenu, hasSingleSelection);
            ToggleRemoteBranchContextMenu(contextMenu, hasSingleSelection);
            ToggleRemoteRepoContextMenu(contextMenu, hasSingleSelection);
            ToggleRemotesTreeContextMenu(contextMenu, hasSingleSelection);
            ToggleTagContextMenu(contextMenu, hasSingleSelection);
            ToggleSubmoduleContextMenu(contextMenu, hasSingleSelection);
            ToggleSortContextMenu(contextMenu, hasSingleSelection);
            ToggleExpandCollapseContextMenu(contextMenu, selectedNodes);
            ToggleMoveTreeUpDownContexMenu(contextMenu, hasSingleSelection);

            /* Cancel context menu if no items are enabled.
             * This relies on the items' Enabled flag being toggled (e.g. by ToggleMenuItems) and BEFORE this line. */
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

    internal static class ContextMenuExtensions
    {
        internal static RepoObjectsTree.NodeBase GetSelectedNode(this ContextMenuStrip menu)
            => (menu.SourceControl as TreeView)?.SelectedNode?.Tag as RepoObjectsTree.NodeBase;

        /// <summary>Inserts <paramref name="items"/> into the <paramref name="menu"/>; optionally <paramref name="before"/> or
        /// <paramref name="after"/> an existing item or at the start of the menu before other existing items if neither is specified.</summary>
        internal static void InsertItems(this ContextMenuStrip menu, IEnumerable<ToolStripItem> items, ToolStripItem? before = null, ToolStripItem? after = null)
        {
            Debug.Assert(!(after is not null && before is not null), $"Only {nameof(before)} or {nameof(after)} is allowed.");

            menu.SuspendLayout();

            int index;
            if (before is not null)
            {
                index = Math.Max(0, menu.Items.IndexOf(before) - 1);
                items.ForEach(item => menu.Items.Insert(++index, item));
            }
            else
            {
                index = after is null ? 0 : Math.Max(0, menu.Items.IndexOf(after) + 1);
                items.ForEach(item => menu.Items.Insert(index++, item));
            }

            menu.ResumeLayout();
        }

        /// <summary>Adds the <paramref name="item"/> to the <paramref name="menu"/> if that doesn't contain it already.</summary>
        internal static void AddOnce(this ContextMenuStrip menu, ToolStripItem item)
        {
            if (!menu.Items.Contains(item))
            {
                menu.Items.Add(item);
            }
        }

        /// <summary>Toggles the <paramref name="item"/>'s <see cref="ToolStripItem.Visible"/>
        /// as well as <see cref="ToolStripItem.Enabled"/> properties depending on <paramref name="enabled"/>.
        /// Prefer this over only toggling the visibility of an item to enable determining whether the context menu will (once open)
        /// contain any visible items via <see cref="ToolStripItem.Enabled"/> even before the menu itself (as the visual parent)
        /// is visble and <see cref="ToolStripItem.Visible"/> of any item therefore false.</summary>
        internal static void Toggle(this ToolStripItem item, bool enabled)
            => item.Visible = item.Enabled = enabled;
    }
}
