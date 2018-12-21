using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
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

        private static readonly int LaneLineWidth = DpiUtil.Scale(2);
        private static readonly int LaneWidth = DpiUtil.Scale(16);
        private static readonly int NodeDimension = DpiUtil.Scale(10);

        private readonly LaneInfoProvider _laneInfoProvider;
        private readonly RevisionGridControl _grid;
        private readonly RevisionGraph _revisionGraph;
        private readonly GraphCache _graphCache = new GraphCache();

        private RevisionGraphDrawStyleEnum _revisionGraphDrawStyleCache;
        private RevisionGraphDrawStyleEnum _revisionGraphDrawStyle;

        public RevisionGraphColumnProvider(RevisionGridControl grid, RevisionGraph revisionGraph)
            : base("Graph")
        {
            _grid = grid;
            _revisionGraph = revisionGraph;
            _laneInfoProvider = new LaneInfoProvider(new LaneNodeLocator(_revisionGraph));

            // TODO is it worth creating a lighter-weight column type?

            Column = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Resizable = DataGridViewTriState.False,
                MinimumWidth = LaneWidth
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

                int height = _graphCache.Capacity * rowHeight;
                int width = Column.Width;

                if (width <= 0 || height <= 0)
                {
                    return false;
                }

                _graphCache.Allocate(width, height, LaneWidth);

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

                var newRows = _graphCache.Count < _graphCache.Capacity
                    ? (rowIndex - _graphCache.Count) + 1
                    : 0;

                // Adjust the head of the cache
                _graphCache.Head = _graphCache.Head + neededHeadAdjustment;
                _graphCache.HeadRow = (_graphCache.HeadRow + neededHeadAdjustment) % _graphCache.Capacity;
                if (_graphCache.HeadRow < 0)
                {
                    _graphCache.HeadRow = _graphCache.Capacity + _graphCache.HeadRow;
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
                        ((_graphCache.HeadRow + rowIndex - _graphCache.Head) % _graphCache.Capacity) * rowHeight,
                        width,
                        rowHeight);

                    graphics.DrawImage(
                        _graphCache.GraphBitmap,
                        cellBounds,
                        cellRect,
                        GraphicsUnit.Pixel);
                }

                bool DrawVisibleGraph()
                {
                    for (var index = start; index < end; index++)
                    {
                        // Get the x,y value of the current item's upper left in the cache
                        var curCacheRow = (_graphCache.HeadRow + index - _graphCache.Head) % _graphCache.Capacity;
                        var x = ColumnLeftMargin;
                        var y = curCacheRow * rowHeight;

                        var laneRect = new Rectangle(0, y, width, rowHeight);
                        var oldClip = _graphCache.GraphBitmapGraphics.Clip;

                        if (index > 0 && (index == start || curCacheRow == 0))
                        {
                            // Draw previous row first. Clip top to row. We also need to clear the area
                            // before we draw since nothing else would clear the top 1/2 of the item to draw.
                            _graphCache.GraphBitmapGraphics.RenderingOrigin = new Point(x, y - rowHeight);
                            _graphCache.GraphBitmapGraphics.Clip = new Region(laneRect);
                            _graphCache.GraphBitmapGraphics.Clear(Color.Transparent);
                            DrawItem(_graphCache.GraphBitmapGraphics, index - 1);
                            _graphCache.GraphBitmapGraphics.Clip = oldClip;
                        }

                        if (index == end - 1)
                        {
                            // Use a custom clip for the last row
                            _graphCache.GraphBitmapGraphics.Clip = new Region(laneRect);
                        }

                        _graphCache.GraphBitmapGraphics.RenderingOrigin = new Point(x, y);

                        var success = DrawItem(_graphCache.GraphBitmapGraphics, index);

                        _graphCache.GraphBitmapGraphics.Clip = oldClip;

                        if (!success)
                        {
                            _graphCache.Reset();
                            return false;
                        }
                    }

                    return true;
                }

                bool DrawItem(Graphics g, int index)
                {
                    // Clip to the area we're drawing in, but draw 1 pixel past so
                    // that the top/bottom of the line segment's anti-aliasing isn't
                    // visible in the final rendering.
                    int top = g.RenderingOrigin.Y; //// + rowHeight;
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

                    if (currentRow != null)
                    {
                        int startY = top - rowHeight + (rowHeight / 2);
                        int centerY = top + (rowHeight / 2);
                        int endY = top + rowHeight + (rowHeight / 2);

                        foreach (RevisionGraphSegment revisionGraphRevision in currentRow.Segments.Reverse().OrderBy(s => s.Child.IsRelative))
                        {
                            int startLane = -10;
                            int centerLane = -10;
                            int endLane = -10;

                            if (revisionGraphRevision.Parent == currentRow.Revision)
                            {
                                // This lane ends here
                                startLane = GetLaneForRow(previousRow, revisionGraphRevision);
                                centerLane = GetLaneForRow(currentRow, revisionGraphRevision);
                            }
                            else
                            {
                                if (revisionGraphRevision.Child == currentRow.Revision)
                                {
                                    // This lane starts here
                                    centerLane = GetLaneForRow(currentRow, revisionGraphRevision);
                                    endLane = GetLaneForRow(nextRow, revisionGraphRevision);
                                }
                                else
                                {
                                    // this lane crosses
                                    startLane = GetLaneForRow(previousRow, revisionGraphRevision);
                                    centerLane = GetLaneForRow(currentRow, revisionGraphRevision);
                                    endLane = GetLaneForRow(nextRow, revisionGraphRevision);
                                }
                            }

                            int startX = g.RenderingOrigin.X + (int)((startLane + 0.5) * LaneWidth);
                            int centerX = g.RenderingOrigin.X + (int)((centerLane + 0.5) * LaneWidth);
                            int endX = g.RenderingOrigin.X + (int)((endLane + 0.5) * LaneWidth);

                            Brush brush;

                            if (!revisionGraphRevision.Parent.Children.IsEmpty
                                && !revisionGraphRevision.Parent.Children.Pop().IsEmpty)
                            {
                                // revisionGraphRevision.Parent.Children has more than one element
                                brush = GetBrushForRevision(revisionGraphRevision.Child, revisionGraphRevision.Child.IsRelative);
                            }
                            else
                            {
                                brush = GetBrushForRevision(revisionGraphRevision.Parent, revisionGraphRevision.Child.IsRelative);
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
                        }

                        if (currentRow.GetCurrentRevisionLane() < MaxLanes)
                        {
                            int centerX = g.RenderingOrigin.X + (int)((currentRow.GetCurrentRevisionLane() + 0.5) * LaneWidth);
                            Rectangle nodeRect = new Rectangle(centerX - (NodeDimension / 2), centerY - (NodeDimension / 2), NodeDimension, NodeDimension);

                            var square = currentRow.Revision.HasRef;
                            var hasOutline = currentRow.Revision.IsCheckedOut;

                            Brush brush = GetBrushForRevision(currentRow.Revision, currentRow.Revision.IsRelative);
                            if (square)
                            {
                                g.SmoothingMode = SmoothingMode.None;
                                g.FillRectangle(brush, nodeRect);
                            }
                            else //// Circle
                            {
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

                    // Reset graphics options
                    g.Clip = oldClip;

                    g.SmoothingMode = oldSmoothingMode;

                    return true;
                }
            }
        }

        private Brush GetBrushForRevision(RevisionGraphRevision revisionGraphRevision, bool isRelative)
        {
            Brush brush;
            if (!isRelative && (_revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.DrawNonRelativesGray || _revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.HighlightSelected))
            {
                brush = RevisionGraphLaneColor.NonRelativeBrush;
            }
            else
            {
                brush = RevisionGraphLaneColor.GetBrushForLane(revisionGraphRevision.LaneColor);
            }

            return brush;
        }

        private static int GetLaneForRow(IRevisionGraphRow row, RevisionGraphSegment revisionGraphRevision)
        {
            if (row != null)
            {
                return row.GetLaneIndexForSegment(revisionGraphRevision);
            }

            return -1;
        }

        private void DrawSegment(Graphics g, Brush laneBrush, int x0, int y0, int x1, int y1)
        {
            var p0 = new Point(x0, y0);
            var p1 = new Point(x1, y1);

            using (var lanePen = new Pen(laneBrush, LaneLineWidth))
            {
                if (x0 == x1)
                {
                    g.SmoothingMode = SmoothingMode.None;
                    g.DrawLine(lanePen, p0, p1);
                }
                else
                {
                    // Anti-aliasing with bezier & PixelOffsetMode.HighQuality
                    // introduces an offset of ~1/8 px - compensate it.
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    const float offset = -1f / 8f;

                    var yMid = (y0 + y1) / 2f;
                    var c0 = new PointF(offset + p0.X, offset + yMid);
                    var c1 = new PointF(offset + p1.X, offset + yMid);
                    var e0 = new PointF(offset + p0.X, offset + p0.Y);
                    var e1 = new PointF(offset + p1.X, offset + p1.Y);
                    g.DrawBezier(lanePen, e0, c0, c1, e1);
                }
            }
        }

        public override void Clear()
        {
            _revisionGraph.Clear();
            _graphCache.Clear();
            _graphCache.Reset();
        }

        public override void Refresh(int rowHeight, in VisibleRowRange range)
        {
            // Hide graph column when there it is disabled OR when a filter is active
            // allowing for special case when history of a single file is being displayed
            Column.Visible
                = AppSettings.ShowRevisionGridGraphColumn &&
                  !_grid.ShouldHideGraph(inclBranchFilter: false);
            if (!Column.Visible)
            {
                return;
            }

            _graphCache.Reset();
            Column.Width = CalculateGraphColumnWidth(range, Column.Width, Column.MinimumWidth);
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
            Column.Width = CalculateGraphColumnWidth(range, Column.Width, Column.MinimumWidth);
        }

        // TODO when rendering, if we notice a row has too many lanes, trigger updating the column's width

        private int CalculateGraphColumnWidth(in VisibleRowRange range, int currentWidth, int minimumWidth)
        {
            int laneCount = range.Select(index => _revisionGraph.GetSegmentsForRow(index))
                                 .Where(laneRow => laneRow != null)
                                 .Select(laneRow => laneRow.GetLaneCount())
                                 .DefaultIfEmpty()
                                 .Max();

            laneCount = Math.Min(laneCount, MaxLanes);
            var columnWidth = (LaneWidth * laneCount) + ColumnLeftMargin;
            if (columnWidth > minimumWidth)
            {
                return columnWidth;
            }

            return minimumWidth + ColumnLeftMargin;
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, out string toolTip)
        {
            if (e.X >= ColumnLeftMargin && LaneWidth >= 0 && e.RowIndex >= 0)
            {
                int lane = (e.X - ColumnLeftMargin) / LaneWidth;
                toolTip = _laneInfoProvider.GetLaneInfo(e.RowIndex, lane);
                return true;
            }

            toolTip = default;
            return false;
        }
    }

    public sealed class GraphCache
    {
        [CanBeNull] private Bitmap _graphBitmap;
        [CanBeNull] private Graphics _graphBitmapGraphics;

        public Bitmap GraphBitmap => _graphBitmap;
        public Graphics GraphBitmapGraphics => _graphBitmapGraphics;

        /// <summary>
        /// The 'slot' that is the head of the circular bitmap.
        /// </summary>
        public int Head { get; set; } = -1;

        /// <summary>
        /// The node row that is in the head slot.
        /// </summary>
        public int HeadRow { get; set; }

        /// <summary>
        /// Number of elements in the cache.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Number of elements allowed in the cache. Is based on control height.
        /// </summary>
        public int Capacity { get; private set; }

        public void AdjustCapacity(int capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            Capacity = capacity;
        }

        public void Allocate(int width, int height, int laneWidth)
        {
            if (_graphBitmap != null && _graphBitmap.Width >= width && _graphBitmap.Height == height)
            {
                return;
            }

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
                Math.Max(width, laneWidth * 3),
                height,
                PixelFormat.Format32bppPArgb);
            _graphBitmapGraphics = Graphics.FromImage(_graphBitmap);
            _graphBitmapGraphics.SmoothingMode = SmoothingMode.AntiAlias;

            // With SmoothingMode != None it is better to use PixelOffsetMode.HighQuality
            // e.g. to avoid shrinking rectangles, ellipses and etc. by 1 px from right bottom
            _graphBitmapGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Head = 0;
            Count = 0;
        }

        public void Clear()
        {
            Head = -1;
            HeadRow = 0;
        }

        public void Reset()
        {
            Head = 0;
            Count = 0;
        }
    }
}