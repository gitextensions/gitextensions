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
using GitExtUtils;
using GitUI.CommandsDialogs;
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

public partial class RevisionGridControl : GitModuleControl, IRevisionGridInfo
{
    private const int RowHeight = 24;

    private readonly CancellationTokenSequence _refreshSequence = new();
    private readonly AuthorRevisionHighlighting _authorHighlighting = new();
    private readonly List<ColumnProvider> _columnProviders = [];
    private readonly List<GitRevision> _revisions = [];
    private readonly RevisionGraph _revisionGraph = new();
    private readonly RevisionGraphColumnProvider _revisionGraphColumnProvider;
    private readonly MessageColumnProvider _messageColumnProvider;
    private ObjectId? _headId;
    private ObjectId _pendingSelectedObjectId;
    private bool _headHighlighted;
    private bool _parentsAreRewritten;
    private ILookup<ObjectId, IGitRef>? _refsByObjectId;
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
        checkoutBranchToolStripMenuItem.Click += PerformFirstDropdownItemClick;
        createNewBranchToolStripMenuItem.Click += CreateNewBranchToolStripMenuItemClick;
        createTagToolStripMenuItem.Click += CreateTagToolStripMenuItemClick;
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

    /// <summary>Occurs when the selected revision is double-clicked.</summary>
    public event EventHandler<DoubleClickRevisionEventArgs>? DoubleClickRevision;

    internal IReadOnlyList<ColumnProvider> ColumnProviders => _columnProviders;

    internal int GraphColumnWidth => (int)_revisionGraphColumnProvider.Column.Width.Value;

    internal static int CalculateGraphColumnWidth(int visibleLaneCount)
        => RevisionGraphColumnProvider.CalculateGraphColumnWidth(visibleLaneCount);

    internal bool IsCurrentCheckout(GitRevision revision)
        => _headId is ObjectId headId && revision.ObjectId == headId;

    internal void SetAheadBehindDataProvider(IAheadBehindDataProvider? provider)
        => _messageColumnProvider.SetAheadBehindDataProvider(provider);

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
        bool enabled = revision is not null
            && !revision.IsArtificial
            && TryGetUICommandsDirect(out IGitUICommands? commands)
            && !commands.Module.IsBareRepository();
        checkoutBranchToolStripMenuItem.IsEnabled = enabled;
        createNewBranchToolStripMenuItem.IsEnabled = enabled;
        createTagToolStripMenuItem.IsEnabled = enabled;

        // Like the WinForms dropdown: one entry per renameable local branch on the commit.
        renameBranchToolStripMenuItem.Items.Clear();
        if (enabled && revision!.Refs is not null)
        {
            foreach (IGitRef head in new GitRefListsForRevision(revision).GetRenameableLocalBranches())
            {
                // Double the underscores so branch names are not treated as access keys.
                MenuItem branchItem = new() { Header = head.Name.Replace("_", "__") };
                branchItem.Click += delegate { UICommands.StartRenameDialog(GetOwner(), head.Name); };
                renameBranchToolStripMenuItem.Items.Add(branchItem);
            }
        }

        renameBranchToolStripMenuItem.IsEnabled = renameBranchToolStripMenuItem.Items.Count > 0;
    }

    private void PerformFirstDropdownItemClick(object? sender, EventArgs e)
    {
        if (SelectedRevision is GitRevision revision)
        {
            // The reduced menu has no per-branch submenu yet; the checkout dialog provides
            // the equivalent choice, filtered to branches containing this commit.
            UICommands.StartCheckoutBranch(GetOwner(), [revision.ObjectId]);
        }
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

        if (revisionFilter == "--all")
        {
            List<string> filterArguments = [];
            if (!AppSettings.ShowGitNotes)
            {
                filterArguments.Add($"--exclude={GitRefName.RefsNotesPrefix}");
            }

            if (!AppSettings.ShowStashes)
            {
                filterArguments.Add($"--exclude={GitRefName.RefsStashPrefix}");
            }

            if (!AppSettings.ShowSessionRefs)
            {
                filterArguments.Add($"--exclude={GitRefName.RefsSessionsPrefix}**");
            }

            filterArguments.Add("--all");
            revisionFilter = string.Join(' ', filterArguments);
        }

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
        ThreadHelper.FileAndForget(async () =>
        {
            SuperProjectInfo? superProjectInfo = await GetSuperprojectCheckoutAsync(module).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            _superprojectCurrentCheckout = superProjectInfo;
            if (superProjectInfo is not null)
            {
                await this.SwitchToMainThreadAsync(cancellationToken);
                RefreshRealizedRows();
            }
        });

        ThreadHelper.FileAndForget(() =>
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
        if (_revisionGraphColumnProvider.Column.Width.Value == graphColumnWidth)
        {
            return;
        }

        _revisionGraphColumnProvider.Column.Width = new GridLength(graphColumnWidth);
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
