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

        /// <summary>Toggles the <paramref name="items"/> <see cref="ToolStripItem.Visible"/>
        /// as well as <see cref="ToolStripItem.Enabled"/> properties depending on <paramref name="enabled"/>.
        /// Prefer this over only toggling the visibility of an item to enable determining whether the context menu will (once open)
        /// contain any visible items via <see cref="ToolStripItem.Enabled"/> in <see cref="contextMenu_Opening(object, CancelEventArgs)"/>
        /// even before the menu itself (as the visual parent) is visble and <see cref="ToolStripItem.Visible"/> of any item therefore false.</summary>
        private void ToggleMenuItems(bool enabled, params ToolStripItem[] items) => items.ForEach(item => item.Visible = item.Enabled = enabled);

        private void ToggleMenuItems<TNode>(MenuItemsGenerator<TNode> generator, Func<ToolStripItemWithKey, bool> isEnabled) where TNode : class, INode
            => generator.ForEach(i => ToggleMenuItems(isEnabled(i), i.Item));

        private void ContextMenuAddExpandCollapseTree(ContextMenuStrip contextMenu)
        {
            var node = (contextMenu.SourceControl as TreeView)?.SelectedNode?.Tag;

            /* add Expand All / Collapse All menu entry
             * depending on whether node is expanded or collapsed and has child nodes at all */
            if (node is NodeBase nodeList && nodeList.HasChildren())
            {
                if (contextMenu == menuMain)
                {
                    contextMenu.Items.Clear();
                }
                else if (contextMenu.Items.Count > 0)
                {
                    Add(tsmiMainMenuSpacer1); // add a separator if any items exist already

                    // Display separator if any preceding items are enabled. This relies on menu items Enabled being toggled by ToggleMenuItems and before this method.
                    tsmiMainMenuSpacer1.Visible = contextMenu.Items.Cast<ToolStripItem>().TakeWhile(item => item != tsmiMainMenuSpacer1).Any(item => item.Enabled);
                }

                Add(mnubtnCollapse);
                Add(mnubtnExpand);

                var isExpanded = nodeList.TreeViewNode.IsExpanded;
                mnubtnExpand.Visible = !isExpanded;
                mnubtnCollapse.Visible = isExpanded;
            }

            // add Move Up / Move Down menu entries for re-arranging top level tree nodes
            if (node is Tree tree)
            {
                Add(tsmiMainMenuSpacer2); // add another separator
                Add(mnubtnMoveUp);
                Add(mnubtnMoveDown);

                var treeNode = tree.TreeViewNode;
                mnubtnMoveUp.Enabled = treeNode.PrevNode is not null;
                mnubtnMoveDown.Enabled = treeNode.NextNode is not null;
            }

            void Add(ToolStripItem item)
            {
                if (!contextMenu.Items.Contains(item))
                {
                    contextMenu.Items.Add(item);
                }
            }
        }

        private void ContextMenuBranchSpecific(ContextMenuStrip contextMenu, bool areMultipleBranchesSelected)
        {
            if (contextMenu != menuBranch || (contextMenu.SourceControl as TreeView)?.SelectedNode?.Tag is not LocalBranchNode localBranch)
            {
                return;
            }

            ToggleMenuItems(_localBranchMenuItems,
                t => !areMultipleBranchesSelected // hide items if multiple branches are selected
                 && (!localBranch.IsCurrent // otherwise display all items for non-current branches
                     || LocalBranchMenuItems<LocalBranchNode>.CurrentBranchItemKeys.Contains(t.Key))); // or only those applying to the current branch

            ToggleMenuItems(localBranch.Visible, _menuBranchCopyContextMenuItems);

            if (localBranch.Visible && !areMultipleBranchesSelected)
            {
                contextMenu.AddUserScripts(runScriptToolStripMenuItem, _scriptRunner.Execute);
            }
            else
            {
                contextMenu.RemoveUserScripts(runScriptToolStripMenuItem);
            }
        }

        private void ContextMenuBranchPathSpecific(ContextMenuStrip contextMenu, bool areMultipleBranchesSelected)
        {
            if (contextMenu != menuBranchPath || (contextMenu.SourceControl as TreeView)?.SelectedNode?.Tag is not BranchPathNode)
            {
                return;
            }

            // don't display items in multi-selection context
            ToggleMenuItems(!areMultipleBranchesSelected, mnubtnCreateBranch, mnubtnDeleteAllBranches);
        }

        private void ContextMenuRemoteSpecific(ContextMenuStrip contextMenu, bool areMultipleBranchesSelected)
        {
            if (contextMenu != menuRemote || (contextMenu.SourceControl as TreeView)?.SelectedNode?.Tag is not RemoteBranchNode node)
            {
                return;
            }

            ToggleMenuItems(_remoteBranchMenuItems, _ => !areMultipleBranchesSelected);

            // toggle remote branch menu items operating on a single branch
            ToggleMenuItems(!areMultipleBranchesSelected, mnubtnFetchOneBranch, mnubtnPullFromRemoteBranch,
                mnubtnRemoteBranchFetchAndCheckout, mnubtnFetchCreateBranch, mnubtnFetchRebase, toolStripSeparator1);

            ToggleMenuItems(node.Visible, _menuRemoteCopyContextMenuItems);
        }

        private void ContextMenuRemoteRepoSpecific(ContextMenuStrip contextMenu)
        {
            if (contextMenu != menuRemoteRepoNode || (contextMenu.SourceControl as TreeView)?.SelectedNode?.Tag is not RemoteRepoNode node)
            {
                return;
            }

            // Actions on enabled remotes
            ToggleMenuItems(node.Enabled, mnubtnFetchAllBranchesFromARemote, mnubtnDisableRemote, mnuBtnPruneAllBranchesFromARemote);

            ToggleMenuItems(node.IsRemoteUrlUsingHttp, mnuBtnOpenRemoteUrlInBrowser);

            // Actions on disabled remotes
            ToggleMenuItems(!node.Enabled, mnubtnEnableRemote, mnubtnEnableRemoteAndFetch);
        }

        private void ContextMenuSort(ContextMenuStrip contextMenu, bool areMultipleBranchesSelected)
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

            // sorting doesn't make a lot of sense if multiple branches are selected
            ToggleMenuItems(!areMultipleBranchesSelected, _tsmiSortMenuSpacer, _sortByContextMenuItem);

            // If refs are sorted by git (GitRefsSortBy = Default) don't show sort order options
            var showSortOrder = AppSettings.RefsSortBy != GitUIPluginInterfaces.GitRefsSortBy.Default;
            ToggleMenuItems(!areMultipleBranchesSelected && showSortOrder, _sortOrderContextMenuItem);
        }

        private void ContextMenuSubmoduleSpecific(ContextMenuStrip contextMenu)
        {
            TreeNode? selectedNode = (contextMenu.SourceControl as TreeView)?.SelectedNode;
            if (selectedNode is null)
            {
                return;
            }

            if (contextMenu == menuAllSubmodules)
            {
                if (!(selectedNode.Tag is SubmoduleTree submoduleTree))
                {
                    return;
                }
            }
            else if (contextMenu == menuSubmodule)
            {
                if (!(selectedNode.Tag is SubmoduleNode submoduleNode))
                {
                    return;
                }

                var bareRepository = Module.IsBareRepository();

                ToggleMenuItems(submoduleNode.CanOpen, mnubtnOpenSubmodule, mnubtnOpenGESubmodule);
                ToggleMenuItems(true, mnubtnUpdateSubmodule);
                ToggleMenuItems(!bareRepository && submoduleNode.IsCurrent, mnubtnManageSubmodules, mnubtnSynchronizeSubmodules);
                ToggleMenuItems(!bareRepository, mnubtnResetSubmodule, mnubtnStashSubmodule, mnubtnCommitSubmodule);
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
            _menuBranchCopyContextMenuItems = CreateCopyContextMenuItems();
            _menuRemoteCopyContextMenuItems = CreateCopyContextMenuItems();

            AddContextMenuItems(menuBranch, _menuBranchCopyContextMenuItems);
            AddContextMenuItems(menuRemote, _menuRemoteCopyContextMenuItems);

            _sortOrderContextMenuItem = new GitRefsSortOrderContextMenuItem(() => Refresh(new FilteredGitRefsProvider(UICommands.GitModule).GetRefs));
            _sortByContextMenuItem = new GitRefsSortByContextMenuItem(() => Refresh(new FilteredGitRefsProvider(UICommands.GitModule).GetRefs));

            _localBranchMenuItems = new LocalBranchMenuItems<LocalBranchNode>(this);
            AddContextMenuItems(menuBranch, _localBranchMenuItems.Select(s => s.Item), insertAfter: _menuBranchCopyContextMenuItems[1]);

            _remoteBranchMenuItems = new RemoteBranchMenuItems<RemoteBranchNode>(this);
            AddContextMenuItems(menuRemote, _remoteBranchMenuItems.Select(s => s.Item), insertAfter: toolStripSeparator1);

            _tagNodeMenuItems = new TagMenuItems<TagNode>(this);
            AddContextMenuItems(menuTag, _tagNodeMenuItems.Select(s => s.Item));

            RegisterClick(mnubtnCollapse, () => treeMain.SelectedNode?.Collapse());
            RegisterClick(mnubtnExpand, () => treeMain.SelectedNode?.ExpandAll());
            RegisterClick(mnubtnMoveUp, () => ReorderTreeNode(treeMain.SelectedNode, up: true));
            RegisterClick(mnubtnMoveDown, () => ReorderTreeNode(treeMain.SelectedNode, up: false));

            RegisterClick(mnubtnFilterLocalBranchInRevisionGrid, FilterSelectedBranchesInRevisionGrid);
            Node.RegisterContextMenu(typeof(LocalBranchNode), menuBranch);

            RegisterClick<BranchPathNode>(mnubtnDeleteAllBranches, branchPath => branchPath.DeleteAll());
            Node.RegisterContextMenu(typeof(BranchPathNode), menuBranchPath);

            RegisterClick<BranchPathNode>(mnubtnCreateBranch, branchPath => branchPath.CreateBranch());

            RegisterClick<RemoteBranchNode>(mnubtnFetchOneBranch, remoteBranch => remoteBranch.Fetch());
            RegisterClick<RemoteBranchNode>(mnubtnPullFromRemoteBranch, remoteBranch => remoteBranch.FetchAndMerge());
            RegisterClick(mnubtnFilterRemoteBranchInRevisionGrid, FilterSelectedBranchesInRevisionGrid);
            RegisterClick<RemoteBranchNode>(mnubtnRemoteBranchFetchAndCheckout, remoteBranch => remoteBranch.FetchAndCheckout());
            RegisterClick<RemoteBranchNode>(mnubtnFetchCreateBranch, remoteBranch => remoteBranch.FetchAndCreateBranch());
            RegisterClick<RemoteBranchNode>(mnubtnFetchRebase, remoteBranch => remoteBranch.FetchAndRebase());
            Node.RegisterContextMenu(typeof(RemoteBranchNode), menuRemote);

            RegisterClick<RemoteRepoNode>(mnubtnManageRemotes, remoteBranch => remoteBranch.PopupManageRemotesForm());
            RegisterClick<RemoteRepoNode>(mnubtnFetchAllBranchesFromARemote, remote => remote.Fetch());
            RegisterClick<RemoteRepoNode>(mnuBtnPruneAllBranchesFromARemote, remote => remote.Prune());
            RegisterClick<RemoteRepoNode>(mnuBtnOpenRemoteUrlInBrowser, remote => remote.OpenRemoteUrlInBrowser());
            RegisterClick<RemoteRepoNode>(mnubtnEnableRemote, remote => remote.Enable(fetch: false));
            RegisterClick<RemoteRepoNode>(mnubtnEnableRemoteAndFetch, remote => remote.Enable(fetch: true));
            RegisterClick<RemoteRepoNode>(mnubtnDisableRemote, remote => remote.Disable());
            Node.RegisterContextMenu(typeof(RemoteRepoNode), menuRemoteRepoNode);

            Node.RegisterContextMenu(typeof(TagNode), menuTag);

            RegisterClick(mnuBtnManageRemotesFromRootNode, () => _remotesTree.PopupManageRemotesForm(remoteName: null));
            RegisterClick(mnuBtnFetchAllRemotes, () => _remotesTree.FetchAll());
            RegisterClick(mnuBtnPruneAllRemotes, () => _remotesTree.FetchPruneAll());

            RegisterClick<SubmoduleNode>(mnubtnManageSubmodules, _ => _submoduleTree.ManageSubmodules(this));
            RegisterClick<SubmoduleNode>(mnubtnSynchronizeSubmodules, _ => _submoduleTree.SynchronizeSubmodules(this));
            RegisterClick<SubmoduleNode>(mnubtnOpenSubmodule, node => _submoduleTree.OpenSubmodule(this, node));
            RegisterClick<SubmoduleNode>(mnubtnOpenGESubmodule, node => _submoduleTree.OpenSubmoduleInGitExtensions(this, node));
            RegisterClick<SubmoduleNode>(mnubtnUpdateSubmodule, node => _submoduleTree.UpdateSubmodule(this, node));
            RegisterClick<SubmoduleNode>(mnubtnResetSubmodule, node => _submoduleTree.ResetSubmodule(this, node));
            RegisterClick<SubmoduleNode>(mnubtnStashSubmodule, node => _submoduleTree.StashSubmodule(this, node));
            RegisterClick<SubmoduleNode>(mnubtnCommitSubmodule, node => _submoduleTree.CommitSubmodule(this, node));
            Node.RegisterContextMenu(typeof(SubmoduleNode), menuSubmodule);
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

        private void FilterSelectedBranchesInRevisionGrid() => _branchFilterAction(GetSelectedBranches().Select(b => b.FullPath).Join(" "));

        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (sender is not ContextMenuStrip contextMenu)
            {
                return;
            }

            var areMultipleBranchesSelected = GetSelectedBranches().Count() > 1;

            ContextMenuBranchSpecific(contextMenu, areMultipleBranchesSelected);
            ContextMenuBranchPathSpecific(contextMenu, areMultipleBranchesSelected);
            ContextMenuRemoteSpecific(contextMenu, areMultipleBranchesSelected);
            ContextMenuRemoteRepoSpecific(contextMenu);
            ContextMenuSubmoduleSpecific(contextMenu);
            ContextMenuSort(contextMenu, areMultipleBranchesSelected);
            ContextMenuAddExpandCollapseTree(contextMenu);

            // Set Cancel to false.  It is optimized to true based on empty entry.
            // See https://docs.microsoft.com/en-us/dotnet/framework/winforms/controls/how-to-handle-the-contextmenustrip-opening-event
            e.Cancel = false;
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

        private void AddContextMenuItems(ContextMenuStrip menu, IEnumerable<ToolStripItem> items, ToolStripItem? insertBefore = null, ToolStripItem? insertAfter = null)
        {
            Debug.Assert(!(insertAfter is not null && insertBefore is not null), $"Only {nameof(insertBefore)} or {nameof(insertAfter)} is allowed.");

            menu.SuspendLayout();

            int index;
            if (insertBefore is not null)
            {
                index = Math.Max(0, menu.Items.IndexOf(insertBefore) - 1);
                items.ForEach(item => menu.Items.Insert(++index, item));
            }
            else
            {
                index = insertAfter is null ? 0 : Math.Max(0, menu.Items.IndexOf(insertAfter) + 1);
                items.ForEach(item => menu.Items.Insert(index++, item));
            }

            menu.ResumeLayout();
        }
    }
}
