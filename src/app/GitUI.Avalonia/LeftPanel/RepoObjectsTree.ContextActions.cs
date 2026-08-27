using Avalonia.Controls;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.LeftPanel.ContextMenu;
using GitUI.LeftPanel.Interfaces;
using GitUI.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

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
        Tree[] visibleTrees = [.. _trees.Where(tree => tree.IsEnabled).OrderBy(tree => tree.PositionIndex)];
        int index = selectedNode is Tree tree ? Array.IndexOf(visibleTrees, tree) : -1;
        SetAction(RepoAction.MoveUp, hasSingleSelection && index >= 0, index > 0);
        SetAction(RepoAction.MoveDown, hasSingleSelection && index >= 0, index < visibleTrees.Length - 1);
    }

    private static void RegisterClick(MenuItem item, Action onClick)
    {
        item.Click += (o, e) => onClick();
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
        menuMain.Items.Add(_actionSeparator);
        AddAction(RepoAction.Copy, nameof(RepoObjectsTree), "copyContextMenuItem", "&Copy to clipboard", Images.CopyToClipboard);
        AddAction(RepoAction.Filter, nameof(RepoObjectsTree), "filterForSelectedRefsMenuItem", "&Filter for selected", Images.ShowThisBranchOnly);

        // git refs (tag, local & remote branch) menu items (rename, delete, merge, etc)
        _tagNodeMenuItems = new TagMenuItems<TagNode>(this);
        _remoteBranchMenuItems = new RemoteBranchMenuItems<RemoteBranchNode>(this);
        _localBranchMenuItems = new LocalBranchMenuItems<LocalBranchNode>(this);
        menuMain.InsertItems(_tagNodeMenuItems.Select(s => s.Item).Prepend(new Separator()), after: _actionItems[RepoAction.Filter]);
        menuMain.InsertItems(_remoteBranchMenuItems.Select(s => s.Item).Prepend(new Separator()), after: _actionItems[RepoAction.Filter]);
        menuMain.InsertItems(_localBranchMenuItems.Select(s => s.Item).Prepend(new Separator()), after: _actionItems[RepoAction.Filter]);

        AddAction(RepoAction.FetchBranch, nameof(RepoObjectsTree), "mnubtnFetchOneBranch", "Fe&tch", Images.Stage);
        AddAction(RepoAction.FetchMerge, nameof(RepoObjectsTree), "mnubtnPullFromRemoteBranch", "Fetch && Merge (&Pull)", Images.Pull);
        AddAction(RepoAction.FetchCheckout, nameof(RepoObjectsTree), "mnubtnRemoteBranchFetchAndCheckout", "&Fetch && Checkout", Images.BranchCheckout);
        AddAction(RepoAction.FetchRebase, nameof(RepoObjectsTree), "mnubtnFetchRebase", "Fetch && Re&base", Images.Rebase);
        AddAction(RepoAction.FetchCreate, nameof(RepoObjectsTree), "mnubtnFetchCreateBranch", "Fetc&h && Create Branch", Images.Branch.AdaptLightness());
        AddAction(RepoAction.CreateInFolder, nameof(RepoObjectsTree), "mnubtnCreateBranch", "Create Branch...", Images.BranchCreate);
        AddAction(RepoAction.DeleteFolderBranches, nameof(RepoObjectsTree), "mnubtnDeleteAllBranches", "Delete All", Images.BranchDelete);
        AddAction(RepoAction.ManageRemotes, nameof(RepoObjectsTree), "mnuBtnManageRemotesFromRootNode", "&Manage...", Images.Remotes);
        AddAction(RepoAction.FetchAllRemotes, nameof(RepoObjectsTree), "mnuBtnFetchAllRemotes", "Fetch all remotes", Images.PullFetchAll);
        AddAction(RepoAction.PruneAllRemotes, nameof(RepoObjectsTree), "mnuBtnPruneAllRemotes", "Fetch and prune all remotes", Images.PullFetchPruneAll);
        AddAction(RepoAction.ManageRemote, nameof(RepoObjectsTree), "mnubtnManageRemotes", "&Manage...", Images.Remotes);
        AddAction(RepoAction.EnableRemote, nameof(RepoObjectsTree), "mnubtnEnableRemote", "&Activate", Images.EyeOpened.AdaptLightness());
        AddAction(RepoAction.EnableRemoteAndFetch, nameof(RepoObjectsTree), "mnubtnEnableRemoteAndFetch", "A&ctivate and fetch", Images.RemoteEnableAndFetch.AdaptLightness());
        AddAction(RepoAction.DisableRemote, nameof(RepoObjectsTree), "mnubtnDisableRemote", "&Deactivate", Images.EyeClosed.AdaptLightness());
        AddAction(RepoAction.FetchRemote, nameof(RepoObjectsTree), "mnubtnFetchAllBranchesFromARemote", "&Fetch", Images.PullFetch);
        AddAction(RepoAction.PruneRemote, nameof(RepoObjectsTree), "mnuBtnPruneAllBranchesFromARemote", "Fetch and &prune", Images.PullFetchPrune);
        AddAction(RepoAction.OpenRemoteUrl, nameof(RepoObjectsTree), "mnuBtnOpenRemoteUrlInBrowser", "Open remote Url", Images.Globe);
        AddAction(RepoAction.OpenSubmodule, nameof(RepoObjectsTree), "mnubtnOpenSubmodule", "&Open", Images.FolderOpen);
        AddAction(RepoAction.OpenSubmoduleInGitExtensions, nameof(RepoObjectsTree), "mnubtnOpenGESubmodule", "O&pen", Images.GitExtensionsLogo16);
        AddAction(RepoAction.ManageSubmodules, nameof(RepoObjectsTree), "mnubtnManageSubmodules", "&Manage...", Images.SubmodulesManage);
        AddAction(RepoAction.UpdateSubmodule, nameof(RepoObjectsTree), "mnubtnUpdateSubmodule", "&Update", Images.SubmodulesUpdate);
        AddAction(RepoAction.SynchronizeSubmodules, nameof(RepoObjectsTree), "mnubtnSynchronizeSubmodules", "Synchronize", Images.SubmodulesSync);
        AddAction(RepoAction.ResetSubmodule, nameof(RepoObjectsTree), "mnubtnResetSubmodule", "&Reset", Images.ResetWorkingDirChanges);
        AddAction(RepoAction.StashSubmodule, nameof(RepoObjectsTree), "mnubtnStashSubmodule", "&Stash", Images.Stash);
        AddAction(RepoAction.CommitSubmodule, nameof(RepoObjectsTree), "mnubtnCommitSubmodule", "&Commit", Images.RepoStateDirtySubmodules);
        AddAction(RepoAction.Collapse, nameof(RepoObjectsTree), "mnubtnCollapse", "Collapse", Images.CollapseAll.AdaptLightness());
        AddAction(RepoAction.Expand, nameof(RepoObjectsTree), "mnubtnExpand", "Expand", Images.ExpandAll.AdaptLightness());
        AddAction(RepoAction.MoveUp, nameof(RepoObjectsTree), "mnubtnMoveUp", "Move Up", Images.ArrowUp);
        AddAction(RepoAction.MoveDown, nameof(RepoObjectsTree), "mnubtnMoveDown", "Move Down", Images.ArrowDown);

        // Sort by / order
        _sortByContextMenuItem = new GitRefsSortByContextMenuItem(() => ResortRefs(new FilteredGitRefsProvider(UICommands.Module).GetRefs));
        _sortOrderContextMenuItem = new GitRefsSortOrderContextMenuItem(() => ResortRefs(new FilteredGitRefsProvider(UICommands.Module).GetRefs));
        menuMain.InsertItems(new Control[] { new Separator(), _sortByContextMenuItem, _sortOrderContextMenuItem }, after: _actionItems[RepoAction.MoveDown]);
    }

    private void contextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
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

        bool stashTreeSelected = SelectedNode is StashTree;
        bool stashSelected = SelectedStashNode is not null;
        bool worktreeTreeSelected = SelectedNode is WorktreeTree;
        bool worktreeSelected = SelectedWorktreeNode is not null;
        bool canRunCommands = TryGetUICommandsDirect(out IGitUICommands? commands);
        bool canChangeWorkingTree = canRunCommands && !commands!.Module.IsBareRepository();

        mnubtnStashAllFromRootNode.IsVisible = stashTreeSelected;
        mnubtnStashStagedFromRootNode.IsVisible = stashTreeSelected;
        mnubtnManageStashFromRootNode.IsVisible = stashTreeSelected;
        mnubtnOpenStash.IsVisible = stashSelected;
        mnubtnApplyStash.IsVisible = stashSelected;
        mnubtnPopStash.IsVisible = stashSelected;
        mnubtnDropStash.IsVisible = stashSelected;

        mnubtnStashAllFromRootNode.IsEnabled = canRunCommands;
        mnubtnStashStagedFromRootNode.IsEnabled = canRunCommands;
        mnubtnManageStashFromRootNode.IsEnabled = canRunCommands;
        mnubtnOpenStash.IsEnabled = canChangeWorkingTree;
        mnubtnApplyStash.IsEnabled = canChangeWorkingTree;
        mnubtnPopStash.IsEnabled = canChangeWorkingTree;
        mnubtnDropStash.IsEnabled = canChangeWorkingTree;

        mnubtnCreateWorktreeFromRootNode.IsVisible = worktreeTreeSelected;
        mnubtnPruneWorktreesFromRootNode.IsVisible = worktreeTreeSelected;
        mnubtnManageWorktreesFromRootNode.IsVisible = worktreeTreeSelected;
        mnubtnCreateWorktreeFromRootNode.IsEnabled = canChangeWorkingTree;
        mnubtnPruneWorktreesFromRootNode.IsEnabled = canRunCommands;
        mnubtnManageWorktreesFromRootNode.IsEnabled = canRunCommands;

        bool canActOnWorktree = worktreeSelected
            && SelectedWorktreeNode is { IsCurrent: false, Worktree.IsDeleted: false };
        mnubtnOpenWorktree.IsVisible = worktreeSelected;
        mnubtnDeleteWorktree.IsVisible = worktreeSelected;
        worktreePathSeparator.IsVisible = worktreeSelected;
        mnubtnCopyWorktreePath.IsVisible = worktreeSelected;
        mnubtnShowWorktreeInFolder.IsVisible = worktreeSelected;
        mnubtnOpenWorktree.IsEnabled = canActOnWorktree && canRunCommands;
        mnubtnDeleteWorktree.IsEnabled = canActOnWorktree && canRunCommands;
        mnubtnCopyWorktreePath.IsEnabled = worktreeSelected;
        mnubtnShowWorktreeInFolder.IsEnabled = worktreeSelected
            && Directory.Exists(SelectedWorktreeNode!.Worktree.Path);

        bool canCopy = SelectedNode is BaseBranchLeafNode or StashNode;
        bool canFilter = GetSelectedNodes().OfType<IGitRefActions>().Any()
            && _filterRevisionGridBySpaceSeparatedRefs is not null;
        SetAction(RepoAction.Copy, canCopy, canCopy);
        SetAction(RepoAction.Filter, canFilter, canFilter);

        LocalBranchNode? selectedLocalBranch = SelectedNode as LocalBranchNode;

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

        EnableMenuItems(_remoteBranchMenuItems, _ => hasSingleSelection && SelectedNode is RemoteBranchNode);
        EnableMenuItems(_tagNodeMenuItems, _ => hasSingleSelection && SelectedNode is TagNode);

        bool isSingleRefSelected = hasSingleSelection && SelectedNode is IGitRefActions;
        _sortByContextMenuItem.Enable(isSingleRefSelected);

        // If refs are sorted by git (GitRefsSortBy = Default) don't show sort order options
        bool showSortOrder = AppSettings.RefsSortBy != GitRefsSortBy.Default;
        _sortOrderContextMenuItem.Enable(isSingleRefSelected && showSortOrder);

        switch (SelectedNode)
        {
            case RemoteBranchNode:
                SetActionVisible(RepoAction.FetchBranch, canRunCommands);
                SetActionVisible(RepoAction.FetchMerge, canChangeWorkingTree);
                SetActionVisible(RepoAction.FetchCheckout, canChangeWorkingTree);
                SetActionVisible(RepoAction.FetchRebase, canChangeWorkingTree);
                SetActionVisible(RepoAction.FetchCreate, canChangeWorkingTree);
                break;
            case BranchPathNode:
                SetActionVisible(RepoAction.CreateInFolder, canChangeWorkingTree);
                SetActionVisible(RepoAction.DeleteFolderBranches, canChangeWorkingTree);
                break;
            case RemoteBranchTree:
                SetActionVisible(RepoAction.ManageRemotes, canRunCommands);
                SetActionVisible(RepoAction.FetchAllRemotes, canRunCommands);
                SetActionVisible(RepoAction.PruneAllRemotes, canRunCommands);
                break;
            case RemoteRepoNode remoteRepo:
                SetActionVisible(RepoAction.ManageRemote, canRunCommands);
                if (remoteRepo.Enabled)
                {
                    SetActionVisible(RepoAction.FetchRemote, canRunCommands);
                    SetActionVisible(RepoAction.PruneRemote, canRunCommands);
                    SetActionVisible(RepoAction.DisableRemote, canRunCommands && remoteRepo.CanToggle);
                }
                else
                {
                    SetActionVisible(RepoAction.EnableRemote, canRunCommands && remoteRepo.CanToggle);
                    SetActionVisible(RepoAction.EnableRemoteAndFetch, canRunCommands && remoteRepo.CanToggle);
                }

                if (remoteRepo.IsRemoteUrlUsingHttp)
                {
                    SetActionVisible(RepoAction.OpenRemoteUrl, enabled: true);
                }

                break;
            case SubmoduleNode submodule:
                bool singleSubmodule = GetSelectedNodes().Take(2).Count() == 1;
                SetAction(RepoAction.OpenSubmodule, singleSubmodule && !submodule.IsCurrent, enabled: true);
                SetAction(RepoAction.OpenSubmoduleInGitExtensions, singleSubmodule, enabled: true);
                SetAction(RepoAction.UpdateSubmodule, singleSubmodule, canRunCommands);
                SetAction(RepoAction.ManageSubmodules, singleSubmodule && submodule.IsCurrent, canChangeWorkingTree);
                SetAction(RepoAction.SynchronizeSubmodules, singleSubmodule && submodule.IsCurrent, canChangeWorkingTree);
                SetAction(RepoAction.ResetSubmodule, singleSubmodule, canChangeWorkingTree);
                SetAction(RepoAction.StashSubmodule, singleSubmodule, canChangeWorkingTree);
                SetAction(RepoAction.CommitSubmodule, singleSubmodule, canChangeWorkingTree);
                break;
        }

        if (SelectedNode?.TreeViewNode.Items.Count > 0)
        {
            SetActionVisible(RepoAction.Collapse, SelectedNode.TreeViewNode.IsExpanded);
            SetActionVisible(RepoAction.Expand, !SelectedNode.TreeViewNode.IsExpanded);
        }

        if (SelectedNode is Tree selectedTree)
        {
            Tree[] visibleTrees = [.. _trees.Where(tree => tree.IsEnabled).OrderBy(tree => tree.PositionIndex)];
            int index = Array.IndexOf(visibleTrees, selectedTree);
            SetActionVisible(RepoAction.MoveUp, index > 0);
            SetActionVisible(RepoAction.MoveDown, index >= 0 && index < visibleTrees.Length - 1);
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
