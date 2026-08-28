using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitCommands.Remotes;
using GitCommands.Submodules;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.LeftPanel.ContextMenu;
using GitUI.LeftPanel.Interfaces;
using GitUI.Properties;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;

using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.LeftPanel;

public sealed partial class RepoObjectsTree : GitModuleControl
{
    public const string HotkeySettingsName = "LeftPanel";
    private readonly Dictionary<RepoAction, MenuItem> _actionItems = [];
    private readonly CancellationTokenSequence _selectionCancellationTokenSequence = new();
    private readonly TranslationString _searchTooltip = new("Search");

    private readonly List<Tree> _rootNodes = [];
    private LocalBranchTree _branchesTree = null!;
    private RemoteBranchTree _remotesTree = null!;
    private IReadOnlyCollection<GitRevision> _currentStashes = [];
    private string _currentBranch = string.Empty;
    private IReadOnlyList<GitWorktree> _currentWorktrees = [];
    private string _currentWorkingDirectory = string.Empty;
    private IReadOnlyList<Remote>? _disabledRemotes;
    private IReadOnlyList<Remote>? _enabledRemotes;

    private bool _includeStashes;
    private Action<string, ObjectId, ObjectId>? _openRepository;
    private IConfigFileRemoteSettingsManager? _remotesManager;
    private TagTree _tagTree = null!;
    private ISubmoduleStatusProvider? _submoduleStatusProvider;
    private StashTree _stashTree = null!;
    private readonly SubmoduleTree _submoduleTree;
    private readonly WorktreeTree _worktreeTree;
    private List<TreeViewItem>? _searchResult;

    /// <summary>Occurs when the selected node changes.</summary>
    public event EventHandler? NodeSelectionChanged;

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
    private Action<string?> _filterRevisionGridBySpaceSeparatedRefs = null!;
    private IAheadBehindDataProvider? _aheadBehindDataProvider;
    private bool _searchCriteriaChanged;
    private ICheckRefs _refsSource = null!;
    private IRevisionGridInfo _revisionGridInfo = null!;

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
        _currentStashes = stashes;
        _includeStashes = includeStashes;
        _currentBranch = currentBranch;
        _enabledRemotes = enabledRemotes;
        _disabledRemotes = disabledRemotes;
        _remotesManager = remotesManager;
        _currentWorktrees = worktrees ?? [];
        _currentWorkingDirectory = currentWorkingDirectory;

        HashSet<string> expandedNodes =
        [
            .. _rootNodes
                .SelectMany(tree => tree.DescendantsAndSelf())
                .Where(node => node.TreeViewNode.IsExpanded)
                .Select(GetNodeIdentity),
        ];
        HashSet<string> selectedNodes = [.. GetSelectedNodes().Select(GetNodeIdentity)];
        bool restoreState = _rootNodes.Count > 0;

        ClearSearchResults();
        _rootNodes.Clear();

        IGitRef[] branches = [.. refs.Where(gitRef => gitRef.IsHead && !gitRef.IsTag)];
        IGitRef[] remotes = [.. refs.Where(gitRef => gitRef.IsRemote)];
        IGitRef[] tags = [.. refs.Where(gitRef => gitRef.IsTag && !gitRef.IsDereference)];
        _branchesTree = new LocalBranchTree(this, branches, currentBranch);
        _remotesTree = new RemoteBranchTree(this, remotes, enabledRemotes, disabledRemotes, remotesManager);
        _tagTree = new TagTree(this, tags);
        _rootNodes.Add(_branchesTree);
        _rootNodes.Add(_remotesTree);
        _worktreeTree.Load(_currentWorktrees, currentWorkingDirectory);
        _rootNodes.Add(_worktreeTree);
        _rootNodes.Add(_tagTree);
        _rootNodes.Add(_submoduleTree);

        _stashTree = new StashTree(this, stashes);
        if (includeStashes)
        {
            _rootNodes.Add(_stashTree);
        }

