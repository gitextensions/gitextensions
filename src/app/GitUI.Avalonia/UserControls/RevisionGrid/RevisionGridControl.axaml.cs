using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
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

using ResourceManager;
using ResourceManager.Hotkey;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI;

// Twin of the enum declared in GitUI/UserControls/RevisionGrid/RevisionGridControl.cs.
public enum RevisionGraphDrawStyle
{
    Normal,
    DrawNonRelativesGray,
    HighlightSelected
}

// Twin of the enum declared in GitUI/UserControls/RevisionGrid/RevisionGridControl.cs.
public enum SortDirection
{
    Ascending,
    Descending
}

public partial class RevisionGridControl : GitModuleControl, ICheckRefs, IRevisionGridInfo, IRevisionGridFilter, IRevisionGridUpdate
{
    public static readonly string HotkeySettingsName = "RevisionGrid";

    private const int RowHeight = 24;
    private const string ObjectIdPrefix = "????";
    private readonly CancellationTokenSequence _refreshSequence = new();

    // Avalonia's designer constructs views before the application initializes ThreadHelper.
    private readonly TaskManager _taskManager = GitUI.Compat.DesignTimeTaskManager.Create();
    private readonly FilterInfo _filterInfo = new();
    private readonly NavigationHistory _navigationHistory = new();
    private readonly RevisionGridToolTipProvider _toolTipProvider;
    private readonly QuickSearchProvider _quickSearchProvider;
    private readonly ParentChildNavigationHistory _parentChildNavigationHistory;
    private readonly AuthorRevisionHighlighting _authorHighlighting = new();
    private readonly Lazy<IndexWatcher> _indexWatcher;
    private readonly List<ColumnProvider> _columnProviders = [];
    private readonly Avalonia.Collections.AvaloniaList<GitRevision> _revisions = [];
    private readonly TranslationString _areYouSureRebase = new("Are you sure you want to rebase? This action will rewrite commit history.");
    private readonly TranslationString _dontShowAgain = new("Don't show me this message again.");
    private readonly TranslationString _rebaseBranch = new("Rebase branch.");
    private readonly TranslationString _rebaseBranchInteractive = new("Rebase branch interactively.");
    private readonly TranslationString _rebaseConfirmTitle = new("Rebase Confirmation");
    private readonly TranslationString _droppingFilesBlocked = new("For you own protection dropping more than 10 patch files at once is blocked!");
    private readonly RevisionGraph _revisionGraph = new();
    private readonly ArtificialCommitChangeCount _workTreeChangeCount = new();
    private readonly ArtificialCommitChangeCount _indexChangeCount = new();
    private readonly BuildServerWatcher _buildServerWatcher;
    private readonly RevisionGraphColumnProvider _revisionGraphColumnProvider;
    private readonly MessageColumnProvider _messageColumnProvider;
    private ObjectId? _headId;
    private ObjectId _pendingSelectedObjectId;
    private bool _headHighlighted;
    private string _lastPathFilter = string.Empty;
    private string _lastRevisionFilter = "--all";
    private IGitModule? _lastModule;
    private bool _parentsAreRewritten;
    private ILookup<ObjectId, IGitRef>? _refsByObjectId;
    private string? _rebaseOnTopOf;
    private SuperProjectInfo? _superprojectCurrentCheckout;

