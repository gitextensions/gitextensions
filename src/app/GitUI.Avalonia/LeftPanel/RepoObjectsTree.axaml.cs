using System.ComponentModel;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
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
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.LeftPanel.ContextMenu;
using GitUI.LeftPanel.Interfaces;
using GitUI.Properties;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;

using ResourceManager;
using Keys = Avalonia.Input.KeyModifiers;
using TreeNode = Avalonia.Controls.TreeViewItem;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.LeftPanel;

public sealed partial class RepoObjectsTree : GitModuleControl
{
    public const string HotkeySettingsName = "LeftPanel";
    private readonly Dictionary<RepoAction, MenuItem> _actionItems = [];
    private readonly CancellationTokenSequence _selectionCancellationTokenSequence = new();
    private readonly TranslationString _searchTooltip = new("Search");

    private readonly NativeTreeViewDoubleClickDecorator _doubleClickDecorator;
    private readonly NativeTreeViewExplorerNavigationDecorator _explorerNavigationDecorator;
    private readonly NativeTreeScrollAdapter _scrollAdapter;

    private readonly List<Tree> _rootNodes = [];
    private readonly SearchControl<string> _txtBranchCriterion;
    private IGitUICommandsSource? _commandsSource;
    private LocalBranchTree _branchesTree = null!;
    private RemoteBranchTree _remotesTree = null!;
    private IReadOnlyList<IGitRef> _currentRefs = [];
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
    private int _selectionUpdateDepth;
    private KeyModifiers _selectionModifiers;

    internal IGitUICommandsSource? CommandsSource => _commandsSource;

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

    internal KeyModifiers SelectionModifiers => _selectionModifiers;
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
        => UpdateNodes(() =>
    {
        bool moduleChanged = !string.IsNullOrEmpty(_currentWorkingDirectory)
            && !string.IsNullOrEmpty(currentWorkingDirectory)
            && !IsSameWorkingDirectory(_currentWorkingDirectory, currentWorkingDirectory);
        if (moduleChanged)
        {
            PrepareForModuleChange();
        }

        _currentStashes = stashes;
        _currentRefs = refs;
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
        HashSet<string> selectedNodes = [.. _rootNodes.SelectMany(tree => tree.GetSelectedNodes()).Select(GetNodeIdentity)];
        string? highlightedNode = SelectedNode is { } caret ? GetNodeIdentity(caret) : null;
        bool restoreState = _rootNodes.Count > 0 && !moduleChanged;

        ClearSearchResults();
        foreach (Tree tree in _rootNodes.Where(tree => !ReferenceEquals(tree, _submoduleTree) && !ReferenceEquals(tree, _worktreeTree)))
        {
            tree.Dispose();
        }

        _rootNodes.Clear();

        CreateBranches();
        CreateRemotes();
        CreateWorktrees();
        CreateTags();
        CreateSubmodules();
        CreateStashes();

        RefreshRevisionsLoaded();
        if (restoreState)
        {
            NodeBase[] nodes = [.. _rootNodes.SelectMany(tree => tree.DescendantsAndSelf())];
            foreach (NodeBase node in nodes)
            {
                node.TreeViewNode.IsExpanded = expandedNodes.Contains(GetNodeIdentity(node));
            }

            foreach (NodeBase node in nodes.Where(node => selectedNodes.Contains(GetNodeIdentity(node))))
            {
                node.Select(true);
            }

            treeMain.SelectedItem = nodes.FirstOrDefault(node => GetNodeIdentity(node) == highlightedNode)?.TreeViewNode;
        }

        if (treeMain.SelectedItem is null
            && _branchesTree.DepthEnumerator<LocalBranchNode>().FirstOrDefault(node => node.IsCurrent) is { } current)
        {
            treeMain.SelectedItem = current.TreeViewNode;
        }
    });

    internal void UpdateNodes(Action update)
    {
        // WinForms suppresses node selection callbacks with Tree.IgnoreSelectionChangedEvent.
        // Avalonia raises them on the owning TreeView, including removal during a nested reload.
        _selectionUpdateDepth++;
        try
        {
            update();
        }
        finally
        {
            _selectionUpdateDepth--;
        }
    }

