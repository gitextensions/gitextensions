using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
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
    private readonly RevisionGridControl _grid;
    private readonly LaneInfoProvider _laneInfoProvider;
    private readonly RevisionGraph _revisionGraph;
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
            GraphCellControl graph = (GraphCellControl)control;
            graph.DataContext = revision;
            ToolTip.SetTip(graph, null);
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

    // The native retained cell draws from the revision graph's cached segments.
    private void DrawGraphCellFromCache(DrawingContext graphics, GitRevision revision, double rowHeight)
        => _grid.DrawGraphCell(graphics, revision, RevisionGraphDrawStyle, rowHeight, _hoverHighlight.HighlightedIds);

    public override void ApplySettings()
    {
        Column.IsVisible = AppSettings.ShowRevisionGridGraphColumn;
    }

    public override void Clear()
    {
        _cachedVisibleRange = default;
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
        _columnWidth = CalculateGraphColumnWidth(_cachedVisibleRange);
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

        // Hover state changed since last render. Invalidate so all rows re-render with new highlight.
        foreach (GraphCellControl graph in _grid.GetVisualDescendants().OfType<GraphCellControl>())
        {
            graph.InvalidateVisual();
        }
    }

    private int CalculateGraphColumnWidth(in VisibleRowRange range)
    {
        int maxLaneCount = range.Max(index => _revisionGraph.GetSegmentsForRow(index)?.GetLaneCount()) ?? 0;
        int visibleLaneCount = Math.Min(maxLaneCount, GraphRenderer.MaxLanes);
        int lanesWidth = GraphRenderer.LaneWidth * visibleLaneCount;

        // Avalonia column dimensions are DIPs; the graph minimum is an integral lane width.
        return ColumnLeftMargin + Math.Max(lanesWidth, (int)Column.MinimumWidth);
    }

    public override bool TryGetToolTip(double? x, GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        if (x is null or < 0
            || !_revisionGraph.TryGetRowIndex(revision.ObjectId, out int rowIndex))
        {
            toolTip = null;
            return false;
        }

        int lane = (int)(x.Value / GraphRenderer.LaneWidth);
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

        internal HoverHighlightCalculator.TestAccessor HoverHighlight
            => RevisionGraphColumnProvider._hoverHighlight.GetTestAccessor();
    }

    // Avalonia realizes graph cells as retained controls instead of DataGridView cell-paint events.
    private sealed class GraphCellControl : Control
    {
        private readonly RevisionGraphColumnProvider _provider;

        public GraphCellControl(RevisionGraphColumnProvider provider)
        {
            _provider = provider;
            PointerExited += (_, _) =>
            {
                ToolTip.SetTip(this, null);
                Cursor = null;
            };
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            // DataGridView hit-tests the entire cell, while the retained lane operations
            // intentionally expose no input geometry of their own.
            context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));
            if (DataContext is GitRevision revision)
            {
                _provider.DrawGraphCellFromCache(context, revision, Bounds.Height);
            }
        }
    }
}