    public RevisionGridControl()
    {
        InitializeComponent();

        _buildServerWatcher = new BuildServerWatcher(this, this, () => Module);
        GitRevisionSummaryBuilder gitRevisionSummaryBuilder = new();
        _revisionGraphColumnProvider = new RevisionGraphColumnProvider(_revisionGraph, this, gitRevisionSummaryBuilder);
        AddColumn(_revisionGraphColumnProvider);
        _messageColumnProvider = new MessageColumnProvider(this);
        AddColumn(_messageColumnProvider);
        AddColumn(new NotesColumnProvider());
        AddColumn(new AvatarColumnProvider(this, AvatarService.DefaultProvider, AvatarService.CacheCleaner));
        AddColumn(new AuthorNameColumnProvider(_authorHighlighting));
        AddColumn(new DateColumnProvider());
        AddColumn(new CommitIdColumnProvider());
        AddColumn(_buildServerWatcher.ColumnProvider);
        ApplyColumnSettings();

        _toolTipProvider = new RevisionGridToolTipProvider(this);
        _toolTipProvider.ShowRevisionGridTooltips = AppSettings.ShowRevisionGridTooltips.Value;
        _quickSearchProvider = new QuickSearchProvider(_gridView, pnlRevisionGrid, () => Module.WorkingDir);
        _gridView.ItemsSource = _revisions;

        MenuCommands = new RevisionGridMenuCommands(this);
        FillMenuFromMenuCommands(MenuCommands.ViewMenuCommands, viewToolStripMenuItem);
        FillMenuFromMenuCommands(MenuCommands.NavigateMenuCommands, navigateToolStripMenuItem);
        MenuCommands.TriggerMenuChanged();

        // Parent-child navigation can expect that SetSelectedRevision is always successful since it always uses first-parents
        _parentChildNavigationHistory = new ParentChildNavigationHistory(commitId => SetSelectedRevision(commitId));
        _indexWatcher = new Lazy<IndexWatcher>(() => new IndexWatcher(UICommandsSource));

        _gridView.ItemTemplate = new FuncDataTemplate<GitRevision>((_, _) => new RevisionRowControl(this), supportsRecycling: true);
        _gridView.SelectionChanged += (_, _) =>
        {
            _parentChildNavigationHistory.RevisionsSelectionChanged();
            HighlightRevisionsByAuthor();
            UpdateContextMenuItems();
            GitRevision[] selectedRevisions = [.. _gridView.SelectedItems?.OfType<GitRevision>().Take(2) ?? []];
            if (selectedRevisions.Length == 1)
            {
                _navigationHistory.Push(selectedRevisions[0].ObjectId);
            }

            SelectionChanged?.Invoke(this, EventArgs.Empty);
        };
        _gridView.KeyDown += OnGridViewKeyDown;
        _gridView.TextInput += (_, e) => _quickSearchProvider.OnTextInput(e);
        _gridView.DoubleTapped += (_, _) =>
            DoubleClickRevision?.Invoke(this, new DoubleClickRevisionEventArgs(SelectedRevision));
        _gridView.PointerPressed += _gridView_PointerPressed;

        // Allow to drop patch file on revision grid
        DragDrop.SetAllowDrop(_gridView, true);
        DragDrop.AddDragEnterHandler(_gridView, OnGridViewDragEnter);
        DragDrop.AddDragOverHandler(_gridView, OnGridViewDragEnter);
        DragDrop.AddDropHandler(_gridView, OnGridViewDragDrop);
        _gridView.LayoutUpdated += (_, _) => UpdateVisibleGraphColumnWidth();
        mainContextMenu.Opening += (_, _) => UpdateContextMenuItems();
        copyToClipboardToolStripMenuItem.SetRevisionFunc(GetSelectedRevisions);
        applyStashToolStripMenuItem.Click += ApplyStashToolStripMenuItemClick;
        popStashToolStripMenuItem.Click += PopStashToolStripMenuItemClick;
        dropStashToolStripMenuItem.Click += DropStashToolStripMenuItemClick;
        rebaseToolStripMenuItem.Click += RebaseToolStripMenuItemClick;
        rebaseInteractivelyToolStripMenuItem.Click += RebaseInteractivelyToolStripMenuItemClick;
        rebaseWithAdvOptionsToolStripMenuItem.Click += RebaseWithAdvOptionsToolStripMenuItemClick;
        resetCurrentBranchToHereToolStripMenuItem.Click += ResetCurrentBranchToHereToolStripMenuItemClick;
        resetChangesToolStripMenuItem.Click += ResetChangesToolStripMenuItemClick;
        commitToolStripMenuItem.Click += CommitToolStripMenuItemClick;
        createNewBranchToolStripMenuItem.Click += CreateNewBranchToolStripMenuItemClick;
        createTagToolStripMenuItem.Click += CreateTagToolStripMenuItemClick;
        checkoutRevisionToolStripMenuItem.Click += CheckoutRevisionToolStripMenuItemClick;
        revertCommitToolStripMenuItem.Click += RevertCommitToolStripMenuItemClick;
        cherryPickCommitToolStripMenuItem.Click += CherryPickCommitToolStripMenuItemClick;
        archiveRevisionToolStripMenuItem.Click += ArchiveRevisionToolStripMenuItemClick;
        markRevisionAsBadToolStripMenuItem.Click += (_, _) => ContinueBisect(GitBisectOption.Bad);
        markRevisionAsGoodToolStripMenuItem.Click += (_, _) => ContinueBisect(GitBisectOption.Good);
        bisectSkipRevisionToolStripMenuItem.Click += (_, _) => ContinueBisect(GitBisectOption.Skip);
        stopBisectToolStripMenuItem.Click += StopBisectToolStripMenuItemClick;
        tsmiSelectInLeftPanel.Click += SelectInLeftPanel_Click;
        fixupCommitToolStripMenuItem.Click += FixupCommitToolStripMenuItemClick;
        squashCommitToolStripMenuItem.Click += SquashCommitToolStripMenuItemClick;
        amendCommitToolStripMenuItem.Click += AmendCommitToolStripMenuItemClick;
        editCommitToolStripMenuItem.Click += editCommitToolStripMenuItem_Click;
        rewordCommitToolStripMenuItem.Click += rewordCommitToolStripMenuItem_Click;
        openCommitsWithDiffToolMenuItem.Click += (_, _) => DiffSelectedCommitsWithDifftool();
        getHelpOnHowToUseTheseFeaturesToolStripMenuItem.Click += GetHelpOnHowToUseTheseFeaturesToolStripMenuItem_Click;
        openBuildReportToolStripMenuItem.Click += (_, _) => OpenBuildReport(SelectedRevision);
        openPullRequestPageStripMenuItem.Click += OpenPullRequestPageStripMenuItem_Click;
        HotkeysEnabled = true;
        UICommandsSourceSet += (_, _) =>
        {
            LoadHotkeys(HotkeySettingsName);
            MenuCommands.CreateOrUpdateMenuCommands();
        };
        UpdateContextMenuItems();
        DetachedFromVisualTree += (_, _) =>
        {
            _buildServerWatcher.Dispose();
            if (_indexWatcher.IsValueCreated)
            {
                _indexWatcher.Value.Dispose();
            }
        };

        InitializeComplete();
    }