    public RepoObjectsTree()
    {
        InitializeComponent();
        _scrollAdapter = new NativeTreeScrollAdapter(treeMain);
        _txtBranchCriterion = CreateSearchBox();
        Grid.SetColumn(_txtBranchCriterion, 0);
        branchSearchPanel.Children.Add(_txtBranchCriterion);
        tsbCollapseAll.Icon = Images.CollapseAll.AdaptLightness();
        _submoduleTree = new SubmoduleTree(this);
        _worktreeTree = new WorktreeTree(this);
        HotkeysEnabled = true;
        RegisterContextActions();
        treeMain.AddHandler(InputElement.KeyDownEvent, OnTreeKeyDown, RoutingStrategies.Tunnel, handledEventsToo: true);

        _doubleClickDecorator = new NativeTreeViewDoubleClickDecorator(treeMain);
        _doubleClickDecorator.BeforeDoubleClickExpandCollapse += BeforeDoubleClickExpandCollapse;
        _explorerNavigationDecorator = new NativeTreeViewExplorerNavigationDecorator(treeMain);
        _explorerNavigationDecorator.AfterSelect += OnNodeSelected;

        // Avalonia's Ctrl-click toggles its caret even in single-selection mode. Native
        // TreeView keeps that caret independent of the source's underlined logical flags.
        treeMain.AddHandler(InputElement.PointerPressedEvent, OnNodeClick, RoutingStrategies.Tunnel, handledEventsToo: true);
        treeMain.AddHandler(InputElement.DoubleTappedEvent, OnNodeDoubleClick, RoutingStrategies.Bubble, handledEventsToo: true);
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

        return;

        SearchControl<string> CreateSearchBox()
        {
            SearchControl<string> search = new(SearchForBranch, onSizeChanged: size => { })
            {
                Name = "txtBranchCritierion",
                Height = 23,
                Margin = default,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Background = Brushes.Transparent
            };
            KeyboardNavigation.SetTabIndex(search, 1);
            search.OnTextEntered += () =>
            {
                OnBranchCriterionChanged(this, EventArgs.Empty);
                OnBtnSearchClicked(this, EventArgs.Empty);
            };
            search.TextChanged += OnBranchCriterionChanged;
            search.KeyDown += TxtBranchCriterion_KeyDown;

            search.SearchBoxBorderStyle = WinFormsShims.BorderStyle.FixedSingle;
            search.SearchBoxBorderDefaultColor = System.Drawing.Color.LightGray.AdaptBackColor();
            search.SearchBoxBorderHoveredColor = System.Drawing.SystemColors.Highlight;
            search.SearchBoxBorderFocusedColor = System.Drawing.SystemColors.HotTrack;

            return search;

            IEnumerable<string> SearchForBranch(string arg)
                => _rootNodes
                    .SelectMany(tree => tree.DescendantsAndSelf())
                    .Select(node => node.SearchText)
                    .Where(path => path.Contains(arg, StringComparison.OrdinalIgnoreCase));
        }
    }

    private void BeforeDoubleClickExpandCollapse(object? sender, CancelEventArgs e)
    {
        // If node is an inner node, and overrides OnDoubleClick, then disable expand/collapse
        TreeViewItem? item = treeMain.SelectedItem as TreeViewItem;
        if (item?.Tag is Node { HasChildren: true } node
            && IsOverride(node.GetType().GetMethod(nameof(Node.OnDoubleClick), BindingFlags.Instance | BindingFlags.NonPublic)))
        {
            e.Cancel = true;
        }

        return;

        static bool IsOverride(MethodInfo? method)
            => method is not null && method.GetBaseDefinition().DeclaringType != method.DeclaringType;
    }

    private void PrepareForModuleChange()
    {
        _selectionCancellationTokenSequence.CancelCurrent();
        ClearSearchResults();
        if (treeMain.SelectedItems is not null)
        {
            treeMain.SelectedItems.Clear();
        }
        else
        {
            treeMain.SelectedItem = null;
        }

        foreach (Tree tree in _rootNodes)
        {
            foreach (NodeBase node in tree.GetSelectedNodes().ToArray())
            {
                node.Select(false);
            }

            tree.OnModuleChanged();
        }
    }

