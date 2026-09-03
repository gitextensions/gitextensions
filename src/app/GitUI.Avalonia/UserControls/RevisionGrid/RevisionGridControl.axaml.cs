using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Utils;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.Avatars;
using GitUI.BuildServerIntegration;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Compat;
using GitUI.HelperDialogs;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUI.UserControls.RevisionGrid.Graph.Rendering;
using GitUIPluginInterfaces;
using Microsoft;
using ResourceManager;
using ResourceManager.Hotkey;
using DrawingPoint = System.Drawing.Point;
using Keys = GitExtensions.Shims.WinForms.Keys;
using ToolStripDropDownItem = GitUI.Compat.WinFormsControls.ToolStripDropDownItem;
using ToolStripMenuItem = GitUI.Compat.WinFormsControls.ToolStripMenuItem;
using ToolStripSeparator = GitUI.Compat.WinFormsControls.ToolStripSeparator;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI;

public enum RevisionGraphDrawStyle
{
    Normal,
    DrawNonRelativesGray,
    HighlightSelected
}

public enum SortDirection
{
    Ascending,
    Descending
}

public partial class RevisionGridControl : GitModuleControl, ICheckRefs, IRevisionGridInfo, IRevisionGridFilter, IRevisionGridUpdate
{
    /// <summary>Occurs when the selected revision is double-clicked.</summary>
    // Mnemonics:
    // A manipulateCommitToolStripMenuItem
    // B openBuildReportToolStripMenuItem
    // C Copy to clipboard
    // D deleteBranchToolStripMenuItem, deleteTagToolStripMenuItem, dropStashToolStripMenuItem
    // E renameBranchToolStripMenuItem
    // F
    // G createTagToolStripMenuItem
    // H tsmiPushBranch
    // I archiveRevisionToolStripMenuItem
    // J
    // K checkoutBranchToolStripMenuItem
    // L tsmiSelectInLeftPanel
    // M mergeBranchToolStripMenuItem
    // N navigateToolStripMenuItem
    // O resetAnotherBranchToHereToolStripMenuItem, tsmiOtherActions
    // P compareToolStripMenuItem
    // Q
    // R rebaseOnToolStripMenuItem
    // S runScriptToolStripMenuItem, popStashToolStripMenuItem
    // T checkoutRevisionToolStripMenuItem
    // U resetCurrentBranchToHereToolStripMenuItem
    // V revertCommitToolStripMenuItem
    // W openPullRequestPageStripMenuItem
    // X createNewBranchToolStripMenuItem
    // Y cherryPickCommitToolStripMenuItem, applyStashToolStripMenuItem
    // Z
    public event EventHandler<DoubleClickRevisionEventArgs>? DoubleClickRevision;

    /// <inheritdoc />
    public event EventHandler<FilterChangedEventArgs>? FilterChanged;

    /// <summary>
    ///  Occurs when the selected revision changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    public event EventHandler? ArtificialChanged;

    /// <summary>
    ///  Occurs whenever the revision graph has started loading the data.
    /// </summary>
    public event EventHandler<RevisionLoadEventArgs>? RevisionsLoading;

    /// <summary>
    ///  Occurs whenever the revision graph has been populated with the data.
    /// </summary>
    public event EventHandler<RevisionLoadEventArgs>? RevisionsLoaded;

    private const int RowSpacing = 9;

    public event EventHandler? ToggledBetweenArtificialAndHeadCommits;
    public static readonly string HotkeySettingsName = "RevisionGrid";
    private readonly List<ColumnProvider> _columnProviders = [];
    private readonly Avalonia.Collections.AvaloniaList<GitRevision> _revisions = [];
    private readonly TranslationString _droppingFilesBlocked = new("For you own protection dropping more than 10 patch files at once is blocked!");
    private readonly TranslationString _noRevisionFoundError = new("No revision found.");
    private readonly TranslationString _baseForCompareNotSelectedError = new("Base commit for compare is not selected.");
    private readonly TranslationString _strLoading = new("Loading");
    private readonly TranslationString _rebaseConfirmTitle = new("Rebase Confirmation");
    private readonly TranslationString _rebaseBranch = new("Rebase branch.");
    private readonly TranslationString _rebaseBranchInteractive = new("Rebase branch interactively.");
    private readonly TranslationString _areYouSureRebase = new("Are you sure you want to rebase? This action will rewrite commit history.");
    private readonly TranslationString _noMergeBaseCommit = new("There is no merge base for the selected revisions.");
    private readonly TranslationString _invalidDiffContainsFilter = new("The diff contains filter is invalid.");
    private readonly RevisionGraph _revisionGraph = new();
    private readonly FilterInfo _filterInfo = new();
    private readonly NavigationHistory _navigationHistory = new();
    private readonly RevisionGridToolTipProvider _toolTipProvider;
    private readonly QuickSearchProvider _quickSearchProvider;
    private readonly ParentChildNavigationHistory _parentChildNavigationHistory;
    private readonly AuthorRevisionHighlighting _authorHighlighting = new();
    private readonly Lazy<IndexWatcher> _indexWatcher;
    private readonly BuildServerWatcher _buildServerWatcher;
    private readonly RevisionGraphColumnProvider _revisionGraphColumnProvider;
    private readonly MessageColumnProvider _messageColumnProvider;
    private readonly RevisionGridColumn? _maximizedColumn;
    private RevisionGridColumn? _lastVisibleResizableColumn;
    private readonly ArtificialCommitChangeCount _workTreeChangeCount = new();
    private readonly ArtificialCommitChangeCount _indexChangeCount = new();
    private ObjectId? _headId;
    private ObjectId _pendingSelectedObjectId;
    private bool _headHighlighted;
    private bool _focusGridWhenShown;
    private string _lastPathFilter = string.Empty;
    private string _lastRevisionFilter = "--all";
    private IGitModule? _lastModule;
    private bool _parentsAreRewritten;
    private readonly CancellationTokenSequence _customDiffToolsSequence = new();

    // Avalonia's designer constructs views before the application initializes ThreadHelper.
    private readonly TaskManager _taskManager = GitUI.Compat.DesignTimeTaskManager.Create();
    private readonly CancellationTokenSequence _refreshRevisionsSequence = new();
    private Lazy<IReadOnlyCollection<string>>? _ambiguousRefs;
    private ILookup<ObjectId, IGitRef>? _refsByObjectId;
    private int _updatingFilters;
    private GitRevision? _baseCommitToCompare;
    private string? _rebaseOnTopOf;
    private bool _isRefreshingRevisions;
    private SuperProjectInfo? _superprojectCurrentCheckout;
    private int _latestSelectedRowIndex;
    private const string _objectIdPrefix = "????";

    #region IRevisionGridInfo

    public ObjectId CurrentCheckout => _headId ?? default;

    internal Lazy<string> CurrentBranch { get; private set; } = new(() => "");

    internal FilterInfo CurrentFilter => _filterInfo;

    internal bool ShowUncommittedChangesIfPossible { get; set; } = true;

    internal bool ShowBuildServerInfo { get; set; }

    internal bool DoubleClickDoesNotOpenCommitInfo { get; set; }

    /// <summary>
    /// The last selected commit in the grid (with related CommitInfo in Browse).
    /// </summary>
    public ObjectId SelectedId
    {
        get => SelectedRevision?.ObjectId ?? _pendingSelectedObjectId;
        set => _pendingSelectedObjectId = value;
    }

    internal ObjectId FirstId { private get; set; }

    internal RevisionGridMenuCommands MenuCommands { get; }

    /// <summary>
    /// The (first) seen name for commits, for FileHistory with path filters.
    /// See BuildFilter() for limitations of commits included.
    /// The property is explicitly initialized by FileHistory.
    /// </summary>
    internal Dictionary<ObjectId, string>? FilePathByObjectId { get; set; }

    internal Action<string>? SelectInLeftPanel { get; set; } = null;

    private MenuItem ToggleBetweenArtificialAndHeadCommitsMenuItem => GetMenuItem(navigateToolStripMenuItem, "ToggleBetweenArtificialAndHeadCommits");
    private MenuItem GotoCurrentRevisionMenuItem => GetMenuItem(navigateToolStripMenuItem, "GotoCurrentRevision");
    private MenuItem GotoChildCommitMenuItem => GetMenuItem(navigateToolStripMenuItem, "GotoChildCommit");
    private MenuItem GotoParentCommitMenuItem => GetMenuItem(navigateToolStripMenuItem, "GotoParentCommit");
    private MenuItem GotoFirstParentCommitMenuItem => GetMenuItem(navigateToolStripMenuItem, "GotoFirstParentCommit");
    private MenuItem GotoLastParentCommitMenuItem => GetMenuItem(navigateToolStripMenuItem, "GotoLastParentCommit");
    private MenuItem ShowAllBranchesMenuItem => GetMenuItem(viewToolStripMenuItem, "ShowAllBranches");
    private MenuItem ShowCurrentBranchOnlyMenuItem => GetMenuItem(viewToolStripMenuItem, "ShowCurrentBranchOnly");
    private MenuItem ShowFilteredBranchesMenuItem => GetMenuItem(viewToolStripMenuItem, "ShowFilteredBranches");
    private MenuItem ShowReflogReferencesMenuItem => GetMenuItem(viewToolStripMenuItem, "ShowReflogReferences");
    private MenuItem FilterMenuItem => GetMenuItem(viewToolStripMenuItem, "filterToolStripMenuItem");
    private MenuItem DrawNonRelativesGrayMenuItem => GetMenuItem(viewToolStripMenuItem, "drawNonrelativesGrayToolStripMenuItem");
    private MenuItem HighlightSelectedBranchMenuItem => GetMenuItem(viewToolStripMenuItem, "HighlightSelectedBranch");
    private MenuItem ShowGitNotesMenuItem => GetMenuItem(viewToolStripMenuItem, "showGitNotesToolStripMenuItem");
    private MenuItem ShowRemoteBranchesMenuItem => GetMenuItem(viewToolStripMenuItem, "ShowRemoteBranches");
    private MenuItem ShowTagsMenuItem => GetMenuItem(viewToolStripMenuItem, "showTagsToolStripMenuItem");
    private MenuItem ShowAuthorDateMenuItem => GetMenuItem(viewToolStripMenuItem, "showAuthorDateToolStripMenuItem");
    private MenuItem ShowRelativeDateMenuItem => GetMenuItem(viewToolStripMenuItem, "showRelativeDateToolStripMenuItem");
    private MenuItem ShowRevisionGraphColumnMenuItem => GetMenuItem(viewToolStripMenuItem, "showRevisionGraphColumnToolStripMenuItem");
    private MenuItem ShowGitNotesColumnMenuItem => GetMenuItem(viewToolStripMenuItem, "showGitNotesColumnToolStripMenuItem");
    private MenuItem ShowAuthorNameColumnMenuItem => GetMenuItem(viewToolStripMenuItem, "showAuthorNameColumnToolStripMenuItem");
    private MenuItem ShowDateColumnMenuItem => GetMenuItem(viewToolStripMenuItem, "showDateColumnToolStripMenuItem");
    private MenuItem ShowIdColumnMenuItem => GetMenuItem(viewToolStripMenuItem, "showIdColumnToolStripMenuItem");

    internal MenuItem NavigateMenuItem => navigateToolStripMenuItem;

    internal MenuItem ViewMenuItem => viewToolStripMenuItem;

    internal void RefreshMainMenuState()
    {
        MenuCommands.TriggerMenuChanged();
    }

    internal void RefreshMenuShortcutKeys(IEnumerable<HotkeyCommand>? hotkeys)
    {
        SetInputGesture(ToggleBetweenArtificialAndHeadCommitsMenuItem, Command.ToggleBetweenArtificialAndHeadCommits);
        SetInputGesture(GotoCurrentRevisionMenuItem, Command.SelectCurrentRevision);
        SetInputGesture(GotoChildCommitMenuItem, Command.GoToChild);
        SetInputGesture(GotoParentCommitMenuItem, Command.GoToParent);
        SetInputGesture(GotoFirstParentCommitMenuItem, Command.GoToFirstParent);
        SetInputGesture(GotoLastParentCommitMenuItem, Command.GoToLastParent);
        SetInputGesture(ShowAllBranchesMenuItem, Command.ShowAllBranches);
        SetInputGesture(ShowCurrentBranchOnlyMenuItem, Command.ShowCurrentBranchOnly);
        SetInputGesture(ShowFilteredBranchesMenuItem, Command.ShowFilteredBranches);
        SetInputGesture(ShowReflogReferencesMenuItem, Command.ShowReflogReferences);
        SetInputGesture(FilterMenuItem, Command.RevisionFilter);
        SetInputGesture(HighlightSelectedBranchMenuItem, Command.ToggleHighlightSelectedBranch);
        SetInputGesture(ShowRemoteBranchesMenuItem, Command.ShowRemoteBranches);
        SetInputGesture(ShowTagsMenuItem, Command.ToggleShowTags);

        return;

        void SetInputGesture(MenuItem menuItem, Command command)
            => menuItem.InputGesture = KeysMapper.ToKeyGesture(
                hotkeys?.FirstOrDefault(hotkey => hotkey.CommandCode == (int)command)?.KeyData);
    }

    internal void SetAheadBehindDataProvider(IAheadBehindDataProvider? provider)
        => _messageColumnProvider.SetAheadBehindDataProvider(provider);

    public RevisionGridControl()
        : this(commitDataManager: null)
    {
    }

    /// <summary>
    ///  Gets the revision currently selected in the list, or <see langword="null"/>.
    /// </summary>
    public GitRevision? SelectedRevision => _gridView.SelectedItem as GitRevision;

