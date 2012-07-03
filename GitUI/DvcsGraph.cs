using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public sealed partial class DvcsGraph : DataGridView
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

        private int NODE_DIMENSION = 8;
        private int LANE_WIDTH = 13;
        private int LANE_LINE_WIDTH = 2;
        private const int MAX_LANES = 30;
        private Brush selectionBrush;

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
        private volatile bool shouldRun = true;
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

        public void SetDimensions(int node_dimension, int lane_width, int lane_line_width, int row_height, Brush selectionBrush)
        {
            RowTemplate.Height = row_height;
            NODE_DIMENSION = node_dimension;
            LANE_WIDTH = lane_width;
            LANE_LINE_WIDTH = lane_line_width;
            this.selectionBrush = selectionBrush;

            dataGrid_Resize(null, null);
        }

        public DvcsGraph()
        {
            syncContext = SynchronizationContext.Current;
            graphData = new Graph();

            backgroundThread = new Thread(BackgroundThreadEntry)
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

        public void ShowAuthor(bool show)
        {
            this.Columns[2].Visible = show;
            this.Columns[3].Visible = show;
        }

        [DefaultValue(FilterType.None)]
        [Category("Behavior")]
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

        [Browsable(false)]
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

        [Browsable(false)]
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
            shouldRun = false;
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

        [Description("Loading Handler. NOTE: This will often happen on a background thread so UI operations may not be safe!")]
        [Category("Behavior")]
        public event LoadingEventHandler Loading;

        public void ShowRevisionGraph()
        {
            Columns[0].Visible = true;
            //updateData();
            backgroundEvent.Set();
        }

        public void HideRevisionGraph()
        {
            Columns[0].Visible = false;
            //updateData();
            backgroundEvent.Set();
        }

        [Browsable(false)]
        public bool RevisionGraphVisible
        {
            get
            {
                return Columns[0].Visible;
            }
        }

        public void Add(IComparable aId, IComparable[] aParentIds, DataType aType, GitRevision aData)
        {
            lock (graphData)
            {
                graphData.Add(aId, aParentIds, aType, aData);
            }

            UpdateData();
        }

        public void Clear()
        {
            lock (backgroundThread)
            {
                backgroundScrollTo = 0;
            }
            lock (graphData)
            {
                SetRowCount(0);
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
                Graph.ILaneRow row = graphData[aRow];
                if (row == null)
                {
                    return false;
                }

                if (row.Node.Ancestors.Count > 0)
                    return row.Node.Ancestors[0].IsRelative;

                return true;
            }
        }

        public GitRevision GetRowData(int aRow)
        {
            lock (graphData)
            {
                Graph.ILaneRow row = graphData[aRow];
                return row == null ? null : row.Node.Data;
            }
        }

        public IComparable GetRowId(int aRow)
        {
            lock (graphData)
            {
                Graph.ILaneRow row = graphData[aRow];
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

        public void Prune()
        {
            int count;
            lock (graphData)
            {
                graphData.Prune();
                count = graphData.Count;
            }
            SetRowCount(count);
        }

        private void RebuildGraph()
        {
            // Redraw
            cacheHead = -1;
            cacheHeadRow = 0;
            ClearDrawCache();
            UpdateData();
            Invalidate(true);
        }

        private void SetRowCount(int count)
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
                    ClearDrawCache();
                    Invalidate();
                }, this);
        }

        private void dataGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            if (Rows[e.RowIndex].Height != RowTemplate.Height)
            {
                Rows[e.RowIndex].Height = RowTemplate.Height;
                dataGrid_Scroll(null, null);
            }

            lock (graphData)
            {
                Graph.ILaneRow row = graphData[e.RowIndex];
                if (row == null || (e.State & DataGridViewElementStates.Visible) == 0)
                    return;
                if (e.ColumnIndex != 0)
                    return;
                var brush = (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected
                                ? selectionBrush : new SolidBrush(Color.White);
                e.Graphics.FillRectangle(brush, e.CellBounds);

                Rectangle srcRect = DrawGraph(e.RowIndex);
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

        private void dataGrid_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            ClearDrawCache();
        }

        private void dataGrid_Scroll(object sender, ScrollEventArgs e)
        {
            UpdateData();
            UpdateColumnWidth();
        }

        private void BackgroundThreadEntry()
        {
            while (shouldRun && backgroundEvent.WaitOne())
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

                    if (RevisionGraphVisible)
                        UpdateGraph(curCount, scrollTo);
                    else
                    {
                        //do nothing... do not cache, the graph is invisible
                        syncContext.Post(o => UpdateRow((int) o), curCount);
                        Thread.Sleep(100);
                    }
                }
            }
        }

        private void UpdateGraph(int curCount, int scrollTo)
        {
            while (curCount < scrollTo)
            {
                lock (graphData)
                {
                    // Cache the next item
                    if (!graphData.CacheTo(curCount))
                    {
                        Console.WriteLine("Cached item FAILED {0}", curCount.ToString());
                        lock (backgroundThread)
                        {
                            backgroundScrollTo = curCount;
                        }
                        break;
                    }

                    // Update the row (if needed)
                    if (curCount < visibleBottom || toBeSelected.Count > 0)
                    {
                        syncContext.Post(o => UpdateRow((int)o), curCount);
                    }

                    int count = 0;
                    if (FirstDisplayedCell != null)
                        count = FirstDisplayedCell.RowIndex + DisplayedRowCount(true);
                    if (curCount == count)
                        syncContext.Post(state1 => UpdateColumnWidth(), null);

                    curCount = graphData.CachedCount;
                    graphDataCount = curCount;
                }
            }
        }

        private void UpdateData()
        {
            lock (backgroundThread)
            {
                visibleTop = FirstDisplayedCell == null ? 0 : FirstDisplayedCell.RowIndex;
                visibleBottom = rowHeight > 0 ? visibleTop + (Height / rowHeight) : visibleTop;

                //Subtract 2 for safe merge (1 for rounding and 1 for whitespace)....
                if (visibleBottom - 2 > graphData.Count)
                {
                    //Currently we are doing some important work; we are recieving
                    //rows that the user is viewing
                    SetBackgroundThreadToNormalPriority();
                    if (Loading != null && graphData.Count > RowCount)// && graphData.Count != RowCount)
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

                int targetBottom = visibleBottom + 2000;
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

        private void UpdateRow(int row)
        {
            lock (graphData)
            {
                if (RowCount < graphData.Count)
                {
                    SetRowCount(graphData.Count);
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

        private void UpdateColumnWidth()
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
                    if (dataGridColumnGraph.Width != LANE_WIDTH * laneCount && LANE_WIDTH * laneCount > dataGridColumnGraph.MinimumWidth)
                        dataGridColumnGraph.Width = LANE_WIDTH * laneCount;
                }
            }
        }

        //Color of non-relative branches.

        private List<Color> GetJunctionColors(IEnumerable<Junction> aJunction)
        {
            List<Color> colors = new List<Color>();
            foreach (Junction j in aJunction)
            {
                colors.Add(GetJunctionColor(j));
            }

            if (colors.Count == 0)
            {
                colors.Add(Color.Black);
            }

            return colors;
        }

        // http://en.wikipedia.org/wiki/File:RBG_color_wheel.svg

        private Color GetJunctionColor(Junction aJunction)
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
            ClearDrawCache();
            base.Refresh();
        }

        private void ClearDrawCache()
        {
            cacheHead = 0;
            cacheCount = 0;
        }

        private Rectangle DrawGraph(int aNeededRow)
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

                if (RevisionGraphVisible)
                {
                    if (!DrawVisibleGraph(start, end))
                        return Rectangle.Empty;
                }

                return CreateRectangle(aNeededRow, width);
            } // end lock
        }

        private bool DrawVisibleGraph(int start, int end)
        {
            for (int rowIndex = start; rowIndex < end; rowIndex++)
            {
                Graph.ILaneRow row = graphData[rowIndex];
                if (row == null)
                {
                    // This shouldn't be happening...If it does, clear the cache so we
                    // eventually pick it up.
                    Console.WriteLine("Draw lane {0} NO DATA", rowIndex.ToString());
                    ClearDrawCache();
                    return false;
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
                    DrawItem(graphWorkArea, graphData[rowIndex - 1]);
                    graphWorkArea.Clip = oldClip;
                }

                bool isLast = (rowIndex == end - 1);
                if (isLast)
                {
                    var newClip = new Region(laneRect);
                    graphWorkArea.Clip = newClip;
                }

                graphWorkArea.RenderingOrigin = new Point(x, y);
                bool success = DrawItem(graphWorkArea, row);

                graphWorkArea.Clip = oldClip;

                if (!success)
                {
                    ClearDrawCache();
                    return false;
                }
            }
            return true;
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

        private bool DrawItem(Graphics wa, Graph.ILaneRow row)
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

            for (int r = 0; r < 2; r++)
                for (int lane = 0; lane < row.Count; lane++)
                {
                    int mid = wa.RenderingOrigin.X + (int)((lane + 0.5) * LANE_WIDTH);

                    for (int item = 0; item < row.LaneInfoCount(lane); item++)
                    {
                        Graph.LaneInfo laneInfo = row[lane, item];

                        //Draw all non-relative items first, them draw
                        //all relative items on top
                        if (laneInfo.Junctions.FirstOrDefault() != null)
                            if (laneInfo.Junctions.First().IsRelative == (r == 0))
                                continue;

                        List<Color> curColors = GetJunctionColors(laneInfo.Junctions);

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
                List<Color> nodeColors = GetJunctionColors(row.Node.Ancestors);
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
            ClearDrawCache();
            dataGrid_Scroll(null, null);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Home)
            {
                if (RowCount != 0)
                {
                    ClearSelection();
                    Rows[0].Selected = true;
                    CurrentCell = Rows[0].Cells[1];
                }
                return;
            }
            else if (e.KeyData == Keys.End)
            {
                if (RowCount != 0)
                {
                    ClearSelection();
                    Rows[RowCount - 1].Selected = true;
                    CurrentCell = Rows[RowCount - 1].Cells[1];
                }
                return;
            }
            base.OnKeyDown(e);
        }

        #region Nested type: Node

        private sealed class Node
        {
            public readonly List<Junction> Ancestors = new List<Junction>();
            public readonly List<Junction> Descendants = new List<Junction>();
            public readonly IComparable Id;
            public GitRevision Data;
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
                return Data.ToString();
            }
        }

        #endregion
    }

    // end of class DvcsGraph
}