    private static bool IsSameWorkingDirectory(string first, string second)
    {
        string firstNormalized = first.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string secondNormalized = second.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return string.Equals(firstNormalized, secondNormalized, comparison);
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

        // This lazily sets the command source, invoking OnUICommandsSourceSet, which is required for setting up
        // notifications for each Tree.
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

    internal TreeSelectionState CaptureSelectionState(NodeBase root)
    {
        NodeBase[] descendants = [.. root.DescendantsAndSelf()];
        HashSet<string> identities =
        [
            .. descendants.Where(node => node.IsSelected)
                .Select(GetNodeIdentity),
        ];
        string? highlightedNode = SelectedNode is { } caret && descendants.Any(node => ReferenceEquals(node, caret))
            ? GetNodeIdentity(caret) : null;
        return new TreeSelectionState(identities, highlightedNode);
    }

    internal void RestoreSelectionState(NodeBase root, TreeSelectionState state)
    {
        foreach (NodeBase node in root.DescendantsAndSelf())
        {
            string identity = GetNodeIdentity(node);
            if (state.SelectedNodes.Contains(identity))
            {
                node.Select(true);
            }

            if (identity == state.HighlightedNode)
            {
                treeMain.SelectedItem = node is not BaseRevisionNode || node.Visible ? node.TreeViewNode : null;
            }
        }
    }

    internal static Control CreateHeader(string caption, IImage icon, bool isBold = false, bool isItalic = false)
        => new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,

            // The native image/text slots leave three DIPs before the text rectangle.
            Spacing = 3,
            Children =
            {
                new Image
                {
                    Width = 16,
                    Height = 16,
                    Stretch = Stretch.Uniform,
                    Source = icon,
                },
                new NativeTreeTextBlock
                {
                    UsesAmbientFont = !isBold && !isItalic,
                    FontFamily = new FontFamily(AppSettings.Font.Name),
                    FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(AppSettings.Font.Size),
                    FontWeight = isBold ? FontWeight.Bold : FontWeight.Normal,
                    FontStyle = isItalic ? FontStyle.Italic : FontStyle.Normal,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Text = caption,
                },
            },
        };

