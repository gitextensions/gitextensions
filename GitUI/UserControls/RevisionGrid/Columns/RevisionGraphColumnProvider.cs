using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GitCommands;
using GitUI.NBugReports;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUI.UserControls.RevisionGrid.Graph.Rendering;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class RevisionGraphColumnProvider : ColumnProvider
    {
        private readonly LaneInfoProvider _laneInfoProvider;
        private readonly RevisionGraph _revisionGraph;
        private readonly GraphCache _graphCache = new();

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
            if (AppSettings.ShowRevisionGridGraphColumn
                && e.State.HasFlag(DataGridViewElementStates.Visible)
                && e.RowIndex >= 0
                && e.RowIndex < _revisionGraph.Count)
            {
                try
                {
                    if (PaintGraphCell(e.RowIndex, e.CellBounds, e.Graphics))
                    {
                        e.Handled = true;
                    }
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

            return;

            bool PaintGraphCell(int rowIndex, Rectangle cellBounds, Graphics graphics)
            {
                // Renders the required row into _graphCache.GraphBitmap if the row is available and not yet cached, and draws it from the cache.

                int height = _graphCache.Capacity * rowHeight;
                int width = Column.Width;

                if (width <= 0 || height <= 0)
                {
                    // Nothing to be drawn
                    return true;
                }

                if (_revisionGraph.GetSegmentsForRow(rowIndex) is null)
                {
                    // Needs to be refreshed when available
                    return false;
                }

                _graphCache.Allocate(Math.Max(width, GraphRenderer.LaneWidth * 3), height);

                // Compute how much the head needs to move to show the requested item.
                int neededHeadAdjustment = rowIndex - _graphCache.Head;
                if (neededHeadAdjustment > 0)
                {
                    neededHeadAdjustment -= _graphCache.Capacity - 1;
                    if (neededHeadAdjustment < 0)
                    {
                        neededHeadAdjustment = 0;
                    }
                }

                int newRows = _graphCache.Count < _graphCache.Capacity
                    ? (rowIndex - _graphCache.Count) + 1
                    : 0;

                // Adjust the head of the cache
                _graphCache.Head += neededHeadAdjustment;
                _graphCache.HeadRow += neededHeadAdjustment;
                _graphCache.HeadRow %= _graphCache.Capacity;
                if (_graphCache.HeadRow < 0)
                {
                    _graphCache.HeadRow += _graphCache.Capacity;
                }

                int start;
                int end;
                if (newRows > 0)
                {
                    start = _graphCache.Head + _graphCache.Count;
                    _graphCache.Count = Math.Min(_graphCache.Count + newRows, _graphCache.Capacity);
                    end = _graphCache.Head + _graphCache.Count;
                }
                else if (neededHeadAdjustment > 0)
                {
                    end = _graphCache.Head + _graphCache.Count;
                    start = Math.Max(_graphCache.Head, end - neededHeadAdjustment);
                }
                else if (neededHeadAdjustment < 0)
                {
                    start = _graphCache.Head;
                    end = start + Math.Min(_graphCache.Capacity, -neededHeadAdjustment);
                }
                else
                {
                    // Item already in the cache
                    DrawRectangleFromCache();
                    return true;
                }

                RenderVisibleGraphToCache();
                DrawRectangleFromCache();
                return true;

                void DrawRectangleFromCache()
                {
                    Rectangle cellRect = new(
                        0,
                        ((_graphCache.HeadRow + rowIndex - _graphCache.Head) % _graphCache.Capacity) * rowHeight,
                        width,
                        rowHeight);

                    graphics.DrawImage(
                        _graphCache.GraphBitmap,
                        cellBounds,
                        cellRect,
                        GraphicsUnit.Pixel);
                }

                void RenderVisibleGraphToCache()
                {
                    for (int index = start; index < end; index++)
                    {
                        // Get the x,y value of the current item's upper left in the cache
                        int curCacheRow = (_graphCache.HeadRow + index - _graphCache.Head) % _graphCache.Capacity;
                        int x = ColumnLeftMargin;
                        int y = curCacheRow * rowHeight;

                        Validates.NotNull(_graphCache.GraphBitmapGraphics);

                        Rectangle laneRect = new(0, y, width, rowHeight);
                        Region oldClip = _graphCache.GraphBitmapGraphics.Clip;

                        if (index > 0 && (index == start || curCacheRow == 0))
                        {
                            // Draw previous row first. Clip top to row. We also need to clear the area
                            // before we draw since nothing else would clear the top 1/2 of the item to draw.
                            _graphCache.GraphBitmapGraphics.RenderingOrigin = new Point(x, y - rowHeight);
                            _graphCache.GraphBitmapGraphics.Clip = new Region(laneRect);
                            _graphCache.GraphBitmapGraphics.Clear(Color.Transparent);
                            DrawItem(index - 1);
                            _graphCache.GraphBitmapGraphics.Clip = oldClip;
                        }

                        if (index == end - 1)
                        {
                            // Use a custom clip for the last row
                            _graphCache.GraphBitmapGraphics.Clip = new Region(laneRect);
                        }

                        _graphCache.GraphBitmapGraphics.RenderingOrigin = new Point(x, y);

                        DrawItem(index);

                        _graphCache.GraphBitmapGraphics.Clip = oldClip;
                    }
                }

                void DrawItem(int index)
                {
                    GraphRenderer.DrawItem(_revisionGraph.Config, _graphCache.GraphBitmapGraphics, index, width, rowHeight, _revisionGraph.GetSegmentsForRow, RevisionGraphDrawStyle, _revisionGraph.HeadId);
                }
            }
        }

        public override void Clear()
        {
            _graphCache.Reset();
        }

        public override void Refresh(int rowHeight, in VisibleRowRange range)
        {
            // Hide graph column when there it is disabled
            Column.Visible = AppSettings.ShowRevisionGridGraphColumn;
            if (!Column.Visible)
            {
                return;
            }

            _graphCache.Reset();
            Column.Width = CalculateGraphColumnWidth(range);
        }

        public override void OnColumnWidthChanged(DataGridViewColumnEventArgs e)
        {
            _graphCache.Reset();
        }

        public void HighlightBranch(ObjectId id)
        {
            _revisionGraph.HighlightBranch(id);
        }

        public override void OnVisibleRowsChanged(in VisibleRowRange range)
        {
            if (!Column.Visible)
            {
                return;
            }

            // Keep an extra page in the cache
            _graphCache.AdjustCapacity((range.Count * 2) + 1);
            int width = CalculateGraphColumnWidth(range);
            if (Column.Width != width)
            {
                Column.Width = width;
                Column.DataGridView.InvalidateColumn(Column.Index);
            }
        }

        private int CalculateGraphColumnWidth(in VisibleRowRange range)
        {
            int maxLaneCount = range.Max(index => _revisionGraph.GetSegmentsForRow(index)?.GetLaneCount()) ?? 0;
            int visibleLaneCount = Math.Min(maxLaneCount, GraphRenderer.MaxLanes);
            int lanesWidth = GraphRenderer.LaneWidth * visibleLaneCount;
            return ColumnLeftMargin + Math.Max(lanesWidth, Column.MinimumWidth);
        }

        public bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision)
        {
            string? toolTip;
            return TryGetToolTip(e, revision, out toolTip);
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
    }
}
