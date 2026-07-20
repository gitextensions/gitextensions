using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUI.UserControls.RevisionGrid.Graph.Rendering;
using GitUIPluginInterfaces;

using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI;

// Twin of the enum declared in GitUI/UserControls/RevisionGrid/RevisionGridControl.cs.
public enum RevisionGraphDrawStyle
{
    Normal,
    DrawNonRelativesGray,
    HighlightSelected
}

public partial class RevisionGridControl : GitModuleControl, IRevisionGridInfo, IRevisionGridFilter, IRevisionGridUpdate
{
    public static readonly string HotkeySettingsName = "RevisionGrid";

    private const int RowHeight = 24;
    private const string RevisionGridTranslationCategory = "RevisionGrid";

    private static readonly (string Name, string Text)[] MenuCommandTranslations =
    [
        ("GotoCurrentRevision", "Go to c&urrent revision"),
        ("GotoChildCommit", "Go to c&hild commit"),
        ("GotoParentCommit", "Go to &parent commit"),
        ("GotoFirstParentCommit", "Go to f&irst parent commit"),
        ("GotoLastParentCommit", "Go to &last parent commit"),
        ("drawNonrelativesGrayToolStripMenuItem", "Draw non relatives gra&y"),
        ("ShowRemoteBranches", "Show remote &branches"),
        ("showTagsToolStripMenuItem", "Show &tags"),
        ("showAuthorDateToolStripMenuItem", "Sho&w author date"),
        ("showRelativeDateToolStripMenuItem", "Show relati&ve date"),
        ("showRevisionGraphColumnToolStripMenuItem", "Show revision &graph column"),
        ("showGitNotesColumnToolStripMenuItem", "Show Git &notes column"),
        ("showAuthorNameColumnToolStripMenuItem", "Show a&uthor name column"),
        ("showDateColumnToolStripMenuItem", "Show &date column"),
        ("showIdColumnToolStripMenuItem", "Show SHA&-1 column")
    ];

    private readonly CancellationTokenSequence _refreshSequence = new();
    private readonly TaskManager _taskManager = ThreadHelper.CreateTaskManager();
    private readonly FilterInfo _filterInfo = new();
    private readonly AuthorRevisionHighlighting _authorHighlighting = new();
    private readonly List<ColumnProvider> _columnProviders = [];
    private readonly List<GitRevision> _revisions = [];
    private readonly TranslationString _areYouSureRebase = new("Are you sure you want to rebase? This action will rewrite commit history.");
    private readonly TranslationString _dontShowAgain = new("Don't show me this message again.");
    private readonly TranslationString _rebaseBranch = new("Rebase branch.");
    private readonly TranslationString _rebaseBranchInteractive = new("Rebase branch interactively.");
    private readonly TranslationString _rebaseConfirmTitle = new("Rebase Confirmation");
    private readonly RevisionGraph _revisionGraph = new();
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

        GitRevisionSummaryBuilder gitRevisionSummaryBuilder = new();
        _revisionGraphColumnProvider = new RevisionGraphColumnProvider(_revisionGraph, this, gitRevisionSummaryBuilder);
        AddColumn(_revisionGraphColumnProvider);
        _messageColumnProvider = new MessageColumnProvider(this);
        AddColumn(_messageColumnProvider);
        AddColumn(new NotesColumnProvider());
        AddColumn(new AvatarColumnProvider());
        AddColumn(new AuthorNameColumnProvider(_authorHighlighting));
        AddColumn(new DateColumnProvider());
        AddColumn(new CommitIdColumnProvider());
        AddColumn(new BuildStatusColumnProvider());
        ApplyColumnSettings();