    public RevisionGridControl(ICommitDataManager? commitDataManager)
    {
        InitializeComponent();

        _buildServerWatcher = new BuildServerWatcher(this, this, () => Module);
        commitDataManager ??= new CommitDataManager(() => Module);
        commitDataManager.RevisionDetailsLoaded += (_, _) => Dispatcher.UIThread.Post(RefreshRealizedRows);
        GitRevisionSummaryBuilder gitRevisionSummaryBuilder = new();
        _revisionGraphColumnProvider = new RevisionGraphColumnProvider(_revisionGraph, this, gitRevisionSummaryBuilder);
        AddColumn(_revisionGraphColumnProvider);
        _messageColumnProvider = new MessageColumnProvider(this, gitRevisionSummaryBuilder, commitDataManager);
        AddColumn(_messageColumnProvider);
        AddColumn(new NotesColumnProvider());
        AddColumn(new AvatarColumnProvider(this, AvatarService.DefaultProvider, AvatarService.CacheCleaner));
        AddColumn(new AuthorNameColumnProvider(_authorHighlighting));
        AddColumn(new DateColumnProvider());
        AddColumn(new CommitIdColumnProvider());
        AddColumn(_buildServerWatcher.ColumnProvider);
        _maximizedColumn = _columnProviders
            .Select(provider => provider.Column)
            .FirstOrDefault(column => column.Resizable && column.Width.IsStar);
        ApplyColumnSettings();

        _toolTipProvider = new RevisionGridToolTipProvider(this);
        _toolTipProvider.ShowRevisionGridTooltips = AppSettings.ShowRevisionGridTooltips.Value;
        _quickSearchProvider = new QuickSearchProvider(_gridView, pnlRevisionGrid, () => Module.WorkingDir);
        _gridView.ItemsSource = _revisions;

        MenuCommands = new RevisionGridMenuCommands(this);

        // fill View context menu from MenuCommands
        FillMenuFromMenuCommands(MenuCommands.ViewMenuCommands, viewToolStripMenuItem);

        // fill Navigate context menu from MenuCommands
        FillMenuFromMenuCommands(MenuCommands.NavigateMenuCommands, navigateToolStripMenuItem);

        // Apply checkboxes changes also to FormBrowse main menu
        MenuCommands.TriggerMenuChanged();

        // Parent-child navigation can expect that SetSelectedRevision is always successful since it always uses first-parents
        _parentChildNavigationHistory = new ParentChildNavigationHistory(commitId => SetSelectedRevision(commitId));
        _indexWatcher = new Lazy<IndexWatcher>(() => new IndexWatcher(UICommandsSource));

        _gridView.ItemTemplate = new FuncDataTemplate<GitRevision>((_, _) => new RevisionRowControl(this), supportsRecycling: true);
        _gridView.SelectionChanged += OnGridViewSelectionChanged;
        _gridView.GotFocus += (_, _) => RefreshRealizedRows();
        _gridView.LostFocus += (_, _) => RefreshRealizedRows();
        _gridView.AttachedToVisualTree += (_, _) =>
            Dispatcher.UIThread.Post(FocusRevisionGridWhenShown, DispatcherPriority.Loaded);
        _gridView.KeyDown += OnGridViewKeyDown;
        _gridView.TextInput += (_, e) => _quickSearchProvider.OnKeyPress(e);
        _gridView.DoubleTapped += (_, _) =>
        {
            DoubleClickRevision?.Invoke(this, new DoubleClickRevisionEventArgs(GetSelectedRevisionOrDefault()));
            if (!DoubleClickDoesNotOpenCommitInfo)
            {
                ViewSelectedRevisions();
            }
        };
        _gridView.PointerPressed += _gridView_PointerPressed;

        // Allow to drop patch file on revision grid
        DragDrop.SetAllowDrop(_gridView, true);
        DragDrop.AddDragEnterHandler(_gridView, OnGridViewDragEnter);
        DragDrop.AddDragOverHandler(_gridView, OnGridViewDragEnter);
        DragDrop.AddDropHandler(_gridView, OnGridViewDragDrop);
        _gridView.LayoutUpdated += (_, _) => UpdateVisibleGraphColumnWidth();
        mainContextMenu.Opening += ContextMenuOpening;
        mainContextMenu.Opened += (_, _) =>
        {
            _gridView.Classes.Set("context-menu-open", true);
            RefreshRealizedRows();
        };
        mainContextMenu.Closed += (_, _) =>
        {
            _gridView.Classes.Set("context-menu-open", false);
            ClearRefHighlight();
            RefreshRealizedRows();
        };
        copyToClipboardToolStripMenuItem.SetRevisionFunc(GetSelectedRevisions);
        applyStashToolStripMenuItem.Click += ApplyStashToolStripMenuItemClick;
        popStashToolStripMenuItem.Click += PopStashToolStripMenuItemClick;
        dropStashToolStripMenuItem.Click += DropStashToolStripMenuItemClick;
        rebaseOnToolStripMenuItem.SubmenuOpened += RebaseOnToolStripMenuItem_DropDownOpening;
        rebaseToolStripMenuItem.Click += ToolStripItemClickRebaseBranch;
        rebaseInteractivelyToolStripMenuItem.Click += OnRebaseInteractivelyClicked;
        rebaseWithAdvOptionsToolStripMenuItem.Click += OnRebaseWithAdvOptionsClicked;
        resetCurrentBranchToHereToolStripMenuItem.Click += ResetCurrentBranchToHereToolStripMenuItemClick;
        resetAnotherBranchToHereToolStripMenuItem.Click += ResetAnotherBranchToHereToolStripMenuItemClick;
        resetChangesToolStripMenuItem.Click += ResetChangesToolStripMenuItemClick;
        commitToolStripMenuItem.Click += CommitToolStripMenuItemClick;
        createNewBranchToolStripMenuItem.Click += CreateNewBranchToolStripMenuItemClick;
        createTagToolStripMenuItem.Click += CreateTagToolStripMenuItemClick;
        checkoutRevisionToolStripMenuItem.Click += CheckoutRevisionToolStripMenuItemClick;
        revertCommitToolStripMenuItem.Click += RevertCommitToolStripMenuItemClick;
        cherryPickCommitToolStripMenuItem.Click += CherryPickCommitToolStripMenuItemClick;
        archiveRevisionToolStripMenuItem.Click += ArchiveRevisionToolStripMenuItemClick;
        markRevisionAsBadToolStripMenuItem.Click += MarkRevisionAsBadToolStripMenuItemClick;
        markRevisionAsGoodToolStripMenuItem.Click += MarkRevisionAsGoodToolStripMenuItemClick;
        bisectSkipRevisionToolStripMenuItem.Click += BisectSkipRevisionToolStripMenuItemClick;
        stopBisectToolStripMenuItem.Click += StopBisectToolStripMenuItemClick;
        tsmiSelectInLeftPanel.Click += SelectInLeftPanel_Click;
        fixupCommitToolStripMenuItem.Click += FixupCommitToolStripMenuItemClick;
        squashCommitToolStripMenuItem.Click += SquashCommitToolStripMenuItemClick;
        amendCommitToolStripMenuItem.Click += AmendCommitToolStripMenuItemClick;
        editCommitToolStripMenuItem.Click += editCommitToolStripMenuItem_Click;
        rewordCommitToolStripMenuItem.Click += rewordCommitToolStripMenuItem_Click;
        openCommitsWithDiffToolMenuItem.Click += diffSelectedCommitsMenuItem_Click;
        compareToBranchToolStripMenuItem.Click += CompareToBranchToolStripMenuItem_Click;
        compareWithCurrentBranchToolStripMenuItem.Click += CompareWithCurrentBranchToolStripMenuItem_Click;
        selectAsBaseToolStripMenuItem.Click += selectAsBaseToolStripMenuItem_Click;
        compareToBaseToolStripMenuItem.Click += compareToBaseToolStripMenuItem_Click;
        compareToWorkingDirectoryMenuItem.Click += compareToWorkingDirectoryMenuItem_Click;
        compareSelectedCommitsMenuItem.Click += compareSelectedCommitsMenuItem_Click;
        getHelpOnHowToUseTheseFeaturesToolStripMenuItem.Click += getHelpOnHowToUseTheseFeaturesToolStripMenuItem_Click;
        openBuildReportToolStripMenuItem.Click += openBuildReportToolStripMenuItem_Click;
        openPullRequestPageStripMenuItem.Click += openPullRequestPageStripMenuItem_Click;
        HotkeysEnabled = true;
        UICommandsSourceSet += (_, _) =>
        {
            OnRuntimeLoad();
        };
        UpdateContextMenuItems();
        DetachedFromVisualTree += (_, _) =>
        {
            _revisionGraphColumnProvider.Dispose();
            _buildServerWatcher.Dispose();
            if (_indexWatcher.IsValueCreated)
            {
                _indexWatcher.Value.Dispose();
            }
        };

        InitializeComplete();
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        MenuCommands.AddTranslationItems(translation);
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        MenuCommands.TranslateItems(translation);
    }

    internal IReadOnlyList<ColumnProvider> ColumnProviders => _columnProviders;

    internal int GraphColumnWidth => (int)_revisionGraphColumnProvider.Column.Width.Value;

    internal static int CalculateGraphColumnWidth(int visibleLaneCount)
        => RevisionGraphColumnProvider.CalculateGraphColumnWidth(visibleLaneCount);

    internal bool IsCurrentCheckout(GitRevision revision)
        => _headId is ObjectId headId && revision.ObjectId == headId;

    internal void CancelBackgroundTasks()
    {
        _refreshRevisionsSequence.CancelCurrent();
        _isRefreshingRevisions = false;
        _customDiffToolsSequence.CancelCurrent();
        _buildServerWatcher.CancelBuildStatusFetchOperation();
        _taskManager.JoinPendingOperations();
    }

    /// <summary>
    /// Reset the controls to the supplied content.
    /// This is used to remove spinners added when loading and to replace the gridview at errors.
    /// </summary>
    /// <param name="content">The content to show.</param>
    private void SetPage(Control content)
        => revisionPage.Content = content;

    internal IndexWatcher IndexWatcher => _indexWatcher.Value;

    internal bool HasRevisionSource => _lastModule is not null;

    internal void FocusRevisionGrid()
    {
        // Avalonia's UserControl does not delegate focus to its inner list like the WinForms control.
        _focusGridWhenShown = !ReferenceEquals(revisionPage.Content, _gridView)
            || !_gridView.Focus(NavigationMethod.Tab);
    }

    internal GitRevision? LatestSelectedRevision => SelectedRevision;

    internal bool MultiSelect
    {
        get => _gridView.SelectionMode == SelectionMode.Multiple;
        set => _gridView.SelectionMode = value ? SelectionMode.Multiple : SelectionMode.Single;
    }

    private static void FillMenuFromMenuCommands(IEnumerable<MenuCommand> menuCommands, ToolStripDropDownItem targetItem)
    {
        targetItem.Items.Clear();
        foreach (MenuCommand menuCommand in menuCommands)
        {
            Control item = MenuCommand.CreateToolStripItem(menuCommand);
            targetItem.Items.Add(item);
            if (item is MenuItem menuItem)
            {
                menuCommand.RegisterMenuItem(menuItem);
            }
        }
    }

    // returns " --find-renames=... --find-copies=..." according to app settings
    private static ArgumentString FindRenamesAndCopiesOpts()
        => AppSettings.FollowRenamesInFileHistoryExactOnly
            ? " --find-renames=\"100%\" --find-copies=\"100%\""
            : " --find-renames --find-copies";

    public void ResetAllFilters()
        => _filterInfo.ResetAllFilters();

    /// <inheritdoc />
    public void ResetAllFiltersAndRefresh()
    {
        ResetAllFilters();
        PerformRefreshRevisions();
    }

    /// <inheritdoc />
    public void SetAndApplyPathFilter(string filter)
    {
        _filterInfo.ByPathFilter = !string.IsNullOrWhiteSpace(filter);
        _filterInfo.PathFilter = filter;
        PerformRefreshRevisions();
    }

    private void InitiateRefAction(
        IReadOnlyList<IGitRef>? gitRefs,
        Action<IGitRef> action,
        FormQuickGitRefSelector.QuickAction actionLabel)
    {
        if (gitRefs?.Count is not > 0)
        {
            return;
        }

        if (gitRefs.Count == 1)
        {
            action(gitRefs[0]);
            return;
        }

        using FormQuickGitRefSelector dlg = new();
        dlg.Init(actionLabel, gitRefs);
        dlg.Location = GetQuickItemSelectorLocation();
        if (dlg.ShowDialog(GetOwner()) != WinFormsShims.DialogResult.OK || dlg.SelectedRef is null)
        {
            return;
        }

        action(dlg.SelectedRef);
    }

    public DrawingPoint GetQuickItemSelectorLocation()
    {
        if (_gridView.ContainerFromIndex(_latestSelectedRowIndex) is Control container
            && TopLevel.GetTopLevel(this) is TopLevel topLevel
            && container.TranslatePoint(
                new Avalonia.Point(container.Bounds.Width, container.Bounds.Height),
                topLevel) is Avalonia.Point location)
        {
            PixelPoint screenPoint = topLevel.PointToScreen(location);
            return new DrawingPoint(screenPoint.X, screenPoint.Y);
        }

        return default;
    }

    private void ResetNavigationHistory()
    {
        _navigationHistory.Clear();
    }

    public void NavigateBackward()
    {
        if (_navigationHistory.CanNavigateBackward)
        {
            SetSelectedRevision(_navigationHistory.NavigateBackward(), updateNavigationHistory: false);
        }
    }

    public void NavigateForward()
    {
        if (_navigationHistory.CanNavigateForward)
        {
            SetSelectedRevision(_navigationHistory.NavigateForward(), updateNavigationHistory: false);
        }
    }

    /// <summary>Removes the row context menu, like the WinForms grid method.</summary>
    public void DisableContextMenu()
    {
        _gridView.ContextMenu = null;
    }

    /// <summary>
    ///  Prevents revisions refreshes and stops <see cref="PerformRefreshRevisions"/> from executing
    ///  until <see cref="ResumeRefreshRevisions"/> is called.
    /// </summary>
    internal void SuspendRefreshRevisions() => _updatingFilters++;

    /// <summary>
    ///  Resume revisions refreshes.
    /// </summary>
    internal void ResumeRefreshRevisions()
    {
        --_updatingFilters;
        DebugHelpers.Assert(_updatingFilters >= 0, $"{nameof(ResumeRefreshRevisions)} was called without matching {nameof(SuspendRefreshRevisions)}!");
    }

    /// <inheritdoc />
    public void SetAndApplyBranchFilter(string filter)
    {
        _filterInfo.SetBranchFilter(filter);
        PerformRefreshRevisions();
    }

    private void RefreshFilteredRevisions()
        => PerformRefreshRevisions();

    /// <inheritdoc />
    public void SetAndApplyRevisionFilter(RevisionFilter filter)
    {
        if (_filterInfo.Apply(filter))
        {
            PerformRefreshRevisions();
        }
    }

    public void Refresh()
    {
        ApplyColumnSettings();
        _toolTipProvider.Clear();
        RefreshRealizedRows();
        UpdateViewMenuChecks();
    }

    private void OnRuntimeLoad()
    {
        ReloadHotkeys();
        LoadCustomDifftools();
    }

    public void Load()
    {
        if (!Design.IsDesignMode)
        {
            PerformRefreshRevisions();
        }
    }

    public void LoadCustomDifftools()
    {
        List<CustomDiffMergeTool> menus =
        [
            new(openCommitsWithDiffToolMenuItem, diffSelectedCommitsMenuItem_Click)
        ];

        new CustomDiffMergeToolProvider().LoadCustomDiffMergeTools(
            Module,
            menus,
            isDiff: true,
            cancellationToken: _customDiffToolsSequence.Next());
    }

    public void CancelLoadCustomDifftools()
        => _customDiffToolsSequence.CancelCurrent();

    bool IRevisionGridUpdate.SetSelectedRevision(ObjectId commitId, bool toggleSelection, bool updateNavigationHistory)
        => SetSelectedRevision(commitId, toggleSelection, updateNavigationHistory);

    /// <summary>Selects and scrolls to the given revision if it is loaded.</summary>
    public bool SetSelectedRevision(ObjectId objectId, bool toggleSelection = false, bool updateNavigationHistory = true)
    {
        GitRevision? revision = _revisions.FirstOrDefault(r => r.ObjectId == objectId);
        if (revision is null)
        {
            return false;
        }

        if (objectId.IsZero)
        {
            throw new ArgumentException("Value cannot be a zero ObjectId.", nameof(objectId));
        }

        if (toggleSelection && _gridView.SelectedItems is { } selectedItems)
        {
            bool wasSelected = selectedItems.Contains(revision);
            if (wasSelected && selectedItems.Count > 1)
            {
                selectedItems.Remove(revision);
            }
            else if (!wasSelected)
            {
                selectedItems.Add(revision);
            }
        }
        else if (_gridView.SelectedItems is { } currentSelection)
        {
            if (currentSelection.Count != 1 || !currentSelection.Contains(revision))
            {
                if (currentSelection.Count > 1)
                {
                    currentSelection.Clear();
                }

                _gridView.SelectedItem = revision;
            }
        }

        _gridView.ScrollIntoView(revision);
        if (updateNavigationHistory)
        {
            _navigationHistory.Push(objectId);
        }

        return true;
    }

    public GitRevision GetRevision(ObjectId objectId)
    {
        // Like WinForms, may return null; callers null-check.
        return _revisions.FirstOrDefault(r => r.ObjectId == objectId)!;
    }

    private void HighlightBranch(ObjectId id)
    {
        _revisionGraphColumnProvider.RevisionGraphDrawStyle = RevisionGraphDrawStyle.HighlightSelected;
        _revisionGraph.HighlightBranch(id);
        RefreshRealizedRows();
    }

    public string DescribeRevision(GitRevision revision, int maxLength = 0)
    {
        string description = revision.IsArtificial
            ? string.Empty
            : revision.ObjectId.ToShortString() + ": ";

        GitRefListsForRevision gitRefListsForRevision = new(revision);

        IGitRef? descriptiveRef = gitRefListsForRevision.AllBranches
            .Concat(gitRefListsForRevision.AllTags)
            .FirstOrDefault();

        // The WinForms grid disambiguates ref names against ambiguous refs; not ported.
        description += descriptiveRef is not null
            ? descriptiveRef.Name
            : revision.Subject;

        if (maxLength > 0)
        {
            description = description.ShortenTo(maxLength);
        }

        return description;
    }

    /// <summary>
    /// Get the (last) selected revision in the grid.
    /// </summary>
    /// <returns>The selected revisions or <see langword="null"/> if none selected.</returns>
    public GitRevision? GetSelectedRevisionOrDefault()
        => LatestSelectedRevision;

    public IReadOnlyList<GitRevision> GetSelectedRevisions()
        => GetSelectedRevisions(direction: null);

    private (ObjectId firstId, GitRevision? selectedRev) GetFirstAndSelected()
    {
        IReadOnlyList<GitRevision> revisions = GetSelectedRevisions();

        return revisions.Count switch
        {
            0 => (default, null),
            1 => (firstId: revisions[0].FirstParentId, selectedRev: revisions[0]),
            _ => (firstId: revisions[^1].ObjectId, selectedRev: revisions[0])
        };
    }

    public IReadOnlyList<ObjectId> GetRevisionChildren(ObjectId objectId)
        => [.. _revisions
            .Where(revision => revision.ParentIds?.Contains(objectId) == true)
            .Select(revision => revision.ObjectId)];