    internal void PrepareTreeViewItem(NodeBase node)
    {
        // Avalonia input events bubble through treeMain, so one routed handler covers recycled
        // item templates without attaching a second handler to every model node.
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
        foreach (NodeBase node in _rootNodes.SelectMany(tree => tree.DescendantsAndSelf()))
        {
            node.TreeViewNode.Classes.Set("leaf-node", !node.HasChildren);
        }

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
    {
        if (_rootNodes.Count == 0)
        {
            return;
        }

        // Some refs may not be visible
        _branchesTree.UpdateVisibility();
        _remotesTree.UpdateVisibility();
        _tagTree.UpdateVisibility();
        _stashTree.UpdateVisibility();
        ApplyRoots();
    }

    /// <summary>
    /// Refresh after resorting.
    /// </summary>
    /// <param name="getRefs">Git references</param>
    public void ResortRefs(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
    {
        _branchesTree.RefreshInternal(getRefs);
        _remotesTree.RefreshInternal(getRefs);
        _tagTree.RefreshInternal(getRefs);
        RefreshRevisionsLoaded();
    }

    public void ReloadHotkeys()
    {
        LoadHotkeys(HotkeySettingsName);
    }

    public void SelectionChanged(IReadOnlyList<GitRevision> selectedRevisions)
    {
        // If we arrived here through the chain of events after selecting a node in the tree,
        // and the selected revision is the one we have selected - do nothing.
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

        // Local or remote branch nodes or tag nodes
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
        EnsureVerticallyVisible(node.TreeViewNode);
        treeMain.SelectedItem = node.TreeViewNode;
    }

    protected override void OnRuntimeLoad()
    {
        base.OnRuntimeLoad();
        ReloadHotkeys();
    }

    protected override void OnUICommandsSourceSet(IGitUICommandsSource source)
    {
        _selectionCancellationTokenSequence.CancelCurrent();

        base.OnUICommandsSourceSet(source);
        _commandsSource = source;
        _submoduleTree.SetUICommandsSource(source);
        _worktreeTree.SetUICommandsSource(source);
        foreach (Tree tree in _rootNodes)
        {
            tree.SetUICommandsSource(source);
        }

        source.UICommandsChanged += (_, _) => UpdateNodes(PrepareForModuleChange);
        _submoduleStatusProvider = source.UICommands.GetService(typeof(ISubmoduleStatusProvider)) as ISubmoduleStatusProvider;
        _submoduleTree.Attach(_submoduleStatusProvider);
        if (source.UICommands.GetService(typeof(IHotkeySettingsLoader)) is not null)
        {
            OnRuntimeLoad();
        }
    }

    private static void AddTreeNodeToSearchResult(ICollection<TreeViewItem> ret, TreeViewItem node)
    {
        node.Classes.Remove("repo-search-cleared");
        node.Classes.Add("repo-search-result");
        node.Classes.Add("repo-search-foreground");
        ret.Add(node);
    }

    private void CreateBranches()
    {
        IGitRef[] branches = [.. _currentRefs.Where(gitRef => gitRef.IsHead && !gitRef.IsTag)];
        _branchesTree = new LocalBranchTree(this, branches, _currentBranch, _aheadBehindDataProvider, _refsSource, _revisionGridInfo);
        AddTree(_branchesTree);
    }

    private void CreateRemotes()
    {
        IGitRef[] remotes = [.. _currentRefs.Where(gitRef => gitRef.IsRemote)];
        _remotesTree = new RemoteBranchTree(
            this,
            remotes,
            _enabledRemotes,
            _disabledRemotes,
            _remotesManager,
            _aheadBehindDataProvider,
            _refsSource);
        AddTree(_remotesTree);
    }

    private void CreateWorktrees()
    {
        _worktreeTree.Load(_currentWorktrees, _currentWorkingDirectory);
        AddTree(_worktreeTree);
    }

    private void CreateTags()
    {
        IGitRef[] tags = [.. _currentRefs.Where(gitRef => gitRef.IsTag && !gitRef.IsDereference)];
        _tagTree = new TagTree(this, tags, _refsSource);
        AddTree(_tagTree);
    }

    private void CreateSubmodules()
    {
        AddTree(_submoduleTree);
    }

    private void CreateStashes()
    {
        _stashTree = new StashTree(this, _currentStashes, _refsSource);
        if (_includeStashes)
        {
            AddTree(_stashTree);
        }
    }

    private void AddTree(Tree tree)
    {
        // Add Tree's node in position index order. Because TreeNodeCollections cannot be sorted,
        // we create a list from it, sort it, then clear and re-add the nodes back to the collection.
        if (!_rootNodes.Contains(tree))
        {
            _rootNodes.Add(tree);
        }

        ApplyRoots();
    }

    internal void FocusTree()
        => treeMain.Focus();

    private void RemoveTree(Tree tree)
    {
        _rootNodes.Remove(tree);
        ApplyRoots();
    }

    private void DoSearch()
    {
        _txtBranchCriterion.CloseDropdown();
        if (_searchCriteriaChanged && _searchResult?.Count is > 0)
        {
            _searchCriteriaChanged = false;
            ClearSearchResults();
            if (string.IsNullOrWhiteSpace(_txtBranchCriterion.Text))
            {
                _txtBranchCriterion.FocusSearchBox();
                return;
            }
        }

        string criterion = _txtBranchCriterion.Text ?? string.Empty;
        if ((_searchResult is null || _searchResult.Count == 0) && !string.IsNullOrWhiteSpace(criterion))
        {
            _searchResult = [];
            Queue<TreeViewItem> queue = new(treeMain.Items.Cast<TreeViewItem>());
            while (queue.TryDequeue(out TreeViewItem? item))
            {
                string text = item.Tag is BaseRevisionNode revisionNode
                    ? revisionNode.FullPath
                    : (item.Header as StackPanel)?.Children.OfType<TextBlock>().FirstOrDefault()?.Text ?? string.Empty;
                if (text.Contains(criterion, StringComparison.InvariantCultureIgnoreCase))
                {
                    AddTreeNodeToSearchResult(_searchResult, item);
                }

                foreach (TreeViewItem child in item.Items.Cast<TreeViewItem>())
                {
                    queue.Enqueue(child);
                }
            }
        }

        if (_searchResult?.Count is not > 0)
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
        EnsureVerticallyVisible(next);
    }

    private void ClearSearchResults()
    {
        if (_searchResult is not null)
        {
            foreach (TreeViewItem item in _searchResult)
            {
                item.Classes.Remove("repo-search-result");
                item.Classes.Add("repo-search-cleared");
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

    private void OnBranchCriterionChanged(object? sender, EventArgs e)
    {
        _searchCriteriaChanged = true;
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
            case RepoAction.MoveUp: ReorderTreeNode(SelectedNode!.TreeViewNode, up: true); break;
            case RepoAction.MoveDown: ReorderTreeNode(SelectedNode!.TreeViewNode, up: false); break;
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

    internal void EnsureHighlightedNodeVisible()
    {
        if (treeMain.SelectedItem is TreeViewItem caret)
        {
            EnsureVerticallyVisible(caret);
        }
        else if (treeMain.Items.OfType<TreeViewItem>().FirstOrDefault() is { } first)
        {
            EnsureVerticallyVisible(first);
        }
    }

    internal void EnsureVerticallyVisible(TreeViewItem item)
    {
        if (item.Tag is NodeBase node)
        {
            for (NodeBase? parent = node.Parent; parent is not null; parent = parent.Parent)
            {
                parent.TreeViewNode.IsExpanded = true;
            }
        }

        _scrollAdapter.EnsureVerticallyVisible(item);
    }

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

    private void OnNodeSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (_selectionUpdateDepth != 0)
        {
            return;
        }

        // prevent selection of refs hidden from the revision grid by filtering
        if (SelectedNode is Node selectedNode
            && SelectedNode is not BaseRevisionNode { Visible: false })
        {
            selectedNode.OnSelected();
        }

        NodeSelectionChanged?.Invoke(this, EventArgs.Empty);
        Dispatcher.UIThread.Post(() => _selectionModifiers = KeyModifiers.None);
    }

    private IEnumerable<NodeBase> GetSelectedNodes()
        => _rootNodes.Where(tree => tree.IsEnabled).SelectMany(tree => tree.GetSelectedNodes());

    private void OnNodeClick(object? sender, PointerPressedEventArgs e)
    {
        _selectionModifiers = e.KeyModifiers;
        TreeViewItem? item = GetTreeViewItem(e.Source);
        NodeBase? node = item?.Tag as NodeBase;
        bool rightButton = e.GetCurrentPoint(treeMain).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed;

        if (node is null || IsExpansionToggle(e.Source))
        {
            return;
        }

        item!.Focus(NavigationMethod.Pointer, e.KeyModifiers);
        treeMain.SelectedItem = item;
        e.Handled = true;
        if (rightButton && node.IsSelected)
        {
            return; // don't undo multi-selection on opening context menu, even without Ctrl
        }

        bool multiple = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
        SelectNode(node, multiple, includingDescendants: e.KeyModifiers.HasFlag(KeyModifiers.Shift));
        if (node is Node clickable)
        {
            clickable.OnClick();
        }
    }

    private void SelectNode(NodeBase node, bool multiple, bool includingDescendants)
    {
        if (multiple)
        {
            node.Select(!node.IsSelected, includingDescendants: includingDescendants);
        }
        else
        {
            // deselect all selected nodes
            foreach (NodeBase selected in GetSelectedNodes().ToArray())
            {
                selected.Select(false);
            }

            node.Select(true); // and only select the clicked one
        }
    }

    private void OnNodeDoubleClick(object? sender, TappedEventArgs e)
    {
        // Don't consider-double clicking on the PlusMinus as a double-click event
        // for nodes in tree. This prevents opening inner submodules, for example,
        // when quickly collapsing/expanding them.
        if (IsExpansionToggle(e.Source))
        {
            return;
        }

        // Don't use e.Node, when folding/unfolding a node,
        // e.Node won't be the one you double clicked, but a child node instead
        Node.OnNode<Node>(treeMain.SelectedItem as TreeViewItem, node => node.OnDoubleClick());
    }

    private void OnTreeKeyDown(object? sender, KeyEventArgs e)
        => _selectionModifiers = e.KeyModifiers;

    private static TreeViewItem? GetTreeViewItem(object? source)
        => source as TreeViewItem ?? (source as Visual)?.FindAncestorOfType<TreeViewItem>();

    private static bool IsExpansionToggle(object? source)
        => source is ToggleButton || (source as Visual)?.FindAncestorOfType<ToggleButton>() is not null;

    internal TestAccessor GetTestAccessor()
        => new(this);

    private enum RepoAction
    {
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

        internal IEnumerable<TreeViewItem> LogicalSelection
            => control.GetSelectedNodes().Select(node => node.TreeViewNode);

        internal Avalonia.Controls.ContextMenu ContextMenu => control.menuMain;

        internal SearchControl<string> SearchBox => control._txtBranchCriterion;

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

        internal void SetSelectionModifiers(KeyModifiers modifiers)
            => control._selectionModifiers = modifiers;

        /// <summary>Simulates a left click on the <see cref="TreeNode"/> in <see cref="TreeView"/>
        /// identified by the path of <paramref name="nodeTexts"/> for UI tests.</summary>
        /// <typeparam name="TExpected">The expected type of the selected node. The type of the returned node will be validated against this.</typeparam>
        /// <param name="nodeTexts">The path of node texts used to select a single node starting from the first tree level.</param>
        /// <param name="multiple">Whether to select multiple; simulates holding <see cref="Keys.Control"/> while clicking.</param>
        /// <param name="includingDescendants">Whether to include descendants in the multi-selection;
        /// simulates holding <see cref="Keys.Shift"/> while clicking.</param>
        /// <exception cref="ArgumentException">Thrown if either <paramref name="nodeTexts"/> don't point to an existing node
        /// or the selected node is not of type <typeparamref name="TExpected"/>.</exception>
        internal void SelectNode<TExpected>(string[] nodeTexts, bool multiple = false, bool includingDescendants = false)
            where TExpected : NodeBase
        {
            IEnumerable<TreeViewItem> nodes = control.treeMain.Items.Cast<TreeViewItem>();
            TreeViewItem? item = null;

            foreach (string text in nodeTexts)
            {
                item = nodes.SingleOrDefault(node => GetText(node) == text)
                    ?? throw new ArgumentException(
                        $"Node '{text}' not found. Available nodes on this level: "
                        + string.Join(", ", nodes.Select(GetText)),
                        nameof(nodeTexts));
                nodes = item.Items.Cast<TreeViewItem>();
            }

            if (item?.Tag?.GetType() != typeof(TExpected))
            {
                throw new ArgumentException($"The selected node is of type {item?.Tag?.GetType()} instead of the expected type {typeof(TExpected)}.", nameof(TExpected));
            }

            control.treeMain.SelectedItem = item;
            control.SelectNode((NodeBase)item.Tag, multiple, includingDescendants);

            // simulates a node click well enough for UI tests

            static string? GetText(TreeViewItem node)
                => (node.Header as StackPanel)?.Children.OfType<TextBlock>().FirstOrDefault()?.Text;
        }
    }
}

internal readonly record struct TreeSelectionState(IReadOnlySet<string> SelectedNodes, string? HighlightedNode);

/// <summary>
/// Draws the dotted hierarchy lines supplied by the native WinForms tree but absent from
/// Avalonia's Fluent TreeView template.
/// </summary>
internal sealed class TreeConnectorControl : Control
{
    private const double ChevronCenter = 10;
    private const double ChevronGapHalfHeight = 6;
    private const double ChevronGapHalfWidth = 6;
    private static readonly DashStyle DottedLine = new([1, 1], 0);

    public static readonly StyledProperty<double> IndentProperty =
        AvaloniaProperty.Register<TreeConnectorControl, double>(nameof(Indent), 19);

    public TreeConnectorControl()
    {
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
    }

    public double Indent
    {
        get => GetValue(IndentProperty);
        set => SetValue(IndentProperty, value);
    }

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
        int index = siblings.IndexOf(item);
        if (index < 0 && item.DataContext is not null)
        {
            // Data-bound TreeViews store the model in Items and generate TreeViewItem
            // containers. Native sibling lines follow the model order, not realization order.
            index = siblings.IndexOf(item.DataContext);
        }

        return (Math.Max(index, 0), siblings.Count);
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