        lstRevisions.ItemTemplate = new FuncDataTemplate<GitRevision>((_, _) => new RevisionRowControl(this), supportsRecycling: true);
        lstRevisions.SelectionChanged += (_, _) =>
        {
            HighlightRevisionsByAuthor();
            UpdateContextMenuItems();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        };
        lstRevisions.DoubleTapped += (_, _) =>
            DoubleClickRevision?.Invoke(this, new DoubleClickRevisionEventArgs(SelectedRevision));
        lstRevisions.PointerPressed += lstRevisions_PointerPressed;
        lstRevisions.LayoutUpdated += (_, _) => UpdateVisibleGraphColumnWidth();
        revisionContextMenu.Opening += (_, _) => UpdateContextMenuItems();
        copyToClipboardToolStripMenuItem.SetRevisionFunc(GetSelectedRevisions);
        applyStashToolStripMenuItem.Click += ApplyStashToolStripMenuItemClick;
        popStashToolStripMenuItem.Click += PopStashToolStripMenuItemClick;
        dropStashToolStripMenuItem.Click += DropStashToolStripMenuItemClick;
        rebaseToolStripMenuItem.Click += RebaseToolStripMenuItemClick;
        rebaseInteractivelyToolStripMenuItem.Click += RebaseInteractivelyToolStripMenuItemClick;
        rebaseWithAdvOptionsToolStripMenuItem.Click += RebaseWithAdvOptionsToolStripMenuItemClick;
        resetChangesToolStripMenuItem.Click += ResetChangesToolStripMenuItemClick;
        commitToolStripMenuItem.Click += CommitToolStripMenuItemClick;
        createNewBranchToolStripMenuItem.Click += CreateNewBranchToolStripMenuItemClick;
        createTagToolStripMenuItem.Click += CreateTagToolStripMenuItemClick;
        GotoCurrentRevisionMenuItem.Click += (_, _) => SelectCurrentRevision();
        GotoChildCommitMenuItem.Click += (_, _) => GoToChild();
        GotoParentCommitMenuItem.Click += (_, _) => GoToParent(firstParent: true);
        GotoFirstParentCommitMenuItem.Click += (_, _) => GoToParent(firstParent: true);
        GotoLastParentCommitMenuItem.Click += (_, _) => GoToParent(firstParent: false);
        DrawNonRelativesGrayMenuItem.Click += (_, _) => ToggleDrawNonRelativesGray();
        ShowRemoteBranchesMenuItem.Click += (_, _) => ToggleShowRemoteBranches();
        ShowTagsMenuItem.Click += (_, _) => ToggleShowTags();
        ShowAuthorDateMenuItem.Click += (_, _) => ToggleShowAuthorDate();
        ShowRelativeDateMenuItem.Click += (_, _) => ToggleShowRelativeDate();
        ShowRevisionGraphColumnMenuItem.Click += (_, _) => ToggleRevisionGraphColumn();
        ShowGitNotesColumnMenuItem.Click += (_, _) => ToggleShowGitNotesColumn();
        ShowAuthorNameColumnMenuItem.Click += (_, _) => ToggleAuthorNameColumn();
        ShowDateColumnMenuItem.Click += (_, _) => ToggleDateColumn();
        ShowIdColumnMenuItem.Click += (_, _) => ToggleObjectIdColumn();
        HotkeysEnabled = true;
        UICommandsSourceSet += (_, _) => LoadHotkeys(HotkeySettingsName);
        UpdateContextMenuItems();