        ApplyRoots();
        if (restoreState)
        {
            NodeBase[] nodes = [.. _rootNodes.SelectMany(tree => tree.DescendantsAndSelf())];
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

    public RepoObjectsTree()
    {
        InitializeComponent();
        tsbCollapseAll.Icon = Images.CollapseAll.AdaptLightness();
        _submoduleTree = new SubmoduleTree(this);
        _worktreeTree = new WorktreeTree(this);
        HotkeysEnabled = true;
        RegisterContextActions();

        treeMain.SelectionChanged += (_, _) => NodeSelectionChanged?.Invoke(this, EventArgs.Empty);
        menuMain.Opening += contextMenu_Opening;
        menuMain.Opened += contextMenu_Opened;
        tsbCollapseAll.Click += btnCollapseAll_Click;
        tsbShowBranches.Click += tsbShowBranches_Click;
        tsbShowRemotes.Click += tsbShowRemotes_Click;
        tsbShowWorktrees.Click += tsbShowWorktrees_Click;
        tsbShowTags.Click += tsbShowTags_Click;
        tsbShowSubmodules.Click += tsbShowSubmodules_Click;
        tsbShowStashes.Click += tsbShowStashes_Click;
        btnSearch.Click += OnBtnSearchClicked;
        _txtBranchCriterion.PropertyChanged += OnBranchCriterionChanged;
        _txtBranchCriterion.KeyDown += TxtBranchCriterion_KeyDown;

        tsbShowBranches.IsChecked = AppSettings.RepoObjectsTreeShowBranches;
        tsbShowRemotes.IsChecked = AppSettings.RepoObjectsTreeShowRemotes;
        tsbShowWorktrees.IsChecked = AppSettings.RepoObjectsTreeShowWorktrees;
        tsbShowTags.IsChecked = AppSettings.RepoObjectsTreeShowTags;
        tsbShowSubmodules.IsChecked = AppSettings.RepoObjectsTreeShowSubmodules;
        tsbShowStashes.IsChecked = AppSettings.RepoObjectsTreeShowStashes;
        FixInvalidTreeToPositionIndices();

        AttachedToLogicalTree += (_, _) => _submoduleTree.Attach(_submoduleStatusProvider);
        DetachedFromLogicalTree += (_, _) =>
        {
            _selectionCancellationTokenSequence.CancelCurrent();
            _submoduleTree.Detach();
        };

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

    public void Initialize(
        IAheadBehindDataProvider? aheadBehindDataProvider,
        Action<string?> filterRevisionGridBySpaceSeparatedRefs,
        ICheckRefs refsSource,
        IRevisionGridInfo revisionGridInfo)
    {
        _aheadBehindDataProvider = aheadBehindDataProvider;
        _filterRevisionGridBySpaceSeparatedRefs = filterRevisionGridBySpaceSeparatedRefs;
        _refsSource = refsSource;
        _revisionGridInfo = revisionGridInfo;

        // Lazily resolve the command source so each tree receives repository notifications.
        _ = UICommandsSource;
    }

    /// <summary>Connects the tree's selected-ref filtering action to the revision grid.</summary>
    public void Initialize(
        Action<string?> filterRevisionGridBySpaceSeparatedRefs,
        Action<string, ObjectId, ObjectId>? openRepository = null)
    {
        _filterRevisionGridBySpaceSeparatedRefs = filterRevisionGridBySpaceSeparatedRefs;
        _openRepository = openRepository;
    }

    /// <summary>
    /// FormBrowse refreshing the left panel when refreshing the grid.
    /// (Update the objects in the panel.)
    /// </summary>
    /// <param name="getRefs">Function to get refs.</param>
    /// <param name="getStashRevs">Lazy accessor for stash commits.</param>
    /// <param name="forceRefresh">Refresh may be required as references may have been changed.</param>
    public void RefreshRevisionsLoading(
        Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs,
        Lazy<IReadOnlyCollection<GitRevision>> getStashRevs,
        bool forceRefresh)
    {
        IReadOnlyList<IGitRef> refs = getRefs(RefsFilter.NoFilter);
        SetRefs(
            refs,
            getStashRevs.Value,
            includeStashes: true,
            Module.GetSelectedBranch(emptyIfDetached: true),
            _enabledRemotes,
            _disabledRemotes,
            _remotesManager,
            _currentWorktrees,
            _currentWorkingDirectory);
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
            .. _rootNodes
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

    /// <summary>
    /// FormBrowse refreshing the left panel after updating the grid.
    /// (Update the visibility for left panel objects.)
    /// </summary>
    public void RefreshRevisionsLoaded()
        => ApplyRoots();

    /// <summary>
    /// Refresh after resorting.
    /// </summary>
    /// <param name="getRefs">Git references</param>
    public void ResortRefs(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
    {
        IReadOnlyList<IGitRef> refs =
        [
            .. getRefs(RefsFilter.Heads),
            .. getRefs(RefsFilter.Remotes),
            .. getRefs(RefsFilter.Tags),
        ];

        SetRefs(
            refs,
            _currentStashes,
            _includeStashes,
            _currentBranch,
            _enabledRemotes,
            _disabledRemotes,
            _remotesManager,
            _currentWorktrees,
            _currentWorkingDirectory);
    }

    public void ReloadHotkeys()
    {
        LoadHotkeys(HotkeySettingsName);
    }

    public void SelectionChanged(IReadOnlyList<GitRevision> selectedRevisions)
    {
        if ((selectedRevisions.Count == 0 && SelectedNode is null)
            || (selectedRevisions.Count == 1 && selectedRevisions[0].ObjectId == SelectedRevisionObjectId))
        {
            return;
        }

        CancellationToken cancellationToken = _selectionCancellationTokenSequence.Next();
        GitRevision? selectedRevision = selectedRevisions.FirstOrDefault();
        string? selectedGuid = selectedRevision is null
            ? null
            : selectedRevision.IsArtificial ? "HEAD" : selectedRevision.Guid;

        ThreadHelper.FileAndForget(async () =>
        {
            HashSet<string> mergedBranches = selectedGuid is null
                ? []
                : [.. await Module.GetMergedBranchesAsync(
                    includeRemote: true,
                    fullRefname: true,
                    commit: selectedGuid,
                    cancellationToken)];

            selectedRevision?.Refs.ForEach(gitRef => mergedBranches.Remove(gitRef.CompleteName));
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                _branchesTree.DescendantsAndSelf()
                    .OfType<BaseBranchLeafNode>()
                    .ForEach(node => node.IsMerged = mergedBranches.Contains(GitRefName.RefsHeadsPrefix + node.FullPath));
                _remotesTree.DescendantsAndSelf()
                    .OfType<BaseBranchLeafNode>()
                    .ForEach(node => node.IsMerged = mergedBranches.Contains(GitRefName.RefsRemotesPrefix + node.FullPath));
            });
        });
    }

    public void SelectGitRef(string gitRef)
    {
        BaseRevisionNode? node = _rootNodes
            .SelectMany(tree => tree.DescendantsAndSelf())
            .OfType<BaseRevisionNode>()
            .FirstOrDefault(revisionNode => revisionNode.FullPath == gitRef);
        if (node is null)
        {
            return;
        }

        SelectNode(node, multiple: false, includingDescendants: false);
        node.TreeViewNode.BringIntoView();
    }

    private void OnRuntimeLoad()
    {
        ReloadHotkeys();
    }

    protected override void OnUICommandsSourceSet(IGitUICommandsSource source)
    {
        base.OnUICommandsSourceSet(source);
        _submoduleStatusProvider = source.UICommands.GetService(typeof(ISubmoduleStatusProvider)) as ISubmoduleStatusProvider;
        _submoduleTree.Attach(_submoduleStatusProvider);
        if (source.UICommands.GetService(typeof(IHotkeySettingsLoader)) is not null)
        {
            OnRuntimeLoad();
        }
    }

    private void AddTree(Tree tree)
    {
        if (!_rootNodes.Contains(tree))
        {
            _rootNodes.Add(tree);
        }

        ApplyRoots();
    }

    internal bool IsNodeSelected(TreeViewItem item)
        => treeMain.SelectedItems?.Contains(item) == true || ReferenceEquals(treeMain.SelectedItem, item);

    internal void SetNodeSelected(TreeViewItem item, bool selected)
    {
        if (treeMain.SelectedItems is null)
        {
            if (selected)
            {
                treeMain.SelectedItem = item;
            }

            return;
        }

        if (selected)
        {
            if (!treeMain.SelectedItems.Contains(item))
            {
                treeMain.SelectedItems.Add(item);
            }
        }
        else
        {
            treeMain.SelectedItems.Remove(item);
        }
    }

    private void RemoveTree(Tree tree)
    {
        _rootNodes.Remove(tree);
        ApplyRoots();
    }

    private void DoSearch()
    {
        if (_searchCriteriaChanged)
        {
            _searchCriteriaChanged = false;
            ClearSearchResults();
        }

        string criterion = _txtBranchCriterion.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(criterion))
        {
            ClearSearchResults();
            _txtBranchCriterion.Focus();
            return;
        }

        if (_searchResult is null)
        {
            _searchResult =
            [
                .. _rootNodes
                    .Where(tree => tree.IsEnabled)
                    .SelectMany(tree => tree.DescendantsAndSelf())
                    .Where(node => node.SearchText.Contains(criterion, StringComparison.InvariantCultureIgnoreCase))
                    .Select(node => node.TreeViewNode),
            ];
            foreach (TreeViewItem result in _searchResult)
            {
                result.Classes.Add("repo-search-result");
            }
        }

        if (_searchResult.Count == 0)
        {
            return;
        }

        TreeViewItem next = _searchResult[0];
        _searchResult.RemoveAt(0);
        _searchResult.Add(next);
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
        if (_searchResult is not null)
        {
            foreach (TreeViewItem item in _searchResult)
            {
                item.Classes.Remove("repo-search-result");
            }
        }

        _searchResult = null;
    }

    private void OnBtnSearchClicked(object? sender, EventArgs e)
    {
        DoSearch();
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

    private void btnCollapseAll_Click(object? sender, EventArgs e)
    {
        foreach (Tree tree in _rootNodes)
        {
            SetExpanded(tree.TreeViewNode, expanded: false, recursive: true);
        }
    }

    private void OnBranchCriterionChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == TextBox.TextProperty)
        {
            _searchCriteriaChanged = true;
            ClearSearchResults();
        }
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
                _filterRevisionGridBySpaceSeparatedRefs?.Invoke(string.Join(" ", GetSelectedNodes().OfType<IGitRefActions>().Select(node => node.FullPath)));
                break;
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

    private void TxtBranchCriterion_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        OnBtnSearchClicked(this, EventArgs.Empty);
        e.Handled = true;
    }

