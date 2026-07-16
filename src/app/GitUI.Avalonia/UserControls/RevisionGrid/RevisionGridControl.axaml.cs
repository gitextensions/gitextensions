using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.UserControls.RevisionGrid;
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

// TODO(avalonia-port): milestones M1.1/M1.2 — a revision list with the commit graph column,
// streamed by the shared RevisionReader and shaped by the shared RevisionGraph model.
// Ref labels, avatars, and the ColumnProvider pattern of the WinForms RevisionGridControl
// come in later milestones.
public partial class RevisionGridControl : GitModuleControl
{
    private const int RowHeight = 24;
    private const int GraphLeftMargin = 6;

    private readonly CancellationTokenSequence _refreshSequence = new();
    private readonly List<GitRevision> _revisions = [];
    private readonly RevisionGraph _revisionGraph = new();
    private RevisionGraphConfig _config = new();
    private ObjectId? _headId;
    private ObjectId _pendingSelectedObjectId;
    private int _graphColumnWidth = CalculateGraphColumnWidth(visibleLaneCount: 0);
    private bool _headHighlighted;
    private ILookup<ObjectId, IGitRef>? _refsByObjectId;

    public RevisionGridControl()
    {
        InitializeComponent();

        lstRevisions.ItemTemplate = new FuncDataTemplate<GitRevision>((_, _) => new RevisionRowControl(this), supportsRecycling: true);
        lstRevisions.SelectionChanged += (_, _) =>
        {
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

    internal int GraphColumnWidth => _graphColumnWidth;

    internal static int CalculateGraphColumnWidth(int visibleLaneCount)
        => GraphLeftMargin
            + Math.Max(
                GraphRenderer.LaneWidth * Math.Min(visibleLaneCount, GraphRenderer.MaxLanes),
                GraphRenderer.LaneWidth);

    /// <summary>
    ///  Selects and scrolls to the given revision if it is loaded.
    /// </summary>
    public void SelectRevision(ObjectId objectId)
    {
        SetSelectedRevision(objectId);
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
        ObjectId selectedObjectId = default)
    {
        CancellationToken cancellationToken = _refreshSequence.Next();

        _revisions.Clear();
        _revisionGraph.Clear();
        _config = new RevisionGraphConfig();
        _headId = module.GetCurrentCheckout();
        _pendingSelectedObjectId = selectedObjectId;
        _headHighlighted = false;
        lstRevisions.ItemsSource = null;
        lblLoadingStatus.Text = "Loading…";

        RevisionObserver observer = new(this, cancellationToken);
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
            reader.GetLog(observer, revisionFilter, pathFilter: "", hasNotes: false, autostashLabel: "autostash", cancellationToken);
        });
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
            .Select(GetLaneCount)
            .DefaultIfEmpty()
            .Max();
        int graphColumnWidth = CalculateGraphColumnWidth(visibleLaneCount);
        if (_graphColumnWidth == graphColumnWidth)
        {
            return;
        }

        _graphColumnWidth = graphColumnWidth;
        foreach (RevisionRowControl row in visibleRows)
        {
            row.SetGraphColumnWidth(graphColumnWidth);
        }
    }

