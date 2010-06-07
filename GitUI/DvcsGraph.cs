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
        public class GraphData
        {
            public void Add(IComparable aId, IComparable[] aParentIds, object aData)
            {
                GraphTree.Add(aId, aParentIds, aData);
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
                object temp = lanes[aRow]; // TODO: Add populate function. Right now everything is
                                           // pre-populated anyway.
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
            CellPainting += new DataGridViewCellPaintingEventHandler(dataGrid_CellPainting);
            ColumnWidthChanged += new DataGridViewColumnEventHandler(dataGrid_ColumnWidthChanged);
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

        public void SetData(GraphData aData)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate()
                    {
                        // TODO: This is the same code as below, move into a helper function.
                        int populate = CurrentRow == null ? 0 : CurrentRow.Index + Height / RowHeight;
                        GraphLanes = (Lanes)aData.Populate(populate);
                        RowCount = GraphLanes.Count;
                        RebuildGraph();
                    }));

                return;
            }

            GraphLanes = (Lanes)aData.Populate(0);
            RowCount = GraphLanes.Count;
            RebuildGraph();
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
        private const int LANE_WIDTH = 14;
        private const int LANE_LINE_WIDTH = 2;

        private void RebuildGraph()
        {
            // Auto scale width on load
            int laneCount = 2;
            if (GraphLanes != null)
            {
                laneCount = Math.Max(laneCount, GraphLanes.Width);
            }
            dataGridColumnGraph.Width = LANE_WIDTH * laneCount;

            // Redraw
            CacheHead = -1;
            CacheHeadRow = 0;
            DrawGraph(-1);
            Invalidate();
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

            Invalidate(false);
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

                foreach (Lanes.LaneRow.LaneInfo parent in row[lane])
                {
                    Color curColor = GetJunctionColor(parent.Junction);
                    Color nextColor;
                    if (parent.JunctionParents.Length >= 1)
                    {
                        // NOTE: If there was more than 1 parent, the parent was a merge
                        // node...we can't really show 3 colors well so just use
                        // the first.
                        nextColor = GetJunctionColor(parent.JunctionParents[0]);
                    }
                    else
                    {
                        nextColor = Color.Black;
                    }

                    // Create the brush for drawing the line
                    Brush brushLine;
                    if (nextColor == curColor)
                    {
                        brushLine = new SolidBrush(curColor);
                    }
                    else
                    {
                        if (GitCommands.Settings.MulticolorBranches)
                        {
                            brushLine = new HatchBrush(HatchStyle.DarkDownwardDiagonal, nextColor, curColor);
                        }
                        else
                        {
                            //brushLine = new SolidBrush(curColor);
                            brushLine = new SolidBrush(nextColor);
                        }
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

                        if (parent.ConnectsTo == lane)
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
                                new Point(mid + (parent.ConnectsTo - lane) * LANE_WIDTH, top - 1),
                                new Point(mid + (parent.ConnectsTo - lane) * LANE_WIDTH, top + RowHeight + 2)
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

            Lanes.LaneRow.LaneInfo[] nodeLaneInfo = row[row.NodeLane];
            Brush nodeBrush = null;
            if (nodeLaneInfo.Length == 0)
            {
                nodeBrush = new SolidBrush(Color.Black);
            }
            else if (nodeLaneInfo.Length == 1)
            {
                nodeBrush = new SolidBrush(GetJunctionColor(nodeLaneInfo[0].Junction));
            }
            else
            {
                nodeBrush = new LinearGradientBrush(nodeRect, GetJunctionColor(nodeLaneInfo[0].Junction), GetJunctionColor(nodeLaneInfo[1].Junction), LinearGradientMode.Horizontal);
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
            public bool IsActive = false;
            public bool IsSpecial = false;

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
            
            public Comparison<object> Sorter = null;

            public Graph()
            {
            }

            public void Add(IComparable aId, IComparable[] aParentIds, object aData)
            {
                // If we haven't seen this node yet, create a new junction.
                Node node = null;
                if (!GetNode(aId, out node) && (aParentIds == null || aParentIds.Length == 0))
                {
                    Junctions.Add(new Junction(node, node));
                }
                //Console.WriteLine(node);

                foreach (IComparable parentId in aParentIds)
                {
                    Node parent;
                    GetNode(parentId, out parent);
                    //Console.WriteLine("\t" + parent);
                    if (node.Descendants.Count == 1 && parent.Ancestors.Count == 0
                        && node.Descendants[0].Parent == node)
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

                node.Data = aData;
            }

            public int Count { get { return Nodes.Count; } }

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
            public int Width
            {
                get
                {
                    int width = 1;
                    foreach (LaneRow row in laneRows)
                    {
                        width = Math.Max(row.Count, width);
                    }
                    return width;
                }
            }

            private Graph sourceGraph;
            private List<LaneRow> laneRows;

            private List<List<Node>> laneNodes = new List<List<Node>>();
            private Dictionary<Junction, List<Node>> junctionNodes = new Dictionary<Junction, List<Node>>();
            private ActiveLaneRow currentRow = new ActiveLaneRow();
            private Comparison<object> sorter;

            public class LaneRow
            {
                // Node information
                public int NodeLane = -1;
                public Node Node = null;

                // Parents for the row
                internal List<LaneInfo[]> ParentLanes = new List<LaneInfo[]>();

                public struct LaneInfo
                {
                    public int ConnectsTo;
                    public Junction Junction;
                    public Junction[] JunctionParents;

                    public LaneInfo(int aConnectsTo, Junction aJunction, Junction[] aJunctionParents)
                    {
                        ConnectsTo = aConnectsTo;
                        Junction = aJunction;
                        JunctionParents = aJunctionParents;
                    }

                    public static implicit operator int(LaneInfo a)
                    {
                        return a.ConnectsTo;
                    }

                    public override string ToString()
                    {
                        return ConnectsTo.ToString();
                    }
                }

                public LaneRow()
                {
                }

                public LaneRow(LaneRow aCopyOf)
                {
                    // Deep copy
                    NodeLane = aCopyOf.NodeLane;
                    Node = aCopyOf.Node;
                    foreach (LaneInfo[] info in aCopyOf.ParentLanes)
                    {
                        LaneInfo[] newInfo = new LaneInfo[info.Length];
                        for (int i = 0; i < info.Length; i++)
                        {
                            newInfo[i] = info[i];
                        }
                        ParentLanes.Add(newInfo);
                    }
                }

                // Lane information
                public int Count
                {
                    get { return ParentLanes.Count; }
                }
                public LaneInfo[] this[int col]
                {
                    get
                    {
                        if (col < ParentLanes.Count)
                        {
                            return ParentLanes[col];
                        }
                        else
                        {
                            return new LaneInfo[0];
                        }
                    }
                    set
                    {
                        while (ParentLanes.Count <= col)
                        {
                            ParentLanes.Add(new LaneInfo[0]);
                        }
                        ParentLanes[col] = value;
                    }
                }

            }

            private class ActiveLaneRow : LaneRow
            {
                public bool Active(int col)
                {
                    for (int j = 0; j < ParentLanes.Count; j++)
                    {
                        for (int k = 0; k < ParentLanes[j].Length; k++)
                        {
                            if (ParentLanes[j][k].ConnectsTo == col)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                public bool Clear(int col)
                {
                    bool isChanged = false;
                    for (int j = 0; j < ParentLanes.Count; j++)
                    {
                        for (int k = 0; k < ParentLanes[j].Length; k++)
                        {
                            if (ParentLanes[j][k].ConnectsTo > col)
                            {
                                ParentLanes[j][k].ConnectsTo--;
                                isChanged = true;
                            }
                        }
                    }
                    return isChanged;
                }

                public void Replace(int aOld, int aNew)
                {
                    for (int j = 0; j < ParentLanes.Count; j++)
                    {
                        for (int k = 0; k < ParentLanes[j].Length; k++)
                        {
                            if (ParentLanes[j][k].ConnectsTo == aOld)
                            {
                                ParentLanes[j][k].ConnectsTo = aNew;
                            }
                        }
                    }
                }

                public void RemoveAt(int col)
                {
                    ParentLanes.RemoveAt(col);
                }

                public override string ToString()
                {
                    string s = NodeLane + "/" + ParentLanes.Count + ": " + Node + " ";
                    for (int i = 0; i < ParentLanes.Count; i++)
                    {
                        if (i == NodeLane)
                            s += "*";
                        s += "{";
                        for (int j = 0; j < ParentLanes[i].Length; j++)
                            s += " " + ParentLanes[i][j];
                        s += " }, ";
                    }
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
                #region Keep merge active & cleanup LaneNodes
                int curLaneCount = Math.Max(currentRow.NodeLane, currentRow.Count);
                for (int curLane = 0; curLane <= curLaneCount; curLane++)
                {
                    if (currentRow[curLane].Length == 1)
                    {
                        int rowLane = currentRow[curLane][0];
                        if (rowLane != curLane)
                        {
                            // We've pointed to the lane that has a node. That means this is a branch 
                            // from that lane. Keep the branch alive in it's new lane
                            currentRow[rowLane] = new LaneRow.LaneInfo[1] { currentRow[curLane][0] };
                            currentRow[curLane] = new LaneRow.LaneInfo[0];
                        }
                    }
                    else if (currentRow[curLane].Length == 2)
                    {
                        LaneRow.LaneInfo[] mergeLanes = currentRow[curLane];
                        currentRow[mergeLanes[0]] = new LaneRow.LaneInfo[1] { mergeLanes[0] };
                        currentRow[mergeLanes[1]] = new LaneRow.LaneInfo[1] { mergeLanes[1] };

                        // This lane just did a merge. Mark both merge lanes as active
                        if (currentRow[curLane][0] != curLane && currentRow[curLane][1] != curLane)
                        {
                            // I'm ok with this...with the code that pushes everything as far left
                            // as possible, sometimes you'll get a merge into a new lane on the right.

                            //Console.WriteLine("Odd...We had a merge that came from 2 different lanes...");
                            //if (Debugger.IsAttached)
                            //    Debugger.Break();
                            currentRow[curLane] = new LaneRow.LaneInfo[0];
                        }
                    }
                }
                #endregion

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
                    else if(lane.Count == 1 && lane[0].Ancestors.Count == 0)
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
                        if (currentRow.Node == null || (lane[0].Data != null && currentRow.Node.Data != null && sorter.Invoke(lane[0].Data, currentRow.Node.Data) > 0))
                        {
                            currentRow.Node = lane[0];
                            currentRow.NodeLane = curLane;
                        }
                    }
                }
                #endregion

                // Check to see if there are available lanes that could be used
                // that are better than the current row node lane
                #region Don't skip lanes
                // Remove unused lanes from the end of CurrentRow
                for (int i = currentRow.Count - 1; i >= 0; --i)
                {
                    if (currentRow[i].Length == 0)
                    {
                        currentRow.RemoveAt(i);
                    }
                    else
                    {
                        break;
                    }
                }
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
                List<LaneRow.LaneInfo> CurRowParents = new List<LaneRow.LaneInfo>();
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
                        // We also want to know the junctions of the guy we're point to
                        List<Junction> junctionParents = new List<Junction>();
                        foreach (Junction ancestor in lane[0].Ancestors)
                        {
                            junctionParents.Add(ancestor);
                        }

                        // TODO: Much more efficient to store junction information along with lane
                        //       which would also eliminate lookup of index of list further down.
                        //       We probably could drop JunctionNodes all together at that point.
                        Junction junction = null;
                        foreach (KeyValuePair<Junction, List<Node>> j in junctionNodes)
                        {
                            if (j.Value == laneNodes[curLane])
                            {
                                junction = j.Key;
                                break;
                            }
                        }
                        if (junction == null && Debugger.IsAttached) Debugger.Break();

                        CurRowParents.Add(new LaneRow.LaneInfo(curLane, junction, junctionParents.ToArray()));
                    }
                }
                // Set the current row's new parentage
                currentRow[currentRow.NodeLane] = CurRowParents.ToArray();
                #endregion

                // Advance the LaneNodes
                #region Check to see if we can move up ancestor(s) into LaneNodes
                // Check to see if any of the lanes are fully processed and a ancestor
                // can be brought in for processing.
                for (int curLane = 0; curLane < laneNodes.Count; curLane++)
                {
                    // If 1 item left in the row, see if we can start to draw any of the 
                    // parents. We can only do it if all of the parent's descendants are 
                    // fully drawn.
                    if (laneNodes[curLane].Count != 1)
                    {
                        continue;
                    }

                    Node node = laneNodes[curLane][0];

                    if (node.Ancestors.Count == 0)
                    {
                        // If we have multiple lanes with this same ancestor on the top,
                        // we need to consolidate them. This happens when the first node in the
                        // graph has a branch
                        for (int nextLane = curLane + 1; nextLane < laneNodes.Count; nextLane++)
                        {
                            if (laneNodes[nextLane].Count == 1 && laneNodes[nextLane][0] == node)
                            {
                                laneNodes[nextLane].Clear();
                                if (currentRow[nextLane].Length > 0)
                                {
                                    currentRow[nextLane][0].ConnectsTo = curLane;
                                }
                            }
                        }
                        // No ancestors. We'll leave this guy in the lanes until
                        // he gets selected out and drawn.
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

                            if (!currentRow.Active(siblingLane))
                            {
                                // First give preference to something already
                                // referenced by this lane (this will avoid messy
                                // merges where neither parent is in the same lane
                                // as the merge
                                foreach (LaneRow.LaneInfo lane in currentRow[siblingLane])
                                {
                                    if (lane > siblingLane)
                                    {
                                        currentRow.Replace(lane, siblingLane);
                                        List<Node> temp = laneNodes[siblingLane];
                                        laneNodes[siblingLane] = laneNodes[lane];
                                        laneNodes[lane] = temp;
                                        break;
                                    }
                                }
                            }
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
                    if (!currentRow.Active(curLane))
                    {
                        if (laneNodes[curLane].Count == 0)
                        {
                            // Clear the empty lane
                            currentRow.Clear(curLane);
                            laneNodes.RemoveAt(curLane);
                            curLane--;
                        }
                        else
                        {
                            // Swap the lane with one that is in use
                            for (int j = curLane + 1; j < laneNodes.Count; j++)
                            {
                                if (currentRow.Active(j))
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

                for (int i = currentRow.Count - 1; i >= 0; --i)
                {
                    if (i > currentRow.NodeLane && currentRow[i].Length == 0)
                    {
                        currentRow.RemoveAt(i);
                    }
                    else
                    {
                        break;
                    }
                }
                #endregion

                // DEBUG: Spit out the information about the lane
                //Console.WriteLine(CurrentRow);

                if (currentRow.Node != null)
                {
                    Lanes.LaneRow row = new Lanes.LaneRow(currentRow);
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
