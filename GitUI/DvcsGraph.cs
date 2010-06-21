using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace GitUI
{
    public partial class DvcsGraph : DataGridView
    {
        public enum DataType
        {
            Normal,
            Active,
            Special,
        }

        public DvcsGraph()
        {
            syncContext = SynchronizationContext.Current;
            graphData = new Graph();

            backgroundThread = new Thread(new ThreadStart(backgroundThreadEntry));
            backgroundThread.IsBackground = true;
            backgroundThread.Name = "DvcsGraph.backgroundThread";
            backgroundThread.Start();

            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            CellPainting += new DataGridViewCellPaintingEventHandler(dataGrid_CellPainting);
            ColumnWidthChanged += new DataGridViewColumnEventHandler(dataGrid_ColumnWidthChanged);
            Scroll += new ScrollEventHandler(dataGrid_Scroll);
            graphData.Updated += new Graph.GraphUpdatedHandler(graphData_Updated);
        
            VirtualMode = true;
            Clear();

        }

        ~DvcsGraph()
        {
            if (graphBitmap != null)
            {
                graphBitmap.Dispose();
            }

            backgroundThread.Abort();
        }

        public delegate void LoadingHandler(bool isLoading);
        public event LoadingHandler Loading;

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
            // This has to happen on the UI thread
            SendOrPostCallback method = new SendOrPostCallback(delegate(object o)
            {
                lock (graphData)
                {
                    RowCount = 0;
                    junctionColors.Clear();
                    graphData.Clear();
                    RebuildGraph();
                }
                lock (backgroundThread)
                {
                    backgroundScrollTo = 0;
                }
                Console.WriteLine("************************************ Clear ************************************");
            });

            syncContext.Send(method, this);
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
                for (i = 0; i < graphData.Count; i++)
                {
                    if (graphData[i] != null && graphData[i].Node.Id.CompareTo(aId) == 0)
                    {
                        break;
                    }
                }

                return (i == graphData.Count ? -1 : i);
            }
        }
        
        public bool Prune()
        {
            lock (graphData)
            {
                return graphData.Prune();
            }
        }

        public void SetExpectedRowCount(int rowCount)
        {
            // This has to happen on the UI thread
            SendOrPostCallback method = new SendOrPostCallback(delegate(object o)
            {
                lock (graphData)
                {
                    if (rowCount > 0)
                    {
                        RowCount = rowCount;
                    }
                    else
                    {
                        RowCount = graphData.Count;
                    }
                }
            });

            syncContext.Send(method, this);
        }

        public Comparison<object> Sorter
        {
            get
            {
                lock (graphData)
                {
                    return graphData.Sorter;
                }
            }
            set
            {
                lock (graphData)
                {
                    graphData.Sorter = value;
                }
            }
        }

        private readonly SynchronizationContext syncContext;

        private Graph graphData;

        private int cacheHead = -1;     // The 'slot' that is the head of the circular bitmap
        private int cacheHeadRow = 0;   // The node row that is in the head slot
        private int cacheCount;         // Number of elements in the cache.
        private int cacheCountMax;      // Number of elements allowed in the cache. Is based on control height.
        private int rowHeight;          // Height of elements in the cache. Is equal to the control's row height.
        private Bitmap graphBitmap = null;
        private Graphics graphWorkArea = null;
        private Dictionary<Junction, int> junctionColors = new Dictionary<Junction, int>();

        private const int NODE_DIMENSION = 8;
        private const int LANE_WIDTH = 12;
        private const int LANE_LINE_WIDTH = 2;
        private const int MAX_LANES = 30;

        private Thread backgroundThread = null;
        private AutoResetEvent backgroundEvent = new AutoResetEvent(false);
        private int backgroundScrollTo = 0;
        private int graphDataCount = 0;
        private int visibleTop = 0;
        private int visibleBottom = 0;
        private bool isLoading = false;

        private void RebuildGraph()
        {
            // Redraw
            cacheHead = -1;
            cacheHeadRow = 0;
            clearDrawCache();
            Invalidate(true);
        }

        private void graphData_Updated(object graph)
        {
            // We have to post this since the thread owns a lock on GraphData that we'll
            // need in order to re-draw the graph.
            syncContext.Post(new SendOrPostCallback(delegate(object o)
            {
                clearDrawCache();
                Invalidate(true);
            }), this);
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
                        e.PaintBackground(e.CellBounds, true);
                        Rectangle srcRect = drawGraph(e.RowIndex);
                        e.Graphics.DrawImage
                            (
                            graphBitmap,
                            e.CellBounds,
                            srcRect,
                            GraphicsUnit.Pixel
                            );
                        //e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                        //e.Graphics.DrawString(e.RowIndex.ToString(), new Font("System", 8), Brushes.Blue, e.CellBounds);
                        //Console.WriteLine("Paint lane {0} {1}", e.RowIndex, srcRect.Y);
                        e.Handled = true;
                    }
                }
            }
        }

        private void dataGrid_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            clearDrawCache();
            Invalidate(false);
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
                int scrollTo = 0;
                lock (backgroundThread)
                {
                    scrollTo = backgroundScrollTo;
                }

                Console.WriteLine("Background thread kicked {0}", scrollTo);

                int curCount;
                lock (graphData)
                {
                    curCount = graphDataCount;
                    graphDataCount = graphData.CachedCount;
                }

                while (curCount < scrollTo)
                {
                    // DEBUG: Make it slow so it is obvious to test
                    //Thread.Sleep(100);
                    lock (graphData)
                    {
                        if (!graphData.CacheTo(curCount))
                        {
                            Console.WriteLine("Cached item FAILED {0}", curCount);
                            lock (backgroundThread)
                            {
                                backgroundScrollTo = curCount;
                            }
                            break;
                        }

                        if (curCount < visibleBottom)
                        {
                            SendOrPostCallback method = new SendOrPostCallback(delegate(object o)
                                {
                                    updateRow((int)o);
                                });
                            syncContext.Post(method, curCount);
                        }
                        else
                        {
                        }

                        //Console.WriteLine("Cached item {0}", curCount);
                        curCount = graphData.CachedCount;
                        graphDataCount = curCount;
                    }
                }
            }
        }

        private void updateData()
        {
            lock (backgroundThread)
            {
                visibleTop = FirstDisplayedCell == null ? 0 : FirstDisplayedCell.RowIndex;
                visibleBottom = visibleTop + DisplayedRowCount(true);

                int targetBottom = visibleBottom + GitCommands.Settings.MaxCommits;
                targetBottom = Math.Min(targetBottom, graphData.Count);
                if (backgroundScrollTo < targetBottom)
                {
                    backgroundScrollTo = targetBottom;
                    backgroundEvent.Set();
                }

                syncContext.Post(new SendOrPostCallback(delegate(object o)
                    {
                        if (visibleBottom > graphDataCount)
                        {
                            if (!isLoading)
                            {
                                isLoading = true;
                                Loading(true);
                            }
                        }
                        else if (isLoading)
                        {
                            isLoading = false;
                            Loading(false);
                        }
                    }), null);
            }
        }

        private void updateRow(int row)
        {
            lock (graphData)
            {
                //Console.WriteLine("updateRow({0}) ", row);
                if (RowCount < graphData.Count)
                {
                    RowCount = graphData.Count;
                }
                if (row < RowCount)
                {
                    //Console.WriteLine("Redraw item {0}", count);
                    InvalidateRow(row);
                }

                if (visibleBottom > graphDataCount)
                {
                    if (!isLoading)
                    {
                        isLoading = true;
                        Loading(true);
                    }
                }
                else if (isLoading)
                {
                    isLoading = false;
                    Loading(false);
                }
            }
            updateColumnWidth();
        }

        private void updateColumnWidth()
        {
            lock (graphData)
            {
                // Auto scale width on scroll
                if (dataGridColumnGraph.Visible)
                {
                    int laneCount = 2;
                    if (graphData != null && FirstDisplayedCell != null)
                    {
                        int width = 1;
                        int start = FirstDisplayedCell.RowIndex;
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

        private List<Color> getJunctionColors(IEnumerable<Junction> aJunction)
        {
            List<Color> colors = new List<Color>();
            foreach (Junction j in aJunction)
            {
                colors.Add( getJunctionColor(j) );
            }

            if (colors.Count == 0)
            {
                colors.Add(Color.Black);
            }

            return colors;
        }

        private Color getJunctionColor(Junction aJunction)
        {
            // http://en.wikipedia.org/wiki/File:RBG_color_wheel.svg
            Color[] possibleColors = 
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
            // This is the order to grab the colors in.
            int[] preferedColors = { 4, 8, 6, 10, 2, 5, 7, 3, 9, 1, 11 };

            int colorIndex;
            if (junctionColors.TryGetValue(aJunction, out colorIndex))
            {
                return possibleColors[colorIndex];
            }


            // Get adjacent junctions
            List<Junction> adjacnetJunctions = new List<Junction>();
            List<int> adjacentColors = new List<int>();
            adjacnetJunctions.AddRange(aJunction.Child.Ancestors);
            adjacnetJunctions.AddRange(aJunction.Child.Descendants);
            adjacnetJunctions.AddRange(aJunction.Parent.Ancestors);
            adjacnetJunctions.AddRange(aJunction.Parent.Descendants);
            foreach (Junction peer in adjacnetJunctions)
            {
                if (junctionColors.TryGetValue(peer, out colorIndex))
                {
                    adjacentColors.Add(colorIndex);
                }
                else
                {
                    colorIndex = -1;
                }
                //Console.WriteLine("{2}: {0} <--> {1}", aJunction, peer, colorIndex);
            }

            if (adjacentColors.Count == 0)
            {
                colorIndex = 0;
            }
            else
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
                    Random r = new Random();
                    colorIndex = r.Next(preferedColors.Length);
                }
            }

            junctionColors[aJunction] = colorIndex;
            return possibleColors[colorIndex];
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
                if (graphBitmap == null || graphBitmap.Width != width || graphBitmap.Height != height)
                {
                    if (graphBitmap != null)
                    {
                        graphBitmap.Dispose();
                        graphBitmap = null;
                    }
                    if (width > 0 && height > 0)
                    {
                        graphBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
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
                    newRows = ( aNeededRow - cacheCount ) + 1;
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
                    cacheCount = Math.Min(cacheCount+newRows, cacheCountMax);
                    end = cacheHead + cacheCount;
                }
                else if (neededHeadAdjustment > 0)
                {
                    end = cacheHead + cacheCount;
                    start = Math.Max( cacheHead, end - neededHeadAdjustment );
                }
                else if (neededHeadAdjustment < 0)
                {
                    start = cacheHead;
                    end = start + Math.Min( cacheCountMax, -neededHeadAdjustment );
                }
                else
                {
                    // Item already in the cache
                    return new Rectangle
                        (
                        0,
                        (cacheHeadRow + aNeededRow - cacheHead) % cacheCountMax * RowTemplate.Height,
                        width,
                        rowHeight
                        );
                }

                //Console.WriteLine("*** Draw lanes from {0} to {1} ***", start, end);
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

                    Rectangle laneRect = new Rectangle(0, y, Width, rowHeight);
                    if (rowIndex == start || curCacheRow == 0)
                    {
                        // Draw previous row first. Clip top to row. We also need to clear the area
                        // before we draw since nothing else would clear the top 1/2 of the item to draw.
                        graphWorkArea.RenderingOrigin = new Point(x, y - rowHeight);
                        Region newClip = new Region(laneRect);
                        graphWorkArea.Clip = newClip;
                        graphWorkArea.Clear(Color.Transparent);
                        drawItem(graphWorkArea, graphData[rowIndex - 1]);
                        graphWorkArea.Clip = oldClip;
                    }

                    bool isLast = (rowIndex == end - 1);
                    if (isLast)
                    {
                        Region newClip = new Region(laneRect);
                        graphWorkArea.Clip = newClip;
                    }

                    graphWorkArea.RenderingOrigin = new Point(x, y);
                    drawItem(graphWorkArea, row);

                    //Console.WriteLine("Draw lane {0} {1}", rowIndex, y);

                    graphWorkArea.Clip = oldClip;

                    //Rectangle rect = laneRect;
                    //graphWorkArea.FillRectangle(Brushes.LightCoral, rect);
                    //rect.X += 30;
                    //graphWorkArea.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    //graphWorkArea.DrawString(rowIndex.ToString(), new Font("System", 8), Brushes.Red, rect);
                }

                return new Rectangle
                    (
                    0,
                    (cacheHeadRow + aNeededRow - cacheHead) % cacheCountMax * RowTemplate.Height,
                    width,
                    rowHeight
                    );
            } // end lock
        } // end drawGraph

        private void drawItem(Graphics wa, Graph.LaneRow row)
        {
            if (row == null)
            {
                return;
            }

            // Clip to the area we're drawing in, but draw 1 pixel past so
            // that the top/bottom of the line segment's anti-aliasing isn't
            // visible in the final rendering.
            int top = wa.RenderingOrigin.Y + rowHeight / 2;
            Rectangle laneRect = new Rectangle(0, top, Width, rowHeight);
            Region oldClip = wa.Clip;
            Region newClip = new Region(laneRect);
            newClip.Intersect(oldClip);
            wa.Clip = newClip;
            wa.Clear(Color.Transparent);

            for (int lane = 0; lane < row.Count; lane++)
            {
                int mid = wa.RenderingOrigin.X + (int)((lane + 0.5) * LANE_WIDTH);

                for( int item = 0; item < row.LaneInfoCount(lane); item++ )
                {
                    Graph.LaneInfo laneInfo = row[lane,item];

                    List<Color> curColors;
                    curColors = getJunctionColors(laneInfo.Junctions);

                    // Create the brush for drawing the line
                    Brush brushLine;
                    if (curColors.Count == 1 || !GitCommands.Settings.MulticolorBranches)
                    {
                        brushLine = new SolidBrush(curColors[0]);
                    }
                    else
                    {
                        brushLine = new HatchBrush(HatchStyle.DarkDownwardDiagonal, curColors[0], curColors[1]);  
                    }

                    // TODO: Drawing 3 times is probably too expensive, no matter how pretty
                    // it looks. Make it optional?
                    for (int i = 0; i < 3; i++)
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
                            penLine = new Pen(brushLine, LANE_LINE_WIDTH);
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

            // Draw node
            Rectangle nodeRect = new Rectangle
                (
                wa.RenderingOrigin.X + (LANE_WIDTH - NODE_DIMENSION) / 2 + row.NodeLane * LANE_WIDTH,
                wa.RenderingOrigin.Y + (rowHeight - NODE_DIMENSION) / 2,
                NODE_DIMENSION,
                NODE_DIMENSION
                );

            Brush nodeBrush;
            List<Color> nodeColors = getJunctionColors(row.Node.Ancestors);
            if (nodeColors.Count == 1)
            {
                nodeBrush = new SolidBrush(nodeColors[0]);
            }
            else
            {
                nodeBrush = new LinearGradientBrush(nodeRect, nodeColors[0], nodeColors[1], LinearGradientMode.Horizontal);
            }
            
            if (row.Node.Data == null)
            {
                wa.FillEllipse(Brushes.White, nodeRect);
                wa.DrawEllipse(new Pen(Color.Red, 2), nodeRect);
            }
            else if (row.Node.DataType == DataType.Active)
            {
                wa.FillRectangle(nodeBrush, nodeRect);
                nodeRect.Inflate(1, 1);
                wa.DrawRectangle(new Pen(Color.Black, 3), nodeRect);
            }
            else if (row.Node.DataType == DataType.Special)
            {
                wa.FillRectangle(nodeBrush, nodeRect);
                wa.DrawRectangle(new Pen(Color.Black, 1), nodeRect);
            }
            else
            {
                wa.FillEllipse(nodeBrush, nodeRect);
                wa.DrawEllipse(new Pen(Color.Black, 1), nodeRect);
            }
        }

        private void dataGrid_Resize(object sender, EventArgs e)
        {
            rowHeight = RowTemplate.Height;
            // Keep an extra page in the cache
            cacheCountMax = Height * 2 / rowHeight + 1;
            clearDrawCache();
        }

        private class Node
        {
            static uint DebugIdNext = 1;
            uint DebugId;

            public IComparable Id;
            public object Data;
            public int InLane = int.MaxValue;

            public List<Junction> Ancestors = new List<Junction>();
            public List<Junction> Descendants = new List<Junction>();
            public DataType DataType;

            public Node(IComparable aId)
            {
                DebugId = DebugIdNext++;
                Id = aId;
            }

            public override string ToString()
            {
                if (Data == null)
                {
                    string name = Id.ToString();
                    name = name.Substring(0, 4) + ".." + name.Substring(name.Length - 4, 4);
                    return string.Format("{1}: {0}", name, DebugId);
                }
                else
                {
                    return Data.ToString();
                }
            }
        } // end of class Node

        private class Junction
        {
            static uint DebugIdNext = 0;
            uint DebugId;

            public List<Node> Bunch = new List<Node>();

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

            public Junction(Junction aOther)
            {
                // Deep copy
                DebugId = DebugIdNext++;
                Bunch.AddRange(aOther.Bunch);
            }

            private Junction(Junction aDescendant, Node aNode)
            {
                // Private constructor used by split. This junction will be a
                // ancestor of an existing junction.
                DebugId = DebugIdNext++;
                aNode.Ancestors.Remove(aDescendant);
                Bunch.Add(aNode);
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
                Junction bottom = new Junction(this, aNode);
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

            public Node Child
            {
                get { return Bunch[0]; }
            }
            public Node Parent
            {
                get { return Bunch[Bunch.Count - 1]; }
            }

            public override string ToString()
            {
                return string.Format("{3}: {0}--({2})--{1}", Child.ToString(), Parent.ToString(), Bunch.Count, DebugId);
            }
        } // end of class Junction

        private class Graph
        {
            public struct LaneInfo
            {
                public int ConnectLane;
                public HashSet<Node> Parent;
                public HashSet<Node> Child;

                public HashSet<Junction> Junctions
                {
                    get
                    {
                        // This is not terribly efficient, but each loop should only be over 1 or 2 items...
                        HashSet<Junction> laneJunctions = new HashSet<Junction>();
                        foreach (Node parent in Parent)
                        {
                            foreach (Junction j in parent.Descendants)
                            {
                                foreach (Node child in Child)
                                {
                                    if (child.Ancestors.Contains(j))
                                    {
                                        laneJunctions.Add(j);
                                        break;
                                    }
                                }
                            }
                        }
                        return laneJunctions;
                    }
                }

                public LaneInfo(int aConnectLane)
                {
                    ConnectLane = aConnectLane;
                    Parent = new HashSet<Node>();
                    Child = new HashSet<Node>();
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

            public interface LaneRow
            {
                // Node information
                int NodeLane { get; }
                Node Node { get; }

                // Lane information
                int Count { get; }
                int LaneInfoCount(int lane);
                LaneInfo this[int lane, int item] { get; }
            }

            public List<Junction> Junctions = new List<Junction>();
            public Dictionary<IComparable, Node> Nodes = new Dictionary<IComparable, Node>();
            public int NodeCount = 0;
            public Comparison<object> Sorter;

            public delegate void GraphUpdatedHandler(object sender);
            public event GraphUpdatedHandler Updated;

            public Graph()
            {
                //syncContext.Post(_ => LoadRevisions(), null);
                Sorter = delegate(object a, object b)
                {
                    IComparable left = (IComparable)a;
                    IComparable right = (IComparable)b;
                    return left.CompareTo(right);
                };
                lanes = new Lanes(this);
            }

            public void Add(IComparable aId, IComparable[] aParentIds, DataType aType, object aData)
            {
                // If we haven't seen this node yet, create a new junction.
                Node node = null;
                if (!GetNode(aId, out node) && (aParentIds == null || aParentIds.Length == 0))
                {
                    Junctions.Add(new Junction(node, node));
                }
                NodeCount++;
                node.Data = aData;
                node.DataType = aType;
                //Console.WriteLine(node);

                foreach (IComparable parentId in aParentIds)
                {
                    Node parent;
                    GetNode(parentId, out parent);
                    //Console.WriteLine("\t" + parent);

                    if (node.Descendants.Count == 1 && node.Ancestors.Count <= 1
                        && node.Descendants[0].Parent == node
                        && parent.Ancestors.Count == 0
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
                        Junction junction = new Junction(node, parent);
                        Junctions.Add(junction);
                    }
                    else if (parent.Descendants.Count == 1 && parent.Descendants[0].Parent != parent)
                    {
                        // The parent is in the middle of a junction. We need to split it.     
                        Junction splitNode = parent.Descendants[0].Split(parent);
                        Junctions.Add(splitNode);

                        // The node is a junction point. We are a new junction
                        Junction junction = new Junction(node, parent);
                        Junctions.Add(junction);
                    }
                    else
                    {
                        // The node is a junction point. We are a new junction
                        Junction junction = new Junction(node, parent);
                        Junctions.Add(junction);
                    }
                }

                bool isRebuild = false;
                foreach (Junction d in node.Ancestors)
                {
                    // Uh, oh, we've already processed this lane. We'll have to update some rows.
                    int idx = d.Bunch.IndexOf(node);
                    if (idx < d.Bunch.Count && d.Bunch[idx+1].InLane != int.MaxValue)
                    {
                        int resetTo = d.Parent.InLane;
                        foreach (Junction dd in d.Parent.Descendants)
                        {
                            resetTo = Math.Min(resetTo, dd.Child.InLane);
                        }

                        Console.WriteLine("We have to start over at lane {0} because of {1}", resetTo, node);
                        isRebuild = true;
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
                    Updated(this);
                }
                else
                {
                    lanes.Update(node);
                }
            }

            public void Clear()
            {
                Junctions.Clear();
                Nodes.Clear();
                lanes.Clear();
                NodeCount = 0;
            }

            public int Count { get { return NodeCount; } }

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
                List<Node> nodes = new List<Node>();
                foreach (Junction j in Junctions)
                {
                    if (j.Child.Descendants.Count == 0 && !nodes.Contains(j.Child))
                    {
                        nodes.Add(j.Child);
                    }
                }
                return nodes;
            }

            public LaneRow this[int col]
            {
                get
                {
                    return lanes[col];
                }
            }

            public int CachedCount
            {
                get { return lanes.CachedCount; }
            }

            public bool CacheTo(int idx)
            {
                return lanes.CacheTo(idx);
            }

            // TopoSorting is an easy way to detect if something has gone wrong with the graph
            public delegate bool Visit(Node n);
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

                Queue<Node> L = new Queue<Node>();
                Queue<Node> S = new Queue<Node>();
                Queue<Node> P = new Queue<Node>();
                foreach (Node h in GetHeads())
                {
                    foreach (Junction j in h.Ancestors)
                    {
                        if (!S.Contains(j.Parent)) S.Enqueue(j.Parent);
                        if (!S.Contains(j.Child)) S.Enqueue(j.Child);
                    }
                }

                Visit visit = null;
                visit = delegate(Node n)
                {
                    if (!P.Contains(n))
                    {
                        P.Enqueue(n);
                        foreach (Junction e in n.Ancestors)
                        {
                            visit(e.Parent);
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
                Queue<Junction> J = new Queue<Junction>();
                Queue<Node> X = new Queue<Node>();
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

            private Lanes lanes;

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
        } // end of class Graph

        private class Lanes : IEnumerable<Graph.LaneRow>
        {
            public Lanes(Graph aGraph)
            {
                sourceGraph = aGraph;
                // Rebuild lanes
                laneRows = new List<Graph.LaneRow>();
            }

            public void Clear()
            {
                laneRows.Clear();
                laneNodes.Clear();
                junctionNodes.Clear();
                while( currentRow.Count > 0 )
                {
                    currentRow.Clear(currentRow.Count - 1);
                }

                foreach (Node aNode in sourceGraph.GetHeads())
                {
                    if (aNode.Descendants.Count == 0)
                    {
                        // This node is a head, create a new lane for it
                        Node h = aNode;
                        if (h.Ancestors.Count == 0)
                        {
                            // This is a single entry with no parents or children.
                            LaneJunctionDetail detail = new LaneJunctionDetail(h);
                            laneNodes.Add(detail);
                        }
                        else
                        {
                            foreach (Junction j in h.Ancestors)
                            {
                                LaneJunctionDetail detail = new LaneJunctionDetail(j);
                                laneNodes.Add(detail);
                                junctionNodes[j] = detail;
                            }
                        }
                    }
                }
            }

            public Graph.LaneRow this[int col]
            {
                get
                {
                    if (col < 0)
                    {
                        return null;
                    }

                    //while (col >= laneRows.Count)
                    //{
                    //    if (!MoveNext())
                    //    {
                    //        break;
                    //    }
                    //}

                    if (col < laneRows.Count)
                    {
                        return laneRows[col];
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            public IEnumerator<Graph.LaneRow> GetEnumerator()
            {
                return new LaneEnumerator(this);
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count
            {
                get
                {
                    return sourceGraph.Count;
                }
            }
            public int CachedCount
            {
                get
                {
                    return laneRows.Count;
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
                        LaneJunctionDetail detail = new LaneJunctionDetail(h);
                        laneNodes.Add(detail);
                    }
                    else
                    {
                        foreach (Junction j in h.Ancestors)
                        {
                            LaneJunctionDetail detail = new LaneJunctionDetail(j);
                            laneNodes.Add(detail);
                            junctionNodes[j] = detail;
                        }
                    }
                }
            }

            private Graph sourceGraph;
            private List<Graph.LaneRow> laneRows;

            private class LaneJunctionDetail
            {
                private Junction junction = null;
                private int index = 0;
                private Node node = null;

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
                        else
                        {
                            return junction.Bunch[index];
                        }
                    }
                }

                public void Clear()
                {
                    node = null;
                    junction = null;
                    index = 0;
                }

                public bool IsClear
                {
                    get { return (junction == null && node == null); }
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
                    return n;
                }

                public override string ToString()
                {
                    if (junction != null)
                    {
                        return index + "~" + junction.ToString();
                    }
                    else if (node != null)
                    {
                        return index + "~" + node.ToString();
                    }
                    else
                    {
                        return "-1~null";
                    }
                }
            }

            private List<LaneJunctionDetail> laneNodes = new List<LaneJunctionDetail>();
            private Dictionary<Junction, LaneJunctionDetail> junctionNodes = new Dictionary<Junction, LaneJunctionDetail>();
            private ActiveLaneRow currentRow = new ActiveLaneRow();
            
            private class ActiveLaneRow : Graph.LaneRow
            {
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
                // Node information
                private int nodeLane = -1;
                private Node node = null;
                private Edges<Graph.LaneInfo> edges = new Edges<Graph.LaneInfo>();

                private class Edges<T>
                {
                    private struct Edge
                    {
                        public T Data;
                        public int Start;
                        public int End;

                        public Edge(T data, int start, int end)
                        {
                            Data = data;
                            Start = start;
                            End = end;
                        }

                        public override string ToString()
                        {
                            return string.Format("{0}->{1}: {2}", Start, End, Data);
                        }
                    }

                    private List<List<Edge>> currentRow = new List<List<Edge>>();
                    private List<List<Edge>> nextRow = new List<List<Edge>>();
                    private readonly T emptyItem = default(T);

                    public T Current(int lane, int item)
                    {
                    
                        if (currentRow.Count > lane && currentRow[lane].Count > item)
                        {
                            return currentRow[lane][item].Data;
                        }
                        else
                        {
                            return emptyItem;
                        }
                    }

                    public T Next(int lane, int item)
                    {

                        if (nextRow.Count > lane && nextRow[lane].Count > item)
                        {
                            return nextRow[lane][item].Data;
                        }
                        else
                        {
                            return emptyItem;
                        }
                    }

                    public T RemoveNext(int lane, int item, out int start, out int end)
                    {
                        if (nextRow.Count > lane && nextRow[lane].Count > item)
                        {
                            Edge e = nextRow[lane][item];
                            start = e.Start;
                            end = e.End;
                            nextRow[lane].RemoveAt(item);
                            currentRow[start].Remove(e);
                            return e.Data;
                        }
                        else
                        {
                            start = -1; 
                            end = -1;
                            return emptyItem;
                        }
                    }

                    public void Add(int from, int to, T data)
                    {
                        Edge e = new Edge( data, from, to );

                        while (currentRow.Count <= from)
                        {
                            currentRow.Add(new List<Edge>());
                        }
                        currentRow[from].Add(e);

                        while (nextRow.Count <= to)
                        {
                            nextRow.Add(new List<Edge>());
                        }
                        nextRow[to].Add(e);
                    }

                    public void Clear(int lane)
                    {
                        if (currentRow.Count > lane)
                        {
                            foreach (Edge e in currentRow[lane])
                            {
                                nextRow[e.End].Remove(e);
                            }
                            currentRow[lane].Clear();
                        }

                        int lastItem = currentRow.Count - 1;
                        while (lastItem >= 0 && lastItem <= lane && currentRow[lastItem].Count == 0)
                        {
                            currentRow.RemoveAt(lastItem);
                            --lastItem;
                        }
                    }

                    public int CountCurrent()
                    {
                        return currentRow.Count;
                    }

                    public int CountCurrent(int lane)
                    {
                        if (lane < currentRow.Count)
                        {
                            return currentRow[lane].Count;
                        }
                        else
                        {
                            return 0;
                        }
                    }

                    public int CountNext()
                    {
                        return nextRow.Count;
                    }

                    public int CountNext(int lane)
                    {
                        if (lane < nextRow.Count)
                        {
                            return nextRow[lane].Count;
                        }
                        else
                        {
                            return 0;
                        }
                    }

                    public bool IsActive(int lane)
                    {
                        if (nextRow.Count <= lane)
                        {
                            return false;
                        }
                        return (nextRow[lane].Count > 0);
                    }

                }

                public ActiveLaneRow()
                {
                }

                public int Count
                {
                    get 
                    { 
                        return edges.CountCurrent(); 
                    }
                }

                public int LaneInfoCount(int lane)
                {
                    return edges.CountCurrent(lane);
                }

                public Graph.LaneInfo this[int col, int row]
                {
                    get
                    {
                        return edges.Current(col,row);
                    }
                }

                public ActiveLaneRow Clone()
                {
                    ActiveLaneRow newLaneRow = new ActiveLaneRow();
                    newLaneRow.nodeLane = nodeLane;
                    newLaneRow.node = node;
                    newLaneRow.edges = edges;
                    return newLaneRow;
                }

                public bool IsActive(int col)
                {
                    return edges.IsActive(col);
                }

                public void Advance()
                {
                    Edges<Graph.LaneInfo> newEdges = new Edges<Graph.LaneInfo>();
                    for (int i = 0; i < edges.CountNext(); i++)
                    {
                        int edgeCount = edges.CountNext(i);
                        if (edgeCount > 0)
                        {
                            Graph.LaneInfo info = new Graph.LaneInfo(i);
                            for (int j = 0; j < edgeCount; j++)
                            {
                                Graph.LaneInfo edgeInfo = edges.Next(i, j);
                                info.Parent.UnionWith(edgeInfo.Parent);
                                info.Child.UnionWith(edgeInfo.Child);
                            }
                            newEdges.Add(i, i, info);
                        }
                    }
                    edges = newEdges;
                }

                public void Add(int lane, Graph.LaneInfo data)
                {
                    edges.Add(lane, data.ConnectLane, data);
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
                            edges.Add(start, info.ConnectLane, info);
                        }
                    }
                }

                public void Replace(int aOld, int aNew)
                {
                    for (int j = 0; j < edges.CountNext(aOld); j++)
                    {
                        int start, end;
                        Graph.LaneInfo info = edges.RemoveNext(aOld, j, out start, out end);
                        info.ConnectLane = aNew;
                        edges.Add(start, aNew, info);
                    }
                }

                public override string ToString()
                {
                    string s = nodeLane + "/" + edges.CountCurrent() + ": ";
                    for (int i = 0; i < edges.CountCurrent(); i++)
                    {
                        if (i == nodeLane)
                            s += "*";
                        s += "{";
                        for (int j = 0; j < edges.CountCurrent(i); j++)
                            s += " " + edges.Current(i,j);
                        s += " }, ";
                    }
                    s += node;
                    return s;
                }
            }

            private class LaneEnumerator : IEnumerator<Graph.LaneRow>
            {
                private Lanes lanes;
                private int index;

                public LaneEnumerator(Lanes aLanes)
                {
                    lanes = aLanes;
                    Reset();
                }

                public void Reset()
                {
                    index = 0;
                }

                void IDisposable.Dispose()
                {
                }

                object System.Collections.IEnumerator.Current
                {
                    get { return Current; }
                }
                public Graph.LaneRow Current
                {
                    get
                    {
                        return lanes[index];
                    }
                }

                public bool MoveNext()
                {
                    index++;
                    return index < lanes.laneRows.Count;
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
                    if (lane.Count == 0 )
                    {
                        continue;
                    }
                
                    if (currentRow.Node == null ||
                        currentRow.Node.Data == null ||
                        (lane.Current.Data != null && sourceGraph.Sorter.Invoke(lane.Current.Data, currentRow.Node.Data) > 0))
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
                if ( currentRow.Node.Data == null)
                {
                    return false;
                }

                #endregion

                // Make sure that lanes that have been merged are active and the branched 
                // lanes are cleared. Also remove any empty lanes from LaneNodes
                currentRow.Advance();

                // Check to see if there are available lanes that could be used
                // that are better than the current row node lane
                #region Don't skip lanes
                // If the current lane is larger than the number of lanes (there is a gap)
                // swap with the last item.
                if (currentRow.NodeLane > currentRow.Count)
                {
                    LaneJunctionDetail temp = laneNodes[currentRow.NodeLane];
                    laneNodes[currentRow.NodeLane] = laneNodes[currentRow.Count];
                    laneNodes[currentRow.Count] = temp;
                    currentRow.NodeLane = currentRow.Count;
                }
                #endregion

                // Check for multiple junctions with this node at the top. Remove the 
                // node from that junction as well. This will happen when there is a branch 
                #region Check for branches
                currentRow.Clear(currentRow.NodeLane);
                for (int curLane = 0; curLane < laneNodes.Count; curLane++)
                {
                    LaneJunctionDetail lane = laneNodes[curLane];
                    if (lane.Count == 0 || currentRow.Node != lane.Current)
                    {
                        // We're only interested in columns that have the same node
                        // at the top of the junction as the current row's node
                        continue;
                    }

                    // Remove the duplicate
                    lane.Next();

                    // We need to draw the graph line for this lane. If there are no items 
                    // left in the lane we don't draw it.
                    if (lane.Count > 0)
                    {
                        Graph.LaneInfo info = new Graph.LaneInfo(curLane);
                        info.Child.Add(currentRow.Node);
                        info.Parent.Add(laneNodes[curLane].Current);
                        currentRow.Add(currentRow.NodeLane, info );
                    }
                }
                #endregion

                // Advance the LaneNodes and check for merges
                #region Check to see if we can move up ancestor(s) into LaneNodes and merge lanes as needed
                // Check to see if any of the lanes are fully processed and a ancestor
                // can be brought in for processing.
                int origLaneCount = laneNodes.Count;
                for (int curLane = 0; curLane < origLaneCount; curLane++)
                {
                    LaneJunctionDetail lane = laneNodes[curLane];

                    // If 1 item left in the row, see if we can start to draw any of the 
                    // parents. We can only do it if all of the parent's descendants are 
                    // fully drawn.
                    if (lane.Count > 1 || lane.Junction == null)
                    {
                        continue;
                    }

                    // Check the node's ancestors to see if we can bring them into
                    // the nodes
                    Node node = lane.Junction.Parent;
                    bool isFirstMerge = true;
                    foreach (Junction parent in node.Ancestors)
                    {
                        if (junctionNodes.ContainsKey(parent))
                        {
                            // We've already merged this junction in
                            continue;
                        }

                        // If all of this junction's descendants are processed, we can
                        // move the parent into the current lane nodes.
                        bool canMerge = true;
                        foreach (Junction sibling in parent.Child.Descendants)
                        {
                            if (!junctionNodes.ContainsKey(sibling) || junctionNodes[sibling].Count > 1)
                            {
                                // Either the sibling is more than one level away, or it still has nodes 
                                // to be processed before we can merge it into the lane nodes
                                canMerge = false;
                                break;
                            }
                        }
                        if (!canMerge)
                        {
                            continue;
                        }

                        if (isFirstMerge)
                        {
                            isFirstMerge = false;

                            // Since we're merging this lane, the LaneNodes & JunctionNodes 
                            // counts need to go to 0
                            laneNodes[curLane].Clear();
                        }

                        // Find available lane to merge into. 
                        int laneIndex;
                        for (laneIndex = 0; laneIndex < laneNodes.Count; laneIndex++)
                        {
                            if (laneNodes[laneIndex].IsClear)
                            {
                                break;
                            }
                        }
                        if (laneIndex == laneNodes.Count)
                        {
                            laneNodes.Add(new LaneJunctionDetail());
                        }
                        //Console.WriteLine("\tMerge {0} into lane {1}", parent, laneIndex);

                        // Clear first so JunctionNodes count goes to 0
                        laneNodes[laneIndex].Clear();

                        // Check each merged sibling to see if there were any nodes
                        // remaining. If so, we need to point it to the new row. If not
                        // it has been taken care of already.
                        foreach (Junction sibling in parent.Child.Descendants)
                        {
                            // If the sibling still has nodes, we need to update
                            // any references to it to point to the newly merged
                            // location instead
                            if (junctionNodes[sibling].Count == 0)
                            {
                                continue;
                            }
                            int siblingLane = laneNodes.IndexOf(junctionNodes[sibling]);
                            currentRow.Replace(siblingLane, laneIndex);
                        }

                        LaneJunctionDetail nodes = new LaneJunctionDetail(parent);
                        laneNodes[laneIndex] = nodes;
                        junctionNodes[parent] = nodes;

                        // If the top of this item is the current node, add the connection.
                        if (currentRow.Node == nodes.Current)
                        {
                            nodes.Next();
                            Graph.LaneInfo info = new Graph.LaneInfo(laneIndex);
                            info.Child.Add(currentRow.Node);
                            info.Parent.Add(nodes.Current);
                            currentRow.Add(currentRow.NodeLane, info);
                        }
                    }
                }
                #endregion

                // If we have multiple lanes with this same ancestor on the top, we can consolidate them.
                #region Consolidate lanes
                for (int curLane = 0; curLane < laneNodes.Count; curLane++)
                {
                    LaneJunctionDetail lane = laneNodes[curLane];

                    // If there aren't any items in the row, there isn't anything to do
                    if (lane.Count == 0)
                    {
                        continue;
                    }
                    Node node = lane.Current;

                    for (int nextLane = curLane + 1; nextLane < laneNodes.Count; nextLane++)
                    {
                        if (laneNodes[nextLane].Count > 0 && laneNodes[nextLane].Current == node)
                        {
                            if (laneNodes[nextLane].Count == 1)
                            {
                                laneNodes[nextLane].Clear();
                                currentRow.Replace(nextLane, curLane);
                            }
                            else if (laneNodes[curLane].Count == 1)
                            {
                                laneNodes[curLane].Clear();
                                currentRow.Replace(curLane, nextLane);
                            }
                            else
                            {
                                // This happens if we have a head that is a merge
                            }
                        }
                    }
                }
                #endregion

                // Clean up any unused LaneNodes at the end (needed so that MoveNext 
                // check is easy) and remove unused lanes from the end of CurrentRow
                // (needed so we know if we are leaving empty lanes the next time 
                // we call MoveNext)
                #region Trim empty lanes
                for (int curLane = 0; curLane < laneNodes.Count; curLane++)
                {
                    if (!currentRow.IsActive(curLane))
                    {
                        if (laneNodes[curLane].Count == 0)
                        {
                            // Clear the empty lane
                            currentRow.Collapse(curLane);
                            laneNodes.RemoveAt(curLane);
                            curLane--;
                        }
                        else
                        {
                            // Swap the lane with one that is in use
                            for (int j = curLane + 1; j < laneNodes.Count; j++)
                            {
                                if (currentRow.IsActive(j))
                                {
                                    currentRow.Replace(j, curLane);
                                    LaneJunctionDetail temp = laneNodes[curLane];
                                    laneNodes[curLane] = laneNodes[j];
                                    laneNodes[j] = temp;
                                    break;
                                }
                            }
                        }
                    }
                }
                #endregion

                // DEBUG: Spit out the information about the lane
                //Console.WriteLine(laneRows.Count + " ~~ " + currentRow);
                //Console.WriteLine("MoveNext {0} {1}", laneRows.Count, currentRow);

                if (currentRow.Node != null)
                {
                    Graph.LaneRow row = currentRow.Clone();
                    row.Node.InLane = laneRows.Count;
                    laneRows.Add(row);
                    return true;
                }
                else
                {
                    // Return that there are more items left
                    return false;
                }
            }
        } // end of class Lanes

    } // end of class DvcsGraph
}
