using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUI.UserControls.RevisionGrid.Graph.Rendering;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class RevisionGraphColumnProvider : ColumnProvider, IDisposable
{
    private static readonly Cursor HandCursor = new(StandardCursorType.Hand);

    private readonly RevisionGridControl _grid;
    private readonly LaneInfoProvider _laneInfoProvider;
    private readonly RevisionGraph _revisionGraph;
    private readonly HoverHighlightCalculator _hoverHighlight;

    private VisibleRowRange _cachedVisibleRange;

    public RevisionGraphColumnProvider(
        RevisionGraph revisionGraph,
        RevisionGridControl grid,
        IGitRevisionSummaryBuilder gitRevisionSummaryBuilder)
        : base(
            "Graph",
            new GridLength(CalculateGraphColumnWidth(visibleLaneCount: 0)),
            GraphRenderer.LaneWidth,
            resizable: false,
            headerText: string.Empty)
    {
        _revisionGraph = revisionGraph;
        _grid = grid;
        _laneInfoProvider = new LaneInfoProvider(new LaneNodeLocator(revisionGraph), gitRevisionSummaryBuilder);
        _hoverHighlight = new HoverHighlightCalculator(_revisionGraph, () => _cachedVisibleRange);
    }

    public RevisionGraphDrawStyle RevisionGraphDrawStyle { get; set; } = RevisionGraphDrawStyle.DrawNonRelativesGray;

    public override void ApplySettings()
    {
        Column.IsVisible = AppSettings.ShowRevisionGridGraphColumn;
        RevisionGraphDrawStyle = AppSettings.RevisionGraphDrawNonRelativesGray
            ? RevisionGraphDrawStyle.DrawNonRelativesGray
            : RevisionGraphDrawStyle.Normal;
    }

    public override Control CreateCell()
    {
        GraphCellControl graph = new(this)
        {
            ClipToBounds = true,
            Margin = new Thickness(ColumnLeftMargin, 0, 0, 0),
        };
        graph.Classes.Add("revision-graph-cell");
        return graph;
    }

    public override void UpdateCell(Control control, GitRevision revision)
    {
        GraphCellControl graph = (GraphCellControl)control;
        graph.Revision = revision;
        ToolTip.SetTip(graph, null);
    }

    public override void Clear()
    {
        _hoverHighlight.Clear();
    }

    internal void UpdateVisibleRange(IEnumerable<GitRevision> revisions)
    {
        int[] rowIndexes =
        [
            .. revisions
                .Select(revision => _revisionGraph.TryGetRowIndex(revision.ObjectId, out int rowIndex) ? rowIndex : -1)
                .Where(rowIndex => rowIndex >= 0),
        ];
        _cachedVisibleRange = rowIndexes.Length == 0
            ? new VisibleRowRange(fromIndex: 0, count: 0)
            : new VisibleRowRange(rowIndexes.Min(), rowIndexes.Max() - rowIndexes.Min() + 1);
    }

    /// <summary>
    ///  Updates the hover highlight to show only the ancestry of the
    ///  <paramref name="gitRef"/> and tracked remote or the tracking local.
    ///  Debounces before computing, cancelling any prior pending computation when called again.
    ///  Set <see langword="null"/> to clear hover highlighting.
    /// </summary>
    /// <param name="gitRef">The ref to highlight, or <see langword="null"/> to clear.</param>
    /// <param name="rowIndex">
    ///  The row index of the hovered ref label, limits the search to the visible range.
    /// </param>
    public async Task SetHoverHighlightAsync(IGitRef? gitRef, int rowIndex = -1)
    {
        await _hoverHighlight.SetAsync(gitRef, rowIndex);
        foreach (GraphCellControl graph in _grid.GetVisualDescendants().OfType<GraphCellControl>())
        {
            graph.InvalidateVisual();
        }
    }

    internal static int CalculateGraphColumnWidth(int visibleLaneCount)
        => 6
            + Math.Max(
                GraphRenderer.LaneWidth * Math.Min(visibleLaneCount, GraphRenderer.MaxLanes),
                GraphRenderer.LaneWidth);

    internal int GetLaneCount(GitRevision revision)
    {
        try
        {
            return _revisionGraph.TryGetRowIndex(revision.ObjectId, out int rowIndex)
                ? _revisionGraph.GetSegmentsForRow(rowIndex)?.GetLaneCount() ?? 0
                : 0;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    internal bool DrawGraph(DrawingContext context, GitRevision revision, double rowHeight)
        => _grid.DrawGraphCell(context, revision, RevisionGraphDrawStyle, rowHeight, _hoverHighlight.HighlightedIds);

    internal string? GetLaneToolTip(GitRevision revision, double x)
    {
        if (!AppSettings.ShowRevisionGridTooltips.Value
            || x < 0
            || !_revisionGraph.TryGetRowIndex(revision.ObjectId, out int rowIndex))
        {
            return null;
        }

        int lane = (int)(x / GraphRenderer.LaneWidth);
        string toolTip = _laneInfoProvider.GetLaneInfo(rowIndex, lane);
        return string.IsNullOrEmpty(toolTip) ? null : toolTip;
    }

    public void Dispose() => _hoverHighlight.Dispose();

    private sealed class GraphCellControl : Control
    {
        private readonly RevisionGraphColumnProvider _provider;
        private GitRevision? _revision;

        public GraphCellControl(RevisionGraphColumnProvider provider)
        {
            _provider = provider;
            PointerMoved += OnPointerMoved;
            PointerExited += (_, _) =>
            {
                ToolTip.SetTip(this, null);
                Cursor = null;
            };
        }

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
            if (_revision is not null)
            {
                _provider.DrawGraph(context, _revision, Bounds.Height);
            }
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            string? toolTip = _revision is null
                ? null
                : _provider.GetLaneToolTip(_revision, e.GetPosition(this).X);
            ToolTip.SetTip(this, toolTip);
            Cursor = toolTip is null
                ? null
                : HandCursor;
        }
    }
}
