using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.NBugReports;
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
    private readonly RetainedGraphCache _graphDisplayCache = new();
    private readonly RetainedGraphCache _graphRenderCache = new();

    private int _columnWidth;

    private VisibleRowRange _cachedVisibleRange;
    private readonly HoverHighlightCalculator _hoverHighlight;

    public RevisionGraphColumnProvider(
        RevisionGraph revisionGraph,
        RevisionGridControl grid,
        IGitRevisionSummaryBuilder gitRevisionSummaryBuilder)
        : base(
            "Graph",
            new GridLength(6 + GraphRenderer.LaneWidth),
            GraphRenderer.LaneWidth,
            resizable: false,
            headerText: string.Empty)
    {
        _revisionGraph = revisionGraph;
        _grid = grid;
        _laneInfoProvider = new LaneInfoProvider(new LaneNodeLocator(revisionGraph), gitRevisionSummaryBuilder);
        _hoverHighlight = new HoverHighlightCalculator(_revisionGraph, () => _cachedVisibleRange);
        _columnWidth = 6 + GraphRenderer.LaneWidth;
    }

    public RevisionGraphDrawStyle RevisionGraphDrawStyle { get; set; } = RevisionGraphDrawStyle.DrawNonRelativesGray;

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

    public override void OnCellPainting(Control control, GitRevision revision)
    {
        try
        {
            DrawGraphCellFromCache(control, revision);
        }
        catch (Exception ex)
        {
            // Consume the exception since it does not bubble up to our handlers
            Trace.Write(ex);
#if DEBUG
            BugReportInvoker.LogError(ex);
#endif
        }
    }

    private void DrawGraphCellFromCache(Control control, GitRevision revision)
    {
        GraphCellControl graph = (GraphCellControl)control;
        graph.Revision = revision;
        ToolTip.SetTip(graph, null);
    }

    public async Task RenderGraphToCacheAsync(
        VisibleRowRange range,
        int toRowIndex,
        int rowHeight,
        CancellationToken cancellationToken)
    {
        RenderGraphToCache(range, toRowIndex, rowHeight);
        cancellationToken.ThrowIfCancellationRequested();
        await Dispatcher.UIThread.InvokeAsync(cancellationToken.ThrowIfCancellationRequested, DispatcherPriority.Render);
        cancellationToken.ThrowIfCancellationRequested();

        _graphDisplayCache.CopyFrom(_graphRenderCache);
        if (!Column.Width.IsAbsolute || Column.Width.Value != _columnWidth)
        {
            Column.Width = new GridLength(_columnWidth);
        }

        _grid.RefreshRealizedRows();
    }

    private void RenderGraphToCache(VisibleRowRange range, int toRowIndex, int rowHeight)
    {
        _cachedVisibleRange = range;
        int width = CalculateGraphColumnWidth(range);
        if (_columnWidth != width)
        {
            _columnWidth = width;
            _graphRenderCache.Reset();
        }

        _graphRenderCache.Range = range;
        for (int rowIndex = Math.Max(0, range.FromIndex); rowIndex <= toRowIndex; rowIndex++)
        {
            RenderRowToCache(rowIndex, rowHeight);
        }
    }

    private void RenderRowToCache(int rowIndex, int rowHeight)
    {
        _graphRenderCache.LastRenderedRow = rowIndex;
        _graphRenderCache.RowHeight = rowHeight;
    }

    public override void ApplySettings()
    {
        Column.IsVisible = AppSettings.ShowRevisionGridGraphColumn;
        RevisionGraphDrawStyle = AppSettings.RevisionGraphDrawNonRelativesGray
            ? RevisionGraphDrawStyle.DrawNonRelativesGray
            : RevisionGraphDrawStyle.Normal;
    }

    public override void Clear()
    {
        _graphRenderCache.Reset();
        _graphDisplayCache.Reset();
        _hoverHighlight.Clear();
    }

    public void HighlightBranch(ObjectId id)
    {
        _revisionGraph.HighlightBranch(id);
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
        RenderGraphToCache(
            _cachedVisibleRange,
            rowIndexes.Length == 0 ? -1 : rowIndexes.Max(),
            (int)Math.Round(RevisionGridControl.GetRowHeight(_grid), MidpointRounding.AwayFromZero));
        _graphDisplayCache.CopyFrom(_graphRenderCache);
        if (!Column.Width.IsAbsolute || Column.Width.Value != _columnWidth)
        {
            Column.Width = new GridLength(_columnWidth);
        }
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

    private int CalculateGraphColumnWidth(in VisibleRowRange range)
    {
        int maxLaneCount = range.Max(index => _revisionGraph.GetSegmentsForRow(index)?.GetLaneCount()) ?? 0;
        int visibleLaneCount = Math.Min(maxLaneCount, GraphRenderer.MaxLanes);
        return CalculateGraphColumnWidth(visibleLaneCount);
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

    public bool TryGetToolTip(GitRevision revision, double x, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        if (!AppSettings.ShowRevisionGridTooltips.Value
            || x < 0
            || !_revisionGraph.TryGetRowIndex(revision.ObjectId, out int rowIndex))
        {
            toolTip = null;
            return false;
        }

        int lane = (int)(x / GraphRenderer.LaneWidth);
        toolTip = _laneInfoProvider.GetLaneInfo(rowIndex, lane);
        return !string.IsNullOrEmpty(toolTip);
    }

    public void Dispose() => _hoverHighlight.Dispose();

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        internal TestAccessor(RevisionGraphColumnProvider revisionGraphColumnProvider)
        {
            RevisionGraphColumnProvider = revisionGraphColumnProvider;
        }

        internal RevisionGraphColumnProvider RevisionGraphColumnProvider { get; }

        internal VisibleRowRange CachedVisibleRange => RevisionGraphColumnProvider._graphRenderCache.Range;

        internal int LastRenderedRow => RevisionGraphColumnProvider._graphRenderCache.LastRenderedRow;

        internal void RenderGraphToCache(VisibleRowRange range, int toRowIndex, int rowHeight)
            => RevisionGraphColumnProvider.RenderGraphToCache(range, toRowIndex, rowHeight);

        internal void RenderRowToCache(int rowIndex, int rowHeight)
            => RevisionGraphColumnProvider.RenderRowToCache(rowIndex, rowHeight);
    }

    private sealed class RetainedGraphCache
    {
        public VisibleRowRange Range { get; set; }

        public int LastRenderedRow { get; set; } = -1;

        public int RowHeight { get; set; }

        public void CopyFrom(RetainedGraphCache source)
        {
            Range = source.Range;
            LastRenderedRow = source.LastRenderedRow;
            RowHeight = source.RowHeight;
        }

        public void Reset()
        {
            Range = default;
            LastRenderedRow = -1;
            RowHeight = 0;
        }
    }

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
            string? toolTip = null;
            if (_revision is not null)
            {
                _provider.TryGetToolTip(_revision, e.GetPosition(this).X, out toolTip);
            }

            ToolTip.SetTip(this, toolTip);
            Cursor = toolTip is null
                ? null
                : HandCursor;
        }
    }
}
