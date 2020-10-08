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
using ResourceManager;

namespace GitUI.BranchTreePanel
{
    partial class RepoObjectsTree : IMenuItemFactory
    {
        private GitRefsSortOrderContextMenuItem _sortOrderContextMenuItem;
        private GitRefsSortByContextMenuItem _sortByContextMenuItem;
        private ToolStripSeparator _tsmiSortMenuSpacer = new ToolStripSeparator { Name = "tsmiSortMenuSpacer" };

        /// <summary>
        /// Local branch context menu [git ref / rename / delete] actions
        /// </summary>
        private LocalBranchMenuItems<LocalBranchNode> _localBranchMenuItems;

        /// <summary>
        /// Remote branch context menu [git ref / rename / delete] actions
        /// </summary>
        private MenuItemsGenerator<RemoteBranchNode> _remoteBranchMenuItems;

        /// <summary>
        /// Tags context menu [git ref] actions
        /// </summary>
        private MenuItemsGenerator<TagNode> _tagNodeMenuItems;

        private void ContextMenuAddExpandCollapseTree(ContextMenuStrip contextMenu)
        {
            // Add the following to the every participating context menu:
            //
            //    ---------
            //    Collapse All
            //    Expand All

            Tree treeNode = (contextMenu.SourceControl as TreeView)?.SelectedNode?.Tag as Tree;

            if (contextMenu == menuMain)
            {
                contextMenu.Items.Clear();
                contextMenu.Items.Add(mnubtnCollapseAll);
                contextMenu.Items.Add(mnubtnExpandAll);
                if (treeNode != null)
                {
                    AddMoveUpDownMenuItems();
                }

                return;
            }

            if (!contextMenu.Items.Contains(tsmiMainMenuSpacer1))
            {
                contextMenu.Items.Add(tsmiMainMenuSpacer1);
            }

            if (!contextMenu.Items.Contains(mnubtnCollapseAll))
            {
                contextMenu.Items.Add(mnubtnCollapseAll);
            }

            if (!contextMenu.Items.Contains(mnubtnExpandAll))
            {
                contextMenu.Items.Add(mnubtnExpandAll);
            }

            if (treeNode != null)
            {
                AddMoveUpDownMenuItems();
            }

            return;

            void AddMoveUpDownMenuItems()
            {
                if (!contextMenu.Items.Contains(tsmiMainMenuSpacer2))
                {
                    contextMenu.Items.Add(tsmiMainMenuSpacer2);
                }

                if (!contextMenu.Items.Contains(mnubtnMoveUp))
                {
                    contextMenu.Items.Add(mnubtnMoveUp);
                }

                if (!contextMenu.Items.Contains(mnubtnMoveDown))
                {
                    contextMenu.Items.Add(mnubtnMoveDown);
                }

                mnubtnMoveUp.Enabled = treeNode.TreeViewNode.PrevNode != null;
                mnubtnMoveDown.Enabled = treeNode.TreeViewNode.NextNode != null;
            }
        }

        private void ContextMenuBranchSpecific(ContextMenuStrip contextMenu)
        {
            if (contextMenu != menuBranch)
            {
                return;
            }

            var node = (contextMenu.SourceControl as TreeView)?.SelectedNode;
            if (node is null)
            {
                return;
            }

            var isNotActiveBranch = !((node.Tag as LocalBranchNode)?.IsActive ?? false);
            _localBranchMenuItems.GetInactiveBranchItems().ForEach(t => t.Item.Visible = isNotActiveBranch);
        }

        private void ContextMenuRemoteRepoSpecific(ContextMenuStrip contextMenu)
        {
            if (contextMenu != menuRemoteRepoNode)
            {
                return;
            }

            var node = (contextMenu.SourceControl as TreeView)?.SelectedNode?.Tag as RemoteRepoNode;
            if (node is null)
            {
                return;
            }

            // Actions on enabled remotes
            mnubtnFetchAllBranchesFromARemote.Visible = node.Enabled;
            mnubtnDisableRemote.Visible = node.Enabled;
            mnuBtnPruneAllBranchesFromARemote.Visible = node.Enabled;
            mnuBtnOpenRemoteUrlInBrowser.Visible = node.IsRemoteUrlUsingHttp;

            // Actions on disabled remotes
            mnubtnEnableRemote.Visible = !node.Enabled;
            mnubtnEnableRemoteAndFetch.Visible = !node.Enabled;
        }

        private void ContextMenuSort(ContextMenuStrip contextMenu)
        {
            // We can only sort refs, i.e. branches and tags
            if (contextMenu != menuBranch &&
                contextMenu != menuRemote &&
                contextMenu != menuTag)
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
                AddContextMenuItems(contextMenu,
                    new ToolStripItem[]
                    {
                        _tsmiSortMenuSpacer,
                        _sortByContextMenuItem,
                        _sortOrderContextMenuItem,
                    },
                    insertBefore: tsmiMainMenuSpacer1);
            }

            // If refs are sorted by git (GitRefsSortBy = Default) don't show sort order options
            contextMenu.Items[GitRefsSortOrderContextMenuItem.MenuItemName].Visible =
                AppSettings.RefsSortBy != GitUIPluginInterfaces.GitRefsSortBy.Default;
        }

