using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class DvcsGraph : DataGridView
    {
        #region Delegates

        public delegate void LoadingEventHandler(bool isLoading);

        #endregion

        #region DataType enum

        [Flags]
        public enum DataType
        {
            Normal = 0,
            Active = 1,
            Special = 2,
            Filtered = 4,
        }

        #endregion

        #region FilterType enum

        public enum FilterType
        {
            None,
            Highlight,
            Hide,
        }

        #endregion

        private const int NODE_DIMENSION = 8;
        private const int LANE_WIDTH = 13;
        private const int LANE_LINE_WIDTH = 2;
        private const int MAX_LANES = 30;
        private readonly AutoResetEvent backgroundEvent = new AutoResetEvent(false);
        private readonly Graph graphData;
        private readonly Dictionary<Junction, int> junctionColors = new Dictionary<Junction, int>();
        private readonly Color nonRelativeColor = Color.LightGray;

        private readonly Color[] possibleColors =
            {
                Color.Red,
                Color.MistyRose,
                Color.Magenta,
                Color.Violet,
                Color.Blue,
                Color.Azure,
                Color.Cyan,
                Color.SpringGreen,
                Color.Green,
                Color.Chartreuse,
                Color.Gold,
                Color.Orange
            };

        private readonly SynchronizationContext syncContext;
        private readonly List<IComparable> toBeSelected = new List<IComparable>();
        private int backgroundScrollTo;
        private Thread backgroundThread;
        private int cacheCount; // Number of elements in the cache.
        private int cacheCountMax; // Number of elements allowed in the cache. Is based on control height.
        private int cacheHead = -1; // The 'slot' that is the head of the circular bitmap
        private int cacheHeadRow; // The node row that is in the head slot
        private FilterType filterMode = FilterType.None;
        private Bitmap graphBitmap;
        private int graphDataCount;
        private Graphics graphWorkArea;
        private int rowHeight; // Height of elements in the cache. Is equal to the control's row height.
        private int visibleBottom;
        private int visibleTop;

        public DvcsGraph()
        {
            syncContext = SynchronizationContext.Current;
            graphData = new Graph();

            backgroundThread = new Thread(backgroundThreadEntry)
                                   {
                                       IsBackground = true,
                                       Priority = ThreadPriority.BelowNormal,
                                       Name = "DvcsGraph.backgroundThread"
                                   };
            backgroundThread.Start();

            InitializeComponent();

            ColumnHeadersDefaultCellStyle.Font = SystemFonts.DefaultFont;
            Font = SystemFonts.DefaultFont;
            DefaultCellStyle.Font = SystemFonts.DefaultFont;
            AlternatingRowsDefaultCellStyle.Font = SystemFonts.DefaultFont;
            RowsDefaultCellStyle.Font = SystemFonts.DefaultFont;
            RowHeadersDefaultCellStyle.Font = SystemFonts.DefaultFont;
            RowTemplate.DefaultCellStyle.Font = SystemFonts.DefaultFont;
            dataGridColumnGraph.DefaultCellStyle.Font = SystemFonts.DefaultFont;

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            CellPainting += dataGrid_CellPainting;
            ColumnWidthChanged += dataGrid_ColumnWidthChanged;
            Scroll += dataGrid_Scroll;
            graphData.Updated += graphData_Updated;

            VirtualMode = true;
            Clear();
        }

        public FilterType FilterMode
        {
            get { return filterMode; }
            set
            {
                // TODO: We only need to rebuild the graph if switching to or from hide
                if (filterMode == value)
                {
                    return;
                }

                syncContext.Send(o =>
                    {
                        lock (backgroundEvent) // Make sure the background thread isn't running
                        {
                            lock (backgroundThread)
                            {
                                backgroundScrollTo = 0;
                                graphDataCount = 0;
                            }
                            lock (graphData)
                            {
                                filterMode = value;
                                graphData.IsFilter = (filterMode & FilterType.Hide) == FilterType.Hide;
                                RebuildGraph();
                            }
                        }
                    }, this);
            }
        }

        public object[] SelectedData
        {
            get
            {
                if (SelectedRows.Count == 0)
                {
                    return null;
                }
                var data = new object[SelectedRows.Count];
                for (int i = 0; i < SelectedRows.Count; i++)
                {
                    data[i] = graphData[i].Node.Data;
                }
                return data;
            }
        }

        public IComparable[] SelectedIds
        {
            get
            {
                if (SelectedRows.Count == 0)
                {
                    return null;
                }
                var data = new IComparable[SelectedRows.Count];
                for (int i = 0; i < SelectedRows.Count; i++)
                {
                    if (graphData[SelectedRows[i].Index] != null)
                        data[i] = graphData[SelectedRows[i].Index].Node.Id;
                }
                return data;
            }
            set
            {
                lock (graphData)
                {
                    ClearSelection();
                    CurrentCell = null;
                    toBeSelected.Clear();
                    if (value == null)
                    {
                        return;
                    }

                    foreach (IComparable rowItem in value)
                    {
                        int row = FindRow(rowItem);
                        if (row >= 0 && Rows.Count > row)
                        {
                            Rows[row].Selected = true;
                            if (CurrentCell == null)
                            {
                                // Set the current cell to the first item. We use cell
                                // 1 because cell 0 could be hidden if they've chosen to
                                // not see the graph
                                CurrentCell = Rows[row].Cells[1];
                            }
                        }
                        else
                        {
                            // Remember this node, and if we see it again, select it.
                            toBeSelected.Add(rowItem);
                        }
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (backgroundThread != null)
            {
                backgroundThread.Abort();
                backgroundThread = null;
            }
            if (disposing)
            {
                if (graphBitmap != null)
                {
                    graphBitmap.Dispose();
                    graphBitmap = null;
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Loading Handler. NOTE: This will often happen on a background thread
        /// so UI operations may not be safe!
        /// </summary>
        public event LoadingEventHandler Loading;

        public void ShowHideRevisionGraph(bool show)
        {
            Columns[0].Visible = show;
        }

        public void Add(IComparable aId, IComparable[] aParentIds, DataType aType, object aData)
        {
            int lastItem = -1;
            lock (graphData)
            {
                lastItem = graphData.Count;
                graphData.Add(aId, aParentIds, aType, aData);
            }

            updateData();
        }

        public void Clear()
        {
            lock (backgroundThread)
            {
                backgroundScrollTo = 0;
            }
            lock (graphData)
            {
                setRowCount(0);
                junctionColors.Clear();
                graphData.Clear();
                graphDataCount = 0;
                RebuildGraph();
            }
            filterMode = FilterType.None;
        }

        public void FilterClear()
        {
            lock (graphData)
            {
                foreach (Node n in graphData.Nodes.Values)
                {
                    n.IsFiltered = false;
                }
                graphData.IsFilter = false;
            }
        }

        public void Filter(IComparable aId)
        {
            lock (graphData)
            {
                graphData.Filter(aId);
            }
        }

        public bool RowIsRelative(int aRow)
        {
            lock (graphData)
            {
                Graph.LaneRow row = graphData[aRow];
                if (row == null)
                {
                    return false;
                }

                if (row.Node.Ancestors.Count > 0)
                    return row.Node.Ancestors[0].IsRelative;

                return true;
            }
        }

        public object GetRowData(int aRow)
        {
            lock (graphData)
            {
                Graph.LaneRow row = graphData[aRow];
                if (row == null)
                {
                    return null;
                }
                return row.Node.Data;
            }
        }

        public IComparable GetRowId(int aRow)
        {
            lock (graphData)
            {
                Graph.LaneRow row = graphData[aRow];
                if (row == null)
                {
                    return null;
                }
                return row.Node.Id;
            }
        }

        public int FindRow(IComparable aId)
        {
            lock (graphData)
            {
                int i;
                for (i = 0; i < graphData.CachedCount; i++)
                {
                    if (graphData[i] != null && graphData[i].Node.Id.CompareTo(aId) == 0)
                    {
                        break;
                    }
                }

                return i == graphData.Count ? -1 : i;
            }
        }

        public bool Prune()
        {
            bool status;
            int count;
            lock (graphData)
            {
                status = graphData.Prune();
                count = graphData.Count;
            }
            setRowCount(count);
            return status;
        }

        private void RebuildGraph()
        {
            // Redraw
            cacheHead = -1;
            cacheHeadRow = 0;
            clearDrawCache();
            updateData();
            Invalidate(true);
        }

        private void setRowCount(int count)
        {
            if (InvokeRequired)
            {
                // DO NOT INVOKE! The RowCount is fixed at other strategic points in time.
                // -Doing this in synch can lock up the application
                // -Doing this asynch causes the scrollbar to flicker and eats performance
                // -At first I was concerned that returning might lead to some cases where 
                //  we have more items in the list than we're showing, but I'm pretty sure 
                //  when we're done processing we'll update with the final count, so the 
                //  problem will only be temporary, and not able to distinguish it from 
                //  just git giving us data slowly.
                //Invoke(new MethodInvoker(delegate { setRowCount(count); }));
                return;
            }

            lock (backgroundThread)
            {
                if (CurrentCell == null)
                {
                    RowCount = count;
                    CurrentCell = null;
                }
                else
                {
                    RowCount = count;
                }
            }
        }

        private void graphData_Updated(object graph)
        {
            // We have to post this since the thread owns a lock on GraphData that we'll
            // need in order to re-draw the graph.
            syncContext.Post(o =>
                {
                    clearDrawCache();
                    Invalidate();
                }, this);
        }

        private void dataGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            lock (graphData)
            {
                Graph.LaneRow row = graphData[e.RowIndex];
                if (row != null && (e.State & DataGridViewElementStates.Visible) != 0)
                {
                    if (e.ColumnIndex == 0)
                    {
                        if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
                            e.Graphics.FillRectangle(
                                new LinearGradientBrush(e.CellBounds, RowTemplate.DefaultCellStyle.SelectionBackColor,
                                                        Color.LightBlue, 90, false), e.CellBounds);
                        else
                            e.Graphics.FillRectangle(new SolidBrush(Color.White), e.CellBounds);

                        Rectangle srcRect = drawGraph(e.RowIndex);
                        if (!srcRect.IsEmpty)
                        {
                            e.Graphics.DrawImage
                                (
                                    graphBitmap,
                                    e.CellBounds,
                                    srcRect,
                                    GraphicsUnit.Pixel
                                );
                        }

                        e.Handled = true;
                    }
                }
            }
        }

        private void dataGrid_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            clearDrawCache();
        }

        private void dataGrid_Scroll(object sender, ScrollEventArgs e)
        {
            updateData();
            updateColumnWidth();
        }

        private void backgroundThreadEntry()
        {
            while (backgroundEvent.WaitOne())
            {
                lock (backgroundEvent)
                {
                    int scrollTo;
                    lock (backgroundThread)
                    {
                        scrollTo = backgroundScrollTo;
                    }

                    int curCount;
                    lock (graphData)
                    {
                        curCount = graphDataCount;
                        graphDataCount = graphData.CachedCount;
                    }

                    while (curCount < scrollTo)
                    {
                        lock (graphData)
                        {
                            // Cache the next item
                            if (!graphData.CacheTo(curCount))
                            {
                                Console.WriteLine("Cached item FAILED {0}", curCount);
                                lock (backgroundThread)
                                {
                                    backgroundScrollTo = curCount;
                                }
                                break;
                            }

                            // Update the row (if needed)
                            if (curCount < visibleBottom || toBeSelected.Count > 0)
                            {
                                syncContext.Post(o => updateRow((int)o), curCount);
                            }

                            if (curCount ==
                                (FirstDisplayedCell == null ? 0 : FirstDisplayedCell.RowIndex + DisplayedRowCount(true)))
                            {
                                syncContext.Post(state1 => updateColumnWidth(), null);
                            }

                            curCount = graphData.CachedCount;
                            graphDataCount = curCount;
                        }
                    }

                    lock (backgroundThread)
                    {
                        int rowCount = RowCount;
                    }
                }
            }
        }

        private void updateData()
        {
            lock (backgroundThread)
            {
                visibleTop = FirstDisplayedCell == null ? 0 : FirstDisplayedCell.RowIndex;
                visibleBottom = rowHeight > 0 ? visibleTop + (Height / rowHeight) : visibleTop;

                //Subtract 2 for safe marge (1 for rounding and 1 for whitspace)....
                if (visibleBottom - 2 > graphData.Count)
                {
                    //Currently we are doing some important work; we are recieving
                    //rows that the user is viewing
                    SetBackgroundThreadToNormalPriority();
                    if (Loading != null)
                    {
                        Loading(true);
                    }
                }
                else
                {
                    //All rows that the user is viewing are loaded. We now can hide the loading
                    //animation that is shown. (the event Loading(bool) triggers this!)
                    //Since the graph is not drawn for the visible graph yet, keep the
                    //priority on Normal. Lower it when the graph is visible.                            
                    if (Loading != null)
                    {
                        Loading(false);
                    }
                }

                if (visibleBottom > graphData.Count)
                {
                    visibleBottom = graphData.Count;
                }

                int targetBottom = visibleBottom + Settings.MaxCommits;
                targetBottom = Math.Min(targetBottom, graphData.Count);
                if (backgroundScrollTo < targetBottom)
                {
                    backgroundScrollTo = targetBottom;
                    backgroundEvent.Set();
                }
            }
        }

        private void SetBackgroundThreadToNormalPriority()
        {
            backgroundThread.Priority = ThreadPriority.Normal;
        }

        private void SetBackgroundThreadToLowPriority()
        {
            backgroundThread.Priority = ThreadPriority.BelowNormal;
        }

        private void updateRow(int row)
        {
            lock (graphData)
            {
                if (RowCount < graphData.Count)
                {
                    setRowCount(graphData.Count);
                }

                // Check to see if the newly added item should be selected
                if (graphData.Count > row)
                {
                    IComparable id = graphData[row].Node.Id;
                    if (toBeSelected.Contains(id))
                    {
                        toBeSelected.Remove(id);
                        Rows[row].Selected = true;
                        if (CurrentCell == null)
                        {
                            // Set the current cell to the first item. We use cell
                            // 1 because cell 0 could be hidden if they've chosen to
                            // not see the graph
                            CurrentCell = Rows[row].Cells[1];
                        }
                    }
                }


                if (visibleBottom < graphDataCount)
                {
                    //All data for the current view is loaded! Lower the thread priority.
                    SetBackgroundThreadToLowPriority();
                }
                else
                {
                    //We need to draw the graph for the visible part of the grid. Higher the priority.
                    SetBackgroundThreadToNormalPriority();
                }

                try
                {
                    InvalidateRow(row);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Ignore. It is possible that RowCount gets changed before
                    // this is processed and the row is larger than RowCount.
                }
            }
        }

        private void updateColumnWidth()
        {
            lock (graphData)
            {
                // Auto scale width on scroll
                if (dataGridColumnGraph.Visible)
                {
                    int laneCount = 2;
                    if (graphData != null)
                    {
                        int width = 1;
                        int start = VerticalScrollBar.Value / rowHeight;
                        int stop = start + DisplayedRowCount(true);
                        for (int i = start; i < stop && graphData[i] != null; i++)
                        {
                            width = Math.Max(graphData[i].Count, width);
                        }

                        laneCount = Math.Min(Math.Max(laneCount, width), MAX_LANES);
                    }
                    dataGridColumnGraph.Width = LANE_WIDTH * laneCount;
                }
            }
        }

        //Color of non-relative branches.

        private List<Color> getJunctionColors(IEnumerable<Junction> aJunction)
        {
            List<Color> colors = aJunction.Select(getJunctionColor).ToList();

            if (colors.Count == 0)
            {
                colors.Add(Color.Black);
            }

            return colors;
        }

        // http://en.wikipedia.org/wiki/File:RBG_color_wheel.svg

        private Color getJunctionColor(Junction aJunction)
        {
            //Draw non-relative branches gray
            if (!aJunction.IsRelative && Settings.RevisionGraphDrawNonRelativesGray)
                return nonRelativeColor;

            if (!Settings.MulticolorBranches)
                return Settings.GraphColor;

            // This is the order to grab the colors in.
            int[] preferedColors = { 4, 8, 6, 10, 2, 5, 7, 3, 9, 1, 11 };

            int colorIndex;
            if (junctionColors.TryGetValue(aJunction, out colorIndex))
            {
                return possibleColors[colorIndex];
            }


            // Get adjacent junctions
            var adjacentJunctions = new List<Junction>();
            var adjacentColors = new List<int>();
            adjacentJunctions.AddRange(aJunction.Child.Ancestors);
            adjacentJunctions.AddRange(aJunction.Child.Descendants);
            adjacentJunctions.AddRange(aJunction.Parent.Ancestors);
            adjacentJunctions.AddRange(aJunction.Parent.Descendants);
            foreach (Junction peer in adjacentJunctions)
            {
                if (junctionColors.TryGetValue(peer, out colorIndex))
                {
                    adjacentColors.Add(colorIndex);
                }
                else
                {
                    colorIndex = -1;
                }
            }

            if (adjacentColors.Count == 0) //This is an end-point. We need to 'pick' a new color
            {
                colorIndex = 0;
            }
            else //This is a parent branch, calculate new color based on parent branch
            {
                int start = adjacentColors[0];
                int i;
                for (i = 0; i < preferedColors.Length; i++)
                {
                    colorIndex = (start + preferedColors[i]) % possibleColors.Length;
                    if (!adjacentColors.Contains(colorIndex))
                    {
                        break;
                    }
                }
                if (i == preferedColors.Length)
                {
                    var r = new Random();
                    colorIndex = r.Next(preferedColors.Length);
                }
            }

            junctionColors[aJunction] = colorIndex;
            return possibleColors[colorIndex];
        }

        public override void Refresh()
        {
            clearDrawCache();
            base.Refresh();
        }

        private void clearDrawCache()
        {
            cacheHead = 0;
            cacheCount = 0;
        }

        private Rectangle drawGraph(int aNeededRow)
        {
            lock (graphData)
            {
                if (aNeededRow < 0 || graphData.Count == 0 || graphData.Count <= aNeededRow)
                {
                    return Rectangle.Empty;
                }

                #region Make sure the graph cache bitmap is setup

                int height = cacheCountMax * rowHeight;
                int width = dataGridColumnGraph.Width;
                if (graphBitmap == null ||
                    //Resize the bitmap when the with or height is changed. The height won't change very often.
                    //The with changes more often, when branches become visible/invisible.
                    //Try to be 'smart' and not resize the bitmap for each little change. Enlarge when needed
                    //but never shrink the bitmap since the huge performance hit is worse than the little extra memory.
                    graphBitmap.Width < width || graphBitmap.Height != height)
                {
                    if (graphBitmap != null)
                    {
                        graphBitmap.Dispose();
                        graphBitmap = null;
                    }
                    if (width > 0 && height > 0)
                    {
                        graphBitmap = new Bitmap(Math.Max(width, LANE_WIDTH * 3), height, PixelFormat.Format32bppPArgb);
                        graphWorkArea = Graphics.FromImage(graphBitmap);
                        graphWorkArea.SmoothingMode = SmoothingMode.AntiAlias;
                        cacheHead = 0;
                        cacheCount = 0;
                    }
                    else
                    {
                        return Rectangle.Empty;
                    }
                }

                #endregion

                // Compute how much the head needs to move to show the requested item. 
                int neededHeadAdjustment = aNeededRow - cacheHead;
                if (neededHeadAdjustment > 0)
                {
                    neededHeadAdjustment -= cacheCountMax - 1;
                    if (neededHeadAdjustment < 0)
                    {
                        neededHeadAdjustment = 0;
                    }
                }
                int newRows = 0;
                if (cacheCount < cacheCountMax)
                {
                    newRows = (aNeededRow - cacheCount) + 1;
                }

                // Adjust the head of the cache
                cacheHead = cacheHead + neededHeadAdjustment;
                cacheHeadRow = (cacheHeadRow + neededHeadAdjustment) % cacheCountMax;
                if (cacheHeadRow < 0)
                {
                    cacheHeadRow = cacheCountMax + cacheHeadRow;
                }

                int start;
                int end;
                if (newRows > 0)
                {
                    start = cacheHead + cacheCount;
                    cacheCount = Math.Min(cacheCount + newRows, cacheCountMax);
                    end = cacheHead + cacheCount;
                }
                else if (neededHeadAdjustment > 0)
                {
                    end = cacheHead + cacheCount;
                    start = Math.Max(cacheHead, end - neededHeadAdjustment);
                }
                else if (neededHeadAdjustment < 0)
                {
                    start = cacheHead;
                    end = start + Math.Min(cacheCountMax, -neededHeadAdjustment);
                }
                else
                {
                    // Item already in the cache
                    return CreateRectangle(aNeededRow, width);
                }

                for (int rowIndex = start; rowIndex < end; rowIndex++)
                {
                    Graph.LaneRow row = graphData[rowIndex];
                    if (row == null)
                    {
                        // This shouldn't be happening...If it does, clear the cache so we
                        // eventually pick it up.
                        Console.WriteLine("Draw lane {0} {1}", rowIndex, "NO DATA");
                        clearDrawCache();
                        return Rectangle.Empty;
                    }

                    Region oldClip = graphWorkArea.Clip;

                    // Get the x,y value of the current item's upper left in the cache
                    int curCacheRow = (cacheHeadRow + rowIndex - cacheHead) % cacheCountMax;
                    int x = 0;
                    int y = curCacheRow * rowHeight;

                    var laneRect = new Rectangle(0, y, Width, rowHeight);
                    if (rowIndex == start || curCacheRow == 0)
                    {
                        // Draw previous row first. Clip top to row. We also need to clear the area
                        // before we draw since nothing else would clear the top 1/2 of the item to draw.
                        graphWorkArea.RenderingOrigin = new Point(x, y - rowHeight);
                        var newClip = new Region(laneRect);
                        graphWorkArea.Clip = newClip;
                        graphWorkArea.Clear(Color.Transparent);
                        drawItem(graphWorkArea, graphData[rowIndex - 1]);
                        graphWorkArea.Clip = oldClip;
                    }

                    bool isLast = (rowIndex == end - 1);
                    if (isLast)
                    {
                        var newClip = new Region(laneRect);
                        graphWorkArea.Clip = newClip;
                    }

                    graphWorkArea.RenderingOrigin = new Point(x, y);
                    bool success = drawItem(graphWorkArea, row);

                    graphWorkArea.Clip = oldClip;

                    if (!success)
                    {
                        clearDrawCache();
                        return Rectangle.Empty;
                    }
                }

                return CreateRectangle(aNeededRow, width);
            } // end lock
        }

        private Rectangle CreateRectangle(int aNeededRow, int width)
        {
            return new Rectangle
                (
                0,
                (cacheHeadRow + aNeededRow - cacheHead) % cacheCountMax * RowTemplate.Height,
                width,
                rowHeight
                );
        }

        // end drawGraph

        private bool drawItem(Graphics wa, Graph.LaneRow row)
        {
            if (row == null || row.NodeLane == -1)
            {
                return false;
            }

            // Clip to the area we're drawing in, but draw 1 pixel past so
            // that the top/bottom of the line segment's anti-aliasing isn't
            // visible in the final rendering.
            int top = wa.RenderingOrigin.Y + rowHeight / 2;
            var laneRect = new Rectangle(0, top, Width, rowHeight);
            Region oldClip = wa.Clip;
            var newClip = new Region(laneRect);
            newClip.Intersect(oldClip);
            wa.Clip = newClip;
            wa.Clear(Color.Transparent);

            for (int lane = 0; lane < row.Count; lane++)
            {
                int mid = wa.RenderingOrigin.X + (int)((lane + 0.5) * LANE_WIDTH);

                for (int item = 0; item < row.LaneInfoCount(lane); item++)
                {
                    Graph.LaneInfo laneInfo = row[lane, item];

                    List<Color> curColors = getJunctionColors(laneInfo.Junctions);

                    // Create the brush for drawing the line
                    Brush brushLineColor;
                    bool drawBorder = Settings.BranchBorders; //hide border for "non-relatives"
                    if (curColors.Count == 1 || !Settings.StripedBranchChange)
                    {
                        if (curColors[0] != nonRelativeColor)
                        {
                            brushLineColor = new SolidBrush(curColors[0]);
                        }
                        else if (curColors.Count > 1 && curColors[1] != nonRelativeColor)
                        {
                            brushLineColor = new SolidBrush(curColors[1]);
                        }
                        else
                        {
                            drawBorder = false;
                            brushLineColor = new SolidBrush(nonRelativeColor);
                        }
                    }
                    else
                    {
                        brushLineColor = new HatchBrush(HatchStyle.DarkDownwardDiagonal, curColors[0], curColors[1]);
                        if (curColors[0] == nonRelativeColor && curColors[1] == nonRelativeColor) drawBorder = false;
                    }

                    for (int i = drawBorder ? 0 : 2; i < 3; i++)
                    {
                        Pen penLine;
                        if (i == 0)
                        {
                            penLine = new Pen(new SolidBrush(Color.White), LANE_LINE_WIDTH + 2);
                        }
                        else if (i == 1)
                        {
                            penLine = new Pen(new SolidBrush(Color.Black), LANE_LINE_WIDTH + 1);
                        }
                        else
                        {
                            penLine = new Pen(brushLineColor, LANE_LINE_WIDTH);
                        }

                        if (laneInfo.ConnectLane == lane)
                        {
                            wa.DrawLine
                                (
                                    penLine,
                                    new Point(mid, top - 1),
                                    new Point(mid, top + rowHeight + 2)
                                );
                        }
                        else
                        {
                            wa.DrawBezier
                                (
                                    penLine,
                                    new Point(mid, top - 1),
                                    new Point(mid, top + rowHeight + 2),
                                    new Point(mid + (laneInfo.ConnectLane - lane) * LANE_WIDTH, top - 1),
                                    new Point(mid + (laneInfo.ConnectLane - lane) * LANE_WIDTH, top + rowHeight + 2)
                                );
                        }
                    }
                }
            }

            // Reset the clip region
            wa.Clip = oldClip;
            {
                // Draw node
                var nodeRect = new Rectangle
                    (
                    wa.RenderingOrigin.X + (LANE_WIDTH - NODE_DIMENSION) / 2 + row.NodeLane * LANE_WIDTH,
                    wa.RenderingOrigin.Y + (rowHeight - NODE_DIMENSION) / 2,
                    NODE_DIMENSION,
                    NODE_DIMENSION
                    );

                Brush nodeBrush;
                bool drawBorder = Settings.BranchBorders;
                List<Color> nodeColors = getJunctionColors(row.Node.Ancestors);
                if (nodeColors.Count == 1)
                {
                    nodeBrush = new SolidBrush(nodeColors[0]);
                    if (nodeColors[0] == nonRelativeColor) drawBorder = false;
                }
                else
                {
                    nodeBrush = new LinearGradientBrush(nodeRect, nodeColors[0], nodeColors[1],
                                                        LinearGradientMode.Horizontal);
                    if (nodeColors[0] == nonRelativeColor && nodeColors[1] == Color.LightGray) drawBorder = false;
                }

                if (filterMode == FilterType.Highlight && row.Node.IsFiltered)
                {
                    Rectangle highlightRect = nodeRect;
                    highlightRect.Inflate(2, 3);
                    wa.FillRectangle(Brushes.Yellow, highlightRect);
                    wa.DrawRectangle(new Pen(Brushes.Black), highlightRect);
                }

                if (row.Node.Data == null)
                {
                    wa.FillEllipse(Brushes.White, nodeRect);
                    wa.DrawEllipse(new Pen(Color.Red, 2), nodeRect);
                }
                else if (row.Node.IsActive)
                {
                    wa.FillRectangle(nodeBrush, nodeRect);
                    nodeRect.Inflate(1, 1);
                    wa.DrawRectangle(new Pen(Color.Black, 3), nodeRect);
                }
                else if (row.Node.IsSpecial)
                {
                    wa.FillRectangle(nodeBrush, nodeRect);
                    if (drawBorder)
                    {
                        wa.DrawRectangle(new Pen(Color.Black, 1), nodeRect);
                    }
                }
                else
                {
                    wa.FillEllipse(nodeBrush, nodeRect);
                    if (drawBorder)
                    {
                        wa.DrawEllipse(new Pen(Color.Black, 1), nodeRect);
                    }
                }
            }
            return true;
        }

        private void dataGrid_Resize(object sender, EventArgs e)
        {
            rowHeight = RowTemplate.Height;
            // Keep an extra page in the cache
            cacheCountMax = Height * 2 / rowHeight + 1;
            clearDrawCache();
        }

        // end of class Junction

        #region Nested type: Graph

        private class Graph
        {
            #region Delegates

            public delegate void GraphUpdatedHandler(object sender);

            public delegate bool Visit(Node n);

            #endregion

            public readonly List<Node> AddedNodes = new List<Node>();

            public readonly List<Junction> Junctions = new List<Junction>();
            public readonly Dictionary<IComparable, Node> Nodes = new Dictionary<IComparable, Node>();
            private readonly Lanes lanes;
            private int filterNodeCount;

            private bool isFilter;
            private int nodeCount;
            private int processedNodes;

            public Graph()
            {
                lanes = new Lanes(this);
            }

            public bool IsFilter
            {
                get { return isFilter; }
                set
                {
                    isFilter = value;
                    lanes.Clear();
                    foreach (Node n in Nodes.Values)
                    {
                        n.InLane = int.MaxValue;
                    }
                    foreach (Junction j in Junctions)
                    {
                        j.CurrentState = Junction.State.Unprocessed;
                    }

                    // We need to signal the DvcsGraph object that it needs to 
                    // redraw everything.
                    Updated(this);
                }
            }

            public int Count
            {
                get
                {
                    if (IsFilter)
                    {
                        return filterNodeCount;
                    }

                    return nodeCount;
                }
            }

            public LaneRow this[int col]
            {
                get { return lanes[col]; }
            }

            public int CachedCount
            {
                get { return lanes.CachedCount; }
            }

            public void Filter(IComparable aId)
            {
                Node node = Nodes[aId];

                if (!node.IsFiltered)
                {
                    filterNodeCount++;
                    node.IsFiltered = true;
                }

                // Clear the filtered lane data. 
                // TODO: We could be smart and only clear items after Node[aId]. The check
                // below isn't valid, since it could be either the filtered or unfiltered
                // lane...
                //if (node.InLane != int.MaxValue)
                //{
                //    filteredLanes.Clear();
                //}
            }

            public event GraphUpdatedHandler Updated;

            public void Add(IComparable aId, IComparable[] aParentIds, DataType aType, object aData)
            {
                // If we haven't seen this node yet, create a new junction.
                Node node = null;
                if (!GetNode(aId, out node) && (aParentIds == null || aParentIds.Length == 0))
                {
                    var newJunction = new Junction(node, node);
                    Junctions.Add(newJunction);
                }
                nodeCount++;
                node.Data = aData;
                node.DataType = aType;
                node.Index = AddedNodes.Count;
                AddedNodes.Add(node);

                foreach (IComparable parentId in aParentIds)
                {
                    Node parent;
                    GetNode(parentId, out parent);
                    if (parent.Index < node.Index)
                    {
                        // TODO: We might be able to recover from this with some work, but
                        // since we build the graph async it might be tough to figure out.
                        //throw new ArgumentException("The nodes must be added such that all children are added before their parents", "aParentIds");
                        continue;
                    }

                    if (node.Descendants.Count == 1 && node.Ancestors.Count <= 1
                        && node.Descendants[0].Parent == node
                        && parent.Ancestors.Count == 0
                        //If this is true, the current revision is in the middle of a branch 
                        //and is about to start a new branch. This will also mean that the last
                        //revisions are non-relative. Make sure a new junction is added and this
                        //is the start of a new branch (and color!)
                        && (aType & DataType.Active) != DataType.Active
                        )
                    {
                        // The node isn't a junction point. Just the parent to the node's
                        // (only) ancestor junction.
                        node.Descendants[0].Add(parent);
                    }
                    else if (node.Ancestors.Count == 1 && node.Ancestors[0].Child != node)
                    {
                        // The node is in the middle of a junction. We need to split it.                   
                        Junction splitNode = node.Ancestors[0].Split(node);
                        Junctions.Add(splitNode);

                        // The node is a junction point. We are a new junction
                        var junction = new Junction(node, parent);
                        Junctions.Add(junction);
                    }
                    else if (parent.Descendants.Count == 1 && parent.Descendants[0].Parent != parent)
                    {
                        // The parent is in the middle of a junction. We need to split it.     
                        Junction splitNode = parent.Descendants[0].Split(parent);
                        Junctions.Add(splitNode);

                        // The node is a junction point. We are a new junction
                        var junction = new Junction(node, parent);
                        Junctions.Add(junction);
                    }
                    else
                    {
                        // The node is a junction point. We are a new junction
                        var junction = new Junction(node, parent);
                        Junctions.Add(junction);
                    }
                }

                bool isRelative = (aType & DataType.Active) == DataType.Active;
                if (!isRelative && node.Descendants.Any(d => d.IsRelative))
                {
                    isRelative = true;
                }

                bool isRebuild = false;
                foreach (Junction d in node.Ancestors)
                {
                    d.IsRelative = isRelative || d.IsRelative;

                    // Uh, oh, we've already processed this lane. We'll have to update some rows.
                    int idx = d.Bunch.IndexOf(node);
                    if (idx < d.Bunch.Count && d.Bunch[idx + 1].InLane != int.MaxValue)
                    {
                        int resetTo = d.Parent.Descendants.Aggregate(d.Parent.InLane, (current, dd) => Math.Min(current, dd.Child.InLane));

                        Console.WriteLine("We have to start over at lane {0} because of {1}", resetTo, node);
                        isRebuild = true;
                        break;
                    }
                }

                if (isRebuild)
                {
                    // TODO: It would be nice if we didn't have to start completely over...but it wouldn't
                    // be easy since we don't keep around all of the necessary lane state for each step.
                    int lastLane = lanes.Count - 1;
                    lanes.Clear();
                    lanes.CacheTo(lastLane);

                    // We need to signal the DvcsGraph object that it needs to redraw everything.
                    if (Updated != null)
                    {
                        Updated(this);
                    }
                }
                else
                {
                    lanes.Update(node);
                }
            }

            public void Clear()
            {
                AddedNodes.Clear();
                Junctions.Clear();
                Nodes.Clear();
                lanes.Clear();
                nodeCount = 0;
                filterNodeCount = 0;
            }

            public void ProcessNode(Node aNode)
            {
                if (isFilter)
                {
                    return;
                }
                for (int i = processedNodes; i < AddedNodes.Count; i++)
                {
                    if (AddedNodes[i] == aNode)
                    {
                        bool isChanged = false;
                        while (i > processedNodes)
                        {
                            // This only happens if we weren't in topo order
                            if (Debugger.IsAttached) Debugger.Break();

                            Node temp = AddedNodes[i];
                            AddedNodes[i] = AddedNodes[i - 1];
                            AddedNodes[i - 1] = temp;
                            i--;
                            isChanged = true;
                        }

                        // Signal that these rows have changed
                        if (isChanged && Updated != null)
                        {
                            Updated(this);
                        }

                        processedNodes++;
                        break;
                    }
                }
            }

            public bool Prune()
            {
                bool isPruned = false;
            // Remove all nodes that don't have a value associated with them.
            start_over:
                foreach (Node n in Nodes.Values)
                {
                    if (n.Data == null)
                    {
                        Nodes.Remove(n.Id);
                        // This guy should have been at the end of some junctions
                        foreach (Junction j in n.Descendants)
                        {
                            j.Bunch.Remove(n);
                            j.Parent.Ancestors.Remove(j);
                        }
                        isPruned = true;
                        goto start_over;
                    }
                }

                return isPruned;
            }

            public IEnumerable<Node> GetHeads()
            {
                var nodes = new List<Node>();
                foreach (Junction j in Junctions)
                {
                    if (j.Child.Descendants.Count == 0 && !nodes.Contains(j.Child))
                    {
                        nodes.Add(j.Child);
                    }
                }
                return nodes;
            }

            public bool CacheTo(int idx)
            {
                return lanes.CacheTo(idx);
            }

            // TopoSorting is an easy way to detect if something has gone wrong with the graph

            public Node[] TopoSortedNodes()
            {
                //http://en.wikipedia.org/wiki/Topological_ordering
                //L ← Empty list that will contain the sorted nodes
                //S ← Set of all nodes with no incoming edges

                //function visit(node n)
                //    if n has not been visited yet then
                //        mark n as visited
                //        for each node m with an edge from n to m do
                //            visit(m)
                //        add n to L

                //for each node n in S do
                //    visit(n)

                var L = new Queue<Node>();
                var S = new Queue<Node>();
                var P = new Queue<Node>();
                foreach (Node h in GetHeads())
                {
                    foreach (Junction j in h.Ancestors)
                    {
                        if (!S.Contains(j.Parent)) S.Enqueue(j.Parent);
                        if (!S.Contains(j.Child)) S.Enqueue(j.Child);
                    }
                }

                Visit visit = null;
                Visit localVisit = visit;
                visit = (Node n) =>
                    {
                        if (!P.Contains(n))
                        {
                            P.Enqueue(n);
                            foreach (Junction e in n.Ancestors)
                            {
                                if (localVisit != null) localVisit(e.Parent);
                            }
                            L.Enqueue(n);
                            return true;
                        }
                        return false;
                    };
                foreach (Node n in S)
                {
                    visit(n);
                }

                // Sanity check
                var J = new Queue<Junction>();
                var X = new Queue<Node>();
                foreach (Node n in L)
                {
                    foreach (Junction e in n.Descendants)
                    {
                        if (X.Contains(e.Child))
                        {
                            Debugger.Break();
                        }
                        if (!J.Contains(e))
                        {
                            J.Enqueue(e);
                        }
                    }
                    X.Enqueue(n);
                }

                if (J.Count != Junctions.Count)
                {
                    foreach (Junction j in Junctions)
                    {
                        if (!J.Contains(j))
                        {
                            if (j.Parent != j.Child)
                            {
                                Console.WriteLine("*** {0} *** {1} {2}", j, Nodes.Count, Junctions.Count);
                            }
                        }
                    }
                }

                return L.ToArray();
            }

            private bool GetNode(IComparable aId, out Node aNode)
            {
                if (!Nodes.TryGetValue(aId, out aNode))
                {
                    aNode = new Node(aId);
                    Nodes.Add(aId, aNode);
                    return false;
                }
                return true;
            }

            #region Nested type: LaneInfo

            public struct LaneInfo
            {
                private int connectLane;
                private List<Junction> junctions;

                public LaneInfo(int aConnectLane, Junction aJunction)
                {
                    connectLane = aConnectLane;
                    junctions = new List<Junction>(1);
                    junctions.Add(aJunction);
                }

                public int ConnectLane
                {
                    get { return connectLane; }
                    set { connectLane = value; }
                }

                public IEnumerable<Junction> Junctions
                {
                    get { return junctions; }
                }

                public LaneInfo Clone()
                {
                    var other = new LaneInfo { connectLane = connectLane, junctions = new List<Junction>(junctions) };
                    return other;
                }

                public void UnionWith(LaneInfo aOther)
                {
                    foreach (Junction other in aOther.junctions)
                    {
                        if (!junctions.Contains(other))
                        {
                            junctions.Add(other);
                        }
                    }
                    junctions.TrimExcess();
                }

                public static implicit operator int(LaneInfo a)
                {
                    return a.ConnectLane;
                }

                public override string ToString()
                {
                    return ConnectLane.ToString();
                }
            }

            #endregion

            #region Nested type: LaneRow

            public interface LaneRow
            {
                // Node information
                int NodeLane { get; }
                Node Node { get; }

                // Lane information
                int Count { get; }
                LaneInfo this[int lane, int item] { get; }
                int LaneInfoCount(int lane);
            }

            #endregion
        }

        #endregion

        #region Nested type: Junction

        private class Junction
        {
            #region State enum

            public enum State
            {
                Unprocessed,
                Processing,
                Processed,
            }

            #endregion

            private static uint DebugIdNext;

            public readonly List<Node> Bunch = new List<Node>();
            private readonly uint DebugId;

            public State CurrentState = State.Unprocessed;
            public bool IsRelative;

            public Junction(Node aNode, Node aParent)
            {
                DebugId = DebugIdNext++;

                Bunch.Add(aNode);
                if (aNode != aParent)
                {
                    aNode.Ancestors.Add(this);
                    aParent.Descendants.Add(this);
                    Bunch.Add(aParent);
                }
            }

            private Junction(Junction aDescendant, Node aNode)
            {
                // Private constructor used by split. This junction will be a
                // ancestor of an existing junction.
                DebugId = DebugIdNext++;
                aNode.Ancestors.Remove(aDescendant);
                Bunch.Add(aNode);
            }

            public Node Child
            {
                get { return Bunch[0]; }
            }

            public Node Parent
            {
                get { return Bunch[Bunch.Count - 1]; }
            }

            public void Add(Node aParent)
            {
                aParent.Descendants.Add(this);
                Parent.Ancestors.Add(this);
                Bunch.Add(aParent);
            }

            public Junction Split(Node aNode)
            {
                // The 'top' (Child->node) of the junction is retained by this.
                // The 'bottom' (node->Parent) of the junction is returned.
                int index = Bunch.IndexOf(aNode);
                if (index == -1)
                {
                    return null;
                }
                var bottom = new Junction(this, aNode);
                // Add 1, since aNode was at the index
                index += 1;
                while (Bunch.Count > index)
                {
                    Node node = Bunch[index];
                    Bunch.RemoveAt(index);
                    node.Ancestors.Remove(this);
                    node.Descendants.Remove(this);
                    bottom.Add(node);
                }

                return bottom;
            }

            public override string ToString()
            {
                return string.Format("{3}: {0}--({2})--{1}", Child, Parent, Bunch.Count, DebugId);
            }
        }

        #endregion

        // end of class Graph

        #region Nested type: Lanes

        private class Lanes : IEnumerable<Graph.LaneRow>
        {
            private readonly ActiveLaneRow currentRow = new ActiveLaneRow();
            private readonly List<LaneJunctionDetail> laneNodes = new List<LaneJunctionDetail>();
            private readonly List<Graph.LaneRow> laneRows;
            private readonly Graph sourceGraph;

            public Lanes(Graph aGraph)
            {
                sourceGraph = aGraph;
                // Rebuild lanes
                laneRows = new List<Graph.LaneRow>();
            }

            public Graph.LaneRow this[int row]
            {
                get
                {
                    if (row < 0)
                    {
                        return null;
                    }

                    if (row < laneRows.Count)
                    {
                        return laneRows[row];
                    }

                    if (row < sourceGraph.AddedNodes.Count)
                    {
                        return new SavedLaneRow(sourceGraph.AddedNodes[row]);
                    }

                    return null;
                }
            }

            public int Count
            {
                get { return sourceGraph.Count; }
            }

            public int CachedCount
            {
                get { return laneRows.Count; }
            }

            #region IEnumerable<LaneRow> Members

            public IEnumerator<Graph.LaneRow> GetEnumerator()
            {
                return new LaneEnumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            public void Clear()
            {
                laneRows.Clear();
                laneNodes.Clear();
                currentRow.Clear();

                foreach (Node aNode in sourceGraph.GetHeads())
                {
                    if (aNode.Descendants.Count == 0)
                    {
                        // This node is a head, create a new lane for it
                        Node h = aNode;
                        if (h.Ancestors.Count == 0)
                        {
                            // This is a single entry with no parents or children.
                            var detail = new LaneJunctionDetail(h);
                            laneNodes.Add(detail);
                        }
                        else
                        {
                            foreach (Junction j in h.Ancestors)
                            {
                                var detail = new LaneJunctionDetail(j);
                                laneNodes.Add(detail);
                            }
                        }
                    }
                }
            }

            public bool CacheTo(int row)
            {
                bool isValid = true;
                while (isValid && row >= CachedCount)
                {
                    isValid = MoveNext();
                }

                return isValid;
            }

            public void Update(Node aNode)
            {
                if (aNode.Descendants.Count == 0)
                {
                    // This node is a head, create a new lane for it
                    Node h = aNode;
                    if (h.Ancestors.Count == 0)
                    {
                        // This is a single entry with no parents or children.
                        var detail = new LaneJunctionDetail(h);
                        laneNodes.Add(detail);
                    }
                    else
                    {
                        foreach (Junction j in h.Ancestors)
                        {
                            var detail = new LaneJunctionDetail(j);
                            laneNodes.Add(detail);
                        }
                    }
                }
            }

            private bool MoveNext()
            {
                // If there are no lanes, there is nothing more to draw
                if (laneNodes.Count == 0 || sourceGraph.Count <= laneRows.Count)
                {
                    return false;
                }

                // Find the new current row's node (newest item in the row)

                #region Find current node & index

                currentRow.Node = null;
                for (int curLane = 0; curLane < laneNodes.Count; curLane++)
                {
                    LaneJunctionDetail lane = laneNodes[curLane];
                    if (lane.Count == 0)
                    {
                        continue;
                    }

                    // NOTE: We could also compare with sourceGraph sourceGraph.AddedNodes[sourceGraph.processedNodes],
                    // since it should always be the same value
                    if (currentRow.Node == null ||
                        currentRow.Node.Data == null ||
                        (lane.Current.Data != null && lane.Current.Index < currentRow.Node.Index))
                    {
                        currentRow.Node = lane.Current;
                        currentRow.NodeLane = curLane;
                    }
                }
                if (currentRow.Node == null)
                {
                    // DEBUG: The check above didn't find anything, but should have
                    if (Debugger.IsAttached) Debugger.Break();
                    //Node[] topo = this.sourceGraph.TopoSortedNodes();
                    return false;
                }

                // If this row doesn't contain data, we're to the end of the valid entries.
                if (currentRow.Node.Data == null)
                {
                    return false;
                }

                sourceGraph.ProcessNode(currentRow.Node);

                #endregion

                // Check for multiple junctions with this node at the top. Remove the 
                // node from that junction as well. This will happen when there is a branch 

                #region Check for branches

                currentRow.Clear(currentRow.NodeLane);
                for (int curLane = 0; curLane < laneNodes.Count; curLane++)
                {
                    LaneJunctionDetail lane = laneNodes[curLane];
                    if (lane.Count == 0)
                    {
                        continue;
                    }

                    if (currentRow.Node != lane.Current)
                    {
                        // We're only interested in columns that have the same node
                        // at the top of the junction as the current row's node
                        continue;
                    }

                    // Remove the item from the lane, since it is being drawn now.
                    // We need to draw the graph line for this lane. If there are no items 
                    // left in the lane we don't draw it.
                    int intoLane = advanceLane(curLane);
                    if (intoLane < curLane)
                    {
                        // AdvanceLane could have removed lanes so we need to start from
                        // the merged into lane (otherwise we could skip a lane, causing
                        // us to try to insert a node into the graph twice)
                        curLane = intoLane;
                    }

                    // Re-process the lane to make sure there are no actions left.
                    curLane--;
                }

                #endregion

                // Look for lanes that cross and reorder to straighten them out if possible,
                // and keep the lanes that merge next to each other.

                #region Straighten out lanes

                // Look for crossing lanes
                for (int lane = 0; lane < currentRow.Count; lane++)
                {
                    for (int item = 0; item < currentRow.LaneInfoCount(lane); item++)
                    {
                        Graph.LaneInfo laneInfo = currentRow[lane, item];
                        if (laneInfo.ConnectLane <= lane)
                        {
                            continue;
                        }
                        // Lane is moving to the right, check to see if it intersects
                        // with any lanes moving to the left.
                        for (int otherLane = lane + 1; otherLane <= laneInfo.ConnectLane; otherLane++)
                        {
                            if (currentRow.LaneInfoCount(otherLane) != 1)
                            {
                                continue;
                            }
                            Graph.LaneInfo otherLaneInfo = currentRow[otherLane, 0];
                            if (otherLaneInfo.ConnectLane < otherLane)
                            {
                                currentRow.Swap(otherLaneInfo.ConnectLane, otherLane);
                                LaneJunctionDetail temp = laneNodes[otherLane];
                                laneNodes[otherLane] = laneNodes[otherLaneInfo.ConnectLane];
                                laneNodes[otherLaneInfo.ConnectLane] = temp;
                            }
                        }
                    }
                }

                //// Keep the merge lanes next to each other
                //int mergeFromCount = currentRow.LaneInfoCount(currentRow.NodeLane);
                //if (mergeFromCount > 1)
                //{
                //    for (int i = 0; i < mergeFromCount; i++)
                //    {
                //        Graph.LaneInfo laneInfo = currentRow[currentRow.NodeLane, i];
                //        // Check to see if the lane is currently next to us
                //        if (laneInfo.ConnectLane - currentRow.NodeLane > mergeFromCount)
                //        {
                //            // Only move the lane if it isn't already being drawn.
                //            if (currentRow.LaneInfoCount(laneInfo.ConnectLane) == 0)
                //            {
                //                // Remove the row laneInfo.ConnectLane and insert
                //                // it at currentRow.NodeLane+1. 
                //                // Then start over searching for others if i != mergeFromCount-1?
                //                int adjacentLane = currentRow.NodeLane + 1;
                //                if (adjacentLane >= laneNodes.Count) Debugger.Break();
                //                currentRow.Expand(adjacentLane);
                //                currentRow.Replace(laneInfo.ConnectLane + 1, adjacentLane);

                //                LaneJunctionDetail temp = laneNodes[laneInfo.ConnectLane];
                //                laneNodes.RemoveAt(laneInfo.ConnectLane);
                //                laneNodes.Insert(adjacentLane, temp);
                //            }
                //        }
                //    }
                //}

                #endregion

                if (currentRow.Node != null)
                {
                    Graph.LaneRow row = currentRow.Advance();

                    // This means there is a node that got put in the graph twice...
                    if (row.Node.InLane != int.MaxValue)
                    {
                        if (Debugger.IsAttached) Debugger.Break();
                    }

                    row.Node.InLane = laneRows.Count;
                    laneRows.Add(row);
                    return true;
                }

                // Return that there are more items left
                return false;
            }

            /// <summary>
            /// Advance the lane to the next element
            /// </summary>
            /// <param name="curLane">Index of the lane to advance</param>
            /// <returns>True if there will still be nodes in this lane</returns>
            private int advanceLane(int curLane)
            {
                LaneJunctionDetail lane = laneNodes[curLane];
                int minLane = curLane;

                // Advance the lane
                lane.Next();

                // See if we can pull up ancestors
                if (lane.Count == 0 && lane.Junction == null)
                {
                    // Handle a single node branch.
                    currentRow.Collapse(curLane);
                    laneNodes.RemoveAt(curLane);
                }
                else if (lane.Count == 0)
                {
                    Node node = lane.Junction.Parent;
                    foreach (Junction parent in node.Ancestors)
                    {
                        if (parent.CurrentState != Junction.State.Unprocessed)
                        {
                            // This item is already in the lane list, no action needed
                            continue;
                        }

                        var addedLane = new LaneJunctionDetail(parent);
                        addedLane.Next();
                        int addedLaneLane = int.MaxValue;

                        // Check to see if this junction already points to one of the
                        // existing lanes. If so, we'll just add the lane line and not
                        // add it to the laneNodes.
                        if (addedLane.Count == 1)
                        {
                            for (int i = 0; i < laneNodes.Count; i++)
                            {
                                if (laneNodes[i].Current == addedLane.Current)
                                {
                                    // We still advance the lane so it gets
                                    // marked as processed.
                                    addedLane.Next();

                                    addedLaneLane = i;
                                    break;
                                }
                            }
                        }

                        // Add to the lane nodes
                        if (addedLaneLane == int.MaxValue)
                        {
                            if (lane.Count == 0)
                            {
                                lane = addedLane;
                                laneNodes[curLane] = lane;
                                addedLaneLane = curLane;
                            }
                            else
                            {
                                addedLaneLane = curLane + 1;
                                laneNodes.Insert(addedLaneLane, addedLane);
                                currentRow.Expand(addedLaneLane);
                            }
                        }

                        currentRow.Add(curLane, new Graph.LaneInfo(addedLaneLane, parent));
                    }

                    // If the lane count after processing is still 0
                    // this is a root node of the graph
                    if (lane.Count == 0)
                    {
                        currentRow.Collapse(curLane);
                        laneNodes.RemoveAt(curLane);
                    }
                }
                else if (lane.Count == 1)
                {
                    // If any other lanes have this node on top, merge them together
                    for (int i = 0; i < laneNodes.Count; i++)
                    {
                        if (i == curLane || curLane >= laneNodes.Count) continue;
                        if (laneNodes[i].Current == laneNodes[curLane].Current)
                        {
                            int left;
                            int right;
                            Junction junction = laneNodes[curLane].Junction;
                            if (i > curLane)
                            {
                                left = curLane;
                                right = i;
                            }
                            else
                            {
                                left = i;
                                right = curLane;
                                curLane = i;
                            }
                            currentRow.Replace(right, left);
                            currentRow.Collapse(right);
                            laneNodes[right].Clear();
                            laneNodes.RemoveAt(right);

                            currentRow.Add(currentRow.NodeLane, new Graph.LaneInfo(left, junction));
                            minLane = Math.Min(minLane, left);
                        }
                    }

                    // If the current lane is still active, add it. It might not be active
                    // if it got merged above.
                    if (!lane.IsClear)
                    {
                        currentRow.Add(currentRow.NodeLane, new Graph.LaneInfo(curLane, lane.Junction));
                    }
                }
                else // lane.Count > 1
                {
                    currentRow.Add(currentRow.NodeLane, new Graph.LaneInfo(curLane, lane.Junction));
                }

                return curLane;
            }

            #region Nested type: ActiveLaneRow

            private class ActiveLaneRow : Graph.LaneRow
            {
                private Edges edges;
                private Node node;
                private int nodeLane = -1;

                public Edge[] EdgeList
                {
                    get { return edges.EdgeList.ToArray(); }
                }

                #region LaneRow Members

                public int NodeLane
                {
                    get { return nodeLane; }
                    set { nodeLane = value; }
                }

                public Node Node
                {
                    get { return node; }
                    set { node = value; }
                }

                public int Count
                {
                    get { return edges.CountCurrent(); }
                }

                public int LaneInfoCount(int lane)
                {
                    return edges.CountCurrent(lane);
                }

                public Graph.LaneInfo this[int col, int row]
                {
                    get { return edges.Current(col, row); }
                }

                #endregion

                public void Add(int lane, Graph.LaneInfo data)
                {
                    edges.Add(lane, data);
                }

                public void Clear()
                {
                    edges = new Edges();
                }

                public void Clear(int lane)
                {
                    edges.Clear(lane);
                }

                public void Collapse(int col)
                {
                    int edgeCount = Math.Max(edges.CountCurrent(), edges.CountNext());
                    for (int i = col; i < edgeCount; i++)
                    {
                        while (edges.CountNext(i) > 0)
                        {
                            int start, end;
                            Graph.LaneInfo info = edges.RemoveNext(i, 0, out start, out end);
                            info.ConnectLane--;
                            edges.Add(start, info);
                        }
                    }
                }

                public void Expand(int col)
                {
                    int edgeCount = Math.Max(edges.CountCurrent(), edges.CountNext());
                    for (int i = edgeCount - 1; i >= col; --i)
                    {
                        while (edges.CountNext(i) > 0)
                        {
                            int start, end;
                            Graph.LaneInfo info = edges.RemoveNext(i, 0, out start, out end);
                            info.ConnectLane++;
                            edges.Add(start, info);
                        }
                    }
                }

                public void Replace(int aOld, int aNew)
                {
                    for (int j = edges.CountNext(aOld) - 1; j >= 0; --j)
                    {
                        int start, end;
                        Graph.LaneInfo info = edges.RemoveNext(aOld, j, out start, out end);
                        info.ConnectLane = aNew;
                        edges.Add(start, info);
                    }
                }

                public void Swap(int aOld, int aNew)
                {
                    // TODO: There is a more efficient way to do this
                    int temp = edges.CountNext();
                    Replace(aOld, temp);
                    Replace(aNew, aOld);
                    Replace(temp, aNew);
                }

                public Graph.LaneRow Advance()
                {
                    var newLaneRow = new SavedLaneRow(this);

                    var newEdges = new Edges();
                    for (int i = 0; i < edges.CountNext(); i++)
                    {
                        int edgeCount = edges.CountNext(i);
                        if (edgeCount > 0)
                        {
                            Graph.LaneInfo info = edges.Next(i, 0).Clone();
                            for (int j = 1; j < edgeCount; j++)
                            {
                                Graph.LaneInfo edgeInfo = edges.Next(i, j);
                                info.UnionWith(edgeInfo);
                            }
                            newEdges.Add(i, info);
                        }
                    }
                    edges = newEdges;

                    return newLaneRow;
                }

                public override string ToString()
                {
                    string s = nodeLane + "/" + Count + ": ";
                    for (int i = 0; i < Count; i++)
                    {
                        if (i == nodeLane)
                            s += "*";
                        s += "{";
                        for (int j = 0; j < LaneInfoCount(i); j++)
                            s += " " + this[i, j];
                        s += " }, ";
                    }
                    s += node;
                    return s;
                }

                #region Nested type: Edges

                private class Edges
                {
                    private readonly List<int> countEnd = new List<int>();
                    private readonly List<int> countStart = new List<int>();
                    private readonly List<Edge> edges = new List<Edge>();

                    private readonly Graph.LaneInfo emptyItem;

                    public List<Edge> EdgeList
                    {
                        get { return edges; }
                    }

                    public Graph.LaneInfo Current(int lane, int item)
                    {
                        int found = 0;
                        foreach (Edge e in edges)
                        {
                            if (e.Start == lane)
                            {
                                if (item == found)
                                {
                                    return e.Data;
                                }
                                found++;
                            }
                        }
                        return emptyItem;
                    }

                    public Graph.LaneInfo Next(int lane, int item)
                    {
                        int found = 0;
                        foreach (Edge e in edges)
                        {
                            if (e.End == lane)
                            {
                                if (item == found)
                                {
                                    return e.Data;
                                }
                                found++;
                            }
                        }
                        return emptyItem;
                    }

                    public Graph.LaneInfo RemoveNext(int lane, int item, out int start, out int end)
                    {
                        int found = 0;
                        for (int i = 0; i < edges.Count; i++)
                        {
                            if (edges[i].End == lane)
                            {
                                if (item == found)
                                {
                                    Graph.LaneInfo data = edges[i].Data;
                                    start = edges[i].Start;
                                    end = edges[i].End;
                                    countStart[start]--;
                                    countEnd[end]--;
                                    edges.RemoveAt(i);
                                    return data;
                                }
                                found++;
                            }
                        }

                        start = -1;
                        end = -1;
                        return emptyItem;
                    }

                    public void Add(int from, Graph.LaneInfo data)
                    {
                        var e = new Edge(data, from);
                        edges.Add(e);

                        while (countStart.Count <= e.Start)
                        {
                            countStart.Add(0);
                        }
                        countStart[e.Start]++;
                        while (countEnd.Count <= e.End)
                        {
                            countEnd.Add(0);
                        }
                        countEnd[e.End]++;
                    }

                    public void Clear(int lane)
                    {
                        for (int i = edges.Count - 1; i >= 0; --i)
                        {
                            int start = edges[i].Start;
                            if (start == lane)
                            {
                                int end = edges[i].End;
                                countStart[start]--;
                                countEnd[end]--;
                                edges.RemoveAt(i);
                            }
                        }
                    }

                    public int CountCurrent()
                    {
                        int count = countStart.Count;
                        while (count > 0 && countStart[count - 1] == 0)
                        {
                            count--;
                            countStart.RemoveAt(count);
                        }

                        return count;
                    }

                    public int CountCurrent(int lane)
                    {
                        int found = 0;
                        foreach (Edge e in edges)
                        {
                            if (e.Start == lane)
                            {
                                found++;
                            }
                        }
                        return found;
                    }

                    public int CountNext()
                    {
                        int count = countEnd.Count;
                        while (count > 0 && countEnd[count - 1] == 0)
                        {
                            count--;
                            countEnd.RemoveAt(count);
                        }

                        return count;
                    }

                    public int CountNext(int lane)
                    {
                        int found = 0;
                        foreach (Edge e in edges)
                        {
                            if (e.End == lane)
                            {
                                found++;
                            }
                        }
                        return found;
                    }

                    public bool IsActive(int lane)
                    {
                        if (lane >= CountNext())
                        {
                            return false;
                        }
                        return (countEnd[lane] > 0);
                    }

                    private void Remove(int start, int end)
                    {
                    }
                }

                #endregion
            }

            #endregion

            #region Nested type: Edge

            private struct Edge
            {
                public readonly int Start;
                public Graph.LaneInfo Data;

                public Edge(Graph.LaneInfo data, int start)
                {
                    Data = data;
                    Start = start;
                }

                public int End
                {
                    get { return Data.ConnectLane; }
                }

                public override string ToString()
                {
                    return string.Format("{0}->{1}: {2}", Start, End, Data);
                }
            }

            #endregion

            #region Nested type: LaneEnumerator

            private class LaneEnumerator : IEnumerator<Graph.LaneRow>
            {
                private readonly Lanes lanes;
                private int index;

                public LaneEnumerator(Lanes aLanes)
                {
                    lanes = aLanes;
                    Reset();
                }

                #region IEnumerator<LaneRow> Members

                public void Reset()
                {
                    index = 0;
                }

                void IDisposable.Dispose()
                {
                }

                object IEnumerator.Current
                {
                    get { return Current; }
                }

                public Graph.LaneRow Current
                {
                    get { return lanes[index]; }
                }

                public bool MoveNext()
                {
                    index++;
                    return index < lanes.laneRows.Count;
                }

                #endregion
            }

            #endregion

            #region Nested type: LaneJunctionDetail

            private class LaneJunctionDetail
            {
                private int index;
                private Junction junction;
                private Node node;

                public LaneJunctionDetail()
                {
                }

                public LaneJunctionDetail(Node n)
                {
                    node = n;
                }

                public LaneJunctionDetail(Junction j)
                {
                    junction = j;
                    junction.CurrentState = Junction.State.Processing;
                    index = 0;
                }

                public int Count
                {
                    get
                    {
                        if (node != null)
                        {
                            return 1 - index;
                        }
                        else if (junction == null)
                        {
                            return 0;
                        }
                        else
                        {
                            return junction.Bunch.Count - index;
                        }
                    }
                }

                public Junction Junction
                {
                    get { return junction; }
                }

                public Node Current
                {
                    get
                    {
                        if (node != null)
                        {
                            return node;
                        }
                        else if (index < junction.Bunch.Count)
                        {
                            return junction.Bunch[index];
                        }
                        else
                        {
                            return null;
                        }
                    }
                }

                public bool IsClear
                {
                    get { return (junction == null && node == null); }
                }

                public void Clear()
                {
                    node = null;
                    junction = null;
                    index = 0;
                }

                public Node Next()
                {
                    Node n;
                    if (node != null)
                    {
                        n = node;
                    }
                    else
                    {
                        n = junction.Bunch[index];
                    }
                    index++;

                    if (junction != null && index >= junction.Bunch.Count)
                    {
                        junction.CurrentState = Junction.State.Processed;
                    }
                    return n;
                }

                public override string ToString()
                {
                    if (junction != null)
                    {
                        string nodeName = "(null)";
                        if (index < junction.Bunch.Count)
                        {
                            nodeName = junction.Bunch[index].ToString();
                        }
                        return index + "/" + junction.Bunch.Count + "~" + nodeName + "~" + junction;
                    }
                    else if (node != null)
                    {
                        return index + "/n~" + node + "~(null)";
                    }
                    else
                    {
                        return "X/X~(null)~(null)";
                    }
                }
            }

            #endregion

            #region Nested type: SavedLaneRow

            private class SavedLaneRow : Graph.LaneRow
            {
                private readonly Edge[] edges;
                private Node node;
                private int nodeLane = -1;

                public SavedLaneRow(Node aNode)
                {
                    node = aNode;
                    nodeLane = -1;
                    edges = null;
                }

                public SavedLaneRow(ActiveLaneRow activeRow)
                {
                    nodeLane = activeRow.NodeLane;
                    node = activeRow.Node;
                    edges = activeRow.EdgeList;
                }

                #region LaneRow Members

                public int NodeLane
                {
                    get { return nodeLane; }
                    set { nodeLane = value; }
                }

                public Node Node
                {
                    get { return node; }
                    set { node = value; }
                }

                public Graph.LaneInfo this[int col, int row]
                {
                    get
                    {
                        int count = 0;
                        foreach (Edge edge in edges)
                        {
                            if (edge.Start == col)
                            {
                                if (count == row)
                                {
                                    return edge.Data;
                                }
                                count++;
                            }
                        }
                        throw new Exception("Bad lane");
                    }
                }

                public int Count
                {
                    get
                    {
                        if (edges == null)
                        {
                            return 0;
                        }

                        int count = -1;
                        foreach (Edge edge in edges)
                        {
                            if (edge.Start > count)
                            {
                                count = edge.Start;
                            }
                        }
                        return count + 1;
                    }
                }

                public int LaneInfoCount(int lane)
                {
                    int count = 0;
                    foreach (Edge edge in edges)
                    {
                        if (edge.Start == lane)
                        {
                            count++;
                        }
                    }
                    return count;
                }

                #endregion

                public override string ToString()
                {
                    string s = nodeLane + "/" + Count + ": ";
                    for (int i = 0; i < Count; i++)
                    {
                        if (i == nodeLane)
                            s += "*";
                        s += "{";
                        for (int j = 0; j < LaneInfoCount(i); j++)
                            s += " " + this[i, j];
                        s += " }, ";
                    }
                    s += node;
                    return s;
                }

                // Node information
            }

            #endregion
        }

        #endregion

        #region Nested type: Node

        private class Node
        {
            public readonly List<Junction> Ancestors = new List<Junction>();
            public readonly List<Junction> Descendants = new List<Junction>();
            public readonly IComparable Id;
            public object Data;
            public DataType DataType;
            public int InLane = int.MaxValue;
            public int Index = int.MaxValue;

            public Node(IComparable aId)
            {
                Id = aId;
            }

            public bool IsActive
            {
                get { return (DataType & DataType.Active) == DataType.Active; }
            }

            public bool IsFiltered
            {
                get { return (DataType & DataType.Filtered) == DataType.Filtered; }
                set
                {
                    if (value)
                    {
                        DataType |= DataType.Filtered;
                    }
                    else
                    {
                        DataType &= ~DataType.Filtered;
                    }
                }
            }

            public bool IsSpecial
            {
                get { return (DataType & DataType.Special) == DataType.Special; }
            }

            public override string ToString()
            {
                if (Data == null)
                {
                    string name = Id.ToString();
                    if (name.Length > 8)
                    {
                        name = name.Substring(0, 4) + ".." + name.Substring(name.Length - 4, 4);
                    }
                    return string.Format("{0} ({1})", name, Index);
                }
                else
                {
                    return Data.ToString();
                }
            }
        }

        #endregion

        // end of class Lanes
    }

    // end of class DvcsGraph
}