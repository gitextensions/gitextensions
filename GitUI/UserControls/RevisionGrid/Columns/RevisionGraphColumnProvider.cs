using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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

        private RevisionGraphDrawStyleEnum _revisionGraphDrawStyleCache;
        private RevisionGraphDrawStyleEnum _revisionGraphDrawStyle;

        public RevisionGraphColumnProvider(RevisionGraph revisionGraph, IGitRevisionSummaryBuilder gitRevisionSummaryBuilder)
            : base("Graph")
        {
            _revisionGraph = revisionGraph;
            _laneInfoProvider = new LaneInfoProvider(new LaneNodeLocator(_revisionGraph), gitRevisionSummaryBuilder);

            // TODO is it worth creating a lighter-weight column type?

            Column = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Resizable = DataGridViewTriState.False,
                MinimumWidth = GraphRenderer.LaneWidth
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
                    Debug.Write(ex);
#if DEBUG
                    BugReportInvoker.LogError(ex);
#endif
                }
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

                _graphCache.Allocate(width, height, GraphRenderer.LaneWidth);

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

                DrawVisibleGraph();
                CreateRectangle();
                return true;

                void CreateRectangle()
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

                void DrawVisibleGraph()
                {
                    // Getting RevisionGraphDrawStyle results in call to AppSettings. This is not very cheap, cache.
                    _revisionGraphDrawStyleCache = RevisionGraphDrawStyle;

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
                    GraphRenderer.DrawItem(_graphCache.GraphBitmapGraphics, index, width, rowHeight, _revisionGraph.GetSegmentsForRow, _revisionGraphDrawStyleCache, _revisionGraph.HeadId);
                }
            }
        }

        public override void Clear()
        {
            _revisionGraph.Clear();
            _graphCache.Clear();
            _graphCache.Reset();
        }

        public override void LoadingCompleted()
        {
            _revisionGraph.LoadingCompleted();
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
                                 .WhereNotNull()
                                 .Select(laneRow => laneRow.GetLaneCount())
                                 .DefaultIfEmpty()
                                 .Max();

            laneCount = Math.Min(laneCount, GraphRenderer.MaxLanes);
            int columnWidth = (GraphRenderer.LaneWidth * laneCount) + ColumnLeftMargin;
            if (columnWidth > minimumWidth)
            {
                return columnWidth;
            }

            return minimumWidth + ColumnLeftMargin;
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

    public sealed class GraphCache
    {
        private Bitmap? _graphBitmap;
        private Graphics? _graphBitmapGraphics;

        public Bitmap? GraphBitmap => _graphBitmap;
        public Graphics? GraphBitmapGraphics => _graphBitmapGraphics;

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
            if (_graphBitmap is not null && _graphBitmap.Width >= width && _graphBitmap.Height == height)
            {
                return;
            }

            if (_graphBitmap is not null)
            {
                _graphBitmap.Dispose();
                _graphBitmap = null;
            }

            if (_graphBitmapGraphics is not null)
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