    private int GetLaneCount(GitRevision revision)
    {
        try
        {
            return _revisionGraph.TryGetRowIndex(revision.ObjectId, out int rowIndex)
                ? _revisionGraph.GetSegmentsForRow(rowIndex)?.GetLaneCount() ?? 0
                : 0;
        }
        catch (Exception)
        {
            // The reader can advance the row cache while layout is measuring the visible
            // controls. The next layout pass recalculates the width from the stable rows.
            return 0;
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

    /// <summary>
    ///  One list row: graph cell, subject, author, date. Recycled by the virtualizing panel;
    ///  content follows the <c>DataContext</c>.
    /// </summary>
    private sealed class RevisionRowControl : Grid
    {
        private readonly RevisionGridControl _owner;
        private readonly GraphCellControl _graph;
        private readonly StackPanel _messagePanel;
        private readonly TextBlock _subject;
        private readonly TextBlock _author;
        private readonly TextBlock _date;

        public RevisionRowControl(RevisionGridControl owner)
        {
            _owner = owner;
            Height = RowHeight;
            ColumnDefinitions = new ColumnDefinitions($"{owner.GraphColumnWidth},*,180,140");
            Classes.Add("revision-row");

            _graph = new GraphCellControl(owner)
            {
                ClipToBounds = true,
                Margin = new Avalonia.Thickness(GraphLeftMargin, 0, 0, 0),
            };
            _graph.Classes.Add("revision-graph-cell");
            _subject = CreateTextBlock(leftMargin: 0);
            _subject.Classes.Add("revision-subject");
            _messagePanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Margin = new Avalonia.Thickness(6, 0, 2, 0),
                ClipToBounds = true,
            };
            _messagePanel.Classes.Add("revision-message-cell");
            _messagePanel.Children.Add(_subject);
            _author = CreateTextBlock(leftMargin: 8, opacity: 0.85);
            _date = CreateTextBlock(leftMargin: 8, opacity: 0.7);

            SetColumn(_graph, 0);
            SetColumn(_messagePanel, 1);
            SetColumn(_author, 2);
            SetColumn(_date, 3);
            Children.Add(_graph);
            Children.Add(_messagePanel);
            Children.Add(_author);
            Children.Add(_date);

            return;

            static TextBlock CreateTextBlock(int leftMargin, double opacity = 1)
                => new()
                {
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = new Avalonia.Thickness(leftMargin, 0, 2, 0),
                    Opacity = opacity,
                };
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is GitRevision revision)
            {
                // The subject TextBlock is the persistent last child; only ref chips are rebuilt.
                _messagePanel.Children.RemoveRange(0, _messagePanel.Children.Count - 1);
                foreach (Control label in RevisionGridRefRenderer.CreateLabels(revision.Refs))
                {
                    _messagePanel.Children.Insert(_messagePanel.Children.Count - 1, label);
                }

                _subject.Text = revision.Subject;
                _subject.FontWeight = _owner._headId is ObjectId headId
                    && revision.ObjectId == headId
                        ? FontWeight.Bold
                        : FontWeight.Normal;
                _author.Text = revision.Author ?? string.Empty;
                _date.Text = revision.AuthorDate.ToString("yyyy-MM-dd HH:mm");
                _graph.Revision = revision;
            }
        }

        public void SetGraphColumnWidth(int width)
            => ColumnDefinitions[0].Width = new GridLength(width);
    }

    /// <summary>
    ///  Renders the graph cell of one row with the ported <see cref="GraphRenderer"/>.
    /// </summary>
    private sealed class GraphCellControl(RevisionGridControl owner) : Control
    {
        private GitRevision? _revision;

        public GitRevision? Revision
        {
            get => _revision;
            set
            {
                _revision = value;
                InvalidateVisual();
            }
        }

        public override void Render(DrawingContext context)
        {
            if (_revision is null
                || owner._headId is not ObjectId headId
                || !owner._revisionGraph.TryGetRowIndex(_revision.ObjectId, out int rowIndex))
            {
                return;
            }

            // Like the WinForms grid, only paint rows the row cache has fully prepared
            // (drawing looks at the neighbor rows, hence the margin of 2). During loading
            // the reader thread grows the cache concurrently; rows near the frontier are
            // skipped now and repainted with the next batch refresh.
            if (rowIndex + 2 >= owner._revisionGraph.GetCachedCount())
            {
                return;
            }

            try
            {
                GraphRenderer.DrawItem(owner._config, context, rowIndex, RowHeight,
                    owner._revisionGraph.GetSegmentsForRow,
                    RevisionGraphDrawStyle.DrawNonRelativesGray,
                    headId);
            }
            catch (Exception)
            {
                // The row cache can shift under a concurrent CacheTo while loading;
                // skip this frame, the next refresh repaints the row.
            }
        }
    }
}
