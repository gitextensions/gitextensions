using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
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

        public class GraphData
        {
            public void Add(IComparable aId, IComparable[] aParentIds, DataType aType, object aData)
            {
                GraphTree.Add(aId, aParentIds, aType, aData);
            }

            public void Prune()
            {
                GraphTree.Prune();
            }

            public void Clear()
            {
                GraphTree = new Graph();
            }

            public object Populate(int aRow)
            {
                Lanes lanes = new Lanes(GraphTree);
                // TODO: Right now everything is pre-populated.
                return lanes;
            }

            public Comparison<object> Sorter
            {
                get
                {
                    return GraphTree.Sorter;
                }
                set
                {
                    GraphTree.Sorter = value;
                }
            }

            private Graph GraphTree = new Graph();

        };

        public DvcsGraph()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            CellPainting += new DataGridViewCellPaintingEventHandler(dataGrid_CellPainting);
            ColumnWidthChanged += new DataGridViewColumnEventHandler(dataGrid_ColumnWidthChanged);
            Scroll += new ScrollEventHandler(dataGrid_Scroll);
        
            VirtualMode = true;
            RebuildGraph();
        }

        ~DvcsGraph()
        {
            if (GraphBitmap != null)
            {
                GraphBitmap.Dispose();
            }
        }

        public object GetRowData(int aRow)
        {
            Lanes.LaneRow row = GraphLanes[aRow];
            if (row == null)
            {
                return null;
            }
            return row.Node.Data;
        }

        public IComparable GetRowId(int aRow)
        {
            Lanes.LaneRow row = GraphLanes[aRow];
            if (row == null)
            {
                return null;
            }
            return row.Node.Id;
        }

        public int FindRow(IComparable aId)
        {
            int i;
            for (i = 0; i < GraphLanes.Count; i++)
            {
                if (GraphLanes[i].Node.Id.CompareTo(aId) == 0)
                {
                    break;
                }
            }

            return (i == GraphLanes.Count ? -1 : i );
        }

        public void SetData(GraphData aData)
        {
            int populate = CurrentRow == null ? 0 : CurrentRow.Index + Height / RowHeight;
            Lanes newLanes = (Lanes)aData.Populate(populate);

            MethodInvoker method = new MethodInvoker(delegate()
                {
                    JunctionColors.Clear();
                    GraphLanes = newLanes;
                    RowCount = GraphLanes.Count;
                    updateColumnWidth();
                    RebuildGraph();
                });

            if (InvokeRequired)
            {
                Invoke(method);
            }
            else
            {
                method();
            }
        }

        private Lanes GraphLanes;

        private int CacheHead = -1; // The 'slot' that is the head of the circular bitmap
        private int CacheHeadRow = 0; // The node row that is in the head slot
        private int CacheCount; // Number of elements in the cache. Is based on control height.
        private int RowHeight; // Height of elements in the cache. Is based on control's row height.
        private Bitmap GraphBitmap = null;
        private Graphics GraphWorkArea = null;
        private Dictionary<Junction, int> JunctionColors = new Dictionary<Junction, int>();

        private const int NODE_DIMENSION = 8;
        private const int LANE_WIDTH = 12;
        private const int LANE_LINE_WIDTH = 2;

        private void RebuildGraph()
        {
            // Redraw
            CacheHead = -1;
            CacheHeadRow = 0;
            DrawGraph(-1);
            Invalidate(false);
        }

        private void dataGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (GraphLanes == null)
            {
                return;
            }

            Lanes.LaneRow row = GraphLanes[e.RowIndex];
            if (row != null && (e.State & DataGridViewElementStates.Visible) != 0)
            {
                if (e.ColumnIndex == 0)
                {
                    DrawGraph(e.RowIndex);
                    e.PaintBackground(e.CellBounds, true);
                    int y = (CacheHeadRow + e.RowIndex - CacheHead) % CacheCount * RowTemplate.Height;
                    Rectangle srcRect = new Rectangle(0, y, e.CellBounds.Width, e.CellBounds.Height);
                    e.Graphics.DrawImage
                        (
                        GraphBitmap,
                        e.CellBounds,
                        srcRect,
                        GraphicsUnit.Pixel
                        );
                    e.Handled = true;
                }
            }
        }

        private void dataGrid_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            DrawGraph(-1);
            Invalidate(false);
        }

        private void dataGrid_Scroll(object sender, ScrollEventArgs e)
        {
            updateColumnWidth();
        }

        private void updateColumnWidth()
        {
            // Auto scale width on scroll
            if (dataGridColumnGraph.Visible)
            {
                int laneCount = 2;
                if (GraphLanes != null && FirstDisplayedCell != null)
                {
                    int width = 1;
                    int start = FirstDisplayedCell.RowIndex;
                    int stop = start + DisplayedRowCount(true);
                    for (int i = start; i < stop && GraphLanes[i] != null; i++)
                    {
                        width = Math.Max(GraphLanes[i].Count, width);
                    }

                    laneCount = Math.Min(Math.Max(laneCount, width), 30);
                }
                dataGridColumnGraph.Width = LANE_WIDTH * laneCount;
            }
        }

        private List<Color> GetJunctionColors(IEnumerable<Junction> aJunction)
        {
            List<Color> colors = new List<Color>();
            foreach (Junction j in aJunction)
            {
                colors.Add( GetJunctionColor(j) );
            }

            if (colors.Count == 0)
            {
                colors.Add(Color.Black);
            }

            return colors;
        }

        private Color GetJunctionColor(Junction aJunction)
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
            if (JunctionColors.TryGetValue(aJunction, out colorIndex))
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
                if (JunctionColors.TryGetValue(peer, out colorIndex))
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

            JunctionColors[aJunction] = colorIndex;
            return possibleColors[colorIndex];
        }

        private void DrawGraph(int aNeededRow)
        {
            //Console.WriteLine("DrawGraph({0})", aNeededRow);
            bool isForceRedraw = false;
            if (GraphLanes == null)
            {
                return;
            }

            // Force a redraw of everything
            if (CacheHead < 0)
            {
                isForceRedraw = true;
                CacheHead = 0;
            }
            if (aNeededRow < 0)
            {
                isForceRedraw = true;
                aNeededRow = CacheHead;
            }

            int height = CacheCount * RowHeight;
            int width = dataGridColumnGraph.Width;
            if (GraphBitmap == null || GraphBitmap.Width != width || GraphBitmap.Height != height)
            {
                if (GraphBitmap != null)
                {
                    GraphBitmap.Dispose();
                }
                GraphBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                GraphWorkArea = Graphics.FromImage(GraphBitmap);
                GraphWorkArea.SmoothingMode = SmoothingMode.AntiAlias;
                isForceRedraw = true;
            }

            // Compute how much the head needs to move to show the requested item. 
            int neededHeadAdjustment = aNeededRow - (CacheHead);
            if (neededHeadAdjustment > 0)
            {
                neededHeadAdjustment -= CacheCount - 1;
                if (neededHeadAdjustment < 0)
                {
                    neededHeadAdjustment = 0;
                }
            }

            // no need to draw
            if (!isForceRedraw && neededHeadAdjustment == 0)
            {
                return;
            }

            // Compute which items need drawing and adjust the head
            CacheHead = CacheHead + neededHeadAdjustment;
            CacheHeadRow = (CacheHeadRow + neededHeadAdjustment) % CacheCount;
            if (CacheHeadRow < 0)
            {
                CacheHeadRow = CacheCount + CacheHeadRow;
            }
            int start;
            int end;
            if (isForceRedraw || Math.Abs(neededHeadAdjustment) > CacheCount)
            {
                start = CacheHead;
                end = CacheHead + CacheCount;
            }
            else if (neededHeadAdjustment > 0)
            {
                end = CacheHead + CacheCount;
                start = end - neededHeadAdjustment;
            }
            else
            {
                start = CacheHead;
                end = start - neededHeadAdjustment;
            }

            for (int rowIndex = start; rowIndex < end; rowIndex++)
            {
                Lanes.LaneRow row = GraphLanes[rowIndex];
                if (row == null)
                {
                    continue;
                }

                Region oldClip = GraphWorkArea.Clip;

                // Get the x,y value of the current item's upper left in the cache
                int curCacheRow = (CacheHeadRow + rowIndex - CacheHead) % CacheCount;
                int x = 0;
                int y = curCacheRow * RowHeight;

                if (rowIndex == start || curCacheRow == 0)
                {
                    // Draw previous row first. Clip top to row. We also need to clear the area
                    // before we draw since nothing else would clear the top 1/2 of the item to draw.
                    GraphWorkArea.RenderingOrigin = new Point(x, y - RowHeight);
                    Rectangle laneRect = new Rectangle(0, y, Width, RowHeight);
                    Region newClip = new Region(laneRect);
                    GraphWorkArea.Clip = newClip;
                    GraphWorkArea.Clear(Color.Transparent);
                    DrawItem(GraphWorkArea, GraphLanes[rowIndex - 1]);
                    GraphWorkArea.Clip = oldClip;
                }

                bool isLast = (rowIndex == end - 1);
                if (isLast)
                {
                    Rectangle laneRect = new Rectangle(0, y, Width, RowHeight);
                    Region newClip = new Region(laneRect);
                    GraphWorkArea.Clip = newClip;
                }

                GraphWorkArea.RenderingOrigin = new Point(x, y);
                DrawItem(GraphWorkArea, row);

                GraphWorkArea.Clip = oldClip;
            }
        }

        private void DrawItem(Graphics wa, Lanes.LaneRow row)
        {
            if (row == null)
            {
                return;
            }

            // Clip to the area we're drawing in, but draw 1 pixel past so
            // that the top/bottom of the line segment's anti-aliasing isn't
            // visible in the final rendering.
            int top = wa.RenderingOrigin.Y + RowHeight / 2;
            Rectangle laneRect = new Rectangle(0, top, Width, RowHeight);
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
                    Lanes.LaneInfo laneInfo = row[lane,item];

                    List<Color> curColors;
                    curColors = GetJunctionColors(laneInfo.Junctions);

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
                                new Point(mid, top + RowHeight + 2)
                                );
                        }
                        else
                        {
                            wa.DrawBezier
                                (
                                penLine,
                                new Point(mid, top - 1),
                                new Point(mid, top + RowHeight + 2),
                                new Point(mid + (laneInfo.ConnectLane - lane) * LANE_WIDTH, top - 1),
                                new Point(mid + (laneInfo.ConnectLane - lane) * LANE_WIDTH, top + RowHeight + 2)
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
                wa.RenderingOrigin.Y + (RowHeight - NODE_DIMENSION) / 2,
                NODE_DIMENSION,
                NODE_DIMENSION
                );

            Brush nodeBrush;
            List<Color> nodeColors = GetJunctionColors(row.Node.Ancestors);
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

            // DEBUG
            //GraphWorkArea.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            //GraphWorkArea.DrawString(row.Node.Id.ToString(), new Font("Arial", 8), new SolidBrush(Color.FromArgb(r.Next(256), r.Next(256), r.Next(256))), GraphWorkArea.RenderingOrigin.X + 20, GraphWorkArea.RenderingOrigin.Y);

        }

        private void dataGrid_Resize(object sender, EventArgs e)
        {
            RowHeight = RowTemplate.Height;
            CacheCount = Height * 3 / RowHeight;
        }

        private class Node
        {
            static uint DebugIdNext = 1;
            uint DebugId;

            public IComparable Id;
            public object Data;

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
            public List<Junction> Junctions = new List<Junction>();
            public Dictionary<IComparable, Node> Nodes = new Dictionary<IComparable, Node>();
            public int NodeCount = 0;
            
            public Comparison<object> Sorter = null;

            public Graph()
            {
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
            }

            public int Count { get { return NodeCount; } }

            public void Prune()
            {
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
                        goto start_over;
                    }
                }
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

        private class Lanes : IEnumerable<Lanes.LaneRow>
        {
            public Lanes(Graph aGraph)
            {
                sourceGraph = aGraph;
                sorter = sourceGraph.Sorter;
                if (sorter == null)
                {
                    sorter = delegate(object a, object b)
                    {
                        IComparable left = (IComparable)a;
                        IComparable right = (IComparable)b;
                        return left.CompareTo(right);
                    };
                }

                // Add the heads
                laneNodes.Clear();
                foreach (Node h in sourceGraph.GetHeads())
                {
                    if (h.Ancestors.Count == 0)
                    {
                        List<Node> headNode = new List<Node>();
                        headNode.Add(h);                        
                        laneNodes.Add(headNode);
                    }
                    else
                    {
                        foreach (Junction j in h.Ancestors)
                        {
                            List<Node> nodes = new List<Node>(j.Bunch);
                            laneNodes.Add(nodes);
                            junctionNodes[j] = nodes;
                        }
                    }
                }

                // Rebuild lanes
                laneRows = new List<LaneRow>();

                // TODO: If desired, we can pre-load all of the nodes (or at least up to
                // our current row (by SHA1) in the graph). This would be a great idea if we're on 
                // a background thread.
                // Right now, we'll just load everything. Optimize later ;-)
                while (MoveNext()) ;
            }

            public LaneRow this[int col]
            {
                get
                {
                    if (col < 0)
                    {
                        return null;
                    }

                    while (col >= laneRows.Count)
                    {
                        if (!MoveNext())
                        {
                            break;
                        }
                    }

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

            public IEnumerator<LaneRow> GetEnumerator()
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

            private Graph sourceGraph;
            private List<LaneRow> laneRows;

            private List<List<Node>> laneNodes = new List<List<Node>>();
            private Dictionary<Junction, List<Node>> junctionNodes = new Dictionary<Junction, List<Node>>();
            private ActiveLaneRow currentRow = new ActiveLaneRow();
            private Comparison<object> sorter;

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
                int Count{ get; }
                int LaneInfoCount(int lane);
                LaneInfo this[int lane, int item] { get; }
            }

            private class ActiveLaneRow : LaneRow
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
                private Edges<LaneInfo> edges = new Edges<LaneInfo>();

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

                public LaneInfo this[int col, int row]
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
                    Edges<LaneInfo> newEdges = new Edges<LaneInfo>();
                    for (int i = 0; i < edges.CountNext(); i++)
                    {
                        int edgeCount = edges.CountNext(i);
                        if (edgeCount > 0)
                        {
                            LaneInfo info = new LaneInfo(i);
                            for (int j = 0; j < edgeCount; j++)
                            {
                                LaneInfo edgeInfo = edges.Next(i, j);
                                info.Parent.UnionWith(edgeInfo.Parent);
                                info.Child.UnionWith(edgeInfo.Child);
                            }
                            newEdges.Add(i, i, info);
                        }
                    }
                    edges = newEdges;
                }

                public void Add(int lane, LaneInfo data)
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
                            LaneInfo info = edges.RemoveNext(i, 0, out start, out end);
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
                        LaneInfo info = edges.RemoveNext(aOld, j, out start, out end);
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

            private class LaneEnumerator : IEnumerator<LaneRow>
            {
                private Lanes Lanes;
                private int Index;

                public LaneEnumerator(Lanes aLanes)
                {
                    Lanes = aLanes;
                    Reset();
                }

                public void Reset()
                {
                    Index = 0;
                }

                void IDisposable.Dispose()
                {
                }

                object System.Collections.IEnumerator.Current
                {
                    get { return Current; }
                }
                public LaneRow Current
                {
                    get
                    {
                        return Lanes[Index];
                    }
                }

                public bool MoveNext()
                {
                    Index++;
                    return Index < Lanes.laneRows.Count;
                }

            }

            private bool MoveNext()
            {
                // If there are no lanes, there is nothing more to draw
                if (laneNodes.Count == 0)
                {
                    return false;
                }

                // Make sure that lanes that have been merged are active and the branched 
                // lanes are cleared. Also remove any empty lanes from LaneNodes
                currentRow.Advance();

                // Find the new current row's node (newest item in the row)
                #region Find current node & index
                currentRow.Node = null;
                for (int curLane = 0; curLane < laneNodes.Count; curLane++)
                {
                    List<Node> lane = laneNodes[curLane];

                    // Only check if there is more than 1 item in the lane. If there is just 1
                    // item, the remaining item is the child node of the connecting junction.
                    // We will wait to draw it until then.
                    bool isUsable = false;
                    if (lane.Count > 1 )
                    {
                        isUsable = true;
                    }
                    else if(lane.Count == 1)
                    {
                        isUsable = true;
                        foreach (Junction j in lane[0].Descendants)
                        {
                            List<Node> jNodes = null;
                            isUsable = junctionNodes.TryGetValue(j, out jNodes);
                            if( isUsable )
                            {
                                isUsable = jNodes.Count <= 1;
                            }

                            if (!isUsable)
                            {
                                break;
                            }
                        }
                    }
                    if (isUsable)
                    {
                        if (currentRow.Node == null ||
                            currentRow.Node.Data == null ||
                            (lane[0].Data != null && sorter.Invoke(lane[0].Data, currentRow.Node.Data) > 0))
                        {
                            currentRow.Node = lane[0];
                            currentRow.NodeLane = curLane;
                        }
                    }
                }

                // If this row doesn't contain data, we're to the end of the valid entries.
                if (currentRow.Node == null || currentRow.Node.Data == null)
                {
                    // DEBUG: The check above didn't find anything, but should have
                    if (currentRow.Node == null && laneNodes.Count > 0)
                    {
                        //if (Debugger.IsAttached) Debugger.Break();
                        //Node[] topo = this.sourceGraph.TopoSortedNodes();
                    }

                    return false;
                }

                #endregion

                // Check to see if there are available lanes that could be used
                // that are better than the current row node lane
                #region Don't skip lanes
                // If the current lane is larger than the number of lanes (there is a gap)
                // swap with the last item.
                if (currentRow.NodeLane > currentRow.Count)
                {
                    List<Node> temp = laneNodes[currentRow.NodeLane];
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
                    List<Node> lane = laneNodes[curLane];
                    if (lane.Count == 0 || currentRow.Node != lane[0])
                    {
                        // We're only interested in columns that have the same node
                        // at the top of the junction as the current row's node
                        continue;
                    }

                    // Remove the duplicate
                    lane.RemoveAt(0);

                    // We need to draw the graph line for this lane. If there are no items 
                    // left in the lane we don't draw it.
                    if (lane.Count > 0)
                    {
                        LaneInfo info = new LaneInfo(curLane);
                        info.Child.Add(currentRow.Node);
                        info.Parent.Add(laneNodes[curLane][0]);
                        currentRow.Add(currentRow.NodeLane, info );
                    }
                }
                // Set the current row's new parentage
                
                #endregion

                // Advance the LaneNodes
                #region Check to see if we can move up ancestor(s) into LaneNodes
                // Check to see if any of the lanes are fully processed and a ancestor
                // can be brought in for processing.
                for (int curLane = 0; curLane < laneNodes.Count; curLane++)
                {
                    // If there aren't any items in the row, there isn't anything to do
                    if (laneNodes[curLane].Count == 0)
                    {
                        continue;
                    }

                    Node node = laneNodes[curLane][0];

                    // If we have multiple lanes with this same ancestor on the top,
                    // we can to consolidate them.
                    for (int nextLane = curLane + 1; nextLane < laneNodes.Count; nextLane++)
                    {
                        if (laneNodes[nextLane].Count == 1 && laneNodes[nextLane][0] == node)
                        {
                            laneNodes[nextLane].Clear();
                            currentRow.Replace(nextLane, curLane);
                            //if (currentRow.LaneInfoCount(nextLane) > 0)
                            //{
                            //    LaneInfo curRow = currentRow[nextLane, 0];
                            //    curRow.ConnectLane = curLane;
                            //    currentRow.Clear(nextLane);
                            //    currentRow.Add(nextLane, curRow);
                            //}
                        }
                    }
                    
                    // If 1 item left in the row, see if we can start to draw any of the 
                    // parents. We can only do it if all of the parent's descendants are 
                    // fully drawn.
                    if (laneNodes[curLane].Count != 1)
                    {
                        continue;
                    }

                    // Check the node's ancestors to see if we can bring them into
                    // the nodes
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
                            if (laneNodes[laneIndex].Count == 0)
                            {
                                break;
                            }
                        }
                        if (laneIndex == laneNodes.Count)
                        {
                            laneNodes.Add(new List<Node>());
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

                        List<Node> nodes = new List<Node>(parent.Bunch);
                        laneNodes[laneIndex] = nodes;
                        junctionNodes[parent] = nodes;
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
                                    List<Node> temp = laneNodes[curLane];
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
                //Console.WriteLine(CurrentRow);

                if (currentRow.Node != null)
                {
                    Lanes.LaneRow row = currentRow.Clone();
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