    private bool IsValidRevisionIndex(int index)
        => index >= 0 && index < _revisions.Count;

    /// <summary>
    /// Get the actual GitRevision from grid or use GitModule if parents may be rewritten or the commit is not in the grid.
    /// </summary>
    /// <returns>The GitRevision or null if not found</returns>
    public GitRevision? GetActualRevision(ObjectId objectId)
    {
        GitRevision? revision = GetRevision(objectId);
        if (revision is not null)
        {
            return GetActualRevision(revision);
        }

        // Revision is not in grid, try get from Git
        return Module.GetRevision(objectId, shortFormat: true, loadRefs: true);
    }

    /// <summary>
    /// Get the GitRevision with the actual parents as they may be rewritten in filtered grids.
    /// </summary>
    /// <param name="revision">The revision, likely from the grid.</param>
    /// <returns>The revision with parents.</returns>
    public GitRevision GetActualRevision(GitRevision revision)
    {
        // Index commits must have HEAD as parent already
        if (_parentsAreRewritten && !revision.IsArtificial)
        {
            // Grid is filtered and revision may have incorrect parents
            revision = revision.Clone();
            revision.ParentIds = Module.GetParents(revision.ObjectId).ToList();
        }

        return revision;
    }

    public override bool ProcessHotkey(Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Control | Keys.A:
                return true; // never select all revisions

            case Keys.Escape:
                if (_toolTipProvider.Hide())
                {
                    return true;
                }

                break;
        }

