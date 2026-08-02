using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Remotes;
using GitCommands.Submodules;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.Compat;
using GitUI.Properties;
using GitUIPluginInterfaces;

using ResourceManager;

namespace GitUI.LeftPanel;

public partial class RepoObjectsTree : GitModuleControl
{
    private readonly Dictionary<RepoAction, MenuItem> _actionItems = [];
    private readonly Separator _actionSeparator = new();
    private readonly TranslationString _searchTooltip = new("Search");
    private readonly List<Tree> _trees = [];
    private Action<string?>? _filterRevisionGridBySpaceSeparatedRefs;
    private Action<string, ObjectId, ObjectId>? _openRepository;
    private List<TreeViewItem>? _searchResults;
    private StashTree? _stashTree;
    private readonly SubmoduleTree _submoduleTree;
    private readonly WorktreeTree _worktreeTree;
    private ISubmoduleStatusProvider? _submoduleStatusProvider;

    public RepoObjectsTree()
    {
        InitializeComponent();
        _submoduleTree = new SubmoduleTree(this);
        _worktreeTree = new WorktreeTree(this);
        CreateContextActions();

        treeMain.SelectionChanged += (_, _) => SelectionChanged?.Invoke(this, EventArgs.Empty);
        repoObjectsContextMenu.Opening += RepoObjectsContextMenuOpening;
        tsbCollapseAll.Click += (_, _) => CollapseAll();
        tsbShowBranches.Click += (_, _) => ToggleTree(RepoTreeKind.Branches, tsbShowBranches.IsChecked == true);
        tsbShowRemotes.Click += (_, _) => ToggleTree(RepoTreeKind.Remotes, tsbShowRemotes.IsChecked == true);
        tsbShowWorktrees.Click += (_, _) => ToggleTree(RepoTreeKind.Worktrees, tsbShowWorktrees.IsChecked == true);
        tsbShowTags.Click += (_, _) => ToggleTree(RepoTreeKind.Tags, tsbShowTags.IsChecked == true);
        tsbShowSubmodules.Click += (_, _) => ToggleTree(RepoTreeKind.Submodules, tsbShowSubmodules.IsChecked == true);
        tsbShowStashes.Click += (_, _) => ToggleTree(RepoTreeKind.Stashes, tsbShowStashes.IsChecked == true);
        btnSearch.Click += (_, _) => DoSearch();
        txtBranchCriterion.PropertyChanged += (_, e) =>
        {
            if (e.Property == TextBox.TextProperty)
            {
                ClearSearchResults();
            }
        };
        txtBranchCriterion.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                DoSearch();
                e.Handled = true;
            }
        };

        mnubtnStashAllFromRootNode.Click += (_, _) => _stashTree?.StashAll(this);
        mnubtnStashStagedFromRootNode.Click += (_, _) => _stashTree?.StashStaged(this);
        mnubtnManageStashFromRootNode.Click += (_, _) => _stashTree?.OpenStash(this);
        mnubtnOpenStash.Click += (_, _) => SelectedStashNode?.OpenStash(this);
        mnubtnApplyStash.Click += (_, _) => SelectedStashNode?.ApplyStash(this);
        mnubtnPopStash.Click += (_, _) => SelectedStashNode?.PopStash(this);
        mnubtnDropStash.Click += (_, _) => SelectedStashNode?.DropStash(this);
        mnubtnCreateWorktreeFromRootNode.Click += (_, _) => _worktreeTree.CreateWorktree(this);
        mnubtnPruneWorktreesFromRootNode.Click += (_, _) => _worktreeTree.PruneWorktrees(this);
        mnubtnManageWorktreesFromRootNode.Click += (_, _) => _worktreeTree.ManageWorktrees(this);
        mnubtnOpenWorktree.Click += (_, _) => SelectedWorktreeNode?.OpenWorktree();
        mnubtnDeleteWorktree.Click += (_, _) => SelectedWorktreeNode?.DeleteWorktree();
        mnubtnCopyWorktreePath.Click += (_, _) => ClipboardUtil.TrySetText(SelectedWorktreeNode?.Worktree.Path ?? string.Empty);
        mnubtnShowWorktreeInFolder.Click += (_, _) => OsShellUtil.OpenWithFileExplorer(SelectedWorktreeNode!.Worktree.Path);

        tsbShowBranches.IsChecked = AppSettings.RepoObjectsTreeShowBranches;
        tsbShowRemotes.IsChecked = AppSettings.RepoObjectsTreeShowRemotes;
        tsbShowWorktrees.IsChecked = AppSettings.RepoObjectsTreeShowWorktrees;
        tsbShowTags.IsChecked = AppSettings.RepoObjectsTreeShowTags;
        tsbShowSubmodules.IsChecked = AppSettings.RepoObjectsTreeShowSubmodules;
        tsbShowStashes.IsChecked = AppSettings.RepoObjectsTreeShowStashes;

        UICommandsSourceSet += (_, e) =>
        {
            _submoduleStatusProvider = e.GitUICommandsSource.UICommands.GetService(typeof(ISubmoduleStatusProvider)) as ISubmoduleStatusProvider;
            _submoduleTree.Attach(_submoduleStatusProvider);
        };
        AttachedToLogicalTree += (_, _) => _submoduleTree.Attach(_submoduleStatusProvider);
        DetachedFromLogicalTree += (_, _) => _submoduleTree.Detach();

        InitializeComplete();
        ToolTip.SetTip(btnSearch, _searchTooltip.Text);
        ToolTip.SetTip(tsbCollapseAll, Translate(nameof(RepoObjectsTree), "mnubtnCollapse", "Collapse all subnodes", "ToolTipText"));
        ToolTip.SetTip(tsbShowBranches, AvaloniaTranslationUtils.RemoveAvaloniaMnemonics((string)tsbShowBranches.Content!));
        ToolTip.SetTip(tsbShowRemotes, AvaloniaTranslationUtils.RemoveAvaloniaMnemonics((string)tsbShowRemotes.Content!));
        ToolTip.SetTip(tsbShowWorktrees, AvaloniaTranslationUtils.RemoveAvaloniaMnemonics((string)tsbShowWorktrees.Content!));
        ToolTip.SetTip(tsbShowTags, AvaloniaTranslationUtils.RemoveAvaloniaMnemonics((string)tsbShowTags.Content!));
        ToolTip.SetTip(tsbShowSubmodules, AvaloniaTranslationUtils.RemoveAvaloniaMnemonics((string)tsbShowSubmodules.Content!));
        ToolTip.SetTip(tsbShowStashes, AvaloniaTranslationUtils.RemoveAvaloniaMnemonics((string)tsbShowStashes.Content!));
    }

    /// <summary>Occurs when the selected node changes.</summary>
    public event EventHandler? SelectionChanged;

    /// <summary>Gets the ref of the selected node, or <see langword="null"/> for group nodes.</summary>
    public IGitRef? SelectedRef => (SelectedNode as BaseRevisionNode)?.GitRef;

    /// <summary>Gets the object id represented by the selected ref or stash node.</summary>
    public ObjectId? SelectedRevisionObjectId
        => SelectedNode switch
        {
            BaseRevisionNode { GitRef: not null } revisionNode => revisionNode.ObjectId,
            StashNode stashNode => stashNode.ObjectId,
            WorktreeNode worktreeNode => worktreeNode.ObjectId,
            _ => null,
        };

    private NodeBase? SelectedNode => (treeMain.SelectedItem as TreeViewItem)?.Tag as NodeBase;

    private StashNode? SelectedStashNode => SelectedNode as StashNode;

    private WorktreeNode? SelectedWorktreeNode => SelectedNode as WorktreeNode;

    /// <summary>Connects the tree's selected-ref filtering action to the revision grid.</summary>
    public void Initialize(
        Action<string?> filterRevisionGridBySpaceSeparatedRefs,
        Action<string, ObjectId, ObjectId>? openRepository = null)
    {
        _filterRevisionGridBySpaceSeparatedRefs = filterRevisionGridBySpaceSeparatedRefs;
        _openRepository = openRepository;
    }

    internal void OpenRepository(string path, ObjectId selectedId = default, ObjectId firstId = default)
        => _openRepository?.Invoke(path, selectedId, firstId);

    /// <summary>Fills the tree from the repository refs.</summary>
    public void SetRefs(IReadOnlyList<IGitRef> refs)
        => SetRefs(refs, [], includeStashes: false, currentBranch: string.Empty, enabledRemotes: null, disabledRemotes: null, remotesManager: null);

    /// <summary>Fills the tree from repository refs and stash revisions.</summary>
    public void SetRefs(IReadOnlyList<IGitRef> refs, IReadOnlyCollection<GitRevision> stashes)
        => SetRefs(refs, stashes, includeStashes: true, currentBranch: string.Empty, enabledRemotes: null, disabledRemotes: null, remotesManager: null);

    /// <summary>Fills the tree and marks the currently checked-out local branch.</summary>
    public void SetRefs(IReadOnlyList<IGitRef> refs, IReadOnlyCollection<GitRevision> stashes, string currentBranch)
        => SetRefs(refs, stashes, includeStashes: true, currentBranch, enabledRemotes: null, disabledRemotes: null, remotesManager: null);

    /// <summary>Fills the tree with configured enabled and inactive remotes, including remotes without branches.</summary>
    public void SetRefs(
        IReadOnlyList<IGitRef> refs,
        IReadOnlyCollection<GitRevision> stashes,
        string currentBranch,
        IReadOnlyList<Remote> enabledRemotes,
        IReadOnlyList<Remote> disabledRemotes,
        IConfigFileRemoteSettingsManager remotesManager,
        IReadOnlyList<GitWorktree>? worktrees = null,
        string currentWorkingDirectory = "")
        => SetRefs(refs, stashes, includeStashes: true, currentBranch, enabledRemotes, disabledRemotes, remotesManager, worktrees, currentWorkingDirectory);

    private void SetRefs(
        IReadOnlyList<IGitRef> refs,
        IReadOnlyCollection<GitRevision> stashes,
        bool includeStashes,
        string currentBranch,
        IReadOnlyList<Remote>? enabledRemotes,
        IReadOnlyList<Remote>? disabledRemotes,
        IConfigFileRemoteSettingsManager? remotesManager,
        IReadOnlyList<GitWorktree>? worktrees = null,
        string currentWorkingDirectory = "")
    {
        HashSet<string> expandedNodes =
        [
            .. _trees
                .SelectMany(tree => tree.DescendantsAndSelf())
                .Where(node => node.TreeViewNode.IsExpanded)
                .Select(GetNodeIdentity),
        ];
        HashSet<string> selectedNodes = [.. GetSelectedNodes().Select(GetNodeIdentity)];
        bool restoreState = _trees.Count > 0;

        ClearSearchResults();
        _trees.Clear();

        IGitRef[] branches = [.. refs.Where(gitRef => gitRef.IsHead && !gitRef.IsTag)];
        IGitRef[] remotes = [.. refs.Where(gitRef => gitRef.IsRemote)];
        IGitRef[] tags = [.. refs.Where(gitRef => gitRef.IsTag && !gitRef.IsDereference)];
        _trees.Add(new LocalBranchTree(this, branches, currentBranch));
        _trees.Add(new RemoteBranchTree(this, remotes, enabledRemotes, disabledRemotes, remotesManager));
        _worktreeTree.Load(worktrees ?? [], currentWorkingDirectory);
        _trees.Add(_worktreeTree);
        _trees.Add(new TagTree(this, tags));
        _trees.Add(_submoduleTree);

        _stashTree = new StashTree(this, stashes);
        if (includeStashes)
        {
            _trees.Add(_stashTree);
        }

        ApplyRoots();
        if (restoreState)
        {
            NodeBase[] nodes = [.. _trees.SelectMany(tree => tree.DescendantsAndSelf())];
            foreach (NodeBase node in nodes)
            {
                node.TreeViewNode.IsExpanded = expandedNodes.Contains(GetNodeIdentity(node));
            }

            if (treeMain.SelectedItems is not null)
            {
                treeMain.SelectedItems.Clear();
                foreach (NodeBase node in nodes.Where(node => selectedNodes.Contains(GetNodeIdentity(node))))
                {
                    treeMain.SelectedItems.Add(node.TreeViewNode);
                }
            }
        }
    }

    internal static string GetNodeIdentity(NodeBase node)
        => $"{node.GetType().Name}:{node.SearchText}";

    internal HashSet<string> CaptureSelectedNodeIdentities(NodeBase root)
    {
        HashSet<NodeBase> descendants = [.. root.DescendantsAndSelf()];
        TreeViewItem[] selectedItems =
        [
            .. (treeMain.SelectedItems?.OfType<TreeViewItem>()
                ?? (treeMain.SelectedItem is TreeViewItem selected ? [selected] : []))
                .Where(item => item.Tag is NodeBase node && descendants.Contains(node)),
        ];
        HashSet<string> identities =
        [
            .. selectedItems
                .Select(item => (NodeBase)item.Tag!)
                .Select(GetNodeIdentity),
        ];
        if (treeMain.SelectedItems is not null)
        {
            foreach (TreeViewItem item in selectedItems)
            {
                treeMain.SelectedItems.Remove(item);
            }
        }

        return identities;
    }

    internal void RestoreSelectedNodes(NodeBase root, IReadOnlySet<string> identities)
    {
        if (identities.Count == 0 || treeMain.SelectedItems is null)
        {
            return;
        }

        foreach (NodeBase node in root.DescendantsAndSelf().Where(node => identities.Contains(GetNodeIdentity(node))))
        {
            treeMain.SelectedItems.Add(node.TreeViewNode);
        }
    }

    internal static Control CreateHeader(string caption, IImage icon, bool isBold = false, bool isItalic = false)
        => new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 2,
            Children =
            {
                new Image
                {
                    Width = 16,
                    Height = 16,
                    Stretch = Stretch.Uniform,
                    Source = icon,
                },
                new TextBlock
                {
                    FontWeight = isBold ? FontWeight.Bold : FontWeight.Normal,
                    FontStyle = isItalic ? FontStyle.Italic : FontStyle.Normal,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Text = caption,
                },
            },
        };

    internal void PrepareTreeViewItem(NodeBase node)
    {
        node.TreeViewNode.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(node.TreeViewNode).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
            {
                SelectTreeViewItem(node.TreeViewNode);
            }
        };
        node.TreeViewNode.DoubleTapped += (_, _) => node.OnDoubleClick();
    }

    internal int GetTreePosition(RepoTreeKind kind)
        => kind switch
        {
            RepoTreeKind.Branches => AppSettings.RepoObjectsTreeBranchesIndex,
            RepoTreeKind.Remotes => AppSettings.RepoObjectsTreeRemotesIndex,
            RepoTreeKind.Worktrees => AppSettings.RepoObjectsTreeWorktreesIndex,
            RepoTreeKind.Tags => AppSettings.RepoObjectsTreeTagsIndex,
            RepoTreeKind.Submodules => AppSettings.RepoObjectsTreeSubmodulesIndex,
            RepoTreeKind.Stashes => AppSettings.RepoObjectsTreeStashesIndex,
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

    internal void SetTreePosition(RepoTreeKind kind, int value)
    {
        switch (kind)
        {
            case RepoTreeKind.Branches: AppSettings.RepoObjectsTreeBranchesIndex = value; break;
            case RepoTreeKind.Remotes: AppSettings.RepoObjectsTreeRemotesIndex = value; break;
            case RepoTreeKind.Worktrees: AppSettings.RepoObjectsTreeWorktreesIndex = value; break;
            case RepoTreeKind.Tags: AppSettings.RepoObjectsTreeTagsIndex = value; break;
            case RepoTreeKind.Submodules: AppSettings.RepoObjectsTreeSubmodulesIndex = value; break;
            case RepoTreeKind.Stashes: AppSettings.RepoObjectsTreeStashesIndex = value; break;
            default: throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    internal bool GetTreeVisibility(RepoTreeKind kind)
        => kind switch
        {
            RepoTreeKind.Branches => AppSettings.RepoObjectsTreeShowBranches,
            RepoTreeKind.Remotes => AppSettings.RepoObjectsTreeShowRemotes,
            RepoTreeKind.Worktrees => AppSettings.RepoObjectsTreeShowWorktrees,
            RepoTreeKind.Tags => AppSettings.RepoObjectsTreeShowTags,
            RepoTreeKind.Submodules => AppSettings.RepoObjectsTreeShowSubmodules,
            RepoTreeKind.Stashes => AppSettings.RepoObjectsTreeShowStashes,
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

    internal void SetTreeVisibility(RepoTreeKind kind, bool value)
    {
        switch (kind)
        {
            case RepoTreeKind.Branches: AppSettings.RepoObjectsTreeShowBranches = value; break;
            case RepoTreeKind.Remotes: AppSettings.RepoObjectsTreeShowRemotes = value; break;
            case RepoTreeKind.Worktrees: AppSettings.RepoObjectsTreeShowWorktrees = value; break;
            case RepoTreeKind.Tags: AppSettings.RepoObjectsTreeShowTags = value; break;
            case RepoTreeKind.Submodules: AppSettings.RepoObjectsTreeShowSubmodules = value; break;
            case RepoTreeKind.Stashes: AppSettings.RepoObjectsTreeShowStashes = value; break;
            default: throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    private void ApplyRoots()
    {
        TreeViewItem[] visibleRoots =
        [
            .. _trees
                .Where(tree => tree.IsEnabled)
                .OrderBy(tree => tree.PositionIndex)
                .ThenBy(tree => tree.Kind)
                .Select(tree => tree.TreeViewNode),
        ];
        treeMain.ItemsSource = visibleRoots;
    }

    private void ToggleTree(RepoTreeKind kind, bool show)
    {
        SetTreeVisibility(kind, show);
        ClearSearchResults();
        ApplyRoots();
    }

    private void CollapseAll()
    {
        foreach (Tree tree in _trees)
        {
            SetExpanded(tree.TreeViewNode, expanded: false, recursive: true);
        }
    }

    private void DoSearch()
    {
        string criterion = txtBranchCriterion.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(criterion))
        {
            ClearSearchResults();
            txtBranchCriterion.Focus();
            return;
        }

        if (_searchResults is null)
        {
            _searchResults =
            [
                .. _trees
                    .Where(tree => tree.IsEnabled)
                    .SelectMany(tree => tree.DescendantsAndSelf())
                    .Where(node => node.SearchText.Contains(criterion, StringComparison.InvariantCultureIgnoreCase))
                    .Select(node => node.TreeViewNode),
            ];
            foreach (TreeViewItem result in _searchResults)
            {
                result.Classes.Add("repo-search-result");
            }
        }

        if (_searchResults.Count == 0)
        {
            return;
        }

        TreeViewItem next = _searchResults[0];
        _searchResults.RemoveAt(0);
        _searchResults.Add(next);
        if (next.Tag is NodeBase node)
        {
            for (NodeBase? parent = node.Parent; parent is not null; parent = parent.Parent)
            {
                parent.TreeViewNode.IsExpanded = true;
            }
        }

        treeMain.SelectedItem = next;
        next.BringIntoView();
    }

    private void ClearSearchResults()
    {
        if (_searchResults is not null)
        {
            foreach (TreeViewItem item in _searchResults)
            {
                item.Classes.Remove("repo-search-result");
            }
        }

        _searchResults = null;
    }

    private void CreateContextActions()
    {
        repoObjectsContextMenu.Items.Add(_actionSeparator);
        AddAction(RepoAction.Copy, nameof(RepoObjectsTree), "copyContextMenuItem", "&Copy to clipboard", Images.CopyToClipboard);
        AddAction(RepoAction.Filter, nameof(RepoObjectsTree), "filterForSelectedRefsMenuItem", "&Filter for selected", Images.ShowThisBranchOnly);
        AddAction(RepoAction.CheckoutLocal, "BranchMenuItemsStrings", "Checkout", "Chec&kout branch...", Images.BranchCheckout);
        AddAction(RepoAction.CheckoutRemote, "RemoteBranchMenuItemsStrings", "Checkout", "Chec&kout remote branch...", Images.BranchCheckout);
        AddAction(RepoAction.Merge, "MenuItemsStrings", "Merge", "&Merge into current branch...", Images.Merge);
        AddAction(RepoAction.RebaseLocal, "BranchMenuItemsStrings", "Rebase", "&Rebase current branch on this branch...", Images.Rebase);
        AddAction(RepoAction.RebaseRemote, "RemoteBranchMenuItemsStrings", "Rebase", "&Rebase current branch on this remote branch...", Images.Rebase);
        AddAction(RepoAction.RebaseTag, "TagMenuItemsStrings", "Rebase", "&Rebase current branch on this tag revision...", Images.Rebase);
        AddAction(RepoAction.CreateBranch, "MenuItemsStrings", "CreateBranch", "Create &branch...", Images.Branch);
        AddAction(RepoAction.Reset, "MenuItemsStrings", "Reset", "Re&set current branch to here...", Images.ResetCurrentBranchToHere);
        AddAction(RepoAction.RenameBranch, "MenuItemsStrings", "Rename", "R&ename branch...", Images.Renamed);
        AddAction(RepoAction.DeleteBranch, "BranchMenuItemsStrings", "Delete", "&Delete branch...", Images.BranchDelete);
        AddAction(RepoAction.DeleteTag, "TagMenuItemsStrings", "Delete", "&Delete tag...", Images.BranchDelete);
        AddAction(RepoAction.FetchBranch, nameof(RepoObjectsTree), "mnubtnFetchOneBranch", "Fe&tch", Images.Stage);
        AddAction(RepoAction.FetchMerge, nameof(RepoObjectsTree), "mnubtnPullFromRemoteBranch", "Fetch && Merge (&Pull)", Images.Pull);
        AddAction(RepoAction.FetchCheckout, nameof(RepoObjectsTree), "mnubtnRemoteBranchFetchAndCheckout", "&Fetch && Checkout", Images.BranchCheckout);
        AddAction(RepoAction.FetchRebase, nameof(RepoObjectsTree), "mnubtnFetchRebase", "Fetch && Re&base", Images.Rebase);
        AddAction(RepoAction.FetchCreate, nameof(RepoObjectsTree), "mnubtnFetchCreateBranch", "Fetc&h && Create Branch", Images.Branch);
        AddAction(RepoAction.CreateInFolder, nameof(RepoObjectsTree), "mnubtnCreateBranch", "Create Branch...", Images.BranchCreate);
        AddAction(RepoAction.DeleteFolderBranches, nameof(RepoObjectsTree), "mnubtnDeleteAllBranches", "Delete All", Images.BranchDelete);
        AddAction(RepoAction.ManageRemotes, nameof(RepoObjectsTree), "mnuBtnManageRemotesFromRootNode", "&Manage...", Images.Remotes);
        AddAction(RepoAction.FetchAllRemotes, nameof(RepoObjectsTree), "mnuBtnFetchAllRemotes", "Fetch all remotes", Images.PullFetchAll);
        AddAction(RepoAction.PruneAllRemotes, nameof(RepoObjectsTree), "mnuBtnPruneAllRemotes", "Fetch and prune all remotes", Images.PullFetchPruneAll);
        AddAction(RepoAction.ManageRemote, nameof(RepoObjectsTree), "mnubtnManageRemotes", "&Manage...", Images.Remotes);
        AddAction(RepoAction.EnableRemote, nameof(RepoObjectsTree), "mnubtnEnableRemote", "&Activate", Images.EyeOpened);
        AddAction(RepoAction.EnableRemoteAndFetch, nameof(RepoObjectsTree), "mnubtnEnableRemoteAndFetch", "A&ctivate and fetch", Images.RemoteEnableAndFetch);
        AddAction(RepoAction.DisableRemote, nameof(RepoObjectsTree), "mnubtnDisableRemote", "&Deactivate", Images.EyeClosed);
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
        AddAction(RepoAction.Collapse, nameof(RepoObjectsTree), "mnubtnCollapse", "Collapse", Images.CollapseAll);
        AddAction(RepoAction.Expand, nameof(RepoObjectsTree), "mnubtnExpand", "Expand", Images.ExpandAll);
        AddAction(RepoAction.MoveUp, nameof(RepoObjectsTree), "mnubtnMoveUp", "Move Up", Images.ArrowUp);
        AddAction(RepoAction.MoveDown, nameof(RepoObjectsTree), "mnubtnMoveDown", "Move Down", Images.ArrowDown);
    }

    private void AddAction(RepoAction action, string category, string name, string source, IImage icon)
    {
        MenuItem item = new()
        {
            Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(Translate(category, name, source)),
            Icon = new Image { Width = 16, Height = 16, Source = icon, Stretch = Stretch.Uniform },
            IsVisible = false,
        };
        item.Click += (_, _) => ExecuteAction(action);
        _actionItems.Add(action, item);
        repoObjectsContextMenu.Items.Add(item);
    }

    private static string Translate(string category, string name, string source, string property = "Text")
    {
        string text = source;
        foreach (ITranslation translation in Translator.GetTranslation(AppSettings.CurrentTranslation).Values)
        {
            text = translation.TranslateItem(category, name, property, () => source) ?? text;
        }

        return text;
    }

    private void RepoObjectsContextMenuOpening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        foreach (MenuItem item in _actionItems.Values)
        {
            item.IsVisible = false;
        }

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
        bool canFilter = GetSelectedNodes().OfType<BaseRevisionNode>().Any(node => node.GitRef is not null)
            && _filterRevisionGridBySpaceSeparatedRefs is not null;
        SetAction(RepoAction.Copy, canCopy, canCopy);
        SetAction(RepoAction.Filter, canFilter, canFilter);

        switch (SelectedNode)
        {
            case LocalBranchNode localBranch:
                SetActionVisible(RepoAction.CreateBranch, canChangeWorkingTree);
                SetActionVisible(RepoAction.Reset, canChangeWorkingTree);
                SetActionVisible(RepoAction.RenameBranch, canChangeWorkingTree);
                if (!localBranch.IsCurrent)
                {
                    SetActionVisible(RepoAction.CheckoutLocal, canChangeWorkingTree);
                    SetActionVisible(RepoAction.Merge, canChangeWorkingTree);
                    SetActionVisible(RepoAction.RebaseLocal, canChangeWorkingTree);
                    SetActionVisible(RepoAction.DeleteBranch, canChangeWorkingTree);
                }

                break;
            case RemoteBranchNode:
                SetActionVisible(RepoAction.CheckoutRemote, canChangeWorkingTree);
                SetActionVisible(RepoAction.Merge, canChangeWorkingTree);
                SetActionVisible(RepoAction.RebaseRemote, canChangeWorkingTree);
                SetActionVisible(RepoAction.CreateBranch, canChangeWorkingTree);
                SetActionVisible(RepoAction.Reset, canChangeWorkingTree);
                SetActionVisible(RepoAction.FetchBranch, canRunCommands);
                SetActionVisible(RepoAction.FetchMerge, canChangeWorkingTree);
                SetActionVisible(RepoAction.FetchCheckout, canChangeWorkingTree);
                SetActionVisible(RepoAction.FetchRebase, canChangeWorkingTree);
                SetActionVisible(RepoAction.FetchCreate, canChangeWorkingTree);
                break;
            case TagNode:
                SetActionVisible(RepoAction.Merge, canChangeWorkingTree);
                SetActionVisible(RepoAction.RebaseTag, canChangeWorkingTree);
                SetActionVisible(RepoAction.CreateBranch, canChangeWorkingTree);
                SetActionVisible(RepoAction.Reset, canChangeWorkingTree);
                SetActionVisible(RepoAction.DeleteTag, canRunCommands);
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

        bool hasDynamicAction = _actionItems.Values.Any(item => item.IsVisible);
        bool hasStaticAction = stashTreeSelected || stashSelected || worktreeTreeSelected || worktreeSelected;
        _actionSeparator.IsVisible = hasDynamicAction && hasStaticAction;
        e.Cancel = !hasDynamicAction && !hasStaticAction;
    }

    private void SetActionVisible(RepoAction action, bool enabled)
        => SetAction(action, visible: true, enabled: enabled);

    private void SetAction(RepoAction action, bool visible, bool enabled)
    {
        MenuItem item = _actionItems[action];
        item.IsVisible = visible;
        item.IsEnabled = visible && enabled;
    }

    private void ExecuteAction(RepoAction action)
    {
        switch (action)
        {
            case RepoAction.Copy:
                ClipboardUtil.TrySetText(SelectedNode switch
                {
                    BaseRevisionNode revisionNode => revisionNode.FullPath,
                    StashNode stashNode => stashNode.ReflogSelector,
                    _ => string.Empty,
                });
                break;
            case RepoAction.Filter:
                _filterRevisionGridBySpaceSeparatedRefs?.Invoke(string.Join(" ", GetSelectedNodes().OfType<BaseRevisionNode>().Where(node => node.GitRef is not null).Select(node => node.FullPath)));
                break;
            case RepoAction.CheckoutLocal: ((LocalBranchNode)SelectedNode!).Checkout(); break;
            case RepoAction.CheckoutRemote: ((RemoteBranchNode)SelectedNode!).Checkout(); break;
            case RepoAction.Merge:
                if (SelectedNode is LocalBranchNode localMerge)
                {
                    localMerge.Merge();
                }
                else if (SelectedNode is RemoteBranchNode remoteMerge)
                {
                    remoteMerge.Merge();
                }
                else
                {
                    ((TagNode)SelectedNode!).Merge();
                }

                break;
            case RepoAction.RebaseLocal: ((LocalBranchNode)SelectedNode!).Rebase(); break;
            case RepoAction.RebaseRemote: ((RemoteBranchNode)SelectedNode!).Rebase(); break;
            case RepoAction.RebaseTag: ((TagNode)SelectedNode!).Rebase(); break;
            case RepoAction.CreateBranch:
                if (SelectedNode is LocalBranchNode localCreate)
                {
                    localCreate.CreateBranch();
                }
                else if (SelectedNode is RemoteBranchNode remoteCreate)
                {
                    remoteCreate.CreateBranch();
                }
                else
                {
                    ((TagNode)SelectedNode!).CreateBranch();
                }

                break;
            case RepoAction.Reset: ((BaseRevisionNode)SelectedNode!).Reset(); break;
            case RepoAction.RenameBranch: ((LocalBranchNode)SelectedNode!).Rename(); break;
            case RepoAction.DeleteBranch: ((LocalBranchNode)SelectedNode!).Delete(); break;
            case RepoAction.DeleteTag: ((TagNode)SelectedNode!).Delete(); break;
            case RepoAction.FetchBranch: ((RemoteBranchNode)SelectedNode!).Fetch(); break;
            case RepoAction.FetchMerge: ((RemoteBranchNode)SelectedNode!).FetchAndMerge(); break;
            case RepoAction.FetchCheckout: ((RemoteBranchNode)SelectedNode!).FetchAndCheckout(); break;
            case RepoAction.FetchRebase: ((RemoteBranchNode)SelectedNode!).FetchAndRebase(); break;
            case RepoAction.FetchCreate: ((RemoteBranchNode)SelectedNode!).FetchAndCreateBranch(); break;
            case RepoAction.CreateInFolder: ((BranchPathNode)SelectedNode!).CreateBranch(); break;
            case RepoAction.DeleteFolderBranches: ((BranchPathNode)SelectedNode!).DeleteAll(); break;
            case RepoAction.ManageRemotes: ((RemoteBranchTree)SelectedNode!).PopupManageRemotesForm(remoteName: null); break;
            case RepoAction.FetchAllRemotes: ((RemoteBranchTree)SelectedNode!).FetchAll(); break;
            case RepoAction.PruneAllRemotes: ((RemoteBranchTree)SelectedNode!).FetchPruneAll(); break;
            case RepoAction.ManageRemote: ((RemoteRepoNode)SelectedNode!).PopupManageRemotesForm(); break;
            case RepoAction.EnableRemote: ((RemoteRepoNode)SelectedNode!).Enable(fetch: false); break;
            case RepoAction.EnableRemoteAndFetch: ((RemoteRepoNode)SelectedNode!).Enable(fetch: true); break;
            case RepoAction.DisableRemote: ((RemoteRepoNode)SelectedNode!).Disable(); break;
            case RepoAction.FetchRemote: ((RemoteRepoNode)SelectedNode!).Fetch(); break;
            case RepoAction.PruneRemote: ((RemoteRepoNode)SelectedNode!).Prune(); break;
            case RepoAction.OpenRemoteUrl: ((RemoteRepoNode)SelectedNode!).OpenRemoteUrlInBrowser(); break;
            case RepoAction.OpenSubmodule: _submoduleTree.OpenSubmodule((SubmoduleNode)SelectedNode!); break;
            case RepoAction.OpenSubmoduleInGitExtensions: _submoduleTree.OpenSubmoduleInGitExtensions((SubmoduleNode)SelectedNode!); break;
            case RepoAction.ManageSubmodules: _submoduleTree.ManageSubmodules(this); break;
            case RepoAction.UpdateSubmodule: _submoduleTree.UpdateSubmodule(this, (SubmoduleNode)SelectedNode!); break;
            case RepoAction.SynchronizeSubmodules: _submoduleTree.SynchronizeSubmodules(this); break;
            case RepoAction.ResetSubmodule: _submoduleTree.ResetSubmodule(this, (SubmoduleNode)SelectedNode!); break;
            case RepoAction.StashSubmodule: _submoduleTree.StashSubmodule(this, (SubmoduleNode)SelectedNode!); break;
            case RepoAction.CommitSubmodule: _submoduleTree.CommitSubmodule(this, (SubmoduleNode)SelectedNode!); break;
            case RepoAction.Collapse: SetExpanded(SelectedNode!.TreeViewNode, expanded: false, recursive: true); break;
            case RepoAction.Expand: SetExpanded(SelectedNode!.TreeViewNode, expanded: true, recursive: true); break;
            case RepoAction.MoveUp: ReorderTree((Tree)SelectedNode!, up: true); break;
            case RepoAction.MoveDown: ReorderTree((Tree)SelectedNode!, up: false); break;
            default: throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private IEnumerable<NodeBase> GetSelectedNodes()
    {
        IEnumerable<TreeViewItem> items = treeMain.SelectedItems?.OfType<TreeViewItem>()
            ?? (treeMain.SelectedItem is TreeViewItem selected ? [selected] : []);
        return items.Select(item => item.Tag).OfType<NodeBase>();
    }

    private void ReorderTree(Tree tree, bool up)
    {
        Tree[] visibleTrees = [.. _trees.Where(item => item.IsEnabled).OrderBy(item => item.PositionIndex)];
        int index = Array.IndexOf(visibleTrees, tree);
        int swapIndex = up ? index - 1 : index + 1;
        if (index < 0 || swapIndex < 0 || swapIndex >= visibleTrees.Length)
        {
            return;
        }

        Tree swap = visibleTrees[swapIndex];
        int position = tree.PositionIndex;
        tree.PositionIndex = swap.PositionIndex;
        swap.PositionIndex = position;
        ApplyRoots();
        SelectTreeViewItem(tree.TreeViewNode);
    }

    private static void SetExpanded(TreeViewItem item, bool expanded, bool recursive)
    {
        item.IsExpanded = expanded;
        if (!recursive)
        {
            return;
        }

        foreach (TreeViewItem child in item.Items.Cast<TreeViewItem>())
        {
            SetExpanded(child, expanded, recursive: true);
        }
    }

    internal void SelectTreeViewItem(TreeViewItem item)
        => treeMain.SelectedItem = item;

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor(RepoObjectsTree control)
    {
        internal TreeView Tree => control.treeMain;

        internal ContextMenu ContextMenu => control.repoObjectsContextMenu;

        internal TextBox SearchBox => control.txtBranchCriterion;

        internal Button SearchButton => control.btnSearch;

        internal ToggleButton ShowBranchesButton => control.tsbShowBranches;

        internal ToggleButton ShowRemotesButton => control.tsbShowRemotes;

        internal ToggleButton ShowWorktreesButton => control.tsbShowWorktrees;

        internal ToggleButton ShowTagsButton => control.tsbShowTags;

        internal ToggleButton ShowSubmodulesButton => control.tsbShowSubmodules;

        internal ToggleButton ShowStashesButton => control.tsbShowStashes;

        internal MenuItem StashAllMenuItem => control.mnubtnStashAllFromRootNode;

        internal MenuItem StashStagedMenuItem => control.mnubtnStashStagedFromRootNode;

        internal MenuItem ManageStashesMenuItem => control.mnubtnManageStashFromRootNode;

        internal MenuItem OpenStashMenuItem => control.mnubtnOpenStash;

        internal MenuItem ApplyStashMenuItem => control.mnubtnApplyStash;

        internal MenuItem PopStashMenuItem => control.mnubtnPopStash;

        internal MenuItem DropStashMenuItem => control.mnubtnDropStash;

        internal MenuItem CreateWorktreeMenuItem => control.mnubtnCreateWorktreeFromRootNode;

        internal MenuItem PruneWorktreesMenuItem => control.mnubtnPruneWorktreesFromRootNode;

        internal MenuItem ManageWorktreesMenuItem => control.mnubtnManageWorktreesFromRootNode;

        internal MenuItem OpenWorktreeMenuItem => control.mnubtnOpenWorktree;

        internal MenuItem DeleteWorktreeMenuItem => control.mnubtnDeleteWorktree;

        internal MenuItem CopyWorktreePathMenuItem => control.mnubtnCopyWorktreePath;

        internal MenuItem ShowWorktreeInFolderMenuItem => control.mnubtnShowWorktreeInFolder;

        internal MenuItem GetActionMenuItem(string action)
            => control._actionItems[Enum.Parse<RepoAction>(action)];

        internal bool UpdateContextMenu()
        {
            System.ComponentModel.CancelEventArgs eventArgs = new();
            control.RepoObjectsContextMenuOpening(control.repoObjectsContextMenu, eventArgs);
            return !eventArgs.Cancel;
        }

        internal void Search()
            => control.DoSearch();

        internal void SetSubmodules(SubmoduleInfoResult result)
            => control._submoduleTree.Load(result);

        internal void SetWorktrees(IReadOnlyList<GitWorktree> worktrees, string currentWorkingDirectory)
            => control._worktreeTree.Load(worktrees, currentWorkingDirectory);
    }

    private enum RepoAction
    {
        Copy,
        Filter,
        CheckoutLocal,
        CheckoutRemote,
        Merge,
        RebaseLocal,
        RebaseRemote,
        RebaseTag,
        CreateBranch,
        Reset,
        RenameBranch,
        DeleteBranch,
        DeleteTag,
        FetchBranch,
        FetchMerge,
        FetchCheckout,
        FetchRebase,
        FetchCreate,
        CreateInFolder,
        DeleteFolderBranches,
        ManageRemotes,
        FetchAllRemotes,
        PruneAllRemotes,
        ManageRemote,
        EnableRemote,
        EnableRemoteAndFetch,
        DisableRemote,
        FetchRemote,
        PruneRemote,
        OpenRemoteUrl,
        OpenSubmodule,
        OpenSubmoduleInGitExtensions,
        ManageSubmodules,
        UpdateSubmodule,
        SynchronizeSubmodules,
        ResetSubmodule,
        StashSubmodule,
        CommitSubmodule,
        Collapse,
        Expand,
        MoveUp,
        MoveDown,
    }
}

/// <summary>
/// Draws the dotted hierarchy lines supplied by the native WinForms tree but absent from
/// Avalonia's Fluent TreeView template.
/// </summary>
internal sealed class TreeConnectorControl : Control
{
    private const double ChevronCenter = 10;
    private const double ChevronGapHalfHeight = 6;
    private const double ChevronGapHalfWidth = 6;
    private const double Indent = 18;
    private static readonly DashStyle DottedLine = new([1, 1], 0);

    internal TreeViewItem? Item => this.FindAncestorOfType<TreeViewItem>();

    internal bool IsLastSibling
    {
        get
        {
            if (Item is not TreeViewItem item)
            {
                return true;
            }

            (int index, int count) = GetSiblingPosition(item);
            return index == count - 1;
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        TreeViewItem? item = Item;
        if (item is null || Bounds.Height <= 0)
        {
            return;
        }

        IBrush brush = GetConnectorBrush();
        Pen pen = new(brush, 1, DottedLine, PenLineCap.Flat, PenLineJoin.Miter, 10);
        double middle = Bounds.Height / 2;
        (int index, int count) = GetSiblingPosition(item);
        double x = ChevronCenter + (item.Level * Indent);
        double top = item.Level == 0 && index == 0 ? middle : 0;
        double bottom = index == count - 1 ? middle : Bounds.Height;
        bool hasChevron = item.Items.Count > 0;

        foreach ((Avalonia.Point start, Avalonia.Point end) in GetCurrentItemLines(x, top, bottom, middle, hasChevron))
        {
            context.DrawLine(pen, start, end);
        }

        for (TreeViewItem? ancestor = GetParentItem(item);
             ancestor is not null;
             ancestor = GetParentItem(ancestor))
        {
            (int ancestorIndex, int ancestorCount) = GetSiblingPosition(ancestor);
            if (ancestorIndex < ancestorCount - 1)
            {
                double ancestorX = ChevronCenter + (ancestor.Level * Indent);
                context.DrawLine(
                    pen,
                    new Avalonia.Point(ancestorX, 0),
                    new Avalonia.Point(ancestorX, Bounds.Height));
            }
        }
    }

    internal static IEnumerable<(Avalonia.Point Start, Avalonia.Point End)> GetCurrentItemLines(
        double x,
        double top,
        double bottom,
        double middle,
        bool hasChevron)
    {
        if (!hasChevron)
        {
            yield return (new Avalonia.Point(x, top), new Avalonia.Point(x, bottom));
            yield return (new Avalonia.Point(x, middle), new Avalonia.Point(x + ChevronCenter, middle));
            yield break;
        }

        double upperEnd = Math.Min(bottom, middle - ChevronGapHalfHeight);
        if (top < upperEnd)
        {
            yield return (new Avalonia.Point(x, top), new Avalonia.Point(x, upperEnd));
        }

        double lowerStart = Math.Max(top, middle + ChevronGapHalfHeight);
        if (lowerStart < bottom)
        {
            yield return (new Avalonia.Point(x, lowerStart), new Avalonia.Point(x, bottom));
        }

        yield return (
            new Avalonia.Point(x + ChevronGapHalfWidth, middle),
            new Avalonia.Point(x + ChevronCenter, middle));
    }

    private static TreeViewItem? GetParentItem(TreeViewItem item)
        => item.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();

    private static (int Index, int Count) GetSiblingPosition(TreeViewItem item)
    {
        TreeViewItem? parentItem = GetParentItem(item);
        ItemCollection siblings = parentItem is not null
            ? parentItem.Items
            : item.GetVisualAncestors().OfType<TreeView>().First().Items;
        return (Math.Max(siblings.IndexOf(item), 0), siblings.Count);
    }

    private IBrush GetConnectorBrush()
        => Application.Current?.TryGetResource(
                "GitExtensionsTreeConnectorBrush",
                ActualThemeVariant,
                out object? resource) == true
            && resource is IBrush brush
                ? brush
                : Brushes.Gray;
}
