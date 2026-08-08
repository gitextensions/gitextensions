using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.NBugReports;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUI.UserControls.RevisionGrid.Graph.Rendering;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class RevisionGraphColumnProvider : ColumnProvider, IDisposable
{
    private readonly LaneInfoProvider _laneInfoProvider;
    private readonly RevisionGraph _revisionGraph;
    private readonly GraphCache _graphDisplayCache = new();
    private readonly GraphCache _graphRenderCache = new();

    private int _columnWidth = 0;

    private const int HoverHighlightDebounceMs = 100;

    private readonly record struct HighlightCacheEntry(string GitRefCompleteName, string? GitRefGuid, int RowIndex, VisibleRowRange VisibleRange, int RevisionCount, IReadOnlySet<ObjectId>? HighlightedIds);

    private IReadOnlySet<ObjectId>? _hoverHighlightedIds;
    private bool _hoverHighlightDirty;
    private VisibleRowRange _cachedVisibleRange;
    private readonly CancellationTokenSequence _hoverHighlightSequence = new();

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

    private void DrawGraphCellFromCache(int rowIndex, int rowHeight, Rectangle cellBounds, Graphics graphics)
    {
        // Draws the required row from the cache if available.

        int height = _graphDisplayCache.Capacity * rowHeight;
        int width = Column.Width;

        if (width <= 0 || height <= 0)
        {
            // Nothing to be drawn
            return;
        }

        int offsetToHead = rowIndex - _graphDisplayCache.HeadRow;
        if (offsetToHead < 0 || offsetToHead >= _graphDisplayCache.Count)
        {
            // Item not in the cache
            return;
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
        _cachedVisibleRange = range;

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
        _hoverHighlightedIds = null;
        _hoverHighlightDirty = true;
    }

    public void HighlightBranch(ObjectId id)
    {
        _revisionGraph.HighlightBranch(id);
    }

    /// <summary>
    ///  Updates the hover highlight to show only the ancestry of the
    ///  <paramref name="gitRef"/> and tracked remote or the tracking local.
    ///  Debounces by <see cref="HoverHighlightDebounceMs"/> ms before computing,
    ///  cancelling any prior pending computation when called again.
    ///  Set <see langword="null"/> to clear hover highlighting.
    /// </summary>
    /// <param name="gitRef">The ref to highlight, or <see langword="null"/> to clear.</param>
    /// <param name="rowIndex">
    ///  The row index of the hovered ref label, limits the search to the visible range.
    /// </param>
    public async Task SetHoverHighlightAsync(IGitRef? gitRef, int rowIndex = -1)
    {
        CancellationToken cancellationToken = _hoverHighlightSequence.Next();
        await Task.Delay(HoverHighlightDebounceMs, cancellationToken);
        ComputeHoverHighlight(gitRef, rowIndex, cancellationToken);
    }

    private void ComputeHoverHighlight(IGitRef? gitRef, int rowIndex, CancellationToken cancellationToken)
    {
        if (gitRef is null || rowIndex < 0)
        {
            if (_hoverHighlightedIds is null)
            {
                return;
            }

            _hoverHighlightedIds = null;
            _hoverHighlightDirty = true;
            return;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        int revisionCount = _revisionGraph.Count;
        HashSet<ObjectId> ancestorIds = [];

        // Build the set of currently visible ObjectIds below current row (and some parents/children).
        RevisionGraphRevision? hoveredRevision = _revisionGraph.GetNodeForRow(rowIndex);
        Validates.NotNull(hoveredRevision);
        int visibleTo = Math.Max(_cachedVisibleRange.Count - 1, _cachedVisibleRange.FromIndex + _cachedVisibleRange.Count - 1);
        HashSet<ObjectId> visibleIds = new(capacity: 50 + (2 * _cachedVisibleRange.Count));

        // Add visible ids and their parents, find if a tracked branch is in the set
        AddIdAndParents(_revisionGraph, rowIndex, visibleIds);
        bool checkOtherRefs = gitRef.IsRemote || !string.IsNullOrEmpty(gitRef.MergeWith);

        RevisionGraphRevision? belowRev = null;
        for (int row = rowIndex + 1; row <= visibleTo; row++)
        {
            RevisionGraphRevision rev = AddIdAndParents(_revisionGraph, row, visibleIds);
            if (checkOtherRefs && rev.GitRevision?.Refs.Any(r => IsInBranchGroup(r, gitRef)) is true)
            {
                belowRev = rev;
            }
        }

        WalkAncestors(hoveredRevision, ancestorIds, visibleIds);

        if (belowRev is not null)
        {
            WalkAncestors(belowRev, ancestorIds, visibleIds);
        }
        else if (checkOtherRefs)
        {
            // Rows above rowIndex are only needed for the upward search
            int searchFrom = Math.Max(0, _cachedVisibleRange.FromIndex - Math.Max(50, _cachedVisibleRange.Count));
            for (int row = rowIndex - 1; row >= searchFrom; row--)
            {
                RevisionGraphRevision rev = AddIdAndParents(_revisionGraph, row, visibleIds);
                if (rev.GitRevision?.Refs.Any(r => IsInBranchGroup(r, gitRef)) is true)
                {
                    WalkAncestors(rev, ancestorIds, visibleIds);
                    break;
                }
            }
        }

        IReadOnlySet<ObjectId>? highlightedIds = ancestorIds.Count > 0 ? ancestorIds : null;

        _highlightCache[_highlightCacheNext] = new HighlightCacheEntry(gitRef.CompleteName, gitRef.Guid, rowIndex, _lastVisibleRange, revisionCount, highlightedIds);
        _highlightCacheNext = (_highlightCacheNext + 1) % HighlightCacheCapacity;

        if (HightlightIdsSameValues(_hoverHighlightedIds, highlightedIds))
        {
            return;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        _hoverHighlightedIds = highlightedIds;
        _hoverHighlightDirty = true;

        return;

        static RevisionGraphRevision AddIdAndParents(RevisionGraph revisionGraph, int row, HashSet<ObjectId> visibleIds)
        {
            RevisionGraphRevision? rev = revisionGraph.GetNodeForRow(row);
            Validates.NotNull(rev);
            visibleIds.Add(rev.Objectid);

            // A segment whose Child is above the visible area still crosses visible rows and must
            // be highlighted. Include the Parent endpoint of every segment leaving below so that
            // WalkAncestors can reach them without storing all ancestors.
            IRevisionGraphRow? graphRow = revisionGraph.GetSegmentsForRow(row);
            if (graphRow is null)
            {
                return rev;
            }

            foreach (RevisionGraphSegment segment in graphRow.Segments)
            {
                visibleIds.Add(segment.Parent.Objectid);
            }

            return rev;
        }

        static bool IsInBranchGroup(IGitRef r, IGitRef gitRef)
            => gitRef.IsTrackingRemote(r)
            || r.IsTrackingRemote(gitRef)

            // Match NestledRef with grid remote/local
            // Only gitRef.Guid to not allocate strings
            || (gitRef.Guid is null && gitRef.CompleteName == r.CompleteName);

        static void WalkAncestors(RevisionGraphRevision revision, HashSet<ObjectId> result, IReadOnlySet<ObjectId> visibleIds)
        {
            Stack<RevisionGraphRevision> stack = new();
            HashSet<ObjectId> visited = [];
            stack.Push(revision);
            while (stack.Count > 0)
            {
                RevisionGraphRevision current = stack.Pop();
                if (!visited.Add(current.Objectid))
                {
                    continue;
                }

                // If already in result from a previous WalkAncestors call, all ancestors
                // of this commit were already processed — prune this entire subtree.
                if (result.Contains(current.Objectid))
                {
                    continue;
                }

                // Only store revisions that are in the visible set (commit nodes or segment
                // endpoints crossing the viewport boundary). The visible set is pre-extended
                // with one segment above and below, so spanning segments are handled.
                if (visibleIds.Contains(current.Objectid))
                {
                    result.Add(current.Objectid);
                }

                foreach (RevisionGraphRevision parent in current.Parents)
                {
                    stack.Push(parent);
                }
            }
        }

        static bool TryApplyFromCache(HighlightCacheEntry[] highlightCache, IGitRef gitRef, int rowIndex, VisibleRowRange lastVisibleRange, int revisionCount, out IReadOnlySet<ObjectId>? hoverHighlightedIds)
        {
            for (int i = 0; i < HighlightCacheCapacity; i++)
            {
                ref readonly HighlightCacheEntry entry = ref highlightCache[i];
                if (entry.GitRefCompleteName == gitRef.CompleteName
                    && entry.GitRefGuid == gitRef.Guid
                    && entry.RowIndex == rowIndex
                    && entry.VisibleRange == lastVisibleRange
                    && entry.RevisionCount == revisionCount)
                {
                    hoverHighlightedIds = entry.HighlightedIds;
                    return true;
                }
            }

            hoverHighlightedIds = null;
            return false;
        }

        static bool HightlightIdsSameValues(IReadOnlySet<ObjectId>? left, IReadOnlySet<ObjectId>? right)
        {
            if (left is null || right is null)
            {
                return left is null && right is null;
            }

            return left.SetEquals(right);
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

    public void Dispose() => _hoverHighlightSequence.Dispose();

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        internal TestAccessor(RevisionGraphColumnProvider revisionGraphColumnProvider)
        {
            RevisionGraphColumnProvider = revisionGraphColumnProvider;
        }

        internal RevisionGraphColumnProvider RevisionGraphColumnProvider { get; }

        internal GraphCache GraphCache => RevisionGraphColumnProvider._graphRenderCache;

        internal IReadOnlySet<ObjectId>? HoverHighlightedIds => RevisionGraphColumnProvider._hoverHighlightedIds;

        internal bool IsHoverHighlightDirty => RevisionGraphColumnProvider._hoverHighlightDirty;

        internal void RenderGraphToCache(VisibleRowRange range, int toRowIndex, int rowHeight)
            => RevisionGraphColumnProvider.RenderGraphToCache(range, toRowIndex, rowHeight);

        internal void RenderRowToCache(int rowIndex, int rowHeight)
            => RevisionGraphColumnProvider.RenderRowToCache(rowIndex, rowHeight);

        internal void SetHoverHighlight(IGitRef? gitRef, int rowIndex = -1)
            => RevisionGraphColumnProvider.ComputeHoverHighlight(gitRef, rowIndex, CancellationToken.None);
    }
}
