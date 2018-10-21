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

        private readonly JunctionStyler _junctionStyler = new JunctionStyler(new JunctionColorProvider());

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
                    if (end - start > 0)
                    {
                        _revisionGraph.BuildGraph(end);
                    }

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

                    // Getting RevisionGraphDrawStyle results in call to AppSettings. This is not very cheap, cache.
                    _revisionGraphDrawStyleCache = RevisionGraphDrawStyle;

                    var oldSmoothingMode = g.SmoothingMode;

                    var previousRow = _revisionGraph.GetSegmentsForRow(Math.Max(0, index - 1));
                    var currentRow = _revisionGraph.GetSegmentsForRow(index);
                    var nextRow = _revisionGraph.GetSegmentsForRow(index + 1);

                    if (currentRow != null && previousRow != null && nextRow != null)
                    {
                        int lane = 0;
                        foreach (RevisionGraphSegment revisionGraphRevision in currentRow.Segments)
                        {
                            int startLane = previousRow.Segments.IndexOf(x => x.Child == revisionGraphRevision.Parent || x.Parent == revisionGraphRevision.Child || x.Parent == revisionGraphRevision.Parent || x.Child == revisionGraphRevision.Child);
                            if (startLane < 0)
                            {
                                startLane = previousRow.Segments.IndexOf(x => x == revisionGraphRevision);
                            }

                            if (index == 0)
                            {
                                startLane = -10;
                            }

                            Point revisionGraphRevisionPositionStart = new Point(startLane, -1);

                            int centerLane = currentRow.Segments.IndexOf(x => x.Child == revisionGraphRevision.Parent || x.Parent == revisionGraphRevision.Child || x.Parent == revisionGraphRevision.Parent || x.Child == revisionGraphRevision.Child);
                            if (centerLane < 0)
                            {
                                centerLane = currentRow.Segments.IndexOf(x => x == revisionGraphRevision);
                            }

                            Point revisionGraphRevisionPositionCenter = new Point(centerLane, 0);
                            int endlane = nextRow.Segments.IndexOf(x => x.Child == revisionGraphRevision.Parent || x.Parent == revisionGraphRevision.Child || x.Parent == revisionGraphRevision.Parent || x.Child == revisionGraphRevision.Child);
                            if (endlane < 0)
                            {
                                endlane = nextRow.Segments.IndexOf(x => x == revisionGraphRevision);
                            }

                            Point revisionGraphRevisionPositionEnd = new Point(endlane, 1);

                            int startX = g.RenderingOrigin.X + (int)((revisionGraphRevisionPositionStart.X + 0.5) * _laneWidth);
                            int startY = top + (revisionGraphRevisionPositionStart.Y * rowHeight) + (rowHeight / 2);

                            int centerX = g.RenderingOrigin.X + (int)((revisionGraphRevisionPositionCenter.X + 0.5) * _laneWidth);
                            int centerY = top + (revisionGraphRevisionPositionCenter.Y * rowHeight) + (rowHeight / 2);

                            int endX = g.RenderingOrigin.X + (int)((revisionGraphRevisionPositionEnd.X + 0.5) * _laneWidth);
                            int endY = top + (revisionGraphRevisionPositionEnd.Y * rowHeight) + (rowHeight / 2);

                            if (startLane >= 0)
                            {
                                g.DrawLine(Pens.Black, startX, startY, centerX, centerY);
                            }

                            if (endlane >= 0)
                            {
                                g.DrawLine(Pens.Black, centerX, centerY, endX, endY);
                            }

                            if (currentRow.Revision == revisionGraphRevision.Parent || currentRow.Revision == revisionGraphRevision.Child)
                            {
                                g.DrawEllipse(Pens.OrangeRed, centerX - 2, centerY - 2, 4, 4);
                            }

                            lane++;
                        }
                    }

                    // Reset graphics options
                    g.Clip = oldClip;

                    g.SmoothingMode = oldSmoothingMode;

                    return true;
                }
            }
        }

        public override void Clear()
        {
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
            Column.Width = 250;
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