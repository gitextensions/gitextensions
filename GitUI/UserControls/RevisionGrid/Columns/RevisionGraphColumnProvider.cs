using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
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
            e.Handled = false;
            if (AppSettings.ShowRevisionGridGraphColumn
                && e.State.HasFlag(DataGridViewElementStates.Visible)
                && e.RowIndex >= 0
                && e.RowIndex < _revisionGraph.Count)
            {
                try
                {
                    if (PaintGraphCell(e.RowIndex, rowHeight, e.CellBounds, e.Graphics))
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
        }

        private bool PaintGraphCell(int rowIndex, int rowHeight, Rectangle cellBounds, Graphics graphics)
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

            int startRow;
            int endRow;
            if (_graphCache.Count == 0)
            {
                // Start the cache with this line
                startRow = rowIndex;
                endRow = rowIndex + 1;
                _graphCache.HeadRow = startRow;
                _graphCache.Count = 1;
            }
            else
            {
                int offsetToHead = rowIndex - _graphCache.HeadRow;
                if (offsetToHead >= 0 && offsetToHead < _graphCache.Count)
                {
                    // Item already in the cache
                    DrawRectangleFromCache();
                    return true;
                }

                if (offsetToHead < 0 && -offsetToHead < _graphCache.Capacity)
                {
                    // Scroll back, make the current row the head row
                    startRow = rowIndex;
                    endRow = _graphCache.HeadRow;
                    _graphCache.HeadRow = startRow;
                    _graphCache.Count = Math.Min(_graphCache.Count + endRow - startRow, _graphCache.Capacity);
                    _graphCache.Head += _graphCache.Capacity + offsetToHead;
                    _graphCache.Head %= _graphCache.Capacity;
                }
                else if (offsetToHead > 0 && offsetToHead <= 2 * (_graphCache.Capacity - 1))
                {
                    // Scroll forward
                    startRow = _graphCache.HeadRow + _graphCache.Count; // all rows before have already been rendered
                    endRow = rowIndex + 1;
                    _graphCache.Count += endRow - startRow; // Count = Count + (rowIndex + 1) - (HeadRow + Count) = rowIndex + 1 - HeadRow
                    int neededHeadAdjustment = Math.Max(0, _graphCache.Count - _graphCache.Capacity);
                    _graphCache.Count -= neededHeadAdjustment;
                    _graphCache.HeadRow += neededHeadAdjustment;
                    _graphCache.Head += neededHeadAdjustment;
                    _graphCache.Head %= _graphCache.Capacity;
                }
                else
                {
                    // Restart the cache with this line
                    startRow = rowIndex;
                    endRow = rowIndex + 1;
                    _graphCache.HeadRow = startRow;
                    _graphCache.Count = 1;
                }
            }

            RenderVisibleGraphToCache();
            DrawRectangleFromCache();
            return true;

            int GetCacheRow(int rowIndex) => (_graphCache.Head + rowIndex - _graphCache.HeadRow) % _graphCache.Capacity;

            void DrawRectangleFromCache()
            {
                Rectangle cellRect = new(
                    0,
                    GetCacheRow(rowIndex) * rowHeight,
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
                Validates.NotNull(_graphCache.GraphBitmapGraphics);
                SmoothingMode oldSmoothingMode = _graphCache.GraphBitmapGraphics.SmoothingMode;
                Region oldClip = _graphCache.GraphBitmapGraphics.Clip;
                try
                {
                    int x = ColumnLeftMargin;
                    int cellWidth = width - ColumnLeftMargin;
                    Rectangle laneRect = new(x, 0, cellWidth, rowHeight);
                    for (int rowIndex = startRow; rowIndex < endRow; ++rowIndex)
                    {
                        // Get the y coordinate of the current item's upper left in the cache
                        laneRect.Y = GetCacheRow(rowIndex) * rowHeight;

                        using Region newClip = new(laneRect);
                        _graphCache.GraphBitmapGraphics.Clip = newClip;

                        _graphCache.GraphBitmapGraphics.RenderingOrigin = new Point(x, laneRect.Y);

                        GraphRenderer.DrawItem(_revisionGraph.Config, _graphCache.GraphBitmapGraphics, rowIndex, rowHeight, _revisionGraph.GetSegmentsForRow, RevisionGraphDrawStyle, _revisionGraph.HeadId);
                    }
                }
                finally
                {
                    _graphCache.GraphBitmapGraphics.SmoothingMode = oldSmoothingMode;
                    _graphCache.GraphBitmapGraphics.Clip = oldClip;
                }
            }
        }

        public override void ApplySettings()
        {
            Column.Visible = AppSettings.ShowRevisionGridGraphColumn;
        }

        public override void Clear()
        {
            _graphCache.Reset();
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
                Column.DataGridView?.InvalidateColumn(Column.Index);
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

        internal TestAccessor GetTestAccessor() => new(this);

        internal readonly struct TestAccessor
        {
            internal TestAccessor(RevisionGraphColumnProvider revisionGraphColumnProvider)
            {
                RevisionGraphColumnProvider = revisionGraphColumnProvider;
            }

            internal RevisionGraphColumnProvider RevisionGraphColumnProvider { get; }

            internal GraphCache GraphCache => RevisionGraphColumnProvider._graphCache;

            internal bool PaintGraphCell(int rowIndex, int rowHeight, Rectangle cellBounds, Graphics graphics)
                => RevisionGraphColumnProvider.PaintGraphCell(rowIndex, rowHeight, cellBounds, graphics);
        }
    }
}
