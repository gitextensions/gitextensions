using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUI.UserControls.RevisionGrid.Graph.Rendering;
using GitUIPluginInterfaces;

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
public partial class RevisionGridControl : UserControl
{
    private const int RowHeight = 24;
    private const int GraphColumnWidth = 160;

    private readonly CancellationTokenSequence _refreshSequence = new();
    private readonly List<GitRevision> _revisions = [];
    private readonly RevisionGraph _revisionGraph = new();
    private RevisionGraphConfig _config = new();
    private ObjectId? _headId;
    private bool _headHighlighted;

    public RevisionGridControl()
    {
        InitializeComponent();

        lstRevisions.ItemTemplate = new FuncDataTemplate<GitRevision>((_, _) => new RevisionRowControl(this), supportsRecycling: true);
        lstRevisions.SelectionChanged += (_, _) => SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///  Gets the revision currently selected in the list, or <see langword="null"/>.
    /// </summary>
    public GitRevision? SelectedRevision => lstRevisions.SelectedItem as GitRevision;

    /// <summary>
    ///  Occurs when the selected revision changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    ///  Starts (re)loading the history of <paramref name="module"/> in the background,
    ///  streaming batches into the list as they are parsed.
    /// </summary>
    public void ReloadRevisions(IGitModule module)
    {
        CancellationToken cancellationToken = _refreshSequence.Next();

        _revisions.Clear();
        _revisionGraph.Clear();
        _config = new RevisionGraphConfig();
        _headId = module.GetCurrentCheckout();
        _headHighlighted = false;
        lstRevisions.ItemsSource = null;
        lblLoadingStatus.Text = "Loading…";

        RevisionObserver observer = new(this, cancellationToken);
        ThreadHelper.FileAndForget(() =>
        {
            RevisionReader reader = new(module);
            reader.GetLog(observer, revisionFilter: "--all", pathFilter: "", hasNotes: false, autostashLabel: "autostash", cancellationToken);
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
    }

    private void OnLoadingError(Exception exception, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            lblLoadingStatus.Text = $"Failed to load revisions: {exception.Message}";
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
        private readonly GraphCellControl _graph;
        private readonly TextBlock _subject;
        private readonly TextBlock _author;
        private readonly TextBlock _date;

        public RevisionRowControl(RevisionGridControl owner)
        {
            Height = RowHeight;
            ColumnDefinitions = new ColumnDefinitions($"{GraphColumnWidth},*,180,140");

            _graph = new GraphCellControl(owner) { ClipToBounds = true };
            _subject = CreateTextBlock(leftMargin: 8);
            _author = CreateTextBlock(leftMargin: 8, opacity: 0.85);
            _date = CreateTextBlock(leftMargin: 8, opacity: 0.7);

            SetColumn(_graph, 0);
            SetColumn(_subject, 1);
            SetColumn(_author, 2);
            SetColumn(_date, 3);
            Children.Add(_graph);
            Children.Add(_subject);
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
                _subject.Text = revision.Subject;
                _author.Text = revision.Author ?? string.Empty;
                _date.Text = revision.AuthorDate.ToString("yyyy-MM-dd HH:mm");
                _graph.Revision = revision;
            }
        }
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

            GraphRenderer.DrawItem(owner._config, context, rowIndex, RowHeight,
                owner._revisionGraph.GetSegmentsForRow,
                RevisionGraphDrawStyle.DrawNonRelativesGray,
                headId);
        }
    }
}