    /// <summary>
    ///  Gets the revision currently selected in the list, or <see langword="null"/>.
    /// </summary>
    public GitRevision? SelectedRevision => _gridView.SelectedItem as GitRevision;

    internal Dictionary<ObjectId, string>? FilePathByObjectId { get; set; }

    internal FilterInfo CurrentFilter => _filterInfo;

    internal RevisionGridMenuCommands MenuCommands { get; }

    internal Action<string>? SelectInLeftPanel { get; set; } = null;

    internal bool MultiSelect
    {
        get => _gridView.SelectionMode == SelectionMode.Multiple;
        set => _gridView.SelectionMode = value ? SelectionMode.Multiple : SelectionMode.Single;
    }

    internal bool ShowUncommittedChangesIfPossible { get; set; } = true;

    internal bool ShowBuildServerInfo { get; set; }

    internal bool HasRevisionSource => _lastModule is not null;

    internal IndexWatcher IndexWatcher => _indexWatcher.Value;

    /// <summary>
    ///  Occurs when the selected revision changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    public event EventHandler? ArtificialChanged;

    /// <inheritdoc />
    public event EventHandler<FilterChangedEventArgs>? FilterChanged;

    /// <summary>Occurs when the selected revision is double-clicked.</summary>
    public event EventHandler<DoubleClickRevisionEventArgs>? DoubleClickRevision;

    /// <summary>Occurs when refs and stash revisions for a reload become available.</summary>
    public event EventHandler<RevisionLoadEventArgs>? RevisionsLoading;

    /// <summary>Occurs after the corresponding revision reload reaches the UI.</summary>
    public event EventHandler<RevisionLoadEventArgs>? RevisionsLoaded;

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