        InitializeComplete();
    }

    /// <summary>
    ///  Gets the revision currently selected in the list, or <see langword="null"/>.
    /// </summary>
    public GitRevision? SelectedRevision => lstRevisions.SelectedItem as GitRevision;

    /// <summary>
    ///  Occurs when the selected revision changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <inheritdoc />
    public event EventHandler<FilterChangedEventArgs>? FilterChanged;

    /// <summary>Occurs when the advanced-filter command should focus the available filter UI.</summary>
    public event EventHandler? RevisionFilterRequested;

    /// <summary>Occurs when the selected revision is double-clicked.</summary>
    public event EventHandler<DoubleClickRevisionEventArgs>? DoubleClickRevision;

    private MenuItem GotoCurrentRevisionMenuItem => GetMenuItem(navigateToolStripMenuItem, "GotoCurrentRevision");
    private MenuItem GotoChildCommitMenuItem => GetMenuItem(navigateToolStripMenuItem, "GotoChildCommit");
    private MenuItem GotoParentCommitMenuItem => GetMenuItem(navigateToolStripMenuItem, "GotoParentCommit");
    private MenuItem GotoFirstParentCommitMenuItem => GetMenuItem(navigateToolStripMenuItem, "GotoFirstParentCommit");
    private MenuItem GotoLastParentCommitMenuItem => GetMenuItem(navigateToolStripMenuItem, "GotoLastParentCommit");
    private MenuItem DrawNonRelativesGrayMenuItem => GetMenuItem(viewToolStripMenuItem, "drawNonrelativesGrayToolStripMenuItem");
    private MenuItem ShowRemoteBranchesMenuItem => GetMenuItem(viewToolStripMenuItem, "ShowRemoteBranches");
    private MenuItem ShowTagsMenuItem => GetMenuItem(viewToolStripMenuItem, "showTagsToolStripMenuItem");
    private MenuItem ShowAuthorDateMenuItem => GetMenuItem(viewToolStripMenuItem, "showAuthorDateToolStripMenuItem");
    private MenuItem ShowRelativeDateMenuItem => GetMenuItem(viewToolStripMenuItem, "showRelativeDateToolStripMenuItem");
    private MenuItem ShowRevisionGraphColumnMenuItem => GetMenuItem(viewToolStripMenuItem, "showRevisionGraphColumnToolStripMenuItem");
    private MenuItem ShowGitNotesColumnMenuItem => GetMenuItem(viewToolStripMenuItem, "showGitNotesColumnToolStripMenuItem");
    private MenuItem ShowAuthorNameColumnMenuItem => GetMenuItem(viewToolStripMenuItem, "showAuthorNameColumnToolStripMenuItem");
    private MenuItem ShowDateColumnMenuItem => GetMenuItem(viewToolStripMenuItem, "showDateColumnToolStripMenuItem");
    private MenuItem ShowIdColumnMenuItem => GetMenuItem(viewToolStripMenuItem, "showIdColumnToolStripMenuItem");

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        foreach ((string name, string text) in MenuCommandTranslations)
        {
            translation.AddTranslationItem(RevisionGridTranslationCategory, name, "Text", text);
        }
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        foreach ((string name, string text) in MenuCommandTranslations)
        {
            string? translated = translation.TranslateItem(
                RevisionGridTranslationCategory,
                name,
                "Text",
                () => text);
            GetMenuItem(name).Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(
                string.IsNullOrEmpty(translated) ? text : translated);
        }
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
        => RevisionFilterRequested?.Invoke(this, EventArgs.Empty);

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
            SelectedRevision?.ObjectId ?? default,
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

        foreach (RevisionRowControl row in lstRevisions.GetVisualDescendants().OfType<RevisionRowControl>())
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

    private void RefreshRealizedRows()
    {
        foreach (RevisionRowControl row in lstRevisions.GetVisualDescendants().OfType<RevisionRowControl>())
        {
            row.RefreshCells();
        }
    }

    /// <summary>
    ///  Selects and scrolls to the given revision if it is loaded.
    /// </summary>
    public void SelectRevision(ObjectId objectId)
    {
        SetSelectedRevision(objectId);
    }

    /// <summary>Removes the row context menu, like the WinForms grid method.</summary>
    public void DisableContextMenu()
    {
        lstRevisions.ContextMenu = null;
    }

    /// <summary>Gets or replaces the context menu attached to revision rows.</summary>
    public ContextMenu? RevisionContextMenu
    {
        get => lstRevisions.ContextMenu;
        set => lstRevisions.ContextMenu = value;
    }

    /// <summary>Selects and scrolls to the given revision if it is loaded.</summary>
    public bool SetSelectedRevision(ObjectId objectId)
    {
        GitRevision? revision = _revisions.Find(r => r.ObjectId == objectId);
        if (revision is null)
        {
            return false;
        }

        lstRevisions.SelectedItem = revision;
        lstRevisions.ScrollIntoView(revision);
        return true;
    }

    bool IRevisionGridUpdate.SetSelectedRevision(ObjectId commitId, bool toggleSelection, bool updateNavigationHistory)
        => SetSelectedRevision(commitId);

    #region IRevisionGridInfo

    public ObjectId CurrentCheckout => _headId ?? default;

    public GitRevision GetRevision(ObjectId objectId)
    {
        // Like WinForms, may return null; callers null-check.
        return _revisions.Find(r => r.ObjectId == objectId)!;
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
        => SelectedRevision is GitRevision selectedRevision ? [selectedRevision] : [];

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

    private void lstRevisions_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        PointerPointProperties properties = e.GetCurrentPoint(lstRevisions).Properties;
        if (properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed
            && e.Source is Control { DataContext: GitRevision revision })
        {
            lstRevisions.SelectedItem = revision;
        }
    }

    private void UpdateContextMenuItems()
    {
        GitRevision? revision = SelectedRevision;
        bool hasCommands = TryGetUICommandsDirect(out IGitUICommands? commands);
        bool isBareRepository = hasCommands && commands!.Module.IsBareRepository();
        bool regularRevision = revision is { IsArtificial: false } && hasCommands && !isBareRepository;

        copyToClipboardToolStripMenuItem.RefreshItems();
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
        _rebaseOnTopOf = null;

        if (regularRevision)
        {
            PopulateRefMenus(revision!, commands!);
        }

        SetVisible(checkoutBranchToolStripMenuItem, checkoutBranchToolStripMenuItem.Items.Count > 0);
        SetVisible(tsmiPushBranch, tsmiPushBranch.Items.Count > 0);
        SetVisible(mergeBranchToolStripMenuItem, mergeBranchToolStripMenuItem.Items.Count > 0);
        SetVisible(rebaseOnToolStripMenuItem, regularRevision && _rebaseOnTopOf is not null);
        SetVisible(resetChangesToolStripMenuItem, revision is { IsArtificial: true } && hasCommands && !isBareRepository);
        SetVisible(commitToolStripMenuItem, revision is { IsArtificial: true } && hasCommands && !isBareRepository);
        SetVisible(createNewBranchToolStripMenuItem, regularRevision);
        SetVisible(renameBranchToolStripMenuItem, renameBranchToolStripMenuItem.Items.Count > 0);
        SetVisible(deleteBranchToolStripMenuItem, deleteBranchToolStripMenuItem.Items.Count > 0);
        SetVisible(createTagToolStripMenuItem, revision is { IsArtificial: false } && hasCommands);
        SetVisible(deleteTagToolStripMenuItem, deleteTagToolStripMenuItem.Items.Count > 0);

        sepCopy.IsVisible = copyToClipboardToolStripMenuItem.IsVisible;
        sepBranch.IsVisible = checkoutBranchToolStripMenuItem.IsVisible
            || tsmiPushBranch.IsVisible
            || mergeBranchToolStripMenuItem.IsVisible
            || rebaseOnToolStripMenuItem.IsVisible;
        sepBranchModification.IsVisible = createNewBranchToolStripMenuItem.IsVisible
            || renameBranchToolStripMenuItem.IsVisible
            || deleteBranchToolStripMenuItem.IsVisible;
        sepNavigate.IsVisible = revision is not null;

        navigateToolStripMenuItem.IsVisible = revision is not null;
        UpdateNavigationMenu(revision);
        viewToolStripMenuItem.IsVisible = hasCommands;
        UpdateViewMenuChecks();

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

    private void ResetChangesToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartResetChangesDialog(
            GetOwner(),
            Module.GetWorkTreeFiles(),
            onlyWorkTree: SelectedRevision?.ObjectId == ObjectId.WorkTreeId);
    }

    private void CommitToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartCommitDialog(GetOwner());
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

    private bool GoToParent(bool firstParent)
    {
        if (SelectedRevision is not GitRevision revision)
        {
            return false;
        }

        GitRevision actualRevision = GetActualRevision(revision);
        IReadOnlyList<ObjectId>? parentIds = actualRevision.ParentIds;
        ObjectId parentId = firstParent
            ? parentIds?.FirstOrDefault() ?? default
            : parentIds?.LastOrDefault() ?? default;
        return !parentId.IsZero && SetSelectedRevision(parentId);
    }

    private bool GoToChild()
    {
        if (SelectedRevision is not GitRevision revision)
        {
            return false;
        }

        GitRevision? child = _revisions.FirstOrDefault(
            candidate => candidate.ParentIds?.Contains(revision.ObjectId) == true);
        return child is not null && SetSelectedRevision(child.ObjectId);
    }

    private void UpdateNavigationMenu(GitRevision? revision)
    {
        GotoCurrentRevisionMenuItem.IsEnabled = _headId is ObjectId headId
            && _revisions.Any(candidate => candidate.ObjectId == headId);
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
        DrawNonRelativesGrayMenuItem.IsChecked = AppSettings.RevisionGraphDrawNonRelativesGray;
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

    private void ToggleDrawNonRelativesGray()
    {
        AppSettings.RevisionGraphDrawNonRelativesGray = !AppSettings.RevisionGraphDrawNonRelativesGray;
        ApplySettingsAndRefreshRows();
    }

    private void ToggleShowRemoteBranches()
    {
        AppSettings.ShowRemoteBranches = !AppSettings.ShowRemoteBranches;
        ApplySettingsAndRefreshRows();
    }

    private void ToggleShowTags()
    {
        AppSettings.ShowTags = !AppSettings.ShowTags;
        ApplySettingsAndRefreshRows();
    }

    private void ToggleShowAuthorDate()
    {
        AppSettings.ShowAuthorDate = !AppSettings.ShowAuthorDate;
        ApplySettingsAndRefreshRows();
    }

    private void ToggleShowRelativeDate()
    {
        AppSettings.RelativeDate = !AppSettings.RelativeDate;
        ApplySettingsAndRefreshRows();
    }

    private void ToggleRevisionGraphColumn()
    {
        AppSettings.ShowRevisionGridGraphColumn = !AppSettings.ShowRevisionGridGraphColumn;
        ApplySettingsAndRefreshRows();
    }

    private void ToggleShowGitNotesColumn()
    {
        AppSettings.ShowGitNotesColumn.Value = !AppSettings.ShowGitNotesColumn.Value;
        ReloadCurrentView();
    }

    private void ToggleAuthorNameColumn()
    {
        AppSettings.ShowAuthorNameColumn = !AppSettings.ShowAuthorNameColumn;
        ApplySettingsAndRefreshRows();
    }

    private void ToggleDateColumn()
    {
        AppSettings.ShowDateColumn = !AppSettings.ShowDateColumn;
        ApplySettingsAndRefreshRows();
    }

    private void ToggleObjectIdColumn()
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

    protected override bool ExecuteCommand(int command)
    {
        switch ((Command)command)
        {
            case Command.ToggleRevisionGraph: ToggleRevisionGraphColumn(); break;
            case Command.ToggleAuthorDateCommitDate: ToggleShowAuthorDate(); break;
            case Command.ToggleShowRelativeDate: ToggleShowRelativeDate(); break;
            case Command.ToggleDrawNonRelativesGray: ToggleDrawNonRelativesGray(); break;
            case Command.ToggleShowGitNotes:
                AppSettings.ShowGitNotes = !AppSettings.ShowGitNotes;
                ReloadCurrentView();
                break;
            case Command.ToggleShowGitNotesColumn: ToggleShowGitNotesColumn(); break;
            case Command.ToggleShowTags: ToggleShowTags(); break;
            case Command.ShowRemoteBranches: ToggleShowRemoteBranches(); break;
            case Command.SelectCurrentRevision: SelectCurrentRevision(); break;
            case Command.GoToParent:
            case Command.GoToFirstParent:
                return GoToParent(firstParent: true);
            case Command.GoToLastParent: return GoToParent(firstParent: false);
            case Command.GoToChild: return GoToChild();
            case Command.ToggleBetweenArtificialAndHeadCommits: return ToggleBetweenArtificialAndHeadCommits();
            case Command.ToggleHighlightSelectedBranch: return HighlightSelectedBranch();
            case Command.DeleteRef: return DeleteSingleRef();
            case Command.RenameRef: return RenameSingleRef();
            default: return base.ExecuteCommand(command);
        }

        return true;
    }

    private bool ToggleBetweenArtificialAndHeadCommits()
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
        lstRevisions.ItemsSource = null;
        lblLoadingStatus.Text = "Loading…";

        RevisionObserver observer = new(this, cancellationToken);
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
            IReadOnlyList<IGitRef> refs = module.GetRefs(RefsFilter.NoFilter);
            string selectedBranch = module.GetSelectedBranch(emptyIfDetached: true);
            IGitRef? selectedRef = refs.FirstOrDefault(
                gitRef => gitRef.IsHead && gitRef.Name == selectedBranch);
            if (selectedRef is not null)
            {
                selectedRef.IsSelected = true;
                refs.FirstOrDefault(
                    gitRef => gitRef.IsRemote
                        && gitRef.Remote == selectedRef.TrackingRemote
                        && gitRef.LocalName == selectedRef.MergeWith)
                    ?.IsSelectedHeadMergeSource = true;
            }

            _refsByObjectId = refs
                .Where(gitRef => !gitRef.ObjectId.IsZero)
                .ToLookup(gitRef => gitRef.ObjectId);

            RevisionReader reader = new(module);
            bool hasNotes = AppSettings.ShowGitNotesColumn.Value || AppSettings.ShowGitNotes;
            reader.GetLog(observer, revisionFilter, pathFilter, hasNotes, autostashLabel: "autostash", cancellationToken);
        });
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

        // Swapping the ItemsSource per batch keeps the virtualized list simple; batches are
        // few (the reader flushes at most every 500 ms).
        lstRevisions.ItemsSource = _revisions.ToArray();
        lblLoadingStatus.Text = $"{_revisions.Count} revisions…";
        SelectPendingRevision();
    }

    private void OnLoadingCompleted(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        // The graph rows straightened after the final CacheTo become visible only when the
        // realized row controls render again, so refresh the list once at the end.
        lstRevisions.ItemsSource = _revisions.ToArray();
        RefreshRealizedRows();

        lblLoadingStatus.Text = $"{_revisions.Count} revisions";
        SelectPendingRevision();

        // Like the WinForms grid, select a row when loading finishes.
        if (lstRevisions.SelectedItem is null && _revisions.Count > 0)
        {
            lstRevisions.SelectedIndex = 0;
        }
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
        }
    }

    private void UpdateVisibleGraphColumnWidth()
    {
        RevisionRowControl[] visibleRows =
        [
            .. lstRevisions.GetVisualDescendants().OfType<RevisionRowControl>(),
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

    private sealed class RevisionObserver(RevisionGridControl owner, CancellationToken cancellationToken) : IObserver<IReadOnlyList<GitRevision>>
    {
        public void OnNext(IReadOnlyList<GitRevision> value)
        {
            owner.AddToGraph(value, cancellationToken);
            Dispatcher.UIThread.Post(() => owner.AppendRevisions(value, cancellationToken));
        }

        public void OnCompleted()
        {
            owner._revisionGraph.LoadingCompleted();

            // Finish the row cache (including segment straightening); before this final pass
            // GetSegmentsForRow reports the cache dirty and rows render without a graph.
            int lastRowIndex = owner._revisionGraph.Count - 1;
            owner._revisionGraph.CacheTo(lastRowIndex, lastRowIndex, cancellationToken);

            Dispatcher.UIThread.Post(() => owner.OnLoadingCompleted(cancellationToken));
        }

        public void OnError(Exception error)
            => Dispatcher.UIThread.Post(() => owner.OnLoadingError(error, cancellationToken));
    }

    /// <summary>One provider-shaped row recycled by the virtualizing panel.</summary>
    private sealed class RevisionRowControl : Grid
    {
        private readonly List<(ColumnProvider Provider, Control Cell)> _cells = [];

        public RevisionRowControl(RevisionGridControl owner)
        {
            Height = RowHeight;
            Classes.Add("revision-row");

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
            }
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
    }
}