    private void ReorderTree(Tree tree, bool up)
    {
        Tree[] visibleTrees = [.. _rootNodes.Where(item => item.IsEnabled).OrderBy(item => item.PositionIndex)];
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

    public override bool ProcessHotkey(WinFormsShims.Keys keyData)
    {
        if (_txtBranchCriterion.IsKeyboardFocusWithin && GitExtensionsControl.IsTextEditKey(keyData))
        {
            return false;
        }

        return base.ProcessHotkey(keyData);
    }

    protected override bool ExecuteCommand(int cmd)
    {
        TreeViewItem? selectedItem = treeMain.SelectedItem as TreeViewItem;
        switch ((Command)cmd)
        {
            case Command.Delete: Node.OnNode<Node>(selectedItem, node => node.OnDelete()); return true;
            case Command.Rename: Node.OnNode<Node>(selectedItem, node => node.OnRename()); return true;
            case Command.Search: DoSearch(); return true;
            case Command.MultiSelect:
            case Command.MultiSelectWithChildren:
                if (selectedItem?.Tag is not NodeBase node)
                {
                    return false;
                }

                node.Select(!node.IsSelected, includingDescendants: (Command)cmd == Command.MultiSelectWithChildren);
                return true;
            default: return base.ExecuteCommand(cmd);
        }
    }

    private IEnumerable<NodeBase> GetSelectedNodes()
    {
        IEnumerable<TreeViewItem> items = treeMain.SelectedItems?.OfType<TreeViewItem>()
            ?? (treeMain.SelectedItem is TreeViewItem selected ? [selected] : []);
        return items.Select(item => item.Tag).OfType<NodeBase>();
    }

    private void SelectNode(NodeBase node, bool multiple, bool includingDescendants)
    {
        if (!multiple)
        {
            treeMain.SelectedItems?.Clear();
        }

        node.Select(select: true, includingDescendants);
        treeMain.SelectedItem = node.TreeViewNode;
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    private enum RepoAction
    {
        Copy,
        Filter,
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

    internal readonly struct TestAccessor(RepoObjectsTree control)
    {
        internal TreeView Tree => control.treeMain;

        internal Avalonia.Controls.ContextMenu ContextMenu => control.menuMain;

        internal TextBox SearchBox => control._txtBranchCriterion;

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
            => control.GetActionMenuItem(action);

        internal Nodes GetNodes(TreeViewItem item)
            => ((NodeBase)item.Tag!).Nodes;

        internal bool UpdateContextMenu()
        {
            System.ComponentModel.CancelEventArgs eventArgs = new();
            control.contextMenu_Opening(control.menuMain, eventArgs);
            return !eventArgs.Cancel;
        }

        internal void Search()
            => control.DoSearch();

        internal void SetSubmodules(SubmoduleInfoResult result)
            => control._submoduleTree.Load(result);

        internal void SetWorktrees(IReadOnlyList<GitWorktree> worktrees, string currentWorkingDirectory)
            => control._worktreeTree.Load(worktrees, currentWorkingDirectory);
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
