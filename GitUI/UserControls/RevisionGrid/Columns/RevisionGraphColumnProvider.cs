using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class RevisionGraphColumnProvider : ColumnProvider
    {
        private const int MaxLanes = 40;

        private static readonly int _nodeDimension = DpiUtil.Scale(10);
        private static readonly int _laneWidth = DpiUtil.Scale(16);
        private static readonly int _laneLineWidth = DpiUtil.Scale(2);

        private readonly RevisionGridControl _grid;
        private readonly RevisionGraph _revisionGraph;

        private RevisionGraphDrawStyleEnum _revisionGraphDrawStyleCache;
        private RevisionGraphDrawStyleEnum _revisionGraphDrawStyle;
        private int _cacheCount; // Number of elements in the cache.
        private int _cacheCountMax; // Number of elements allowed in the cache. Is based on control height.
        private int _cacheHead = -1; // The 'slot' that is the head of the circular bitmap
        private int _cacheHeadRow; // The node row that is in the head slot
        [CanBeNull] private Bitmap _graphBitmap;
        [CanBeNull] private Graphics _graphBitmapGraphics;

        public RevisionGraphColumnProvider(RevisionGridControl grid, RevisionGraph revisionGraph)
            : base("Graph")
        {
            _grid = grid;
            _revisionGraph = revisionGraph;

            // TODO is it worth creating a lighter-weight column type?

            Column = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Resizable = DataGridViewTriState.False,
                MinimumWidth = DpiUtil.Scale(5)
            };
        }

        public RevisionGraphDrawStyleEnum RevisionGraphDrawStyle
        {
            get
            {
                if (_revisionGraphDrawStyle == RevisionGraphDrawStyleEnum.HighlightSelected)
                {
                    return RevisionGraphDrawStyleEnum.HighlightSelected;
                }

                if (AppSettings.RevisionGraphDrawNonRelativesGray)
                {
                    return RevisionGraphDrawStyleEnum.DrawNonRelativesGray;
                }

                return RevisionGraphDrawStyleEnum.Normal;
            }
            set { _revisionGraphDrawStyle = value; }
        }

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in CellStyle style)
        {
            if (AppSettings.ShowRevisionGridGraphColumn &&
                e.State.HasFlag(DataGridViewElementStates.Visible) &&
                e.RowIndex >= 0 &&
                _revisionGraph.Count != 0 &&
                _revisionGraph.Count > e.RowIndex &&
                PaintGraphCell(e.RowIndex, e.CellBounds, e.Graphics))
            {
                e.Handled = true;
            }

            return;

            bool PaintGraphCell(int rowIndex, Rectangle cellBounds, Graphics graphics)
            {
                // Draws the required row into _graphBitmap, or retrieves an equivalent one from the cache.

                int height = _cacheCountMax * rowHeight;
                int width = Column.Width;

                if (width <= 0 || height <= 0)
                {
                    return false;
                }

                EnsureCacheIsLargeEnough();

                // Compute how much the head needs to move to show the requested item.
                int neededHeadAdjustment = rowIndex - _cacheHead;
                if (neededHeadAdjustment > 0)
                {
                    neededHeadAdjustment -= _cacheCountMax - 1;
                    if (neededHeadAdjustment < 0)
                    {
                        neededHeadAdjustment = 0;
                    }
                }

                var newRows = _cacheCount < _cacheCountMax
                    ? (rowIndex - _cacheCount) + 1
                    : 0;

                // Adjust the head of the cache
                _cacheHead = _cacheHead + neededHeadAdjustment;
                _cacheHeadRow = (_cacheHeadRow + neededHeadAdjustment) % _cacheCountMax;
                if (_cacheHeadRow < 0)
                {
                    _cacheHeadRow = _cacheCountMax + _cacheHeadRow;
                }

                int start;
                int end;
                if (newRows > 0)
                {
                    start = _cacheHead + _cacheCount;
                    _cacheCount = Math.Min(_cacheCount + newRows, _cacheCountMax);
                    end = _cacheHead + _cacheCount;
                }
                else if (neededHeadAdjustment > 0)
                {
                    end = _cacheHead + _cacheCount;
                    start = Math.Max(_cacheHead, end - neededHeadAdjustment);
                }
                else if (neededHeadAdjustment < 0)
                {
                    start = _cacheHead;
                    end = start + Math.Min(_cacheCountMax, -neededHeadAdjustment);
                }
                else
                {
                    // Item already in the cache
                    CreateRectangle();
                    return true;
                }

                if (!DrawVisibleGraph())
                {
                    return false;
                }

                CreateRectangle();
                return true;

                void CreateRectangle()
                {
                    var cellRect = new Rectangle(
                        0,
                        ((_cacheHeadRow + rowIndex - _cacheHead) % _cacheCountMax) * rowHeight,
                        width,
                        rowHeight);

                    graphics.DrawImage(
                        _graphBitmap,
                        cellBounds,
                        cellRect,
                        GraphicsUnit.Pixel);
                }

                bool DrawVisibleGraph()
                {
                    for (var index = start; index < end; index++)
                    {
                        // Get the x,y value of the current item's upper left in the cache
                        var curCacheRow = (_cacheHeadRow + index - _cacheHead) % _cacheCountMax;
                        var x = ColumnLeftMargin;
                        var y = curCacheRow * rowHeight;

                        var laneRect = new Rectangle(0, y, width, rowHeight);
                        var oldClip = _graphBitmapGraphics.Clip;

                        if (index == start || curCacheRow == 0)
                        {
                            // Draw previous row first. Clip top to row. We also need to clear the area
                            // before we draw since nothing else would clear the top 1/2 of the item to draw.
                            _graphBitmapGraphics.RenderingOrigin = new Point(x, y - rowHeight);
                            _graphBitmapGraphics.Clip = new Region(laneRect);
                            _graphBitmapGraphics.Clear(Color.Transparent);
                            DrawItem(_graphBitmapGraphics, index);
                            _graphBitmapGraphics.Clip = oldClip;
                        }

                        if (index == end - 1)
                        {
                            // Use a custom clip for the last row
                            _graphBitmapGraphics.Clip = new Region(laneRect);
                        }

                        _graphBitmapGraphics.RenderingOrigin = new Point(x, y);

                        var success = DrawItem(_graphBitmapGraphics, index);

                        _graphBitmapGraphics.Clip = oldClip;

                        if (!success)
                        {
                            ClearDrawCache();
                            return false;
                        }
                    }

                    return true;
                }

                void EnsureCacheIsLargeEnough()
                {
                    if (_graphBitmap == null ||

                        // Resize the bitmap when the with or height is changed. The height won't change very often.
                        // The with changes more often, when branches become visible/invisible.
                        // Try to be 'smart' and not resize the bitmap for each little change. Enlarge when needed
                        // but never shrink the bitmap since the huge performance hit is worse than the little extra memory.
                        _graphBitmap.Width < width || _graphBitmap.Height != height)
                    {
                        if (_graphBitmap != null)
                        {
                            _graphBitmap.Dispose();
                            _graphBitmap = null;
                        }

                        if (_graphBitmapGraphics != null)
                        {
                            _graphBitmapGraphics.Dispose();
                            _graphBitmapGraphics = null;
                        }

                        _graphBitmap = new Bitmap(
                            Math.Max(width, _laneWidth * 3),
                            height,
                            PixelFormat.Format32bppPArgb);
                        _graphBitmapGraphics = Graphics.FromImage(_graphBitmap);
                        _graphBitmapGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                        _cacheHead = 0;
                        _cacheCount = 0;
                    }
                }

                bool DrawItem(Graphics g, int index)
                {
                    // Clip to the area we're drawing in, but draw 1 pixel past so
                    // that the top/bottom of the line segment's anti-aliasing isn't
                    // visible in the final rendering.
                    int top = g.RenderingOrigin.Y + rowHeight;
                    var laneRect = new Rectangle(0, top, width, rowHeight);
                    Region oldClip = g.Clip;
                    var newClip = new Region(laneRect);
                    newClip.Intersect(oldClip);
                    g.Clip = newClip;
                    g.Clear(Color.Transparent);

                    if (index > _revisionGraph.GetCachedCount())
                    {
                        return true;
                    }

                    // Getting RevisionGraphDrawStyle results in call to AppSettings. This is not very cheap, cache.
                    _revisionGraphDrawStyleCache = RevisionGraphDrawStyle;

                    var oldSmoothingMode = g.SmoothingMode;

                    var previousRow = _revisionGraph.GetSegmentsForRow(Math.Max(0, index - 1));
                    var currentRow = _revisionGraph.GetSegmentsForRow(index);
                    var nextRow = _revisionGraph.GetSegmentsForRow(index + 1);

                    if (currentRow != null && previousRow != null && nextRow != null)
                    {
                        foreach (RevisionGraphSegment revisionGraphRevision in currentRow.Segments.OrderBy(s => s.Child.IsRelative))
                        {
                            int startLane = -10;
                            int centerLane = -10;
                            int endLane = -10;

                            if (revisionGraphRevision.Parent == currentRow.Revision)
                            {
                                // This lane ends here
                                startLane = previousRow.GetLaneIndexForSegment(revisionGraphRevision);
                                centerLane = currentRow.GetLaneIndexForSegment(revisionGraphRevision);
                            }
                            else
                            {
                                if (revisionGraphRevision.Child == currentRow.Revision)
                                {
                                    // This lane starts here
                                    centerLane = currentRow.GetLaneIndexForSegment(revisionGraphRevision);
                                    endLane = nextRow.GetLaneIndexForSegment(revisionGraphRevision);
                                }
                                else
                                {
                                    // this lane crosses
                                    startLane = previousRow.GetLaneIndexForSegment(revisionGraphRevision);
                                    centerLane = currentRow.GetLaneIndexForSegment(revisionGraphRevision);
                                    endLane = nextRow.GetLaneIndexForSegment(revisionGraphRevision);
                                }
                            }

                            Point revisionGraphRevisionPositionStart = new Point(startLane, -1);
                            Point revisionGraphRevisionPositionCenter = new Point(centerLane, 0);
                            Point revisionGraphRevisionPositionEnd = new Point(endLane, 1);

                            int startX = g.RenderingOrigin.X + (int)((revisionGraphRevisionPositionStart.X + 0.5) * _laneWidth);
                            int startY = top + (revisionGraphRevisionPositionStart.Y * rowHeight) + (rowHeight / 2);

                            int centerX = g.RenderingOrigin.X + (int)((revisionGraphRevisionPositionCenter.X + 0.5) * _laneWidth);
                            int centerY = top + (revisionGraphRevisionPositionCenter.Y * rowHeight) + (rowHeight / 2);

                            int endX = g.RenderingOrigin.X + (int)((revisionGraphRevisionPositionEnd.X + 0.5) * _laneWidth);
                            int endY = top + (revisionGraphRevisionPositionEnd.Y * rowHeight) + (rowHeight / 2);

                            Brush brush;

                            if (revisionGraphRevision.Child.IsRelative)
                            {
                                brush = RevisionGraphLaneColor.GetBrushForLane(revisionGraphRevision.Parent.LaneColor);
                            }
                            else
                            {
                                brush = RevisionGraphLaneColor.NonRelativeBrush;
                            }

                            // EndLane
                            if (startLane >= 0 && centerLane >= 0 && (startLane <= MaxLanes || centerLane <= MaxLanes))
                            {
                                DrawSegment(g, brush, startX, startY, centerX, centerY);
                            }

                            // StartLane
                            if (endLane >= 0 && centerLane >= 0 && (endLane <= MaxLanes || centerLane <= MaxLanes))
                            {
                                DrawSegment(g, brush, centerX, centerY, endX, endY);
                            }

                            if (centerLane < MaxLanes &&
                                (currentRow.Revision == revisionGraphRevision.Parent ||
                                 currentRow.Revision == revisionGraphRevision.Child))
                            {
                                Rectangle nodeRect = new Rectangle(centerX - (_nodeDimension / 2), centerY - (_nodeDimension / 2), _nodeDimension, _nodeDimension);

                                var square = currentRow.Revision.HasRef;
                                var hasOutline = currentRow.Revision.IsCheckedOut;

                                if (square)
                                {
                                    g.SmoothingMode = SmoothingMode.None;
                                    g.FillRectangle(brush, nodeRect);
                                }
                                else //// Circle
                                {
                                    nodeRect.Width = nodeRect.Height = _nodeDimension - 1;

                                    g.SmoothingMode = SmoothingMode.AntiAlias;
                                    g.FillEllipse(brush, nodeRect);
                                }

                                if (hasOutline)
                                {
                                    nodeRect.Inflate(1, 1);

                                    var outlineColor = Color.Black;

                                    using (var pen = new Pen(outlineColor, 2))
                                    {
                                        if (square)
                                        {
                                            g.SmoothingMode = SmoothingMode.None;
                                            g.DrawRectangle(pen, nodeRect);
                                        }
                                        else //// Circle
                                        {
                                            g.SmoothingMode = SmoothingMode.AntiAlias;
                                            g.DrawEllipse(pen, nodeRect);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Reset graphics options
                    g.Clip = oldClip;

                    g.SmoothingMode = oldSmoothingMode;

                    return true;
                }
            }
        }

        private void DrawSegment(Graphics g, Brush laneBrush, int x0, int y0, int x1, int y1)
        {
            var p0 = new Point(x0, y0);
            var p1 = new Point(x1, y1);

            using (var lanePen = new Pen(laneBrush, _laneLineWidth))
            {
                if (y0 == y1)
                {
                    g.SmoothingMode = SmoothingMode.None;
                    g.DrawLine(lanePen, p0, p1);
                }
                else
                {
                    // Anti-aliasing seems to introduce an offset of two thirds
                    // of a pixel to the right - compensate it.
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    float offset = -0.667F;

                    // Left shifting int is fast equivalent of dividing by two,
                    // thus computing the average of y0 and y1.
                    var yMid = (y0 + y1) >> 1;
                    var c0 = new PointF(offset + x0, yMid);
                    var c1 = new PointF(offset + x1, yMid);
                    var e0 = new PointF(offset + p0.X, p0.Y);
                    var e1 = new PointF(offset + p1.X, p1.Y);
                    g.DrawBezier(lanePen, e0, c0, c1, e1);
                }
            }
        }

        public override void Clear()
        {
            _revisionGraph.Clear();
            _cacheHead = -1;
            _cacheHeadRow = 0;
            ClearDrawCache();
        }

        public override void Refresh(int rowHeight, in VisibleRowRange range)
        {
            // Hide graph column when there it is disabled OR when a filter is active
            // allowing for special case when history of a single file is being displayed
            Column.Visible
                = AppSettings.ShowRevisionGridGraphColumn &&
                  !_grid.ShouldHideGraph(inclBranchFilter: false);

            ClearDrawCache();
            UpdateGraphColumnWidth(range);
        }

        public override void OnColumnWidthChanged(DataGridViewColumnEventArgs e)
        {
            ClearDrawCache();
        }

        public void HighlightBranch(ObjectId id)
        {
        }

        public override void OnVisibleRowsChanged(in VisibleRowRange range)
        {
            // Keep an extra page in the cache
            _cacheCountMax = (range.Count * 2) + 1;
            UpdateGraphColumnWidth(range);
        }

        // TODO when rendering, if we notice a row has too many lanes, trigger updating the column's width

        private void UpdateGraphColumnWidth(in VisibleRowRange range)
        {
            if (!Column.Visible)
            {
                return;
            }

            int laneCount = 0;
            foreach (var index in range)
            {
                var laneRow = _revisionGraph.GetSegmentsForRow(index);
                if (laneRow != null)
                {
                    laneCount = Math.Max(laneRow.GetLaneCount(), laneCount);
                }
            }

            // When 'git log --first-parent' filtration is enabled and when only current
            // branch needed to be rendered (and this filter actually works),
            // it is much more readable to limit max lanes to 1.
            int maxLanes =
                (AppSettings.ShowFirstParent &&
                 AppSettings.ShowCurrentBranchOnly &&
                 AppSettings.BranchFilterEnabled)
                    ? 1
                    : MaxLanes;

            laneCount = Math.Min(laneCount, maxLanes);
            var columnWidth = (_laneWidth * laneCount) + ColumnLeftMargin;
            if (Column.Width != columnWidth && columnWidth > Column.MinimumWidth)
            {
                Column.Width = columnWidth;
            }
        }

        private void ClearDrawCache()
        {
            _cacheHead = 0;
            _cacheCount = 0;
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, out string toolTip)
        {
            toolTip = string.Empty;
            return false;
        }
    }
}