        return base.ProcessHotkey(keyData);
    }

    public void ReloadHotkeys()
    {
        LoadHotkeys(HotkeySettingsName);
        MenuCommands.CreateOrUpdateMenuCommands();
        SetShortcutKeys();
    }

    public void ReloadTranslation()
        => Translator.Translate(this, AppSettings.CurrentTranslation);

    /// <summary>
    /// Show spinner (in the synchronous part of loading revisions
    /// and creating the control), or the "Loading..." text when first revision is
    /// handled and the grid is being updated.
    /// Note that these controls are removed by SetPage() when the grid is loaded.
    /// </summary>
    /// <param name="showSpinner">Show the spinner or the text controls.</param>
    private void ShowLoading(bool showSpinner = true)
    {
        lblLoadingStatus.IsVisible = !showSpinner;
        SetPage(showSpinner ? new LoadingControl() : _gridView);
    }

    private bool CanRefresh => !_isRefreshingRevisions && _updatingFilters == 0;

    public void PerformRefreshRevisions(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs = null!, bool forceRefresh = false)
    {
        if (!CanRefresh)
        {
            System.Diagnostics.Trace.WriteLine("Ignoring refresh as RefreshRevisions() is already running.");
            return;
        }

        IGitModule module = _lastModule ?? Module;
        CurrentBranch = new(() => module.IsValidGitWorkingDir()
            ? module.GetSelectedBranch(emptyIfDetached: true)
            : string.Empty);

        ReloadRevisions(
            module,
            _lastRevisionFilter,
            SelectedId,
            _lastPathFilter,
            getRefs,
            forceRefresh);
    }

    /// <summary>
    /// Returns the historical name of a file in a revision, following renames and merge commits.
    /// </summary>
    public string? GetRevisionFileName(string path, ObjectId objectId)
    {
        if (objectId.IsZero)
        {
            return null;
        }

        if (FilePathByObjectId?.TryGetValue(objectId, out string? fileName) is true)
        {
            return fileName;
        }

        GitArgumentBuilder args = new("log")
        {
            $"--format=\"{_objectIdPrefix}%H\"",
            "--name-only",
            "--follow",
            "--diff-merges=separate",
            FindRenamesAndCopiesOpts(),
            objectId.ToString(),
            "--max-count=1",
            "--",
            path.QuoteIfNotQuotedAndNE(),
        };

        return ParseFileNames(Module, args, cancellationToken: default).FirstOrDefault();
    }

    private IEnumerable<string> ParseFileNames(IGitModule module, GitArgumentBuilder args, CancellationToken cancellationToken)
    {
        ExecutionResult result = module.GitExecutable.Execute(
            args,
            outputEncoding: GitModule.LosslessEncoding,
            throwOnErrorExit: false,
            cancellationToken: cancellationToken);
        if (!result.ExitedSuccessfully)
        {
            yield break;
        }

        ObjectId currentObjectId = default;
        foreach (string? line in result.StandardOutput.LazySplit('\n').Select(GitModule.ReEncodeFileNameFromLossless))
        {
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            if (line.StartsWith(_objectIdPrefix))
            {
                currentObjectId = line.Length >= ObjectId.Sha1CharCount + _objectIdPrefix.Length
                    && ObjectId.TryParse(line, offset: _objectIdPrefix.Length, out ObjectId parsedId)
                        ? parsedId
                        : default;
                continue;
            }

            if (currentObjectId.IsZero)
            {
                continue;
            }

            cancellationToken.ThrowIfCancellationRequested();
            FilePathByObjectId?.TryAdd(currentObjectId, line);
            yield return line;
        }
    }

    private bool ParentsAreRewritten => _parentsAreRewritten;

    internal bool FilterIsApplied()
        => _filterInfo.HasFilter;

    private void OnGridViewSelectionChanged(object? sender, EventArgs e)
    {
        _latestSelectedRowIndex = _gridView.SelectedIndex;
        _parentChildNavigationHistory.RevisionsSelectionChanged();

        (ObjectId first, GitRevision? selected) = GetFirstAndSelected();
        compareToWorkingDirectoryMenuItem.IsEnabled = selected is not null && selected.ObjectId != ObjectId.WorkTreeId;
        compareWithCurrentBranchToolStripMenuItem.IsEnabled = !string.IsNullOrWhiteSpace(CurrentBranch.Value);
        compareSelectedCommitsMenuItem.IsEnabled = !first.IsZero && selected is not null;
        openCommitsWithDiffToolMenuItem.IsEnabled = !first.IsZero && selected is not null;

        IReadOnlyList<GitRevision> selectedRevisions = GetSelectedRevisions();
        HighlightRevisionsByAuthor();
        RefreshRealizedRows();
        UpdateContextMenuItems();
        if (selectedRevisions.Count == 1 && selected is not null)
        {
            _navigationHistory.Push(selected.ObjectId);
        }

        SelectionChanged?.Invoke(this, e);
    }

    private void AddColumn(ColumnProvider columnProvider)
    {
        columnProvider.Index = _columnProviders.Count;
        _columnProviders.Add(columnProvider);
    }

    internal void ApplyColumnSettings()
    {
        // restore its resizable state
        _lastVisibleResizableColumn?.Resizable = true;

        // columns could change their Resizable state, e.g. the BuildStatusColumnProvider
        foreach (ColumnProvider columnProvider in _columnProviders)
        {
            columnProvider.ApplySettings();
        }

        // suppress the manual resizing of the last visible column because it will be resized when the maximized column is resized
        //// LINQ because the original DataGridView GetLastColumn API has no Avalonia equivalent.
        _lastVisibleResizableColumn = _columnProviders
            .Select(provider => provider.Column)
            .Last(column => column.IsVisible && column.IsAvailable && column.Resizable);
        _lastVisibleResizableColumn.Resizable = false;

        foreach (RevisionRowControl row in _gridView.GetVisualDescendants().OfType<RevisionRowControl>())
        {
            row.ApplyColumnLayout();
        }
    }

    private void HighlightRevisionsByAuthor()
    {
        if (TryGetUICommandsDirect(out IGitUICommands? commands)
            && _authorHighlighting.ProcessRevisionSelectionChange(
                commands.Module,
                GetSelectedRevisions()))
        {
            RefreshRealizedRows();
        }
    }

    private bool DeleteSingleRef()
    {
        if (SelectedRevision is not GitRevision revision)
        {
            return false;
        }

        IGitRef[] refs =
        [
            .. new GitRefListsForRevision(revision)
                .GetDeletableRefs(Module.GetSelectedBranch())
                .Where(gitRef => !gitRef.IsRemote),
        ];
        if (refs.Length != 1)
        {
            return false;
        }

        if (refs[0].IsTag)
        {
            UICommands.StartDeleteTagDialog(GetOwner(), refs[0].Name);
        }
        else
        {
            UICommands.StartDeleteBranchDialog(GetOwner(), refs[0].Name);
        }

        return true;
    }

    private void RenameRef()
    {
        GitRevision? selectedRevision = LatestSelectedRevision;
        if (selectedRevision is null)
        {
            return;
        }

        InitiateRefAction(
            new GitRefListsForRevision(selectedRevision).GetRenameableLocalBranches(),
            gitRef => UICommands.StartRenameDialog(GetOwner(), gitRef.Name),
            FormQuickGitRefSelector.QuickAction.Rename);
    }

    private WinFormsShims.IWin32Window? GetOwner()
        => TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window;

    /// <summary>
    ///  Starts (re)loading the history of <paramref name="module"/> in the background,
    ///  streaming batches into the list as they are parsed.
    /// </summary>
    public void ReloadRevisions(
        IGitModule module,
        string revisionFilter = "--all",
        ObjectId selectedObjectId = default,
        string pathFilter = "",
        Func<RefsFilter, IReadOnlyList<IGitRef>>? getRefs = null,
        bool forceRefresh = true)
    {
        CancellationToken cancellationToken = _refreshRevisionsSequence.Next();
        _isRefreshingRevisions = true;
        _lastModule = module;
        CurrentBranch = new(() => module.IsValidGitWorkingDir()
            ? module.GetSelectedBranch(emptyIfDetached: true)
            : string.Empty);
        _lastRevisionFilter = revisionFilter;
        _lastPathFilter = pathFilter;

        if (revisionFilter == "--all")
        {
            revisionFilter = _filterInfo.GetRevisionFilter(new Lazy<ObjectId>(module.GetCurrentCheckout)).ToString();
            pathFilter = _filterInfo.PathFilter;
        }

        FilterChanged?.Invoke(this, new FilterChangedEventArgs(_filterInfo));

        _revisions.Clear();
        _toolTipProvider.Clear();
        ResetNavigationHistory();
        _parentChildNavigationHistory.Clear();
        _buildServerWatcher.CancelBuildStatusFetchOperation();
        foreach (ColumnProvider columnProvider in _columnProviders)
        {
            columnProvider.Clear();
        }

        _revisionGraph.Clear();
        _headId = module.GetCurrentCheckout();
        _revisionGraph.HeadId = _headId.Value;
        _superprojectCurrentCheckout = null;

        // A path filter makes git rewrite parents ("history simplification"), so revisions
        // may carry parent ids that are not their real parents.
        _parentsAreRewritten = !string.IsNullOrEmpty(pathFilter);
        _pendingSelectedObjectId = selectedObjectId;
        _headHighlighted = false;
        lblLoadingStatus.Text = _strLoading.Text;
        _focusGridWhenShown = true;
        ShowLoading();

        Lazy<IReadOnlyList<IGitRef>> refs = new(() => (getRefs ?? module.GetRefs)(RefsFilter.NoFilter));
        _ambiguousRefs = new(() => GitRef.GetAmbiguousRefNames(refs.Value));
        Lazy<IReadOnlyCollection<GitRevision>> stashes = new(() =>
            !AppSettings.ShowStashes || module.IsBareRepository()
                ? []
                : new RevisionReader(module).GetStashes(cancellationToken));
        RevisionLoadEventArgs loadEventArgs = new(this, UICommands, refs, stashes, forceRefresh);
        RevisionObserver observer = new(this, cancellationToken, loadEventArgs);
        RevisionsLoading?.Invoke(this, loadEventArgs);
        _taskManager.FileAndForget(async () =>
        {
            SuperProjectInfo? superProjectInfo = await GetSuperprojectCheckoutAsync(module).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            _superprojectCurrentCheckout = superProjectInfo;
            if (superProjectInfo is not null)
            {
                await _taskManager.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                RefreshRealizedRows();
            }
        });

        _taskManager.FileAndForget(() =>
        {
            // Like the WinForms grid: fetch the refs first so they can be attached to the
            // revisions as they stream in (ref labels; square graph nodes).
            IReadOnlyList<IGitRef> loadedRefs = refs.Value;
            string selectedBranch = module.GetSelectedBranch(emptyIfDetached: true);
            IGitRef? selectedRef = loadedRefs.FirstOrDefault(
                gitRef => gitRef.IsHead && gitRef.Name == selectedBranch);
            if (selectedRef is not null)
            {
                selectedRef.IsSelected = true;
                loadedRefs.FirstOrDefault(
                    gitRef => selectedRef.IsTrackingRemote(gitRef))
                    ?.IsSelectedHeadMergeSource = true;
            }

            // Exclude the 'stash' ref, it is specially handled when stashes are shown
            _refsByObjectId = (AppSettings.ShowStashes
                    ? loadedRefs.Where(gitRef => gitRef.CompleteName != GitRefName.RefsStashPrefix)
                    : loadedRefs)
                .Where(gitRef => !gitRef.ObjectId.IsZero)
                .ToLookup(gitRef => gitRef.ObjectId);
            observer.InitializeStashes(stashes.Value);

            RevisionReader reader = new(module);
            bool hasNotes = AppSettings.ShowGitNotesColumn.Value || AppSettings.ShowGitNotes;
            string effectivePathFilter = BuildPathFilter(module, pathFilter, cancellationToken);
            reader.GetLog(observer, revisionFilter, effectivePathFilter, hasNotes, autostashLabel: "autostash", cancellationToken);
        });
    }

    private void DeleteRef()
    {
        GitRevision? selectedRevision = LatestSelectedRevision;
        if (selectedRevision is null)
        {
            return;
        }

        InitiateRefAction(
            new GitRefListsForRevision(selectedRevision).GetDeletableRefs(CurrentBranch.Value),
            gitRef =>
            {
                if (gitRef.IsTag)
                {
                    UICommands.StartDeleteTagDialog(GetOwner(), gitRef.Name);
                }
                else if (gitRef.IsRemote)
                {
                    UICommands.StartDeleteRemoteBranchDialog(GetOwner(), gitRef.Name);
                }
                else
                {
                    UICommands.StartDeleteBranchDialog(GetOwner(), gitRef.Name);
                }
            },
            FormQuickGitRefSelector.QuickAction.Delete);
    }

    internal void RefreshRealizedRows()
    {
        foreach (RevisionRowControl row in _gridView.GetVisualDescendants().OfType<RevisionRowControl>())
        {
            row.RefreshCells();
        }
    }

    /// <summary>
    ///  Selects and scrolls to the given revision, or retains it until the active load reaches it.
    /// </summary>
    public void SelectRevision(ObjectId objectId)
    {
        if (!SetSelectedRevision(objectId))
        {
            _pendingSelectedObjectId = objectId;
        }
    }

    internal bool TryGetSuperProjectInfo([System.Diagnostics.CodeAnalysis.NotNullWhen(returnValue: true)] out SuperProjectInfo? spi)
    {
        // If _superprojectCurrentCheckout is not yet calculated when the grid is shown,
        // a separate Refresh() will be invoked to update the row later.
        spi = _superprojectCurrentCheckout;
        return spi is not null;
    }

    private void OnGridViewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.None && e.Key is Key.Home or Key.End && _revisions.Count > 0)
        {
            int index = e.Key == Key.Home ? 0 : _revisions.Count - 1;
            _gridView.SelectedItems?.Clear();
            _gridView.SelectedIndex = index;
            _gridView.ScrollIntoView(_revisions[index]);
            e.Handled = true;
            return;
        }

        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.C)
        {
            IReadOnlyList<GitRevision> selectedRevisions = GetSelectedRevisions();
            if (selectedRevisions.Count > 0)
            {
                ClipboardUtil.TrySetText(string.Join(Environment.NewLine, selectedRevisions.Select(revision => revision.ObjectId)));
            }

            e.Handled = true;
            return;
        }

        _quickSearchProvider.OnPreviewKeyDown(e);
    }

    private void ClearRefHighlight()
        => _messageColumnProvider.ClearRefHighlight();

    internal void UpdateLaneHighlight(IGitRef? gitRef, GitRevision? revision)
    {
        int rowIndex = revision is not null
            && _revisionGraph.TryGetRowIndex(revision.ObjectId, out int revisionRowIndex)
                ? revisionRowIndex
                : -1;
        this.InvokeAndForget(() => _revisionGraphColumnProvider.SetHoverHighlightAsync(gitRef, rowIndex));
    }

    public void ViewSelectedRevisions()
    {
        IReadOnlyList<GitRevision> selectedRevisions = GetSelectedRevisions();
        if (selectedRevisions.Count > 0 && !selectedRevisions[0].IsArtificial)
        {
            FormCommitDiff form = new(UICommands, selectedRevisions[0].ObjectId);
            if (TopLevel.GetTopLevel(this) is Window owner && owner.IsVisible)
            {
                form.Show(owner);
            }
            else
            {
                form.Show();
            }
        }
        else if (selectedRevisions.Count == 0)
        {
            UICommands.StartCompareRevisionsDialog(GetOwner());
        }
    }

    /// <summary>Gets or replaces the context menu attached to revision rows.</summary>
    public ContextMenu? RevisionContextMenu
    {
        get => _gridView.ContextMenu;
        set => _gridView.ContextMenu = value;
    }

    private void CreateTagToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (SelectedRevision is GitRevision revision)
        {
            UICommands.StartCreateTagDialog(GetOwner(), revision);
        }
    }

    private void ResetCurrentBranchToHereToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (SelectedRevision is not GitRevision revision)
        {
            return;
        }

        UICommands.DoActionOnRepo(() =>
        {
            using FormResetCurrentBranch form = FormResetCurrentBranch.Create(UICommands, revision);
            return form.ShowDialog(GetOwner()) == WinFormsShims.DialogResult.OK;
        });
    }

    private void ResetChangesToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartResetChangesDialog(
            GetOwner(),
            Module.GetWorkTreeFiles(),
            onlyWorkTree: SelectedRevision?.ObjectId == ObjectId.WorkTreeId);
        ArtificialChanged?.Invoke(this, EventArgs.Empty);
    }

    private void CommitToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartCommitDialog(GetOwner());
    }

    #endregion

    private void _gridView_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        PointerPointProperties properties = e.GetCurrentPoint(_gridView).Properties;
        if (properties.PointerUpdateKind == PointerUpdateKind.XButton1Pressed)
        {
            NavigateBackward();
            e.Handled = true;
            return;
        }

        if (properties.PointerUpdateKind == PointerUpdateKind.XButton2Pressed)
        {
            NavigateForward();
            e.Handled = true;
            return;
        }

        if (properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed
            && e.Source is Control { DataContext: GitRevision revision }
            && _gridView.SelectedItems?.Contains(revision) != true)
        {
            _gridView.SelectedItem = revision;
        }
    }

    private void ResetAnotherBranchToHereToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (SelectedRevision is not GitRevision revision)
        {
            return;
        }

        UICommands.DoActionOnRepo(() =>
        {
            using FormResetAnotherBranch form = FormResetAnotherBranch.Create(UICommands, revision);
            return form.ShowDialog(GetOwner()) == WinFormsShims.DialogResult.OK;
        });
    }

    private void CreateNewBranchToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (SelectedRevision is GitRevision revision)
        {
            UICommands.StartCreateBranchDialog(GetOwner(), revision.ObjectId);
        }
    }

    internal static bool CanDropPatchFiles(IReadOnlyList<string> fileNames)
        => fileNames.Count > 0
            && fileNames.All(fileName => fileName.EndsWith(".patch", StringComparison.InvariantCultureIgnoreCase));

    private static string[] GetDroppedFileNames(IDataTransfer dataTransfer)
        => [.. (dataTransfer.TryGetFiles() ?? [])
            .Select(file => file.TryGetLocalPath())
            .OfType<string>()];

    /// <inheritdoc />
    public void ShowReflog()
    {
        if (!_filterInfo.ShowReflogReferences)
        {
            _filterInfo.ShowReflogReferences = true;
            RefreshFilteredRevisions();
        }
    }

    /// <inheritdoc />
    public void ShowAllBranches()
    {
        _filterInfo.ByBranchFilter = false;
        _filterInfo.ShowCurrentBranchOnly = false;
        RefreshFilteredRevisions();
    }

    /// <inheritdoc />
    public void ShowFilteredBranches()
    {
        // Must be able to set ByBranchFilter without a filter to edit it
        _filterInfo.ByBranchFilter = true;
        _filterInfo.ShowCurrentBranchOnly = false;
        RefreshFilteredRevisions();
    }

    /// <inheritdoc />
    public void ShowCurrentBranchOnly()
    {
        _filterInfo.ByBranchFilter = false;
        _filterInfo.ShowCurrentBranchOnly = true;
        RefreshFilteredRevisions();
    }

    public void SetLastRevisionToDisplayHash(string hash)
    {
        _filterInfo.LastRevisionToDisplayHash = hash;
    }

    /// <inheritdoc />
    public void ShowRevisionFilterDialog()
    {
        if (!TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            return;
        }

        using FormRevisionFilter form = new(commands, _filterInfo);
        if (form.ShowDialog(GetOwner()) == WinFormsShims.DialogResult.OK)
        {
            RefreshFilteredRevisions();
        }
    }

    private void UpdateContextMenuItems()
    {
        GitRevision? revision = SelectedRevision;
        bool hasCommands = TryGetUICommandsDirect(out IGitUICommands? commands);
        bool isBareRepository = hasCommands && commands!.Module.IsBareRepository();
        bool regularRevision = revision is { IsArtificial: false } && hasCommands && !isBareRepository;

        bool inTheMiddleOfBisect = hasCommands && commands!.Module.InTheMiddleOfBisect();
        SetVisible(markRevisionAsBadToolStripMenuItem, inTheMiddleOfBisect);
        SetVisible(markRevisionAsGoodToolStripMenuItem, inTheMiddleOfBisect);
        SetVisible(bisectSkipRevisionToolStripMenuItem, inTheMiddleOfBisect);
        SetVisible(stopBisectToolStripMenuItem, inTheMiddleOfBisect);
        sepBisect.IsVisible = inTheMiddleOfBisect;
        SetVisible(copyToClipboardToolStripMenuItem, revision is { IsArtificial: false });
        SetVisible(applyStashToolStripMenuItem, regularRevision && revision!.IsAutostash);
        SetVisible(popStashToolStripMenuItem, regularRevision && revision!.IsStash);
        SetVisible(dropStashToolStripMenuItem, regularRevision && revision!.IsStash);
        sepStash.IsVisible = applyStashToolStripMenuItem.IsVisible
            || popStashToolStripMenuItem.IsVisible
            || dropStashToolStripMenuItem.IsVisible;

        checkoutBranchToolStripMenuItem.Items.Clear();
        tsmiPushBranch.Items.Clear();
        mergeBranchToolStripMenuItem.Items.Clear();
        renameBranchToolStripMenuItem.Items.Clear();
        deleteBranchToolStripMenuItem.Items.Clear();
        deleteTagToolStripMenuItem.Items.Clear();
        tsmiSelectInLeftPanel.Items.Clear();
        tsmiSelectInLeftPanel.Tag = null;
        _rebaseOnTopOf = null;

        if (regularRevision)
        {
            PopulateRefMenus(revision!, commands!);
        }

        SetVisible(checkoutBranchToolStripMenuItem, checkoutBranchToolStripMenuItem.Items.Count > 0);
        SetVisible(tsmiPushBranch, tsmiPushBranch.Items.Count > 0);
        SetVisible(mergeBranchToolStripMenuItem, mergeBranchToolStripMenuItem.Items.Count > 0);
        SetVisible(rebaseOnToolStripMenuItem, regularRevision);
        SetVisible(resetCurrentBranchToHereToolStripMenuItem, regularRevision);
        SetVisible(tsmiSelectInLeftPanel, regularRevision && SelectInLeftPanel is not null && tsmiSelectInLeftPanel.Tag is string);
        SetVisible(resetChangesToolStripMenuItem, revision is { IsArtificial: true } && hasCommands && !isBareRepository);
        SetVisible(commitToolStripMenuItem, revision is { IsArtificial: true } && hasCommands && !isBareRepository);
        SetVisible(createNewBranchToolStripMenuItem, regularRevision);
        SetVisible(resetAnotherBranchToHereToolStripMenuItem, regularRevision);
        SetVisible(renameBranchToolStripMenuItem, renameBranchToolStripMenuItem.Items.Count > 0);
        bool isHeadOfCurrentBranch = regularRevision
            && revision!.Refs.Any(gitRef =>
                gitRef.CompleteName == GitRefName.RefsHeadsPrefix + commands!.Module.GetSelectedBranch());
        deleteBranchToolStripMenuItem.IsVisible = deleteBranchToolStripMenuItem.Items.Count > 0 || isHeadOfCurrentBranch;
        deleteBranchToolStripMenuItem.IsEnabled = deleteBranchToolStripMenuItem.Items.Count > 0 && !isBareRepository;
        SetVisible(createTagToolStripMenuItem, revision is { IsArtificial: false } && hasCommands);
        SetVisible(deleteTagToolStripMenuItem, deleteTagToolStripMenuItem.Items.Count > 0);
        SetVisible(checkoutRevisionToolStripMenuItem, regularRevision);
        IReadOnlyList<GitRevision> selectedRevisions = GetSelectedRevisions();
        SetVisible(
            revertCommitToolStripMenuItem,
            hasCommands
            && !Module.IsBareRepository()
            && selectedRevisions.Count > 0
            && selectedRevisions.All(selectedRevision => !selectedRevision.IsArtificial));
        SetVisible(
            cherryPickCommitToolStripMenuItem,
            hasCommands
            && !Module.IsBareRepository()
            && selectedRevisions.Count > 0
            && selectedRevisions.All(selectedRevision => !selectedRevision.IsArtificial));
        SetVisible(
            archiveRevisionToolStripMenuItem,
            hasCommands
            && selectedRevisions.Count is >= 1 and <= 2
            && selectedRevisions.All(selectedRevision => !selectedRevision.IsArtificial));
        SetVisible(openBuildReportToolStripMenuItem, !string.IsNullOrWhiteSpace(revision?.BuildStatus?.Url));
        SetVisible(openPullRequestPageStripMenuItem, !string.IsNullOrWhiteSpace(revision?.BuildStatus?.PullRequestUrl));
        SetVisible(manipulateCommitToolStripMenuItem, regularRevision);
        fixupCommitToolStripMenuItem.IsEnabled = regularRevision;
        squashCommitToolStripMenuItem.IsEnabled = regularRevision;
        amendCommitToolStripMenuItem.IsEnabled = regularRevision && Module.GitVersion.SupportAmendCommits;
        editCommitToolStripMenuItem.IsEnabled = regularRevision;
        rewordCommitToolStripMenuItem.IsEnabled = regularRevision;
        SetVisible(compareToolStripMenuItem, revision is not null);
        openCommitsWithDiffToolMenuItem.IsEnabled = selectedRevisions.Count > 0;
        (ObjectId first, GitRevision? selected) = GetFirstAndSelected();
        compareToBranchToolStripMenuItem.IsEnabled = selected is not null;
        compareWithCurrentBranchToolStripMenuItem.IsEnabled = selected is not null && !string.IsNullOrWhiteSpace(Module.GetSelectedBranch());
        selectAsBaseToolStripMenuItem.IsEnabled = selected is not null;
        compareToBaseToolStripMenuItem.IsEnabled = selected is not null && _baseCommitToCompare is not null;
        compareToWorkingDirectoryMenuItem.IsEnabled = selected is not null && selected.ObjectId != ObjectId.WorkTreeId;
        compareSelectedCommitsMenuItem.IsEnabled = !first.IsZero && selected is not null;
        tsmiOtherActions.IsVisible = false;

        sepCopy.IsVisible = copyToClipboardToolStripMenuItem.IsVisible;
        sepBranch.IsVisible = checkoutBranchToolStripMenuItem.IsVisible
            || tsmiPushBranch.IsVisible
            || mergeBranchToolStripMenuItem.IsVisible
            || rebaseOnToolStripMenuItem.IsVisible
            || resetCurrentBranchToHereToolStripMenuItem.IsVisible;
        sepBranchModification.IsVisible = createNewBranchToolStripMenuItem.IsVisible
            || renameBranchToolStripMenuItem.IsVisible
            || deleteBranchToolStripMenuItem.IsVisible;
        sepCommit.IsVisible = revertCommitToolStripMenuItem.IsVisible
            || cherryPickCommitToolStripMenuItem.IsVisible
            || archiveRevisionToolStripMenuItem.IsVisible
            || manipulateCommitToolStripMenuItem.IsVisible;
        sepCompare.IsVisible = compareToolStripMenuItem.IsVisible;
        sepNavigate.IsVisible = revision is not null;

        navigateToolStripMenuItem.IsVisible = revision is not null;
        UpdateNavigationMenu(revision);
        viewToolStripMenuItem.IsVisible = hasCommands;
        MenuCommands.TriggerMenuChanged();
        if (hasCommands)
        {
            mainContextMenu.AddUserScripts(
                runScriptToolStripMenuItem,
                ExecuteCommand,
                script => script.AddToRevisionGridContextMenu,
                commands!);
        }
        else
        {
            mainContextMenu.RemoveUserScripts(runScriptToolStripMenuItem);
        }

        void SetVisible(MenuItem item, bool visible)
        {
            item.IsVisible = visible;
            item.IsEnabled = visible;
        }
    }

    private void PopulateRefMenus(GitRevision revision, IGitUICommands commands)
    {
        GitRefListsForRevision refLists = new(revision);
        string currentBranchRef = GitRefName.RefsHeadsPrefix + commands.Module.GetSelectedBranch();
        IReadOnlyList<IGitRef> allBranches = refLists.AllBranches;
        IGitRef[] selectableRefs = [.. refLists.AllTags.Concat(allBranches)];
        tsmiSelectInLeftPanel.Tag = selectableRefs.FirstOrDefault()?.Name;
        if (selectableRefs.Length > 1)
        {
            foreach (IGitRef gitRef in selectableRefs)
            {
                AddRefMenuItem(
                    tsmiSelectInLeftPanel,
                    gitRef,
                    () =>
                    {
                        mainContextMenu.Close();
                        SelectInLeftPanel?.Invoke(gitRef.Name);
                    });
            }
        }

        foreach (IGitRef branch in allBranches)
        {
            if (branch.CompleteName != currentBranchRef)
            {
                AddRefMenuItem(
                    checkoutBranchToolStripMenuItem,
                    branch,
                    () =>
                    {
                        if (branch.IsRemote)
                        {
                            commands.StartCheckoutRemoteBranch(GetOwner(), branch.Name);
                        }
                        else
                        {
                            commands.StartCheckoutBranch(GetOwner(), branch.Name);
                        }
                    });
            }

            if (!branch.IsRemote)
            {
                AddRefMenuItem(
                    tsmiPushBranch,
                    branch,
                    () => commands.StartPushDialog(
                        GetOwner(),
                        pushOnShow: false,
                        forceWithLease: false,
                        out _,
                        branch.Name));
                AddRefMenuItem(
                    renameBranchToolStripMenuItem,
                    branch,
                    () => commands.StartRenameDialog(GetOwner(), branch.Name));
                if (branch.CompleteName != currentBranchRef)
                {
                    AddRefMenuItem(
                        deleteBranchToolStripMenuItem,
                        branch,
                        () => commands.StartDeleteBranchDialog(GetOwner(), branch.Name));
                }
            }
        }

        bool firstRemoteBranchForDelete = true;
        foreach (IGitRef branch in allBranches)
        {
            if (branch.IsRemote)
            {
                if (firstRemoteBranchForDelete)
                {
                    firstRemoteBranchForDelete = false;
                    if (deleteBranchToolStripMenuItem.Items.Count > 0)
                    {
                        deleteBranchToolStripMenuItem.Items.Add(new ToolStripSeparator());
                    }
                }

                AddRefMenuItem(
                    deleteBranchToolStripMenuItem,
                    branch,
                    () => commands.StartDeleteRemoteBranchDialog(GetOwner(), branch.Name));
            }
        }

        foreach (IGitRef tag in refLists.AllTags)
        {
            AddRefMenuItem(
                deleteTagToolStripMenuItem,
                tag,
                () => commands.StartDeleteTagDialog(GetOwner(), tag.Name));
        }

        bool currentBranchPointsToRevision = allBranches.Any(branch => branch.CompleteName == currentBranchRef);
        IEnumerable<IGitRef> mergeRefs = refLists.AllTags.Concat(refLists.BranchesWithNoIdenticalRemotes)
            .Where(gitRef => gitRef.CompleteName != currentBranchRef);
        foreach (IGitRef gitRef in mergeRefs)
        {
            string mergeTarget = GetUnambiguousRefName(revision, gitRef);
            AddRefMenuItem(
                mergeBranchToolStripMenuItem,
                gitRef,
                () => commands.StartMergeBranchDialog(GetOwner(), mergeTarget));
            _rebaseOnTopOf ??= mergeTarget;
        }

        if (mergeBranchToolStripMenuItem.Items.Count == 0 && !currentBranchPointsToRevision)
        {
            MenuItem mergeCommit = new() { Header = revision.Guid };
            mergeCommit.Click += delegate { commands.StartMergeBranchDialog(GetOwner(), revision.Guid); };
            mergeBranchToolStripMenuItem.Items.Add(mergeCommit);
            _rebaseOnTopOf = revision.Guid;
        }
        else if (_rebaseOnTopOf is null && !currentBranchPointsToRevision)
        {
            _rebaseOnTopOf = revision.Guid;
        }
    }

    private static string GetUnambiguousRefName(GitRevision revision, IGitRef gitRef)
        => revision.Refs.Count(other => other.Name == gitRef.Name) > 1
            ? gitRef.CompleteName
            : gitRef.Name;

    private static void AddRefMenuItem(MenuItem parent, IGitRef gitRef, Action action)
    {
        MenuItem item = new()
        {
            Header = gitRef.Name.Replace("_", "__", StringComparison.Ordinal),
            Icon = new Image
            {
                Width = 16,
                Height = 16,
                Source = gitRef.IsTag
                    ? Properties.Images.Tag
                    : gitRef.IsRemote
                        ? Properties.Images.BranchRemote
                        : Properties.Images.BranchLocal,
            },
        };
        item.Click += delegate { action(); };
        parent.Items.Add(item);
    }

    private void ContextMenuOpening(object? sender, CancelEventArgs e)
    {
        UpdateContextMenuItems();
        mainContextMenu.InvalidateMeasure();
    }

    private string GetRefUnambiguousName(IGitRef gitRef)
        => _ambiguousRefs?.Value.Contains(gitRef.Name) == true
            ? gitRef.CompleteName
            : gitRef.Name;

    private void RebaseOnToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
    {
        IReadOnlyList<GitRevision> selectedRevisions = GetSelectedRevisions();
        rebaseToolStripMenuItem.IsEnabled
            = rebaseInteractivelyToolStripMenuItem.IsEnabled
            = _rebaseOnTopOf is not null && selectedRevisions.Count == 1;
        rebaseWithAdvOptionsToolStripMenuItem.IsEnabled = _rebaseOnTopOf is not null
            && (selectedRevisions.Count == 1
                || (selectedRevisions.Count == 2 && selectedRevisions.All(r => !r.IsArtificial)));
    }

    private void ToolStripItemClickRebaseBranch(object sender, EventArgs e)
    {
        StartRebase(interactive: false);
    }

    private void StartRebase(bool interactive)
    {
        if (_rebaseOnTopOf is null)
        {
            return;
        }

        if (!MessageBoxes.ConfirmSuppressible(GetOwner(), _areYouSureRebase.Text, _rebaseConfirmTitle.Text, AppSettings.DontConfirmRebase, heading: interactive ? _rebaseBranchInteractive.Text : _rebaseBranch.Text))
        {
            return;
        }

        if (interactive)
        {
            UICommands.StartInteractiveRebase(GetOwner(), _rebaseOnTopOf);
        }
        else
        {
            UICommands.StartRebase(GetOwner(), _rebaseOnTopOf);
        }
    }

    private void OnRebaseInteractivelyClicked(object sender, EventArgs e)
    {
        StartRebase(interactive: true);
    }

    private void SelectCurrentRevision()
    {
        if (_headId is ObjectId headId)
        {
            SetSelectedRevision(headId);
        }
    }

    private bool GoToParent(bool firstParent, bool useHistory)
    {
        if (SelectedRevision is not GitRevision revision)
        {
            return false;
        }

        if (useHistory && _parentChildNavigationHistory.HasPreviousParent)
        {
            _parentChildNavigationHistory.NavigateToPreviousParent(revision.ObjectId);
            return true;
        }

        GitRevision actualRevision = GetActualRevision(revision);
        IReadOnlyList<ObjectId>? parentIds = actualRevision.ParentIds;
        ObjectId parentId = firstParent
            ? parentIds?.FirstOrDefault() ?? default
            : parentIds?.LastOrDefault() ?? default;
        if (parentId.IsZero)
        {
            return false;
        }

        _parentChildNavigationHistory.NavigateToParent(revision.ObjectId, parentId);
        return true;
    }

    private bool GoToChild()
    {
        if (SelectedRevision is not GitRevision revision)
        {
            return false;
        }

        if (_parentChildNavigationHistory.HasPreviousChild)
        {
            _parentChildNavigationHistory.NavigateToPreviousChild(revision.ObjectId);
            return true;
        }

        GitRevision? child = _revisions.FirstOrDefault(
            candidate => candidate.ParentIds?.Contains(revision.ObjectId) == true);
        if (child is null)
        {
            return false;
        }

        _parentChildNavigationHistory.NavigateToChild(revision.ObjectId, child.ObjectId);
        return true;
    }

    private void OnRebaseWithAdvOptionsClicked(object sender, EventArgs e)
    {
        if (_rebaseOnTopOf is not null)
        {
            IReadOnlyList<GitRevision> selectedRevisions = GetSelectedRevisions();
            string from = selectedRevisions.Count == 2
                ? selectedRevisions[1].ObjectId.ToShortString()
                : string.Empty;
            UICommands.StartRebaseDialogWithAdvOptions(GetOwner(), _rebaseOnTopOf, from);
        }
    }

    private void CheckoutRevisionToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (SelectedRevision is GitRevision revision)
        {
            UICommands.StartCheckoutRevisionDialog(GetOwner(), revision.Guid);
        }
    }

    private void ArchiveRevisionToolStripMenuItemClick(object sender, EventArgs e)
    {
        IReadOnlyList<GitRevision> selectedRevisions = GetSelectedRevisions();
        if (selectedRevisions.Count is (< 1 or > 2))
        {
            MessageBoxes.SelectOnlyOneOrTwoRevisions(GetOwner());
            return;
        }

        GitRevision mainRevision = selectedRevisions[0];
        GitRevision? diffRevision = selectedRevisions.Count == 2 ? selectedRevisions[1] : null;
        UICommands.StartArchiveDialog(GetOwner(), mainRevision, diffRevision);
    }

    internal void ToggleShowAuthorDate()
    {
        AppSettings.ShowAuthorDate = !AppSettings.ShowAuthorDate;
        ApplySettingsAndRefreshRows();
    }

    internal void ToggleShowRemoteBranches()
    {
        AppSettings.ShowRemoteBranches = !AppSettings.ShowRemoteBranches;
        ApplySettingsAndRefreshRows();
    }

    internal void ToggleShowArtificialCommits()
    {
        AppSettings.RevisionGraphShowArtificialCommits = !AppSettings.RevisionGraphShowArtificialCommits;
        ReloadCurrentView();
    }

    internal void ToggleAuthorDateSort()
    {
        AppSettings.RevisionSortOrder.Value = AppSettings.RevisionSortOrder != RevisionSortOrder.AuthorDate
            ? RevisionSortOrder.AuthorDate
            : RevisionSortOrder.GitDefault;
        ReloadCurrentView();
    }

    internal void ToggleTopoOrder()
    {
        AppSettings.RevisionSortOrder.Value = AppSettings.RevisionSortOrder != RevisionSortOrder.Topology
            ? RevisionSortOrder.Topology
            : RevisionSortOrder.GitDefault;
        ReloadCurrentView();
    }

    /// <inheritdoc />
    public void ToggleShowReflogReferences()
    {
        _filterInfo.ShowReflogReferences = !_filterInfo.ShowReflogReferences;
        RefreshFilteredRevisions();
    }

    internal void ToggleShowStashes()
    {
        AppSettings.ShowStashes = !AppSettings.ShowStashes;
        ReloadCurrentView();
    }

    internal void ToggleShowSuperprojectTags()
    {
        AppSettings.ShowSuperprojectTags = !AppSettings.ShowSuperprojectTags;
        ReloadCurrentView();
    }

    internal void ShowSuperprojectBranches_ToolStripMenuItemClick()
    {
        AppSettings.ShowSuperprojectBranches = !AppSettings.ShowSuperprojectBranches;
        ReloadCurrentView();
    }

    internal void ShowSuperprojectRemoteBranches_ToolStripMenuItemClick()
    {
        AppSettings.ShowSuperprojectRemoteBranches = !AppSettings.ShowSuperprojectRemoteBranches;
        ReloadCurrentView();
    }

    private void RevertCommitToolStripMenuItemClick(object sender, EventArgs e)
    {
        IReadOnlyList<GitRevision> revisions = GetSelectedRevisions(SortDirection.Ascending);
        foreach (GitRevision revision in revisions)
        {
            UICommands.StartRevertCommitDialog(GetOwner(), revision);
        }
    }

    private void CherryPickCommitToolStripMenuItemClick(object sender, EventArgs e)
    {
        IReadOnlyList<GitRevision> revisions = GetSelectedRevisions(SortDirection.Descending);
        UICommands.StartCherryPickDialog(GetOwner(), revisions);
    }

    private void ApplyStashToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (SelectedRevision is GitRevision revision)
        {
            UICommands.StashApply(GetOwner(), revision.ObjectId.ToString());
            ReloadCurrentView();
        }
    }

    private void PopStashToolStripMenuItemClick(object sender, EventArgs e)
    {
        string? stashName = SelectedRevision?.ReflogSelector;
        if (!string.IsNullOrEmpty(stashName))
        {
            UICommands.StashPop(GetOwner(), stashName);
            ReloadCurrentView();
        }
    }

    private void DropStashToolStripMenuItemClick(object sender, EventArgs e)
    {
        string? stashName = SelectedRevision?.ReflogSelector;
        if (string.IsNullOrEmpty(stashName))
        {
            return;
        }

        if (!AppSettings.DontConfirmStashDrop)
        {
            TaskDialogPage page = new()
            {
                Text = TranslatedStrings.AreYouSure,
                Caption = TranslatedStrings.StashDropConfirmTitle,
                Heading = TranslatedStrings.CannotBeUndone,
                Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                Icon = TaskDialogIcon.Information,
                Verification = new TaskDialogVerificationCheckBox { Text = TranslatedStrings.DontShowAgain },
                SizeToContent = true,
            };
            TaskDialogButton result = TaskDialog.ShowDialog(GetOwner(), page);
            if (page.Verification.Checked)
            {
                AppSettings.DontConfirmStashDrop = true;
            }

            if (result != TaskDialogButton.Yes)
            {
                return;
            }
        }

        UICommands.StashDrop(GetOwner(), stashName);
        ReloadCurrentView();
    }

    private void FixupCommitToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (SelectedRevision is GitRevision revision)
        {
            UICommands.StartFixupCommitDialog(GetOwner(), revision);
        }
    }

    private void SquashCommitToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (SelectedRevision is GitRevision revision)
        {
            UICommands.StartSquashCommitDialog(GetOwner(), revision);
        }
    }

    private void AmendCommitToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (SelectedRevision is GitRevision revision)
        {
            UICommands.StartAmendCommitDialog(GetOwner(), revision);
        }
    }

    private void SelectInLeftPanel_Click(object? sender, EventArgs e)
    {
        mainContextMenu.Close();
        string? gitRef = sender != tsmiSelectInLeftPanel && sender is MenuItem item
            ? item.Header?.ToString()
            : tsmiSelectInLeftPanel.Tag as string;
        if (!string.IsNullOrEmpty(gitRef))
        {
            SelectInLeftPanel?.Invoke(gitRef);
        }
    }

    internal void ToggleShowRelativeDate()
    {
        AppSettings.RelativeDate = !AppSettings.RelativeDate;
        ApplySettingsAndRefreshRows();
    }

    /// <summary>
    ///  Gets the tracked change count for an artificial revision.
    /// </summary>
    public ArtificialCommitChangeCount? GetChangeCount(ObjectId objectId)
        => objectId == ObjectId.WorkTreeId
            ? _workTreeChangeCount
            : objectId == ObjectId.IndexId
                ? _indexChangeCount
                : null;

    /// <summary>
    ///  Updates the Working directory and Commit index counters from one parsed status.
    /// </summary>
    public void UpdateArtificialCommitCount(IReadOnlyList<GitItemStatus>? status)
    {
        // Note that the count is updated also if AppSettings.ShowGitStatusForArtificialCommits is not set
        UpdateChangeCount(ObjectId.WorkTreeId, StagedStatus.WorkTree);
        UpdateChangeCount(ObjectId.IndexId, StagedStatus.Index);
        RefreshRealizedRows();

        void UpdateChangeCount(ObjectId objectId, StagedStatus staged)
        {
            ArtificialCommitChangeCount changeCount = GetChangeCount(objectId)
                ?? throw new InvalidOperationException($"Unexpected artificial revision id {objectId}.");
            changeCount.Update(status?.Where(item => item.Staged == staged).ToList());
        }
    }

    internal void ToggleDrawNonRelativesGray()
    {
        AppSettings.RevisionGraphDrawNonRelativesGray = !AppSettings.RevisionGraphDrawNonRelativesGray;
        ApplySettingsAndRefreshRows();
    }

    private void MarkRevisionAsBadToolStripMenuItemClick(object sender, EventArgs e)
    {
        ContinueBisect(GitBisectOption.Bad);
    }

    private void MarkRevisionAsGoodToolStripMenuItemClick(object sender, EventArgs e)
    {
        ContinueBisect(GitBisectOption.Good);
    }

    private void BisectSkipRevisionToolStripMenuItemClick(object sender, EventArgs e)
    {
        ContinueBisect(GitBisectOption.Skip);
    }

    private void ContinueBisect(GitBisectOption bisectOption)
    {
        if (SelectedRevision is not GitRevision revision)
        {
            return;
        }

        ArgumentString command = Commands.ContinueBisect(bisectOption, revision.ObjectId);
        FormProcess.ShowDialog(GetOwner(), UICommands, arguments: command, Module.WorkingDir, input: null, useDialogSettings: false);
        ReloadCurrentView();
    }

    private void StopBisectToolStripMenuItemClick(object sender, EventArgs e)
    {
        FormProcess.ShowDialog(GetOwner(), UICommands, arguments: Commands.StopBisect(), Module.WorkingDir, input: null, useDialogSettings: true);
        ReloadCurrentView();
    }

    internal void ToggleShowGitNotes()
    {
        AppSettings.ShowGitNotes = !AppSettings.ShowGitNotes;
        UpdateViewMenuChecks();
        ReloadCurrentView();
    }

    internal void ToggleShowSessionRefs()
    {
        AppSettings.ShowSessionRefs = !AppSettings.ShowSessionRefs;
        ReloadCurrentView();
    }

    internal void ToggleShowGitNotesColumn()
    {
        AppSettings.ShowGitNotesColumn.Value = !AppSettings.ShowGitNotesColumn.Value;
        ReloadCurrentView();
    }

    internal void ToggleHideMergeCommits()
    {
        AppSettings.HideMergeCommits = !AppSettings.HideMergeCommits;
        PerformRefreshRevisions();
    }

    internal void ToggleShowCommitBodyInRevisionGrid()
    {
        AppSettings.ShowCommitBodyInRevisionGrid = !AppSettings.ShowCommitBodyInRevisionGrid;
        ReloadCurrentView();
    }

    /// <inheritdoc />
    public void ToggleShowOnlyFirstParent()
    {
        _filterInfo.ShowOnlyFirstParent = !_filterInfo.ShowOnlyFirstParent;
        RefreshFilteredRevisions();
    }

    public void ToggleFullHistory()
    {
        AppSettings.FullHistoryInFileHistory = !AppSettings.FullHistoryInFileHistory;
        PerformRefreshRevisions();
    }

    private void ApplySettingsAndRefreshRows()
    {
        ApplyColumnSettings();
        RefreshRealizedRows();
        UpdateViewMenuChecks();
    }

    private void ReloadCurrentView()
    {
        if (TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            ReloadRevisions(
                commands.Module,
                _lastRevisionFilter,
                SelectedRevision?.ObjectId ?? default,
                _lastPathFilter);
        }
    }

    public void ToggleSimplifyMerges()
    {
        AppSettings.SimplifyMergesInFileHistory = !AppSettings.SimplifyMergesInFileHistory;
        PerformRefreshRevisions();
    }

    internal void ToggleBetweenArtificialAndHeadCommits()
    {
        if (SelectedRevision?.IsArtificial == true)
        {
            SelectCurrentRevision();
            ToggledBetweenArtificialAndHeadCommits?.Invoke(this, EventArgs.Empty);
            return;
        }

        GitRevision? artificial = _revisions.FirstOrDefault(revision => revision.ObjectId == ObjectId.WorkTreeId)
            ?? _revisions.FirstOrDefault(revision => revision.ObjectId == ObjectId.IndexId);
        if (artificial is not null)
        {
            SetSelectedRevision(artificial.ObjectId);
        }

        ToggledBetweenArtificialAndHeadCommits?.Invoke(this, EventArgs.Empty);
    }

    internal void ToggleRevisionGraphColumn()
    {
        AppSettings.ShowRevisionGridGraphColumn = !AppSettings.ShowRevisionGridGraphColumn;
        ApplySettingsAndRefreshRows();
    }

    internal void ToggleAuthorAvatarColumn()
    {
        AppSettings.ShowAuthorAvatarColumn = !AppSettings.ShowAuthorAvatarColumn;
        ApplySettingsAndRefreshRows();
    }

    internal void ToggleAuthorNameColumn()
    {
        AppSettings.ShowAuthorNameColumn = !AppSettings.ShowAuthorNameColumn;
        ApplySettingsAndRefreshRows();
    }

    internal void ToggleDateColumn()
    {
        AppSettings.ShowDateColumn = !AppSettings.ShowDateColumn;
        ApplySettingsAndRefreshRows();
    }

    internal void ToggleObjectIdColumn()
    {
        AppSettings.ShowObjectIdColumn = !AppSettings.ShowObjectIdColumn;
        ApplySettingsAndRefreshRows();
    }

    internal void ToggleBuildStatusIconColumn()
    {
        ////Module.EffectiveSettings.BuildServer.ShowBuildIconInGrid.Value = !Module.EffectiveSettings.BuildServer.ShowBuildIconInGrid.Value;
        AppSettings.ShowBuildStatusIconColumn = !AppSettings.ShowBuildStatusIconColumn;
        ApplySettingsAndRefreshRows();
    }

    internal void ToggleBuildStatusTextColumn()
    {
        ////Module.EffectiveSettings.BuildServer.ShowBuildSummaryInGrid.Value = !Module.EffectiveSettings.BuildServer.ShowBuildSummaryInGrid.Value;
        AppSettings.ShowBuildStatusTextColumn = !AppSettings.ShowBuildStatusTextColumn;
        ApplySettingsAndRefreshRows();
    }

    internal void ToggleShowTags()
    {
        AppSettings.ShowTags = !AppSettings.ShowTags;
        ApplySettingsAndRefreshRows();
    }

    protected override bool ExecuteCommand(int command)
    {
        switch ((Command)command)
        {
            case Command.ToggleRevisionGraph: ToggleRevisionGraphColumn(); break;
            case Command.RevisionFilter: ShowRevisionFilterDialog(); break;
            case Command.ResetRevisionFilter: ResetAllFiltersAndRefresh(); break;
            case Command.ResetRevisionPathFilter: SetAndApplyPathFilter(string.Empty); break;
            case Command.ToggleAuthorDateCommitDate: ToggleShowAuthorDate(); break;
            case Command.ToggleShowRelativeDate: ToggleShowRelativeDate(); break;
            case Command.ToggleDrawNonRelativesGray: ToggleDrawNonRelativesGray(); break;
            case Command.ToggleShowGitNotes: ToggleShowGitNotes(); break;
            case Command.ToggleShowGitNotesColumn: ToggleShowGitNotesColumn(); break;
            case Command.ToggleHideMergeCommits: ToggleHideMergeCommits(); break;
            case Command.ToggleShowTags: ToggleShowTags(); break;
            case Command.ShowAllBranches: ShowAllBranches(); break;
            case Command.ShowCurrentBranchOnly: ShowCurrentBranchOnly(); break;
            case Command.ShowFilteredBranches: ShowFilteredBranches(); break;
            case Command.ShowReflogReferences: ToggleShowReflogReferences(); break;
            case Command.ShowRemoteBranches: ToggleShowRemoteBranches(); break;
            case Command.ShowFirstParent: ToggleShowOnlyFirstParent(); break;
            case Command.SelectCurrentRevision: SelectCurrentRevision(); break;
            case Command.GoToParent:
                return GoToParent(firstParent: true, useHistory: true);
            case Command.GoToFirstParent:
                return GoToParent(firstParent: true, useHistory: false);
            case Command.GoToLastParent: return GoToParent(firstParent: false, useHistory: false);
            case Command.GoToChild: return GoToChild();
            case Command.GoToCommit: MenuCommands.GotoCommitExecute(); break;
            case Command.GoToMergeBase: GoToMergeBase(); break;
            case Command.SelectNextForkPointAsDiffBase: SelectNextForkPointAsDiffBase(); break;
            case Command.NextQuickSearch: _quickSearchProvider.NextResult(down: true); break;
            case Command.PrevQuickSearch: _quickSearchProvider.NextResult(down: false); break;
            case Command.NavigateBackward:
            case Command.NavigateBackward_AlternativeHotkey: NavigateBackward(); break;
            case Command.NavigateForward:
            case Command.NavigateForward_AlternativeHotkey: NavigateForward(); break;
            case Command.ToggleBetweenArtificialAndHeadCommits: ToggleBetweenArtificialAndHeadCommits(); break;
            case Command.ToggleHighlightSelectedBranch: HighlightSelectedBranch(); break;
            case Command.SelectAsBaseToCompare: selectAsBaseToolStripMenuItem_Click(this, EventArgs.Empty); break;
            case Command.CompareToBase: compareToBaseToolStripMenuItem_Click(this, EventArgs.Empty); break;
            case Command.CreateFixupCommit: FixupCommitToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.CreateSquashCommit: SquashCommitToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.CreateAmendCommit: AmendCommitToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.OpenCommitsWithDifftool: DiffSelectedCommitsWithDifftool(); break;
            case Command.CompareToWorkingDirectory: compareToWorkingDirectoryMenuItem_Click(this, EventArgs.Empty); break;
            case Command.CompareToCurrentBranch: CompareWithCurrentBranchToolStripMenuItem_Click(this, EventArgs.Empty); break;
            case Command.CompareToBranch: CompareToBranchToolStripMenuItem_Click(this, EventArgs.Empty); break;
            case Command.CompareSelectedCommits: compareSelectedCommitsMenuItem_Click(this, EventArgs.Empty); break;
            case Command.DeleteRef: DeleteRef(); break;
            case Command.RenameRef: RenameRef(); break;
            default: return base.ExecuteCommand(command);
        }

        return true;
    }

    private void HighlightSelectedBranch()
    {
        if (SelectedRevision is not GitRevision revision)
        {
            return;
        }

        HighlightBranch(revision.ObjectId);
    }

    private void PerformFirstDropdownItemClick(object sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem { Items.Count: 1 } item
            && item.Items[0] is MenuItem firstItem)
        {
            firstItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }
    }

    private void GoToFirstParent()
        => GoToParent(firstParent: true, useHistory: false);

    private void GoToLastParent()
        => GoToParent(firstParent: false, useHistory: false);

    private void goToParentToolStripMenuItem_Click()
        => GoToParent(firstParent: true, useHistory: true);

    private void SelectNextForkPointAsDiffBase()
    {
        IReadOnlyList<GitRevision> revisions = GetSelectedRevisions();
        if (revisions.Count == 0)
        {
            return;
        }

        GitRevision revision = revisions[^1];
        while (revision.IsArtificial && GetRevision(revision.FirstParentId) is GitRevision parent)
        {
            revision = parent;
        }

        while (GetRevision(revision.FirstParentId) is GitRevision previous
               && !revision.Refs.Any(gitRef => gitRef.IsHead || gitRef.IsRemote)
               && GetRevisionChildren(revision.ObjectId).Count == 1)
        {
            revision = previous;
        }

        SetSelectedRevision(revision.ObjectId, toggleSelection: false, updateNavigationHistory: false);
        foreach (GitRevision selectedRevision in revisions.Take(Math.Max(1, revisions.Count - 1)))
        {
            SetSelectedRevision(selectedRevision.ObjectId, toggleSelection: true, updateNavigationHistory: false);
        }
    }

    private void GoToMergeBase()
    {
        List<ObjectId> revisions = [.. GetSelectedRevisions().Select(revision => revision.ObjectId).Where(id => !id.IsArtificial)];
        bool hasArtificial = GetSelectedRevisions().Any(revision => revision.IsArtificial);
        ObjectId headId = Module.RevParse("HEAD");
        if (headId.IsZero || (revisions.Count == 0 && !hasArtificial))
        {
            return;
        }

        GitArgumentBuilder args = new("merge-base")
        {
            { revisions.Count > 2 || (revisions.Count == 2 && hasArtificial), "--octopus" },
            { revisions.Count < 1, headId.ToString() },
            { revisions.Count < 2, headId.ToString() },
            revisions
        };

        ExecutionResult result = Module.GitExecutable.Execute(args, throwOnErrorExit: false);
        const int NoCommonAncestorsExitCode = 1;
        if (result.ExitCode == NoCommonAncestorsExitCode)
        {
            MessageBoxes.ShowError(this, _noMergeBaseCommit.Text);
            return;
        }

        result.ThrowIfErrorExit();
        string mergeBaseCommitId = result.StandardOutput.TrimEnd();
        if (string.IsNullOrWhiteSpace(mergeBaseCommitId))
        {
            MessageBoxes.ShowError(this, _noMergeBaseCommit.Text);
            return;
        }

        ObjectId commitId = ObjectId.Parse(mergeBaseCommitId);
        if (!SetSelectedRevision(commitId))
        {
            MessageBoxes.RevisionFilteredInGrid(this, commitId);
        }
    }

    private void goToChildToolStripMenuItem_Click()
        => GoToChild();

    private void UpdateNavigationMenu(GitRevision? revision)
    {
        bool hasCurrentRevision = _headId is ObjectId headId
            && _revisions.Any(candidate => candidate.ObjectId == headId);
        GotoCurrentRevisionMenuItem.IsEnabled = hasCurrentRevision;
        ToggleBetweenArtificialAndHeadCommitsMenuItem.IsEnabled = revision is not null
            && hasCurrentRevision
            && _revisions.Any(candidate => candidate.ObjectId == ObjectId.WorkTreeId
                || candidate.ObjectId == ObjectId.IndexId);
        GitRevision? actualRevision = revision is null ? null : GetActualRevision(revision);
        bool hasParent = actualRevision?.ParentIds is { Count: > 0 };
        GotoParentCommitMenuItem.IsEnabled = hasParent;
        GotoFirstParentCommitMenuItem.IsEnabled = hasParent;
        GotoLastParentCommitMenuItem.IsEnabled = hasParent;
        GotoChildCommitMenuItem.IsEnabled = revision is not null
            && _revisions.Any(candidate => candidate.ParentIds?.Contains(revision.ObjectId) == true);
    }

    private void UpdateViewMenuChecks()
    {
        ShowAllBranchesMenuItem.IsChecked = _filterInfo.IsShowAllBranchesChecked;
        ShowCurrentBranchOnlyMenuItem.IsChecked = _filterInfo.IsShowCurrentBranchOnlyChecked;
        ShowFilteredBranchesMenuItem.IsChecked = _filterInfo.IsShowFilteredBranchesChecked;
        ShowReflogReferencesMenuItem.IsChecked = _filterInfo.ShowReflogReferences;
        DrawNonRelativesGrayMenuItem.IsChecked = AppSettings.RevisionGraphDrawNonRelativesGray;
        HighlightSelectedBranchMenuItem.IsEnabled = SelectedRevision is not null;
        ShowGitNotesMenuItem.IsChecked = AppSettings.ShowGitNotes;
        ShowRemoteBranchesMenuItem.IsChecked = AppSettings.ShowRemoteBranches;
        ShowTagsMenuItem.IsChecked = AppSettings.ShowTags;
        ShowAuthorDateMenuItem.IsChecked = AppSettings.ShowAuthorDate;
        ShowRelativeDateMenuItem.IsChecked = AppSettings.RelativeDate;
        ShowRevisionGraphColumnMenuItem.IsChecked = AppSettings.ShowRevisionGridGraphColumn;
        ShowGitNotesColumnMenuItem.IsChecked = AppSettings.ShowGitNotesColumn.Value;
        ShowAuthorNameColumnMenuItem.IsChecked = AppSettings.ShowAuthorNameColumn;
        ShowDateColumnMenuItem.IsChecked = AppSettings.ShowDateColumn;
        ShowIdColumnMenuItem.IsChecked = AppSettings.ShowObjectIdColumn;
    }

    private MenuItem GetMenuItem(string name)
    {
        IEnumerable<MenuItem> menuItems = navigateToolStripMenuItem.Items.OfType<MenuItem>()
            .Concat(viewToolStripMenuItem.Items.OfType<MenuItem>());
        return menuItems.Single(menuItem => menuItem.Tag as string == name);
    }

    private static MenuItem GetMenuItem(MenuItem parent, string name)
        => parent.Items.OfType<MenuItem>().Single(menuItem => menuItem.Tag as string == name);

    public void GoToRef(string? refName, bool showNoRevisionMsg, bool toggleSelection = false)
    {
        if (string.IsNullOrEmpty(refName))
        {
            return;
        }

        if (DetachedHeadParser.TryParse(refName, out string? sha1))
        {
            refName = sha1;
        }

        ObjectId commitId = Module.RevParse(refName);
        if (!commitId.IsZero)
        {
            if (!SetSelectedRevision(commitId, toggleSelection) && showNoRevisionMsg)
            {
                MessageBoxes.RevisionFilteredInGrid(this, commitId);
            }
        }
        else if (showNoRevisionMsg)
        {
            MessageBoxes.ShowError(this, _noRevisionFoundError.Text);
        }
    }

    internal bool TryGoToRelatedRef(IGitRef gitRef)
    {
        ObjectId selectedId = SelectedId;
        GoToRelatedRef(gitRef);
        return SelectedId != selectedId;
    }

    private void GoToRelatedRef(IGitRef gitRef, Action<string>? handleGone = null, bool toggleSelection = false)
    {
        if (gitRef is NestledVirtualRef nestledRef)
        {
            if (nestledRef.TrackingBranchIsGone)
            {
                handleGone?.Invoke(nestledRef.MergeWith);
            }
            else
            {
                GoToRef(nestledRef.CompleteName, showNoRevisionMsg: true, toggleSelection);
            }
        }
        else if (_messageColumnProvider.GetAheadBehindData(gitRef.IsRemote, gitRef.CompleteName) is { } aheadBehindData)
        {
            if (aheadBehindData.AheadCount == AheadBehindData.Gone)
            {
                handleGone?.Invoke(gitRef.Name);
            }
            else
            {
                GoToRef(gitRef.IsRemote ? aheadBehindData.Branch : aheadBehindData.RemoteRef, showNoRevisionMsg: true, toggleSelection);
            }
        }
        else
        {
            GoToRef(gitRef.ObjectId.ToString(), showNoRevisionMsg: true, toggleSelection);
        }
    }

    internal void SetFilterShortcutKeys(FilterToolBar filterBar)
        => filterBar.SetShortcutKeys((item, command) =>
            item.InputGesture = KeysMapper.ToKeyGesture(
                Hotkeys.FirstOrDefault(hotkey => hotkey.CommandCode == (int)command)?.KeyData));

    internal void SetShortcutKeys()
    {
        RefreshMenuShortcutKeys(Hotkeys);
        SetShortcutString(fixupCommitToolStripMenuItem, Command.CreateFixupCommit);
        SetShortcutString(squashCommitToolStripMenuItem, Command.CreateSquashCommit);
        SetShortcutString(amendCommitToolStripMenuItem, Command.CreateAmendCommit);
        SetShortcutString(selectAsBaseToolStripMenuItem, Command.SelectAsBaseToCompare);
        SetShortcutString(openCommitsWithDiffToolMenuItem, Command.OpenCommitsWithDifftool);
        SetShortcutString(compareToBaseToolStripMenuItem, Command.CompareToBase);
        SetShortcutString(compareToWorkingDirectoryMenuItem, Command.CompareToWorkingDirectory);
        SetShortcutString(compareSelectedCommitsMenuItem, Command.CompareSelectedCommits);
    }

    private void SetShortcutString(ToolStripMenuItem item, Command command)
        => item.InputGesture = KeysMapper.ToKeyGesture(
            Hotkeys.FirstOrDefault(hotkey => hotkey.CommandCode == (int)command)?.KeyData);

    private void ShowFormDiff(ObjectId baseCommitSha, ObjectId headCommitSha, string baseCommitDisplayStr, string headCommitDisplayStr)
    {
        FormDiff diffForm = new(UICommands, baseCommitSha, headCommitSha, baseCommitDisplayStr, headCommitDisplayStr)
        {
            ShowInTaskbar = true
        };

        diffForm.Show();
    }

    private void CompareToBranchToolStripMenuItem_Click(object sender, EventArgs e)
    {
        GitRevision? headCommit = SelectedRevision;
        if (headCommit is null)
        {
            return;
        }

        using FormCompareToBranch form = new(UICommands, headCommit.ObjectId);
        if (form.ShowDialog(GetOwner()) == WinFormsShims.DialogResult.OK)
        {
            Validates.NotNull(form.BranchName);
            ObjectId baseCommit = Module.RevParse(form.BranchName);
            if (baseCommit.IsZero)
            {
                MessageBoxes.ShowError(this, _noRevisionFoundError.Text);
                return;
            }

            ShowFormDiff(baseCommit, headCommit.ObjectId, form.BranchName, headCommit.Subject);
        }
    }

    private void CompareWithCurrentBranchToolStripMenuItem_Click(object sender, EventArgs e)
    {
        string currentBranch = Module.GetSelectedBranch();
        if (string.IsNullOrWhiteSpace(currentBranch) || CurrentCheckout.IsZero)
        {
            MessageBoxes.Show(this, "No branch is currently selected", TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        GitRevision? baseCommit = SelectedRevision;
        if (baseCommit is null)
        {
            return;
        }

        ShowFormDiff(baseCommit.ObjectId, CurrentCheckout, baseCommit.Subject, currentBranch);
    }

    private void selectAsBaseToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _baseCommitToCompare = SelectedRevision;
        compareToBaseToolStripMenuItem.IsEnabled = _baseCommitToCompare is not null;
    }

    private void compareToBaseToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (_baseCommitToCompare is null)
        {
            MessageBoxes.Show(this, _baseForCompareNotSelectedError.Text, TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        GitRevision? headCommit = SelectedRevision;
        if (headCommit is null)
        {
            return;
        }

        ShowFormDiff(_baseCommitToCompare.ObjectId, headCommit.ObjectId, _baseCommitToCompare.Subject, headCommit.Subject);
    }

    private void compareToWorkingDirectoryMenuItem_Click(object sender, EventArgs e)
    {
        GitRevision? baseCommit = SelectedRevision;
        if (baseCommit is null)
        {
            return;
        }

        if (baseCommit.ObjectId == ObjectId.WorkTreeId)
        {
            MessageBoxes.Show(this, "Cannot diff working directory to itself", TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        ShowFormDiff(baseCommit.ObjectId, ObjectId.WorkTreeId, baseCommit.Subject, "Working directory");
    }

    private void compareSelectedCommitsMenuItem_Click(object sender, EventArgs e)
    {
        (ObjectId firstId, GitRevision? selected) = GetFirstAndSelected();

        if (selected is not null && !firstId.IsZero)
        {
            string firstSubject = GetRevision(firstId)?.Subject ?? "";
            ShowFormDiff(firstId, selected.ObjectId, firstSubject, selected.Subject);
        }
        else
        {
            MessageBoxes.Show(this, "You must have two commits selected to compare", TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
        }
    }

    private void diffSelectedCommitsMenuItem_Click(object? sender, EventArgs e)
    {
        DiffSelectedCommitsWithDifftool();
    }

    public void DiffSelectedCommitsWithDifftool(string? customTool = null)
    {
        IReadOnlyList<GitRevision> selectedRevisions = GetSelectedRevisions();
        if (selectedRevisions.Count > 0)
        {
            string? first = selectedRevisions.Count > 1 ? selectedRevisions[1].ObjectId.ToString() : null;
            Module.OpenWithDifftoolDirDiff(first, selectedRevisions[0].ObjectId.ToString(), customTool: customTool);
        }
    }

    private void getHelpOnHowToUseTheseFeaturesToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser(
            GitUI.UserManual.UserManual.UrlFor("modify_history", "using-autosquash-rebase-feature"));
    }

    private bool RenameSingleRef()
    {
        if (SelectedRevision is not GitRevision revision)
        {
            return false;
        }

        IReadOnlyList<IGitRef> refs = new GitRefListsForRevision(revision).GetRenameableLocalBranches();
        if (refs.Count != 1)
        {
            return false;
        }

        UICommands.StartRenameDialog(GetOwner(), refs[0].Name);
        return true;
    }

    private void openBuildReportToolStripMenuItem_Click(object sender, EventArgs e)
    {
        OpenBuildReport(SelectedRevision);
    }

    private static void OpenBuildReport(GitRevision? revision)
        => OsShellUtil.OpenUrlInDefaultBrowser(revision?.BuildStatus?.Url);

    private string BuildPathFilter(IGitModule module, string? path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        FilePathByObjectId?.Clear();

        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        path = path.Trim();
        bool multipleArgs = false;
        if (!path.Any(c => c == '"') && !path.Any(c => c == '\''))
        {
            if (!path.Any(c => c == ' '))
            {
                path = path.Quote();
            }
            else
            {
                multipleArgs = true;
            }
        }
        else if (path.Count(c => c == '"') + path.Count(c => c == '\'') > 2)
        {
            multipleArgs = true;
        }

        if (!AppSettings.FollowRenamesInFileHistory
            || path.EndsWith('/')
            || path.EndsWith("/\"")
            || multipleArgs)
        {
            return path;
        }

        GitArgumentBuilder args = new("log")
        {
            $"--format=\"{_objectIdPrefix}%H\"",
            "--name-only",
            "--follow",
            FindRenamesAndCopiesOpts(),
            "--",
            path.QuoteIfNotQuotedAndNE(),
        };

        HashSet<string> fileNames = [];
        foreach (string fileName in ParseFileNames(module, args, cancellationToken))
        {
            fileNames.Add(fileName);
        }

        string pathFilter = fileNames.Count == 0
            ? path
            : string.Join(string.Empty, fileNames.Select(fileName => @$" ""{fileName}"""));
        if (pathFilter.Length <= 31000)
        {
            return pathFilter;
        }

        this.InvokeAndForget(() => MessageBoxes.ShowError(
            GetOwner(),
            $"Ignoring too long pathfilter ({pathFilter.Length}). (Are you trying to filter a folder?)",
            "Cannot follow file renames"));
        return path;
    }

    private void openPullRequestPageStripMenuItem_Click(object? sender, EventArgs e)
    {
        string? url = SelectedRevision?.BuildStatus?.PullRequestUrl;
        if (!string.IsNullOrWhiteSpace(url))
        {
            OsShellUtil.OpenUrlInDefaultBrowser(url);
        }
    }

    private static async Task<SuperProjectInfo?> GetSuperprojectCheckoutAsync(IGitModule module)
    {
        if (module.SuperprojectModule is null)
        {
            return null;
        }

        SuperProjectInfo superProjectInfo = new();
        (char code, ObjectId commit) = await module.GetSuperprojectCurrentCheckoutAsync().ConfigureAwait(false);
        if (code == 'U')
        {
            ConflictData conflict = await module.SuperprojectModule.GetConflictAsync(module.SubmodulePath).ConfigureAwait(false);
            superProjectInfo.ConflictBase = conflict.Base.ObjectId;
            superProjectInfo.ConflictLocal = conflict.Local.ObjectId;
            superProjectInfo.ConflictRemote = conflict.Remote.ObjectId;
        }
        else
        {
            superProjectInfo.CurrentCommit = commit;
        }

        Dictionary<IGitRef, IGitItem?> refs = await module.SuperprojectModule
            .GetSubmoduleItemsForEachRefAsync(module.SubmodulePath, noLocks: true)
            .ConfigureAwait(false);
        superProjectInfo.Refs = refs
            .Where(item => item.Value is not null && !item.Value.ObjectId.IsZero)
            .GroupBy(item => item.Value!.ObjectId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<IGitRef>)[.. group.Select(item => item.Key)]);
        return superProjectInfo;
    }

    /// <summary>
    ///  Adds a batch to the shared graph model on the reader thread (like the WinForms grid
    ///  does), so rows are already shaped when the UI displays them.
    /// </summary>
    private void AddToGraph(IReadOnlyList<GitRevision> batch, CancellationToken cancellationToken)
    {
        foreach (GitRevision revision in batch)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (_refsByObjectId is not null)
            {
                revision.Refs = [.. _refsByObjectId[revision.ObjectId]];
            }

            _revisionGraph.Add(revision);
        }

        // Mark the current checkout and its ancestry as relative once its node has arrived;
        // without this every lane renders in the non-relative gray.
        if (!_headHighlighted && _headId is ObjectId headId && _revisionGraph.TryGetNode(headId, out _))
        {
            _revisionGraph.HighlightBranch(headId);
            _headHighlighted = true;
        }

        int lastRowIndex = _revisionGraph.Count - 1;
        _revisionGraph.CacheTo(lastRowIndex, lastRowIndex, cancellationToken);
    }

    private void AppendRevisions(IReadOnlyList<GitRevision> batch, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        _revisions.AddRange(batch);

        // Avalonia observes the range notification, so append in place to preserve selection
        // and the virtualized list's scroll anchor while the reader streams new rows.
        SetPage(_gridView);
        FocusRevisionGridWhenShown();
        lblLoadingStatus.Text = $"{_revisions.Count} revisions…";
        SelectPendingRevision();
    }

    private void OnLoadingCompleted(CancellationToken cancellationToken)
    {
        _isRefreshingRevisions = false;
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        // Avalonia's observable stream preserves arrival order, while the original grid exposes
        // RevisionGraph's final scored order after artificial revisions have been inserted.
        GitRevision[] orderedRevisions = Enumerable.Range(0, _revisionGraph.Count)
            .Select(index => _revisionGraph.GetNodeForRow(index)?.GitRevision)
            .OfType<GitRevision>()
            .ToArray();
        if (!_revisions.SequenceEqual(orderedRevisions))
        {
            ApplyFinalRevisionOrder(orderedRevisions);
        }

        if (_revisions.Count == 0 && !_filterInfo.HasFilter)
        {
            SetPage(new EmptyRepoControl(_lastModule?.IsBareRepository() == true));
        }
        else
        {
            SetPage(_gridView);
            FocusRevisionGridWhenShown();

            // The graph rows straightened after the final CacheTo become visible only when the
            // realized row controls render again, so refresh the realized rows once at the end.
            RefreshRealizedRows();
        }

        lblLoadingStatus.Text = $"{_revisions.Count} revisions";
        SelectPendingRevision();

        // Like the WinForms grid, select a row when loading finishes.
        if (_gridView.SelectedItem is null && _revisions.Count > 0)
        {
            _gridView.SelectedIndex = 0;
        }
    }

    private void ApplyFinalRevisionOrder(IReadOnlyList<GitRevision> orderedRevisions)
    {
        bool restoreFocus = _gridView.IsKeyboardFocusWithin;
        GitRevision? selectedRevision = SelectedRevision;
        GitRevision[] selectedRevisions = [.. _gridView.SelectedItems?.OfType<GitRevision>() ?? []];
        HashSet<GitRevision> finalRevisions = new(orderedRevisions, ReferenceEqualityComparer.Instance);
        GitRevision? restoredPrimarySelection = selectedRevision is not null && finalRevisions.Contains(selectedRevision)
            ? selectedRevision
            : null;

        // Avalonia resets selection and focus when its items collection is replaced; the WinForms grid retains both.
        _revisions.Clear();
        _revisions.AddRange(orderedRevisions);

        if (restoredPrimarySelection is not null)
        {
            _gridView.SelectedItem = restoredPrimarySelection;
        }

        if (_gridView.SelectedItems is { } selectedItems)
        {
            foreach (GitRevision revision in selectedRevisions.Where(finalRevisions.Contains))
            {
                if (!selectedItems.Contains(revision))
                {
                    selectedItems.Add(revision);
                }
            }
        }

        if (restoredPrimarySelection is not null)
        {
            _gridView.ScrollIntoView(restoredPrimarySelection);
        }

        _focusGridWhenShown |= restoreFocus;
    }

    private IReadOnlyList<GitRevision> CreateArtificialRevisions(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested
            || !ShowUncommittedChangesIfPossible
            || !AppSettings.RevisionGraphShowArtificialCommits
            || _lastModule is null
            || _lastModule.IsBareRepository())
        {
            return [];
        }

        string userName = _lastModule.GetEffectiveSetting(SettingKeyString.UserName);
        string userEmail = _lastModule.GetEffectiveSetting(SettingKeyString.UserEmail);
        GitRevision workTreeRevision = new(ObjectId.WorkTreeId)
        {
            Author = userName,
            AuthorEmail = userEmail,
            AuthorUnixTime = 0,
            Committer = userName,
            CommitterEmail = userEmail,
            CommitUnixTime = 0,
            Notes = string.Empty,
            ParentIds = [ObjectId.IndexId],
            Subject = ResourceManager.TranslatedStrings.Workspace,
        };
        GitRevision indexRevision = new(ObjectId.IndexId)
        {
            Author = userName,
            AuthorEmail = userEmail,
            AuthorUnixTime = 0,
            Committer = userName,
            CommitterEmail = userEmail,
            CommitUnixTime = 0,
            Notes = string.Empty,
            ParentIds = _headId is ObjectId { IsZero: false } headId ? [headId] : null,
            Subject = ResourceManager.TranslatedStrings.Index,
        };
        return [workTreeRevision, indexRevision];
    }

    private void InsertArtificialRevisions(IReadOnlyList<GitRevision> artificialRevisions)
    {
        IReadOnlyList<ObjectId> insertionParents = _headId is ObjectId { IsZero: false } currentCheckout
            ? [currentCheckout]
            : [];
        _revisionGraph.Insert(artificialRevisions[0], artificialRevisions[1], insertionParents);
    }

    private void SelectPendingRevision()
    {
        if (!FirstId.IsZero && !_pendingSelectedObjectId.IsZero
            && SetSelectedRevision(FirstId, updateNavigationHistory: false))
        {
            SetSelectedRevision(_pendingSelectedObjectId, toggleSelection: true, updateNavigationHistory: false);
            FirstId = default;
            _pendingSelectedObjectId = default;
            return;
        }

        if (_pendingSelectedObjectId.IsZero || !SetSelectedRevision(_pendingSelectedObjectId))
        {
            return;
        }

        _pendingSelectedObjectId = default;
    }

    private void OnLoadingError(Exception exception, CancellationToken cancellationToken)
    {
        _isRefreshingRevisions = false;
        if (!cancellationToken.IsCancellationRequested)
        {
            lblLoadingStatus.Text = $"Failed to load revisions: {exception.Message}";
            SetPage(new ErrorControl());
        }
    }

    private void editCommitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        LaunchRebase("e");
    }

    private void rewordCommitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        LaunchRebase("r");
    }

    private void FocusRevisionGridWhenShown()
    {
        if (_focusGridWhenShown)
        {
            FocusRevisionGrid();
        }
    }

    private void UpdateVisibleGraphColumnWidth()
    {
        RevisionRowControl[] visibleRows =
        [
            .. _gridView.GetVisualDescendants().OfType<RevisionRowControl>(),
        ];
        _revisionGraphColumnProvider.UpdateVisibleRange(
            visibleRows.Select(row => row.DataContext).OfType<GitRevision>());
        int visibleLaneCount = visibleRows
            .Select(row => row.DataContext)
            .OfType<GitRevision>()
            .Select(_revisionGraphColumnProvider.GetLaneCount)
            .DefaultIfEmpty()
            .Max();
        int graphColumnWidth = CalculateGraphColumnWidth(visibleLaneCount);
        GridLength graphColumnGridLength = new(graphColumnWidth);
        if (_revisionGraphColumnProvider.Column.Width == graphColumnGridLength)
        {
            return;
        }

        _revisionGraphColumnProvider.Column.Width = graphColumnGridLength;
        foreach (RevisionRowControl row in visibleRows)
        {
            row.ApplyColumnLayout();
        }
    }

    internal bool DrawGraphCell(
        DrawingContext context,
        GitRevision revision,
        RevisionGraphDrawStyle drawStyle,
        double rowHeight,
        IReadOnlySet<ObjectId>? hoverHighlightedIds = null)
    {
        if (_headId is not ObjectId headId
            || !_revisionGraph.TryGetRowIndex(revision.ObjectId, out int rowIndex))
        {
            return false;
        }

        try
        {
            GraphRenderer.DrawItem(
                _revisionGraph.Config,
                context,
                rowIndex,
                Math.Max(1, (int)Math.Round(rowHeight)),
                _revisionGraph.GetSegmentsForRow,
                drawStyle,
                headId,
                hoverHighlightedIds);
            return true;
        }
        catch (Exception)
        {
            // The reader can advance the row cache while layout is painting realized rows.
            return false;
        }
    }

    private void LaunchRebase(string command)
    {
        if (SelectedRevision is not GitRevision revision)
        {
            return;
        }

        ArgumentString rebaseCmd = Commands.Rebase(new Commands.RebaseOptions()
        {
            BranchName = GetActualRevision(revision).FirstParentId is { IsZero: false } fid ? fid.ToString() : null,
            Interactive = true,
            AutoStash = true,
            SupportRebaseMerges = Module.GitVersion.SupportRebaseMerges
        });

        using FormProcess formProcess = new(UICommands, arguments: rebaseCmd, Module.WorkingDir, input: null, useDialogSettings: true);

        const string envVarNameGitSequenceEditor = "GIT_SEQUENCE_EDITOR";
        formProcess.ProcessEnvVariables.Add(envVarNameGitSequenceEditor, string.Format("sed -i -re '0,/pick/s//{0}/'", command));
        formProcess.ProcessEnvVariables.ForwardEnvironmentVariableToWsl(Module.WorkingDir, envVarNameGitSequenceEditor);

        formProcess.ShowDialog(GetOwner());
        ReloadCurrentView();
        ArtificialChanged?.Invoke(this, EventArgs.Empty);
    }

    #region Drag/drop patch files on revision grid

    private void OnGridViewDragDrop(object? sender, DragEventArgs e)
    {
        string[] fileNames = GetDroppedFileNames(e.DataTransfer);
        if (fileNames.Length == 0)
        {
            return;
        }

        this.FindAncestorOfType<Window>()?.ForceActivate();

        if (fileNames.Length > 10)
        {
            // Some users need to be protected against themselves!
            MessageBoxes.Show(this, _droppingFilesBlocked.Text, TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        foreach (string fileName in fileNames)
        {
            if (fileName.EndsWith(".patch", StringComparison.InvariantCultureIgnoreCase))
            {
                // Start apply patch dialog for each dropped patch file...
                UICommands.StartApplyPatchDialog(GetOwner(), fileName);
            }
        }
    }

    #endregion

    internal static double GetRowHeight(TemplatedControl control)
    {
        // The WinForms default is Segoe UI 9 pt. Linux font substitution must not change
        // the source grid's measured 26-DIP row height or its row-height-driven avatar width.
        const double winFormsLineSpacing = 2724;
        const double winFormsDesignEmHeight = 2048;
        double renderScale = TopLevel.GetTopLevel(control)?.RenderScaling ?? 1;
        return CalculateRowHeight(control.FontSize, winFormsLineSpacing, winFormsDesignEmHeight, renderScale);
    }

    internal static double CalculateRowHeight(
        double fontSizeDip,
        double lineSpacing,
        double designEmHeight,
        double renderScale)
    {
        if (fontSizeDip <= 0 || lineSpacing <= 0 || designEmHeight <= 0 || renderScale <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fontSizeDip));
        }

        // Avalonia exposes the font metrics directly. GDI+ MeasureString adds one eighth
        // of an em to the font line spacing, then the original truncates that physical-pixel
        // height and adds DpiUtil.Scale(9).
        const double measureStringPaddingEm = 0.125;
        double measuredTextHeightDip = fontSizeDip * ((lineSpacing / designEmHeight) + measureStringPaddingEm);
        int measuredTextHeightPx = (int)(measuredTextHeightDip * renderScale);
        int spacingPx = (int)Math.Round(RowSpacing * renderScale);
        return (measuredTextHeightPx + spacingPx) / renderScale;
    }

    private static void OnGridViewDragEnter(object? sender, DragEventArgs e)
    {
        if (CanDropPatchFiles(GetDroppedFileNames(e.DataTransfer)))
        {
            // Allow drop (copy, not move) patch files
            e.DragEffects = DragDropEffects.Copy;
            return;
        }

        // When a non-patch file is dragged, do not allow it
        e.DragEffects = DragDropEffects.None;
    }

    internal bool ExecuteCommand(Command cmd)
    {
        return ExecuteCommand((int)cmd);
    }

    private IReadOnlyList<GitRevision> GetSelectedRevisions(SortDirection? direction)
    {
        if (_gridView.SelectedItems is not { } selectedItems)
        {
            return [];
        }

        IReadOnlySet<GitRevision> selectedRevisions = selectedItems.OfType<GitRevision>().ToHashSet();
        IEnumerable<GitRevision> revisions = _revisions.Count > 0
            ? _revisions.Where(selectedRevisions.Contains)
            : _gridView.Items.OfType<GitRevision>().Where(selectedRevisions.Contains);
        if (direction == SortDirection.Descending)
        {
            revisions = revisions.Reverse();
        }

        return [.. revisions];
    }

    public IReadOnlyList<GitRevision> GetSelectedRevisions(SortDirection direction)
        => GetSelectedRevisions((SortDirection?)direction);

    public string GetCurrentBranch() => CurrentBranch.Value;

    bool ICheckRefs.Contains(ObjectId objectId)
        => _revisions.Any(revision => revision.ObjectId == objectId);

    private void LaunchBuildServerInfoFetchOperation(CancellationToken cancellationToken)
    {
        if (!ShowBuildServerInfo)
        {
            return;
        }

        _taskManager.FileAndForget(() => _buildServerWatcher.LaunchBuildServerInfoFetchOperationAsync().WaitAsync(cancellationToken));
    }

    // parity-scaffolding: exposes deterministic grid state to capture and behavior tests.
    internal TestAccessor GetTestAccessor() => new(this);

    private sealed class RevisionObserver(
        RevisionGridControl owner,
        CancellationToken cancellationToken,
        RevisionLoadEventArgs loadEventArgs) : IObserver<IReadOnlyList<GitRevision>>
    {
        private bool _artificialRevisionsAddedToStream;
        private readonly HashSet<ObjectId> _insertedStashIds = [];
        private Dictionary<ObjectId, GitRevision>? _stashesById;
        private ILookup<ObjectId, GitRevision>? _stashesByParentId;

        public void InitializeStashes(IReadOnlyCollection<GitRevision> stashes)
        {
            foreach (GitRevision stash in stashes.Where(stash => !stash.FirstParentId.IsZero))
            {
                stash.ParentIds = [stash.FirstParentId];
            }

            _stashesById = stashes.ToDictionary(stash => stash.ObjectId);
            _stashesByParentId = stashes
                .Where(stash => !stash.FirstParentId.IsZero)
                .ToLookup(stash => stash.FirstParentId);
        }

        public void OnNext(IReadOnlyList<GitRevision> value)
        {
            IReadOnlyList<GitRevision> revisions = AddStashRevisions(value);
            revisions = AddArtificialRevisionsBeforeHead(revisions);
            owner.AddToGraph(revisions, cancellationToken);
            Dispatcher.UIThread.Post(() => owner.AppendRevisions(revisions, cancellationToken));
        }

        public void OnCompleted()
        {
            IReadOnlyList<GitRevision> artificialRevisions = _artificialRevisionsAddedToStream
                ? []
                : owner.CreateArtificialRevisions(cancellationToken);
            if (artificialRevisions.Count > 0)
            {
                // HEAD was filtered out (or this is an empty repository), so use the same
                // fallback insertion path as WinForms.
                owner.InsertArtificialRevisions(artificialRevisions);
            }

            owner._revisionGraph.LoadingCompleted();

            // Finish the row cache (including segment straightening); before this final pass
            // GetSegmentsForRow reports the cache dirty and rows render without a graph.
            int lastRowIndex = owner._revisionGraph.Count - 1;
            owner._revisionGraph.CacheTo(lastRowIndex, lastRowIndex, cancellationToken);

            Dispatcher.UIThread.Post(() =>
            {
                owner.OnLoadingCompleted(cancellationToken);
                if (!cancellationToken.IsCancellationRequested)
                {
                    owner.RevisionsLoaded?.Invoke(owner, loadEventArgs);
                    owner.LaunchBuildServerInfoFetchOperation(cancellationToken);
                }
            });
        }

        public void OnError(Exception error)
            => Dispatcher.UIThread.Post(() => owner.OnLoadingError(error, cancellationToken));

        private IReadOnlyList<GitRevision> AddArtificialRevisionsBeforeHead(IReadOnlyList<GitRevision> revisions)
        {
            if (_artificialRevisionsAddedToStream || owner._headId is not ObjectId headId)
            {
                return revisions;
            }

            int headIndex = -1;
            for (int index = 0; index < revisions.Count; ++index)
            {
                if (revisions[index].ObjectId == headId)
                {
                    headIndex = index;
                    break;
                }
            }

            if (headIndex < 0)
            {
                return revisions;
            }

            IReadOnlyList<GitRevision> artificialRevisions = owner.CreateArtificialRevisions(cancellationToken);
            if (artificialRevisions.Count == 0)
            {
                return revisions;
            }

            // Match WinForms insertion timing. Adding the artificial children before HEAD
            // lets RevisionGraph.Add resolve Commit index -> HEAD when HEAD arrives.
            List<GitRevision> revisionsWithArtificial = new(revisions.Count + artificialRevisions.Count);
            revisionsWithArtificial.AddRange(revisions.Take(headIndex));
            revisionsWithArtificial.AddRange(artificialRevisions);
            revisionsWithArtificial.AddRange(revisions.Skip(headIndex));
            _artificialRevisionsAddedToStream = true;
            return revisionsWithArtificial;
        }

        private IReadOnlyList<GitRevision> AddStashRevisions(IReadOnlyList<GitRevision> revisions)
        {
            if (_stashesById is not { } stashesById
                || (stashesById.Count == 0 && _insertedStashIds.Count == 0))
            {
                return revisions;
            }

            List<GitRevision> revisionsWithStashes = new(revisions.Count + stashesById.Count);
            foreach (GitRevision revision in revisions)
            {
                if (_insertedStashIds.Contains(revision.ObjectId))
                {
                    // The --all stream can report refs/stash after its parent batch already inserted it.
                    continue;
                }

                if (stashesById.Remove(revision.ObjectId, out GitRevision? stash))
                {
                    revision.ReflogSelector = stash.ReflogSelector;
                    _insertedStashIds.Add(revision.ObjectId);
                }
                else if (_stashesByParentId is not null)
                {
                    foreach (GitRevision parentStash in _stashesByParentId[revision.ObjectId])
                    {
                        if (stashesById.Remove(parentStash.ObjectId))
                        {
                            revisionsWithStashes.Add(parentStash);
                            _insertedStashIds.Add(parentStash.ObjectId);
                        }
                    }
                }

                revisionsWithStashes.Add(revision);
            }

            return revisionsWithStashes;
        }
    }

    /// <summary>One provider-shaped row recycled by the virtualizing panel.</summary>
    private sealed class RevisionRowControl : Grid
    {
        private readonly List<(ColumnProvider Provider, Control Cell)> _cells = [];
        private readonly RevisionGridControl _owner;

        public RevisionRowControl(RevisionGridControl owner)
        {
            _owner = owner;
            Classes.Add("revision-row");
            AttachedToVisualTree += (_, _) => UpdateColorClasses();

            foreach (ColumnProvider provider in owner._columnProviders)
            {
                ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = provider.Column.EffectiveWidth,
                    MinWidth = provider.Column.IsVisible && provider.Column.IsAvailable
                        ? provider.Column.MinimumWidth
                        : 0,
                });
                Control cell = provider.CreateCell();
                SetColumn(cell, provider.Index);
                Children.Add(cell);
                _cells.Add((provider, cell));
            }

            ApplyColumnLayout();
        }

        protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
        {
            double rowHeight = GetRowHeight(_owner);
            foreach ((ColumnProvider provider, _) in _cells)
            {
                if (provider is AvatarColumnProvider avatarColumnProvider)
                {
                    avatarColumnProvider.ApplyRowHeight(rowHeight);
                    ColumnDefinitions[provider.Index].Width = provider.Column.EffectiveWidth;
                }
            }

            Avalonia.Size measured = base.MeasureOverride(availableSize);
            return new Avalonia.Size(measured.Width, rowHeight);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is GitRevision revision)
            {
                RefreshCells();
            }

            UpdateColorClasses();
        }

        public void RefreshCells()
        {
            if (DataContext is not GitRevision revision)
            {
                return;
            }

            foreach ((ColumnProvider provider, Control cell) in _cells)
            {
                provider.UpdateCell(cell, revision);
                int rowIndex = _owner._revisions.IndexOf(revision);
                _owner._toolTipProvider.UpdateCell(cell, provider.Index, rowIndex, revision);
            }

            UpdateColorClasses();
        }

        public void ApplyColumnLayout()
        {
            foreach ((ColumnProvider provider, Control cell) in _cells)
            {
                ColumnDefinitions[provider.Index].Width = provider.Column.EffectiveWidth;
                bool isVisible = provider.Column.IsVisible && provider.Column.IsAvailable;
                ColumnDefinitions[provider.Index].MinWidth = isVisible ? provider.Column.MinimumWidth : 0;
                cell.IsVisible = isVisible;
            }
        }

        private void UpdateColorClasses()
        {
            if (DataContext is not GitRevision revision)
            {
                Classes.Set("revision-authored", false);
                Classes.Set("revision-alternate", false);
                return;
            }

            bool isAuthored = AppSettings.HighlightAuthoredRevisions
                && !revision.IsArtificial
                && _owner._authorHighlighting.IsHighlighted(revision);
            Classes.Set("revision-authored", isAuthored);

            ListBoxItem? container = this.FindAncestorOfType<ListBoxItem>();
            int rowIndex = container is null ? -1 : _owner._gridView.IndexFromContainer(container);
            bool isAlternate = AppSettings.RevisionGraphDrawAlternateBackColor
                && rowIndex >= 0
                && rowIndex % 2 == 0;
            Classes.Set("revision-alternate", isAlternate);
            bool isSelected = container?.IsSelected == true;
            if (isSelected)
            {
                Background = Brushes.Transparent;
            }
            else
            {
                // Avalonia template styles cannot reach recycled rows, so preserve the original color precedence here.
                string backgroundResourceKey = isAuthored
                    ? "GitExtensionsRevisionAuthoredBrush"
                    : isAlternate
                        ? "GitExtensionsRevisionAlternatingRowBrush"
                        : "GitExtensionsPanelBackgroundBrush";
                this[!BackgroundProperty] = new DynamicResourceExtension(backgroundResourceKey);
            }

            bool isSelectedAndFocused = isSelected
                && (_owner._gridView.IsKeyboardFocusWithin
                    || _owner._gridView.Classes.Contains("context-menu-open"));
            bool isNonRelativeGray = AppSettings.RevisionGraphDrawNonRelativesTextGray
                && rowIndex >= 0
                && !_owner._revisionGraph.IsRowRelative(rowIndex);
            foreach (TextBlock textBlock in this.GetVisualDescendants().OfType<TextBlock>()
                         .Where(textBlock => textBlock.Classes.Contains("revision-subject")
                             || textBlock.Classes.Contains("revision-body")))
            {
                string resourceKey = textBlock.Classes.Contains("revision-subject")
                    ? isNonRelativeGray
                        ? isSelectedAndFocused
                            ? "GitExtensionsRevisionNonRelativeSelectedSubjectBrush"
                            : "GitExtensionsRevisionNonRelativeSubjectBrush"
                        : isSelectedAndFocused
                            ? "GitExtensionsRevisionSelectedSubjectBrush"
                            : "GitExtensionsKnownColorControlTextBrush"
                    : isNonRelativeGray
                        ? isSelected
                            ? "GitExtensionsRevisionNonRelativeSelectedBodyBrush"
                            : "GitExtensionsRevisionNonRelativeBodyBrush"
                        : isSelected
                            ? "GitExtensionsRevisionSelectedBodyBrush"
                            : "GitExtensionsKnownColorGrayTextBrush";
                textBlock[!TextBlock.ForegroundProperty] = new DynamicResourceExtension(resourceKey);
            }
        }
    }

    internal readonly struct TestAccessor(RevisionGridControl control)
    {
        public Control? CurrentPage => control.revisionPage.Content as Control;

        public ListBox Revisions => control._gridView;

        public void SetRevisions(IEnumerable<GitRevision> revisions)
        {
            control.ApplyFinalRevisionOrder([.. revisions]);
            control.SetPage(control._gridView);
            control.FocusRevisionGridWhenShown();
        }

        public void AppendRevisions(IEnumerable<GitRevision> revisions)
            => control.AppendRevisions([.. revisions], cancellationToken: default);

        public bool HasGraphParent(ObjectId childId, ObjectId parentId)
            => control._revisionGraph.TryGetNode(childId, out RevisionGraphRevision? child)
                && child.Parents.Any(parent => parent.Objectid == parentId);
    }

    public void OnRepositoryChanged()
        => _buildServerWatcher.OnRepositoryChanged();
}