    internal void SetShortcutKeys()
    {
        RefreshMenuShortcutKeys(Hotkeys);
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

    internal void SetAheadBehindDataProvider(IAheadBehindDataProvider? provider)
        => _messageColumnProvider.SetAheadBehindDataProvider(provider);

    internal void CancelBackgroundTasks()
    {
        _refreshSequence.CancelCurrent();
        _buildServerWatcher.CancelBuildStatusFetchOperation();
        _taskManager.JoinPendingOperations();
    }

    /// <inheritdoc />
    public void ResetAllFiltersAndRefresh()
    {
        _filterInfo.ResetAllFilters();
        RefreshFilteredRevisions();
    }

    /// <inheritdoc />
    public void SetAndApplyBranchFilter(string filter)
    {
        _filterInfo.SetBranchFilter(filter);
        RefreshFilteredRevisions();
    }

    /// <inheritdoc />
    public void SetAndApplyRevisionFilter(RevisionFilter filter)
    {
        if (_filterInfo.Apply(filter))
        {
            RefreshFilteredRevisions();
        }
    }

    /// <inheritdoc />
    public void SetAndApplyPathFilter(string filter)
    {
        _filterInfo.ByPathFilter = !string.IsNullOrWhiteSpace(filter);
        _filterInfo.PathFilter = filter;
        RefreshFilteredRevisions();
    }

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
    public void ShowCurrentBranchOnly()
    {
        _filterInfo.ByBranchFilter = false;
        _filterInfo.ShowCurrentBranchOnly = true;
        RefreshFilteredRevisions();
    }

    /// <inheritdoc />
    public void ShowFilteredBranches()
    {
        _filterInfo.ByBranchFilter = true;
        _filterInfo.ShowCurrentBranchOnly = false;
        RefreshFilteredRevisions();
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

    /// <inheritdoc />
    public void ToggleShowOnlyFirstParent()
    {
        _filterInfo.ShowOnlyFirstParent = !_filterInfo.ShowOnlyFirstParent;
        RefreshFilteredRevisions();
    }

    /// <inheritdoc />
    public void ToggleShowReflogReferences()
    {
        _filterInfo.ShowReflogReferences = !_filterInfo.ShowReflogReferences;
        RefreshFilteredRevisions();
    }

    private void RefreshFilteredRevisions()
    {
        if (_lastModule is null)
        {
            return;
        }

        ReloadRevisions(
            _lastModule,
            _lastRevisionFilter,
            SelectedId,
            _lastPathFilter);
    }

    internal bool TryGetSuperProjectInfo([System.Diagnostics.CodeAnalysis.NotNullWhen(returnValue: true)] out SuperProjectInfo? superProjectInfo)
    {
        superProjectInfo = _superprojectCurrentCheckout;
        return superProjectInfo is not null;
    }

    internal bool GoToRelatedRef(IGitRef gitRef)
    {
        ObjectId objectId = gitRef.Guid is null
            ? Module.RevParse(gitRef.CompleteName)
            : gitRef.ObjectId;
        return !objectId.IsZero && SetSelectedRevision(objectId);
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

    private void AddColumn(ColumnProvider columnProvider)
    {
        columnProvider.Index = _columnProviders.Count;
        _columnProviders.Add(columnProvider);
    }

    internal void ApplyColumnSettings()
    {
        foreach (ColumnProvider columnProvider in _columnProviders)
        {
            columnProvider.ApplySettings();
        }

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

    /// <summary>Removes the row context menu, like the WinForms grid method.</summary>
    public void DisableContextMenu()
    {
        _gridView.ContextMenu = null;
    }

    /// <summary>Gets or replaces the context menu attached to revision rows.</summary>
    public ContextMenu? RevisionContextMenu
    {
        get => _gridView.ContextMenu;
        set => _gridView.ContextMenu = value;
    }

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

    bool IRevisionGridUpdate.SetSelectedRevision(ObjectId commitId, bool toggleSelection, bool updateNavigationHistory)
        => SetSelectedRevision(commitId, toggleSelection, updateNavigationHistory);

    #region IRevisionGridInfo

    public ObjectId CurrentCheckout => _headId ?? default;

    public GitRevision GetRevision(ObjectId objectId)
    {
        // Like WinForms, may return null; callers null-check.
        return _revisions.FirstOrDefault(r => r.ObjectId == objectId)!;
    }

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

    public IReadOnlyList<GitRevision> GetSelectedRevisions()
        => GetSelectedRevisions(direction: null);

    public IReadOnlyList<GitRevision> GetSelectedRevisions(SortDirection direction)
        => GetSelectedRevisions((SortDirection?)direction);

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
            $"--format=\"{ObjectIdPrefix}%H\"",
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

    public ObjectId SelectedId
    {
        get => SelectedRevision?.ObjectId ?? _pendingSelectedObjectId;
        set => _pendingSelectedObjectId = value;
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

    public string GetCurrentBranch() => Module.GetSelectedBranch();

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

        _quickSearchProvider.OnKeyDown(e);
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

    internal static bool CanDropPatchFiles(IReadOnlyList<string> fileNames)
        => fileNames.Count > 0
            && fileNames.All(fileName => fileName.EndsWith(".patch", StringComparison.InvariantCultureIgnoreCase));

    private static string[] GetDroppedFileNames(IDataTransfer dataTransfer)
        => [.. (dataTransfer.TryGetFiles() ?? [])
            .Select(file => file.TryGetLocalPath())
            .OfType<string>()];

    #endregion

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
        SetVisible(rebaseOnToolStripMenuItem, regularRevision && _rebaseOnTopOf is not null);
        SetVisible(resetCurrentBranchToHereToolStripMenuItem, regularRevision);
        SetVisible(tsmiSelectInLeftPanel, regularRevision && SelectInLeftPanel is not null && tsmiSelectInLeftPanel.Tag is string);
        SetVisible(resetChangesToolStripMenuItem, revision is { IsArtificial: true } && hasCommands && !isBareRepository);
        SetVisible(commitToolStripMenuItem, revision is { IsArtificial: true } && hasCommands && !isBareRepository);
        SetVisible(createNewBranchToolStripMenuItem, regularRevision);
        SetVisible(resetAnotherBranchToHereToolStripMenuItem, regularRevision);
        resetAnotherBranchToHereToolStripMenuItem.IsEnabled = false;
        SetVisible(renameBranchToolStripMenuItem, renameBranchToolStripMenuItem.Items.Count > 0);
        SetVisible(deleteBranchToolStripMenuItem, deleteBranchToolStripMenuItem.Items.Count > 0);
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
        compareToBranchToolStripMenuItem.IsEnabled = false;
        compareWithCurrentBranchToolStripMenuItem.IsEnabled = false;
        selectAsBaseToolStripMenuItem.IsEnabled = false;
        compareToBaseToolStripMenuItem.IsEnabled = false;
        compareToWorkingDirectoryMenuItem.IsEnabled = false;
        compareSelectedCommitsMenuItem.IsEnabled = false;
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

    private void CreateNewBranchToolStripMenuItemClick(object? sender, EventArgs e)
    {
        if (SelectedRevision is GitRevision revision)
        {
            UICommands.StartCreateBranchDialog(GetOwner(), revision.ObjectId);
        }
    }

    private void CreateTagToolStripMenuItemClick(object? sender, EventArgs e)
    {
        if (SelectedRevision is GitRevision revision)
        {
            UICommands.StartCreateTagDialog(GetOwner(), revision);
        }
    }

    private void CheckoutRevisionToolStripMenuItemClick(object? sender, EventArgs e)
    {
        if (SelectedRevision is GitRevision revision)
        {
            UICommands.StartCheckoutRevisionDialog(GetOwner(), revision.Guid);
        }
    }

    private void ArchiveRevisionToolStripMenuItemClick(object? sender, EventArgs e)
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

    private void RevertCommitToolStripMenuItemClick(object? sender, EventArgs e)
    {
        IReadOnlyList<GitRevision> revisions = GetSelectedRevisions(SortDirection.Ascending);
        foreach (GitRevision revision in revisions)
        {
            UICommands.StartRevertCommitDialog(GetOwner(), revision);
        }
    }

    private void CherryPickCommitToolStripMenuItemClick(object? sender, EventArgs e)
    {
        IReadOnlyList<GitRevision> revisions = GetSelectedRevisions(SortDirection.Descending);
        UICommands.StartCherryPickDialog(GetOwner(), revisions);
    }

    private void ResetCurrentBranchToHereToolStripMenuItemClick(object? sender, EventArgs e)
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

    private void ResetChangesToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartResetChangesDialog(
            GetOwner(),
            Module.GetWorkTreeFiles(),
            onlyWorkTree: SelectedRevision?.ObjectId == ObjectId.WorkTreeId);
        ArtificialChanged?.Invoke(this, EventArgs.Empty);
    }

    private void CommitToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartCommitDialog(GetOwner());
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

    private void StopBisectToolStripMenuItemClick(object? sender, EventArgs e)
    {
        FormProcess.ShowDialog(GetOwner(), UICommands, arguments: Commands.StopBisect(), Module.WorkingDir, input: null, useDialogSettings: true);
        ReloadCurrentView();
    }

    private void FixupCommitToolStripMenuItemClick(object? sender, EventArgs e)
    {
        if (SelectedRevision is GitRevision revision)
        {
            UICommands.StartFixupCommitDialog(GetOwner(), revision);
        }
    }

    private void SquashCommitToolStripMenuItemClick(object? sender, EventArgs e)
    {
        if (SelectedRevision is GitRevision revision)
        {
            UICommands.StartSquashCommitDialog(GetOwner(), revision);
        }
    }

    private void AmendCommitToolStripMenuItemClick(object? sender, EventArgs e)
    {
        if (SelectedRevision is GitRevision revision)
        {
            UICommands.StartAmendCommitDialog(GetOwner(), revision);
        }
    }

    private void editCommitToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        LaunchRebase("e");
    }

    private void rewordCommitToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        LaunchRebase("r");
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

    public void DiffSelectedCommitsWithDifftool(string? customTool = null)
    {
        IReadOnlyList<GitRevision> selectedRevisions = GetSelectedRevisions();
        if (selectedRevisions.Count > 0)
        {
            string? first = selectedRevisions.Count > 1 ? selectedRevisions[1].ObjectId.ToString() : null;
            Module.OpenWithDifftoolDirDiff(first, selectedRevisions[0].ObjectId.ToString(), customTool: customTool);
        }
    }

    private void GetHelpOnHowToUseTheseFeaturesToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser(
            GitUI.UserManual.UserManual.UrlFor("modify_history", "using-autosquash-rebase-feature"));
    }

    private void OpenPullRequestPageStripMenuItem_Click(object? sender, EventArgs e)
    {
        string? url = SelectedRevision?.BuildStatus?.PullRequestUrl;
        if (!string.IsNullOrWhiteSpace(url))
        {
            OsShellUtil.OpenUrlInDefaultBrowser(url);
        }
    }

    private void ApplyStashToolStripMenuItemClick(object? sender, EventArgs e)
    {
        if (SelectedRevision is GitRevision revision)
        {
            UICommands.StashApply(GetOwner(), revision.ObjectId.ToString());
            ReloadCurrentView();
        }
    }

    private void PopStashToolStripMenuItemClick(object? sender, EventArgs e)
    {
        string? stashName = SelectedRevision?.ReflogSelector;
        if (!string.IsNullOrEmpty(stashName))
        {
            UICommands.StashPop(GetOwner(), stashName);
            ReloadCurrentView();
        }
    }

    private void DropStashToolStripMenuItemClick(object? sender, EventArgs e)
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

    private void RebaseToolStripMenuItemClick(object? sender, EventArgs e)
    {
        StartRebase(interactive: false);
    }

    private void RebaseInteractivelyToolStripMenuItemClick(object? sender, EventArgs e)
    {
        StartRebase(interactive: true);
    }

    private void StartRebase(bool interactive)
    {
        if (_rebaseOnTopOf is null)
        {
            return;
        }

        if (!AppSettings.DontConfirmRebase)
        {
            TaskDialogPage page = new()
            {
                Text = _areYouSureRebase.Text,
                Caption = _rebaseConfirmTitle.Text,
                Heading = interactive ? _rebaseBranchInteractive.Text : _rebaseBranch.Text,
                Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                Icon = TaskDialogIcon.Information,
                Verification = new TaskDialogVerificationCheckBox { Text = _dontShowAgain.Text },
                SizeToContent = true,
            };
            TaskDialogButton result = TaskDialog.ShowDialog(GetOwner(), page);
            if (page.Verification.Checked)
            {
                AppSettings.DontConfirmRebase = true;
            }

            if (result != TaskDialogButton.Yes)
            {
                return;
            }
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

    private void RebaseWithAdvOptionsToolStripMenuItemClick(object? sender, EventArgs e)
    {
        if (_rebaseOnTopOf is not null)
        {
            UICommands.StartRebaseDialogWithAdvOptions(GetOwner(), _rebaseOnTopOf);
        }
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

    internal void ToggleDrawNonRelativesGray()
    {
        AppSettings.RevisionGraphDrawNonRelativesGray = !AppSettings.RevisionGraphDrawNonRelativesGray;
        ApplySettingsAndRefreshRows();
    }

    internal void ToggleShowRemoteBranches()
    {
        AppSettings.ShowRemoteBranches = !AppSettings.ShowRemoteBranches;
        ApplySettingsAndRefreshRows();
    }

    internal void ToggleShowTags()
    {
        AppSettings.ShowTags = !AppSettings.ShowTags;
        ApplySettingsAndRefreshRows();
    }

    internal void ToggleShowGitNotes()
    {
        AppSettings.ShowGitNotes = !AppSettings.ShowGitNotes;
        UpdateViewMenuChecks();
        ReloadCurrentView();
    }

    internal void ToggleShowAuthorDate()
    {
        AppSettings.ShowAuthorDate = !AppSettings.ShowAuthorDate;
        ApplySettingsAndRefreshRows();
    }

    internal void ToggleShowRelativeDate()
    {
        AppSettings.RelativeDate = !AppSettings.RelativeDate;
        ApplySettingsAndRefreshRows();
    }

    internal void ToggleRevisionGraphColumn()
    {
        AppSettings.ShowRevisionGridGraphColumn = !AppSettings.ShowRevisionGridGraphColumn;
        ApplySettingsAndRefreshRows();
    }

    internal void ToggleShowGitNotesColumn()
    {
        AppSettings.ShowGitNotesColumn.Value = !AppSettings.ShowGitNotesColumn.Value;
        ReloadCurrentView();
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

    internal void ToggleShowSessionRefs()
    {
        AppSettings.ShowSessionRefs = !AppSettings.ShowSessionRefs;
        ReloadCurrentView();
    }

    internal void ToggleShowCommitBodyInRevisionGrid()
    {
        AppSettings.ShowCommitBodyInRevisionGrid = !AppSettings.ShowCommitBodyInRevisionGrid;
        ReloadCurrentView();
    }

    internal void ToggleAuthorAvatarColumn()
    {
        AppSettings.ShowAuthorAvatarColumn = !AppSettings.ShowAuthorAvatarColumn;
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

    protected override bool ExecuteCommand(int command)
    {
        switch ((Command)command)
        {
            case Command.ToggleRevisionGraph: ToggleRevisionGraphColumn(); break;
            case Command.ToggleAuthorDateCommitDate: ToggleShowAuthorDate(); break;
            case Command.ToggleShowRelativeDate: ToggleShowRelativeDate(); break;
            case Command.ToggleDrawNonRelativesGray: ToggleDrawNonRelativesGray(); break;
            case Command.ToggleShowGitNotes: ToggleShowGitNotes(); break;
            case Command.ToggleShowGitNotesColumn: ToggleShowGitNotesColumn(); break;
            case Command.ToggleShowTags: ToggleShowTags(); break;
            case Command.ShowRemoteBranches: ToggleShowRemoteBranches(); break;
            case Command.SelectCurrentRevision: SelectCurrentRevision(); break;
            case Command.GoToParent:
                return GoToParent(firstParent: true, useHistory: true);
            case Command.GoToFirstParent:
                return GoToParent(firstParent: true, useHistory: false);
            case Command.GoToLastParent: return GoToParent(firstParent: false, useHistory: false);
            case Command.GoToChild: return GoToChild();
            case Command.NextQuickSearch: _quickSearchProvider.NextResult(down: true); break;
            case Command.PrevQuickSearch: _quickSearchProvider.NextResult(down: false); break;
            case Command.NavigateBackward:
            case Command.NavigateBackward_AlternativeHotkey: NavigateBackward(); break;
            case Command.NavigateForward:
            case Command.NavigateForward_AlternativeHotkey: NavigateForward(); break;
            case Command.ToggleBetweenArtificialAndHeadCommits: return ToggleBetweenArtificialAndHeadCommits();
            case Command.ToggleHighlightSelectedBranch: return HighlightSelectedBranch();
            case Command.DeleteRef: return DeleteSingleRef();
            case Command.RenameRef: return RenameSingleRef();
            default: return base.ExecuteCommand(command);
        }

        return true;
    }

    internal bool ExecuteCommand(Command cmd)
    {
        return ExecuteCommand((int)cmd);
    }

    internal bool ToggleBetweenArtificialAndHeadCommits()
    {
        if (SelectedRevision?.IsArtificial == true)
        {
            SelectCurrentRevision();
            return true;
        }

        GitRevision? artificial = _revisions.FirstOrDefault(revision => revision.ObjectId == ObjectId.WorkTreeId)
            ?? _revisions.FirstOrDefault(revision => revision.ObjectId == ObjectId.IndexId);
        return artificial is not null && SetSelectedRevision(artificial.ObjectId);
    }

    private bool HighlightSelectedBranch()
    {
        if (SelectedRevision is not GitRevision revision)
        {
            return false;
        }

        _revisionGraph.HighlightBranch(revision.ObjectId);
        _revisionGraphColumnProvider.RevisionGraphDrawStyle = RevisionGraphDrawStyle.HighlightSelected;
        RefreshRealizedRows();
        return true;
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
        string pathFilter = "")
    {
        CancellationToken cancellationToken = _refreshSequence.Next();
        _lastModule = module;
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
        lblLoadingStatus.Text = "Loading…";
        SetPage(new LoadingControl());

        Lazy<IReadOnlyList<IGitRef>> refs = new(() => module.GetRefs(RefsFilter.NoFilter));
        Lazy<IReadOnlyCollection<GitRevision>> stashes = new(() =>
            !AppSettings.ShowStashes || module.IsBareRepository()
                ? []
                : new RevisionReader(module).GetStashes(cancellationToken));
        RevisionLoadEventArgs loadEventArgs = new(this, UICommands, refs, stashes, forceRefresh: true);
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
                    gitRef => gitRef.IsRemote
                        && gitRef.Remote == selectedRef.TrackingRemote
                        && gitRef.LocalName == selectedRef.MergeWith)
                    ?.IsSelectedHeadMergeSource = true;
            }

            _refsByObjectId = loadedRefs
                .Where(gitRef => !gitRef.ObjectId.IsZero)
                .ToLookup(gitRef => gitRef.ObjectId);
            observer.InitializeStashes(stashes.Value);

            RevisionReader reader = new(module);
            bool hasNotes = AppSettings.ShowGitNotesColumn.Value || AppSettings.ShowGitNotes;
            string effectivePathFilter = BuildPathFilter(module, pathFilter, cancellationToken);
            reader.GetLog(observer, revisionFilter, effectivePathFilter, hasNotes, autostashLabel: "autostash", cancellationToken);
        });
    }

    private static ArgumentString FindRenamesAndCopiesOpts()
        => AppSettings.FollowRenamesInFileHistoryExactOnly
            ? " --find-renames=\"100%\" --find-copies=\"100%\""
            : " --find-renames --find-copies";

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
            $"--format=\"{ObjectIdPrefix}%H\"",
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

            if (line.StartsWith(ObjectIdPrefix))
            {
                currentObjectId = line.Length >= ObjectId.Sha1CharCount + ObjectIdPrefix.Length
                    && ObjectId.TryParse(line, offset: ObjectIdPrefix.Length, out ObjectId parsedId)
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
        lblLoadingStatus.Text = $"{_revisions.Count} revisions…";
        SelectPendingRevision();
    }

    private void OnLoadingCompleted(CancellationToken cancellationToken, IReadOnlyList<GitRevision> artificialRevisions)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (artificialRevisions.Count > 0)
        {
            GitRevision? headRevision = _headId is ObjectId headId
                ? _revisions.FirstOrDefault(revision => revision.ObjectId == headId)
                : null;
            int insertIndex = headRevision is null ? -1 : _revisions.IndexOf(headRevision);
            _revisions.InsertRange(insertIndex < 0 ? 0 : insertIndex, artificialRevisions);
        }

        if (_revisions.Count == 0 && !_filterInfo.HasFilter)
        {
            SetPage(new EmptyRepoControl(_lastModule?.IsBareRepository() == true));
        }
        else
        {
            SetPage(_gridView);

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
        if (_pendingSelectedObjectId.IsZero || !SetSelectedRevision(_pendingSelectedObjectId))
        {
            return;
        }

        _pendingSelectedObjectId = default;
    }

    private void OnLoadingError(Exception exception, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            lblLoadingStatus.Text = $"Failed to load revisions: {exception.Message}";
            SetPage(new ErrorControl());
        }
    }

    private static void FillMenuFromMenuCommands(IEnumerable<MenuCommand> menuCommands, MenuItem targetItem)
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

    /// <summary>
    /// Reset the controls to the supplied content.
    /// This is used to remove spinners added when loading and to replace the gridview at errors.
    /// </summary>
    /// <param name="content">The content to show.</param>
    private void SetPage(Control content)
        => revisionPage.Content = content;

    private void UpdateVisibleGraphColumnWidth()
    {
        RevisionRowControl[] visibleRows =
        [
            .. _gridView.GetVisualDescendants().OfType<RevisionRowControl>(),
        ];
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
        RevisionGraphDrawStyle drawStyle)
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
                RowHeight,
                _revisionGraph.GetSegmentsForRow,
                drawStyle,
                headId);
            return true;
        }
        catch (Exception)
        {
            // The reader can advance the row cache while layout is painting realized rows.
            return false;
        }
    }

    bool ICheckRefs.Contains(ObjectId objectId)
        => _revisions.Any(revision => revision.ObjectId == objectId);

    // parity-scaffolding: exposes deterministic grid state to capture and behavior tests.
    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(RevisionGridControl control)
    {
        public Control? CurrentPage => control.revisionPage.Content as Control;

        public ListBox Revisions => control._gridView;

        public void SetRevisions(IEnumerable<GitRevision> revisions)
        {
            control._revisions.Clear();
            control._revisions.AddRange(revisions);
            control.SetPage(control._gridView);
        }

        public void AppendRevisions(IEnumerable<GitRevision> revisions)
            => control.AppendRevisions([.. revisions], cancellationToken: default);

        public bool HasGraphParent(ObjectId childId, ObjectId parentId)
            => control._revisionGraph.TryGetNode(childId, out RevisionGraphRevision? child)
                && child.Parents.Any(parent => parent.Objectid == parentId);
    }

    private sealed class RevisionObserver(
        RevisionGridControl owner,
        CancellationToken cancellationToken,
        RevisionLoadEventArgs loadEventArgs) : IObserver<IReadOnlyList<GitRevision>>
    {
        private bool _artificialRevisionsAddedToStream;
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
                owner.OnLoadingCompleted(cancellationToken, artificialRevisions);
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
            if (_stashesById is not { Count: > 0 } stashesById)
            {
                return revisions;
            }

            List<GitRevision> revisionsWithStashes = new(revisions.Count + stashesById.Count);
            foreach (GitRevision revision in revisions)
            {
                if (stashesById.Remove(revision.ObjectId, out GitRevision? stash))
                {
                    revision.ReflogSelector = stash.ReflogSelector;
                }
                else if (_stashesByParentId is not null)
                {
                    foreach (GitRevision parentStash in _stashesByParentId[revision.ObjectId])
                    {
                        if (stashesById.Remove(parentStash.ObjectId))
                        {
                            revisionsWithStashes.Add(parentStash);
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
            Height = RowHeight;
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

            Classes.Set(
                "revision-authored",
                AppSettings.HighlightAuthoredRevisions
                    && !revision.IsArtificial
                    && _owner._authorHighlighting.IsHighlighted(revision));

            ListBoxItem? container = this.FindAncestorOfType<ListBoxItem>();
            int rowIndex = container is null ? -1 : _owner._gridView.IndexFromContainer(container);
            Classes.Set(
                "revision-alternate",
                AppSettings.RevisionGraphDrawAlternateBackColor
                    && rowIndex >= 0
                    && rowIndex % 2 == 0);
        }
    }

    public void OnRepositoryChanged()
        => _buildServerWatcher.OnRepositoryChanged();

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

    private void LaunchBuildServerInfoFetchOperation(CancellationToken cancellationToken)
    {
        if (!ShowBuildServerInfo)
        {
            return;
        }

        _taskManager.FileAndForget(() => _buildServerWatcher.LaunchBuildServerInfoFetchOperationAsync().WaitAsync(cancellationToken));
    }

    private static void OpenBuildReport(GitRevision? revision)
        => OsShellUtil.OpenUrlInDefaultBrowser(revision?.BuildStatus?.Url);
}
