using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.LeftPanel.ContextMenu;
using GitUI.LeftPanel.Interfaces;
using GitUI.Properties;
using GitUIPluginInterfaces;
using ResourceManager;
using ToolStripSeparator = GitUI.Compat.WinFormsControls.ToolStripSeparator;

namespace GitUI.LeftPanel;

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

    private static void EnableMenuItems(bool enabled, params MenuItem[] items)
    {
        foreach (MenuItem item in items)
        {
            item.IsVisible = enabled;
            item.IsEnabled = enabled;
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
        NodeBase[] multiSelectedParents = [.. selectedNodes.HavingChildren()];
        SetAction(RepoAction.Expand, multiSelectedParents.Length > 0, multiSelectedParents.Expandable().Any());
        SetAction(RepoAction.Collapse, multiSelectedParents.Length > 0, multiSelectedParents.Collapsible().Any());
    }

    private void EnableMoveTreeUpDownContexMenu(bool hasSingleSelection, NodeBase? selectedNode)
    {
        Tree[] visibleTrees = [.. _rootNodes.Where(tree => tree.IsEnabled).OrderBy(tree => tree.PositionIndex)];
        int index = selectedNode is Tree tree ? Array.IndexOf(visibleTrees, tree) : -1;
        SetAction(RepoAction.MoveUp, hasSingleSelection && index >= 0, index > 0);
        SetAction(RepoAction.MoveDown, hasSingleSelection && index >= 0, index < visibleTrees.Length - 1);
    }

    private void EnableRemoteBranchContextMenu(bool hasSingleSelection, NodeBase? selectedNode)
    {
        bool isSingleRemoteBranchSelected = hasSingleSelection && selectedNode is RemoteBranchNode;
        bool canRunCommands = TryGetUICommandsDirect(out IGitUICommands? commands);
        bool canChangeWorkingTree = canRunCommands && !commands!.Module.IsBareRepository();
        EnableMenuItems(_remoteBranchMenuItems, _ => isSingleRemoteBranchSelected);
        SetAction(RepoAction.FetchBranch, isSingleRemoteBranchSelected, canRunCommands);
        SetAction(RepoAction.FetchMerge, isSingleRemoteBranchSelected, canChangeWorkingTree);
        SetAction(RepoAction.FetchCheckout, isSingleRemoteBranchSelected, canChangeWorkingTree);
        SetAction(RepoAction.FetchCreate, isSingleRemoteBranchSelected, canChangeWorkingTree);
        SetAction(RepoAction.FetchRebase, isSingleRemoteBranchSelected, canChangeWorkingTree);
    }

    private void EnableRemoteRepoContextMenu(bool hasSingleSelection, NodeBase? selectedNode)
    {
        bool isSingleRemoteRepoSelected = hasSingleSelection && selectedNode is RemoteRepoNode;
        RemoteRepoNode? remoteRepo = selectedNode as RemoteRepoNode;
        bool canRunCommands = TryGetUICommandsDirect(out _);
        SetAction(RepoAction.ManageRemote, isSingleRemoteRepoSelected, canRunCommands);
        SetAction(RepoAction.FetchRemote, isSingleRemoteRepoSelected && remoteRepo?.Enabled is true, canRunCommands);
        SetAction(RepoAction.DisableRemote, isSingleRemoteRepoSelected && remoteRepo?.Enabled is true, canRunCommands && remoteRepo?.CanToggle is true);
        SetAction(RepoAction.PruneRemote, isSingleRemoteRepoSelected && remoteRepo?.Enabled is true, canRunCommands);
        SetAction(RepoAction.OpenRemoteUrl, isSingleRemoteRepoSelected && remoteRepo?.IsRemoteUrlUsingHttp is true, enabled: true);
        SetAction(RepoAction.EnableRemote, isSingleRemoteRepoSelected && remoteRepo?.Enabled is false, canRunCommands && remoteRepo?.CanToggle is true);
        SetAction(RepoAction.EnableRemoteAndFetch, isSingleRemoteRepoSelected && remoteRepo?.Enabled is false, canRunCommands && remoteRepo?.CanToggle is true);
    }

    private void EnableSortContextMenu(bool hasSingleSelection, NodeBase? selectedNode)
    {
        bool isSingleRefSelected = hasSingleSelection && selectedNode is IGitRefActions;
        _sortByContextMenuItem.Enable(isSingleRefSelected);

        // If refs are sorted by git (GitRefsSortBy = Default) don't show sort order options
        bool showSortOrder = AppSettings.RefsSortBy != GitRefsSortBy.Default;
        _sortOrderContextMenuItem.Enable(isSingleRefSelected && showSortOrder);
    }

    private void EnableWorktreeContextMenu(bool hasSingleSelection, NodeBase? selectedNode)
    {
        bool isSingleWorktreeSelected = hasSingleSelection && selectedNode is WorktreeNode;
        WorktreeNode? worktreeNode = selectedNode as WorktreeNode;
        bool canRunCommands = TryGetUICommandsDirect(out _);
        bool canActOnWorktree = isSingleWorktreeSelected && worktreeNode is { IsCurrent: false, Worktree.IsDeleted: false };
        bool worktreePathExists = isSingleWorktreeSelected && worktreeNode is not null && Directory.Exists(worktreeNode.Worktree.Path);

        // Always show menu items for any worktree node, but disable for current/deleted
        mnubtnOpenWorktree.IsVisible = isSingleWorktreeSelected;
        mnubtnOpenWorktree.IsEnabled = canActOnWorktree && canRunCommands;
        mnubtnDeleteWorktree.IsVisible = isSingleWorktreeSelected;
        mnubtnDeleteWorktree.IsEnabled = canActOnWorktree && canRunCommands;
        toolStripSeparator13.IsVisible = isSingleWorktreeSelected;
        mnubtnCopyWorktreePath.IsVisible = isSingleWorktreeSelected;
        mnubtnCopyWorktreePath.IsEnabled = isSingleWorktreeSelected;
        mnubtnShowWorktreeInFolder.IsVisible = isSingleWorktreeSelected;
        mnubtnShowWorktreeInFolder.IsEnabled = worktreePathExists;
    }

    private void EnableStashContextMenu(bool hasSingleSelection, NodeBase? selectedNode)
    {
        bool isSingleStashSelected = hasSingleSelection && selectedNode is StashNode;
        bool canChangeWorkingTree = TryGetUICommandsDirect(out IGitUICommands? commands) && !commands!.Module.IsBareRepository();
        EnableMenuItems(isSingleStashSelected && canChangeWorkingTree, mnubtnOpenStash, mnubtnApplyStash, mnubtnPopStash, mnubtnDropStash);
    }

    private void EnableSubmoduleContextMenu(bool hasSingleSelection, NodeBase? selectedNode)
    {
        bool isSingleSubmoduleSelected = hasSingleSelection && selectedNode is SubmoduleNode;
        SubmoduleNode? submoduleNode = selectedNode as SubmoduleNode;
        bool canRunCommands = TryGetUICommandsDirect(out IGitUICommands? commands);
        bool canChangeWorkingTree = canRunCommands && !commands!.Module.IsBareRepository();
        SetAction(RepoAction.OpenSubmodule, isSingleSubmoduleSelected && submoduleNode?.IsCurrent is false, enabled: true);
        SetAction(RepoAction.OpenSubmoduleInGitExtensions, isSingleSubmoduleSelected, enabled: true);
        SetAction(RepoAction.UpdateSubmodule, isSingleSubmoduleSelected, canRunCommands);
        SetAction(RepoAction.ManageSubmodules, isSingleSubmoduleSelected && submoduleNode?.IsCurrent is true, canChangeWorkingTree);
        SetAction(RepoAction.SynchronizeSubmodules, isSingleSubmoduleSelected && submoduleNode?.IsCurrent is true, canChangeWorkingTree);
        SetAction(RepoAction.ResetSubmodule, isSingleSubmoduleSelected, canChangeWorkingTree);
        SetAction(RepoAction.StashSubmodule, isSingleSubmoduleSelected, canChangeWorkingTree);
        SetAction(RepoAction.CommitSubmodule, isSingleSubmoduleSelected, canChangeWorkingTree);
    }

    private static void RegisterClick(MenuItem item, Action onClick)
    {
        item.Click += (o, e) => onClick();
    }

    private void RegisterAction(RepoAction action, MenuItem item)
    {
        item.IsVisible = false;
        item.Click += (_, _) => ExecuteAction(action);
        _actionItems.Add(action, item);
    }

    private void RegisterClick<T>(MenuItem item, Action<T> onClick) where T : class, INode
    {
        item.Click += (o, e) => Node.OnNode(treeMain.SelectedItem as TreeViewItem, onClick);
    }

    // parity-scaffolding: Lets headless tests locate generator-owned menu items by the legacy action names.
    private MenuItem GetActionMenuItem(string action)
    {
        if (Enum.TryParse(action, out RepoAction repoAction)
            && _actionItems.TryGetValue(repoAction, out MenuItem? menuItem))
        {
            return menuItem;
        }

        return action switch
        {
            "CheckoutLocal" => GetMenuItem(_localBranchMenuItems, MenuItemKey.GitRefCheckout),
            "CheckoutRemote" => GetMenuItem(_remoteBranchMenuItems, MenuItemKey.GitRefCheckout),
            "Merge" => GetMenuItem(_localBranchMenuItems, MenuItemKey.GitRefMerge),
            "RebaseLocal" => GetMenuItem(_localBranchMenuItems, MenuItemKey.GitRefRebase),
            "RebaseRemote" => GetMenuItem(_remoteBranchMenuItems, MenuItemKey.GitRefRebase),
            "RebaseTag" => GetMenuItem(_tagNodeMenuItems, MenuItemKey.GitRefRebase),
            "CreateBranch" => GetMenuItem(_localBranchMenuItems, MenuItemKey.GitRefCreateBranch),
            "Reset" => GetMenuItem(_localBranchMenuItems, MenuItemKey.GitRefReset),
            "RenameBranch" => GetMenuItem(_localBranchMenuItems, MenuItemKey.Rename),
            "DeleteBranch" => GetMenuItem(_localBranchMenuItems, MenuItemKey.Delete),
            "DeleteRemoteBranch" => GetMenuItem(_remoteBranchMenuItems, MenuItemKey.Delete),
            "CheckoutTag" => GetMenuItem(_tagNodeMenuItems, MenuItemKey.GitRefCheckout),
            "DeleteTag" => GetMenuItem(_tagNodeMenuItems, MenuItemKey.Delete),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };
    }

    private static MenuItem GetMenuItem<TNode>(MenuItemsGenerator<TNode> generator, MenuItemKey key) where TNode : class, INode
    {
        return generator.TryGetMenuItem(key, out Control? item) && item is MenuItem menuItem
            ? menuItem
            : throw new ArgumentOutOfRangeException(nameof(key), key, null);
    }

    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(_sortByContextMenuItem), nameof(_sortOrderContextMenuItem), nameof(_localBranchMenuItems), nameof(_remoteBranchMenuItems), nameof(_tagNodeMenuItems))]
    private void RegisterContextActions()
    {
        RegisterAction(RepoAction.Copy, copyContextMenuItem);
        RegisterAction(RepoAction.Filter, filterForSelectedRefsMenuItem);

        // git refs (tag, local & remote branch) menu items (rename, delete, merge, etc)
        _tagNodeMenuItems = new TagMenuItems<TagNode>(this);

        // RemoteBranchNode
        _remoteBranchMenuItems = new RemoteBranchMenuItems<RemoteBranchNode>(this);
        _localBranchMenuItems = new LocalBranchMenuItems<LocalBranchNode>(this);
        menuMain.InsertItems(_tagNodeMenuItems.Select(s => s.Item).Prepend(new Separator()), after: _actionItems[RepoAction.Filter]);
        menuMain.InsertItems(_remoteBranchMenuItems.Select(s => s.Item).Prepend(new Separator()), after: _actionItems[RepoAction.Filter]);
        menuMain.InsertItems(_localBranchMenuItems.Select(s => s.Item).Prepend(new Separator()), after: _actionItems[RepoAction.Filter]);

        RegisterAction(RepoAction.FetchBranch, mnubtnFetchOneBranch);
        RegisterAction(RepoAction.FetchMerge, mnubtnPullFromRemoteBranch);
        RegisterAction(RepoAction.FetchCheckout, mnubtnRemoteBranchFetchAndCheckout);
        RegisterAction(RepoAction.FetchRebase, mnubtnFetchRebase);
        RegisterAction(RepoAction.FetchCreate, mnubtnFetchCreateBranch);
        RegisterAction(RepoAction.CreateInFolder, mnubtnCreateBranch);
        RegisterAction(RepoAction.DeleteFolderBranches, mnubtnDeleteAllBranches);
        RegisterAction(RepoAction.ManageRemotes, mnuBtnManageRemotesFromRootNode);
        RegisterAction(RepoAction.FetchAllRemotes, mnuBtnFetchAllRemotes);
        RegisterAction(RepoAction.PruneAllRemotes, mnuBtnPruneAllRemotes);
        RegisterAction(RepoAction.ManageRemote, mnubtnManageRemotes);
        RegisterAction(RepoAction.EnableRemote, mnubtnEnableRemote);
        RegisterAction(RepoAction.EnableRemoteAndFetch, mnubtnEnableRemoteAndFetch);
        RegisterAction(RepoAction.DisableRemote, mnubtnDisableRemote);
        RegisterAction(RepoAction.FetchRemote, mnubtnFetchAllBranchesFromARemote);
        RegisterAction(RepoAction.PruneRemote, mnuBtnPruneAllBranchesFromARemote);
        RegisterAction(RepoAction.OpenRemoteUrl, mnuBtnOpenRemoteUrlInBrowser);
        RegisterAction(RepoAction.OpenSubmodule, mnubtnOpenSubmodule);
        RegisterAction(RepoAction.OpenSubmoduleInGitExtensions, mnubtnOpenGESubmodule);
        RegisterAction(RepoAction.ManageSubmodules, mnubtnManageSubmodules);
        RegisterAction(RepoAction.UpdateSubmodule, mnubtnUpdateSubmodule);
        RegisterAction(RepoAction.SynchronizeSubmodules, mnubtnSynchronizeSubmodules);
        RegisterAction(RepoAction.ResetSubmodule, mnubtnResetSubmodule);
        RegisterAction(RepoAction.StashSubmodule, mnubtnStashSubmodule);
        RegisterAction(RepoAction.CommitSubmodule, mnubtnCommitSubmodule);
        RegisterAction(RepoAction.Collapse, mnubtnCollapse);
        RegisterAction(RepoAction.Expand, mnubtnExpand);
        RegisterAction(RepoAction.MoveUp, mnubtnMoveUp);
        RegisterAction(RepoAction.MoveDown, mnubtnMoveDown);

        // Stash
        RegisterClick(mnubtnStashAllFromRootNode, () => _stashTree.StashAll(this));
        RegisterClick(mnubtnStashStagedFromRootNode, () => _stashTree.StashStaged(this));
        RegisterClick(mnubtnManageStashFromRootNode, () => _stashTree.OpenStash(this));
        RegisterClick<StashNode>(mnubtnOpenStash, node => node.OpenStash(this));
        RegisterClick<StashNode>(mnubtnApplyStash, node => node.ApplyStash(this));
        RegisterClick<StashNode>(mnubtnPopStash, node => node.PopStash(this));
        RegisterClick<StashNode>(mnubtnDropStash, node => node.DropStash(this));

        // Worktree
        RegisterClick(mnubtnCreateWorktreeFromRootNode, () => _worktreeTree.CreateWorktree(this));
        RegisterClick(mnubtnPruneWorktreesFromRootNode, () => _worktreeTree.PruneWorktrees(this));
        RegisterClick(mnubtnManageWorktreesFromRootNode, () => _worktreeTree.ManageWorktrees(this));
        RegisterClick(mnubtnOpenWorktree, () => ((WorktreeNode)SelectedNode!).OpenWorktree());
        RegisterClick(mnubtnDeleteWorktree, () => ((WorktreeNode)SelectedNode!).DeleteWorktree());
        RegisterClick(mnubtnCopyWorktreePath, () => ClipboardUtil.TrySetText(((WorktreeNode)SelectedNode!).Worktree.Path));
        RegisterClick(mnubtnShowWorktreeInFolder, () => OsShellUtil.OpenWithFileExplorer(((WorktreeNode)SelectedNode!).Worktree.Path));

        // Sort by / order
        _sortByContextMenuItem = new GitRefsSortByContextMenuItem(() => ResortRefs(new FilteredGitRefsProvider(UICommands.Module).GetRefs));
        _sortOrderContextMenuItem = new GitRefsSortOrderContextMenuItem(() => ResortRefs(new FilteredGitRefsProvider(UICommands.Module).GetRefs));
        menuMain.InsertItems(new Control[] { new ToolStripSeparator(), _sortByContextMenuItem, _sortOrderContextMenuItem }, after: mnubtnMoveDown);
    }

    private void contextMenu_Opening(object sender, CancelEventArgs e)
    {
        foreach (MenuItem item in _actionItems.Values)
        {
            item.IsVisible = false;
        }

        EnableMenuItems(_localBranchMenuItems, _ => false);
        EnableMenuItems(_remoteBranchMenuItems, _ => false);
        EnableMenuItems(_tagNodeMenuItems, _ => false);
        _sortByContextMenuItem.Enable(false);
        _sortOrderContextMenuItem.Enable(false);

        NodeBase[] selectedNodes = [.. GetSelectedNodes()];
        bool hasSingleSelection = selectedNodes.Length == 1;
        NodeBase? selectedNode = SelectedNode;
        bool canRunCommands = TryGetUICommandsDirect(out IGitUICommands? commands);
        bool canChangeWorkingTree = canRunCommands && !commands!.Module.IsBareRepository();

        bool canCopy = selectedNode is BaseBranchLeafNode or StashNode;
        bool canFilter = GetSelectedNodes().OfType<IGitRefActions>().Any()
            && _filterRevisionGridBySpaceSeparatedRefs is not null;
        SetAction(RepoAction.Copy, canCopy, canCopy);
        SetAction(RepoAction.Filter, canFilter, canFilter);

        LocalBranchNode? selectedLocalBranch = selectedNode as LocalBranchNode;

        foreach (ToolStripItemWithKey item in _localBranchMenuItems)
        {
            bool visible = hasSingleSelection && selectedLocalBranch is not null;
            item.Item.IsVisible = visible; // only display for single-selected branch

            /* Enabled items must also be visible; cancellation of menu opening below relies on it.
             * Avalonia exposes IsVisible before the popup is opened, so no visual-parent workaround is needed. */
            item.Item.IsEnabled = visible

                // enable all items for non-current branches or only those applying to the current branch
                && (selectedLocalBranch?.IsCurrent == false || LocalBranchMenuItems<LocalBranchNode>.CurrentBranchItemKeys.Contains(item.Key));
        }

        EnableRemoteBranchContextMenu(hasSingleSelection, selectedNode);
        EnableMenuItems(_tagNodeMenuItems, _ => hasSingleSelection && selectedNode is TagNode);
        SetAction(RepoAction.ManageRemotes, hasSingleSelection && selectedNode is RemoteBranchTree, canRunCommands);
        SetAction(RepoAction.FetchAllRemotes, hasSingleSelection && selectedNode is RemoteBranchTree, canRunCommands);
        SetAction(RepoAction.PruneAllRemotes, hasSingleSelection && selectedNode is RemoteBranchTree, canRunCommands);
        EnableRemoteRepoContextMenu(hasSingleSelection, selectedNode);
        EnableMenuItems(hasSingleSelection && selectedNode is StashTree && canRunCommands, mnubtnStashAllFromRootNode, mnubtnStashStagedFromRootNode, mnubtnManageStashFromRootNode);
        EnableStashContextMenu(hasSingleSelection, selectedNode);
        EnableSubmoduleContextMenu(hasSingleSelection, selectedNode);
        EnableWorktreeContextMenu(hasSingleSelection, selectedNode);
        EnableMenuItems(hasSingleSelection && selectedNode is WorktreeTree && canRunCommands, mnubtnCreateWorktreeFromRootNode, mnubtnPruneWorktreesFromRootNode, mnubtnManageWorktreesFromRootNode);
        SetAction(RepoAction.CreateInFolder, hasSingleSelection && selectedNode is BranchPathNode, canChangeWorkingTree);
        SetAction(RepoAction.DeleteFolderBranches, hasSingleSelection && selectedNode is BranchPathNode, canChangeWorkingTree);
        EnableExpandCollapseContextMenu(selectedNodes);
        EnableMoveTreeUpDownContexMenu(hasSingleSelection, selectedNode);
        EnableSortContextMenu(hasSingleSelection, selectedNode);

        if (hasSingleSelection && selectedLocalBranch is not null && canRunCommands)
        {
            menuMain.AddUserScripts(
                runScriptToolStripMenuItem,
                ExecuteCommand,
                script => script.AddToRevisionGridContextMenu,
                commands!);
        }
        else
        {
            menuMain.RemoveUserScripts(runScriptToolStripMenuItem);
        }

        menuMain.ToggleSeparators();

        /* Cancel context menu opening if no items are Enabled.
         * This relies on that flag being set correctly on all menu items above. */
        e.Cancel = !menuMain.Items.OfType<MenuItem>().Any(i => i.IsVisible && i.IsEnabled);
    }

    private void contextMenu_Opened(object? sender, EventArgs e)
    {
        // Avalonia exposes item visibility before opening, but re-run the original separator
        // normalization after the external popup surface has opened.
        // Waiting for the ContextMenuStrip (as the visual parent of its menu items) to be visible to
        // toggle (depending on ToolStripItem.Visible) existing separators in between item groups as required.
        menuMain.ToggleSeparators();
    }

    /// <inheritdoc />
    public TMenuItem CreateMenuItem<TMenuItem, TNode>(Action<TNode> onClick, TranslationString text, TranslationString toolTip, IImage? icon = null)
        where TMenuItem : MenuItem, new()
        where TNode : class, INode
    {
        TMenuItem result = new()
        {
            Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(text.Text),
            Icon = icon is null ? null : new Image { Width = 16, Height = 16, Source = icon, Stretch = Stretch.Uniform },
        };
        ToolTip.SetTip(result, toolTip.Text);
        RegisterClick(result, onClick);
        return result;
    }
}