        private void ContextMenuSubmoduleSpecific(ContextMenuStrip contextMenu)
        {
            TreeNode selectedNode = (contextMenu.SourceControl as TreeView)?.SelectedNode;
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

                bool bareRepository = Module.IsBareRepository();
                mnubtnOpenSubmodule.Visible = submoduleNode.CanOpen;
                mnubtnUpdateSubmodule.Visible = true;
                mnubtnManageSubmodules.Visible = !bareRepository && submoduleNode.IsCurrent;
                mnubtnSynchronizeSubmodules.Visible = !bareRepository && submoduleNode.IsCurrent;
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
            _sortOrderContextMenuItem = new GitRefsSortOrderContextMenuItem(() =>
            {
                _branchesTree.Refresh();
                _remotesTree.Refresh();
                _tagTree.Refresh();
            });
            _sortByContextMenuItem = new GitRefsSortByContextMenuItem(() =>
            {
                _branchesTree.Refresh();
                _remotesTree.Refresh();
                _tagTree.Refresh();
            });

            _localBranchMenuItems = new LocalBranchMenuItems<LocalBranchNode>(this);
            AddContextMenuItems(menuBranch, _localBranchMenuItems.Select(s => s.Item));

            _remoteBranchMenuItems = new RemoteBranchMenuItems<RemoteBranchNode>(this);
            AddContextMenuItems(menuRemote, _remoteBranchMenuItems.Select(s => s.Item), insertAfter: toolStripSeparator1);

            _tagNodeMenuItems = new TagMenuItems<TagNode>(this);
            AddContextMenuItems(menuTag, _tagNodeMenuItems.Select(s => s.Item));

            RegisterClick(mnubtnCollapseAll, () => treeMain.CollapseAll());
            RegisterClick(mnubtnExpandAll, () => treeMain.ExpandAll());
            RegisterClick(mnubtnMoveUp, () => ReorderTreeNode(treeMain.SelectedNode, up: true));
            RegisterClick(mnubtnMoveDown, () => ReorderTreeNode(treeMain.SelectedNode, up: false));

            RegisterClick<LocalBranchNode>(mnubtnFilterLocalBranchInRevisionGrid, FilterInRevisionGrid);
            Node.RegisterContextMenu(typeof(LocalBranchNode), menuBranch);

            RegisterClick<BranchPathNode>(mnubtnDeleteAllBranches, branchPath => branchPath.DeleteAll());
            Node.RegisterContextMenu(typeof(BranchPathNode), menuBranchPath);

            RegisterClick<BranchPathNode>(mnubtnCreateBranch, branchPath => branchPath.CreateBranch());
            Node.RegisterContextMenu(typeof(BranchPathNode), menuBranchPath);

            RegisterClick<RemoteBranchNode>(mnubtnFetchOneBranch, remoteBranch => remoteBranch.Fetch());
            RegisterClick<RemoteBranchNode>(mnubtnPullFromRemoteBranch, remoteBranch => remoteBranch.FetchAndMerge());
            RegisterClick<RemoteBranchNode>(mnubtnFilterRemoteBranchInRevisionGrid, FilterInRevisionGrid);
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
            RegisterClick<SubmoduleNode>(mnubtnUpdateSubmodule, node => _submoduleTree.UpdateSubmodule(this, node));
            Node.RegisterContextMenu(typeof(SubmoduleNode), menuSubmodule);
        }

        private void FilterInRevisionGrid(BaseBranchNode branch)
        {
            _filterBranchHelper?.SetBranchFilter(branch.FullPath, refresh: true);
        }

        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            var contextMenu = sender as ContextMenuStrip;
            if (contextMenu is null)
            {
                return;
            }

            ContextMenuAddExpandCollapseTree(contextMenu);
            ContextMenuSort(contextMenu);
            ContextMenuBranchSpecific(contextMenu);
            ContextMenuRemoteRepoSpecific(contextMenu);
            ContextMenuSubmoduleSpecific(contextMenu);

            // Set Cancel to false.  It is optimized to true based on empty entry.
            // See https://docs.microsoft.com/en-us/dotnet/framework/winforms/controls/how-to-handle-the-contextmenustrip-opening-event
            e.Cancel = false;
        }

        /// <inheritdoc />
        public TMenuItem CreateMenuItem<TMenuItem, TNode>(Action<TNode> onClick, TranslationString text, TranslationString toolTip, Bitmap icon = null)
            where TMenuItem : ToolStripItem, new()
            where TNode : class, INode
        {
            var result = new TMenuItem();
            result.Image = icon;
            result.Text = text.Text;
            result.ToolTipText = toolTip.Text;
            RegisterClick(result, onClick);
            return result;
        }

        private void AddContextMenuItems(ContextMenuStrip menu, IEnumerable<ToolStripItem> items, ToolStripItem insertBefore = null, ToolStripItem insertAfter = null)
        {
            Debug.Assert(!(insertAfter != null && insertBefore != null), $"Only {nameof(insertBefore)} or {nameof(insertAfter)} is allowed.");

            menu.SuspendLayout();

            int index;
            if (insertBefore != null)
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
