using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.NBugReports;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUI.UserControls.RevisionGrid.Graph.Rendering;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class RevisionGraphColumnProvider : ColumnProvider
{
    private readonly LaneInfoProvider _laneInfoProvider;
    private readonly RevisionGraph _revisionGraph;
    private readonly GraphCache _graphDisplayCache = new();
    private readonly GraphCache _graphRenderCache = new();

    private int _columnWidth = 0;

    private IReadOnlySet<ObjectId>? _hoverHighlightedIds;
    private volatile bool _hoverHighlightDirty;

    public RevisionGraphColumnProvider(RevisionGraph revisionGraph, IGitRevisionSummaryBuilder gitRevisionSummaryBuilder)
        : base("Graph")
    {
        _revisionGraph = revisionGraph;
        _laneInfoProvider = new LaneInfoProvider(new LaneNodeLocator(_revisionGraph), gitRevisionSummaryBuilder);

        Column = new DataGridViewTextBoxColumn
        {
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            Resizable = DataGridViewTriState.False,
            MinimumWidth = GraphRenderer.LaneWidth
        };
    }

    public RevisionGraphDrawStyle RevisionGraphDrawStyle { get; set; }

    public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in CellStyle style)
    {
        try
        {
            DrawGraphCellFromCache(e.RowIndex, rowHeight, e.CellBounds, e.Graphics!);
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

    private bool DrawGraphCellFromCache(int rowIndex, int rowHeight, Rectangle cellBounds, Graphics graphics)
    {
        // Draws the required row from the cache if available.

        int height = _graphDisplayCache.Capacity * rowHeight;
        int width = Column.Width;

        if (width <= 0 || height <= 0)
        {
            // Nothing to be drawn
            return true;
        }

        int offsetToHead = rowIndex - _graphDisplayCache.HeadRow;
        if (offsetToHead < 0 || offsetToHead >= _graphDisplayCache.Count)
        {
            // Item not in the cache
            return false;
        }

        Rectangle cellRect = new(
            0,
            _graphDisplayCache.GetCacheRow(rowIndex) * rowHeight,
            width,
            rowHeight);

        graphics.DrawImage(
            _graphDisplayCache.GraphBitmap!,
            cellBounds,
            cellRect,
            GraphicsUnit.Pixel);

        return true;
    }

    public async Task RenderGraphToCacheAsync(VisibleRowRange range, int toRowIndex, int rowHeight, CancellationToken cancellationToken)
    {
        DataGridView? control = Column.DataGridView;
        if (control is null)
        {
            return;
        }

        RenderGraphToCache(range, toRowIndex, rowHeight);

        await control.SwitchToMainThreadAsync(cancellationToken);

        _graphDisplayCache.CopyFrom(_graphRenderCache);

        cancellationToken.ThrowIfCancellationRequested();

        if (Column.Width != _columnWidth)
        {
            Column.Width = _columnWidth;
        }

        control.InvalidateColumn(Column.Index);
    }

    private void RenderGraphToCache(VisibleRowRange range, int toRowIndex, int rowHeight)
    {
        int width = CalculateGraphColumnWidth(range);
        if (_columnWidth != width)
        {
            _columnWidth = width;
            _graphRenderCache.Reset();
        }

        if (_hoverHighlightDirty)
        {
            // Hover state changed since last render. Reset so all rows re-render with new highlight.
            _hoverHighlightDirty = false;
            _graphRenderCache.Reset();
        }

        int fromRowIndex = Math.Max(0, range.FromIndex - range.Count);
        _graphRenderCache.AdjustCapacity(range.Count * 3);
        int height = _graphRenderCache.Capacity * rowHeight;
        _graphRenderCache.Allocate(Math.Max(_columnWidth, GraphRenderer.LaneWidth * 3), height);

        for (int rowIndex = fromRowIndex; rowIndex <= toRowIndex; ++rowIndex)
        {
            RenderRowToCache(rowIndex, rowHeight);
        }
    }

    private void RenderRowToCache(int rowIndex, int rowHeight)
    {
        // Renders the required row into _graphRenderCache.GraphBitmap if the row is available and not yet cached.

        int startRow;
        int endRow;
        if (_graphRenderCache.Count == 0)
        {
            // Start the cache with this line
            startRow = rowIndex;
            endRow = rowIndex + 1;
            _graphRenderCache.HeadRow = startRow;
            _graphRenderCache.Count = 1;
        }
        else
        {
            int offsetToHead = rowIndex - _graphRenderCache.HeadRow;
            if (offsetToHead >= 0 && offsetToHead < _graphRenderCache.Count)
            {
                // Item already in the cache
                return;
            }

            if (offsetToHead < 0 && -offsetToHead < _graphRenderCache.Capacity)
            {
                // Scroll back, make the current row the head row
                startRow = rowIndex;
                endRow = _graphRenderCache.HeadRow;
                _graphRenderCache.HeadRow = startRow;
                _graphRenderCache.Count = Math.Min(_graphRenderCache.Count + endRow - startRow, _graphRenderCache.Capacity);
                _graphRenderCache.Head += _graphRenderCache.Capacity + offsetToHead;
                _graphRenderCache.Head %= _graphRenderCache.Capacity;
            }
            else if (offsetToHead > 0 && offsetToHead <= 2 * (_graphRenderCache.Capacity - 1))
            {
                // Scroll forward
                startRow = _graphRenderCache.HeadRow + _graphRenderCache.Count; // all rows before have already been rendered
                endRow = rowIndex + 1;
                _graphRenderCache.Count += endRow - startRow; // Count = Count + (rowIndex + 1) - (HeadRow + Count) = rowIndex + 1 - HeadRow
                int neededHeadAdjustment = Math.Max(0, _graphRenderCache.Count - _graphRenderCache.Capacity);
                _graphRenderCache.Count -= neededHeadAdjustment;
                _graphRenderCache.HeadRow += neededHeadAdjustment;
                _graphRenderCache.Head += neededHeadAdjustment;
                _graphRenderCache.Head %= _graphRenderCache.Capacity;
            }
            else
            {
                // Restart the cache with this line
                startRow = rowIndex;
                endRow = rowIndex + 1;
                _graphRenderCache.HeadRow = startRow;
                _graphRenderCache.Count = 1;
            }
        }

        int x = ColumnLeftMargin;
        int cellWidth = _columnWidth - ColumnLeftMargin;
        Rectangle laneRect = new(x, 0, cellWidth, rowHeight);
        for (rowIndex = startRow; rowIndex < endRow; ++rowIndex)
        {
            // Get the y coordinate of the current item's upper left in the cache
            laneRect.Y = _graphRenderCache.GetCacheRow(rowIndex) * rowHeight;

            using Region newClip = new(laneRect);
            _graphRenderCache.GraphBitmapGraphics!.Clip = newClip;

            _graphRenderCache.GraphBitmapGraphics.RenderingOrigin = new Point(x, laneRect.Y);

            GraphRenderer.DrawItem(_revisionGraph.Config, _graphRenderCache.GraphBitmapGraphics, rowIndex, rowHeight, _revisionGraph.GetSegmentsForRow, RevisionGraphDrawStyle, _revisionGraph.HeadId, _hoverHighlightedIds);
        }
    }

    public override void ApplySettings()
    {
        Column.Visible = AppSettings.ShowRevisionGridGraphColumn;
    }

    public override void Clear()
    {
        _graphRenderCache.Reset();
        _graphDisplayCache.Reset();
    }

    public void HighlightBranch(ObjectId id)
    {
        _revisionGraph.HighlightBranch(id);
    }

    /// <summary>
    ///  Updates the hover highlight to show only the ancestry of branches whose local name is in
    ///  <paramref name="branchBaseNames"/>. All lanes outside this set are dimmed. Pass
    ///  <see langword="null"/> to clear hover highlighting.
    /// </summary>
    public void SetHoverHighlight(IReadOnlySet<string>? branchBaseNames)
    {
        if (branchBaseNames is null)
        {
            _hoverHighlightedIds = null;
            _hoverHighlightDirty = true;
            return;
        }

        HashSet<ObjectId> ancestorIds = [];
        foreach (RevisionGraphRevision revision in _revisionGraph.Revisions)
        {
            if (revision.GitRevision?.Refs.Any(r => IsInBranchGroup(r, branchBaseNames)) is true)
            {
                WalkAncestors(revision, ancestorIds);
            }
        }

        _hoverHighlightedIds = ancestorIds.Count > 0 ? ancestorIds : null;
        _hoverHighlightDirty = true;

        return;

        static bool IsInBranchGroup(IGitRef r, IReadOnlySet<string> baseNames)
            => (r.IsHead || r.IsRemote) && baseNames.Contains(r.LocalName);

        static void WalkAncestors(RevisionGraphRevision revision, HashSet<ObjectId> result)
        {
            Stack<RevisionGraphRevision> stack = new();
            stack.Push(revision);
            while (stack.Count > 0)
            {
                RevisionGraphRevision current = stack.Pop();
                if (!result.Add(current.Objectid))
                {
                    continue;
                }

                foreach (RevisionGraphRevision parent in current.Parents)
                {
                    stack.Push(parent);
                }
            }
        }
    }

    private int CalculateGraphColumnWidth(in VisibleRowRange range)
    {
        int maxLaneCount = range.Max(index => _revisionGraph.GetSegmentsForRow(index)?.GetLaneCount()) ?? 0;
        int visibleLaneCount = Math.Min(maxLaneCount, GraphRenderer.MaxLanes);
        int lanesWidth = GraphRenderer.LaneWidth * visibleLaneCount;
        return ColumnLeftMargin + Math.Max(lanesWidth, Column.MinimumWidth);
    }

    public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        if (e.X >= ColumnLeftMargin && GraphRenderer.LaneWidth >= 0 && e.RowIndex >= 0)
        {
            int lane = (e.X - ColumnLeftMargin) / GraphRenderer.LaneWidth;
            toolTip = _laneInfoProvider.GetLaneInfo(e.RowIndex, lane);
            return true;
        }

        toolTip = default;
        return false;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        internal TestAccessor(RevisionGraphColumnProvider revisionGraphColumnProvider)
        {
            RevisionGraphColumnProvider = revisionGraphColumnProvider;
        }

        internal RevisionGraphColumnProvider RevisionGraphColumnProvider { get; }

        internal GraphCache GraphCache => RevisionGraphColumnProvider._graphRenderCache;

        internal void RenderGraphToCache(VisibleRowRange range, int toRowIndex, int rowHeight)
            => RevisionGraphColumnProvider.RenderGraphToCache(range, toRowIndex, rowHeight);

        internal void RenderRowToCache(int rowIndex, int rowHeight)
            => RevisionGraphColumnProvider.RenderRowToCache(rowIndex, rowHeight);
    }
}
