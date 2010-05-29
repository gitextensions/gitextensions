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
        private Graph GraphTree;
        private Lanes GraphLanes;

        private IEnumerator<Lanes.LaneRow> LaneEnum;
        private List<Lanes.LaneRow> LaneRows = new List<Lanes.LaneRow>();

        int CacheHead = -1; // The 'slot' that is the head of the circular bitmap
        int CacheHeadRow = 0; // The node row that is in the head slot
        int CacheCount = 15; // Number of elements in the cache. Will be based on control height.
        Bitmap GraphBitmap = null;
        Graphics GraphWorkArea = null;
        Dictionary<Junction, int> JunctionColors = new Dictionary<Junction, int>();
        Random r = new Random();

        int RowHeight = 0;
        const int NODE_DIMENSION = 8;
        const int LANE_WIDTH = 14;
        const int LANE_LINE_WIDTH = 2;

        public DvcsGraph()
        {
            InitializeComponent();
            CellPainting += new DataGridViewCellPaintingEventHandler(dataGrid_CellPainting);
            ColumnWidthChanged += new DataGridViewColumnEventHandler(dataGrid_ColumnWidthChanged);
            VirtualMode = true;
        }

        public object GetRowData(int aRow)
        {
            Lanes.LaneRow row = GetRow(aRow);
            if (row == null)
            {
                return null;
            }
            return row.Node.Data;
        }

        private Lanes.LaneRow GetRow(int aRow)
        {
            if (LaneEnum == null)
            {
                return null;
            }

            while (aRow >= LaneRows.Count && LaneEnum.MoveNext())
            {
                Lanes.LaneRow row = new Lanes.LaneRow(LaneEnum.Current);
                LaneRows.Add(row);
            }
            if (aRow < 0 || aRow >= LaneRows.Count)
            {
                return null;
            }
            return LaneRows[aRow];
        }

        void dataGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            Lanes.LaneRow row = GetRow(e.RowIndex);
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

        void dataGrid_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            DrawGraph(-1);
        }

        public void SetGraph(Graph aTree)
        {
            GraphTree = aTree;
            if (GraphTree == null)
            {
                return;
            }

            GraphLanes = new Lanes(GraphTree);

            // Reset state
            LaneRows.Clear();
            LaneEnum = GraphLanes.GetEnumerator();
            RowCount = GraphTree.Nodes.Count;

            // Redraw
            CacheHead = -1;
            CacheHeadRow = 0;
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
            List<Junction> adjacnetJunctions = new List<Junction>(); // TODO: Use a set?
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
            if (GraphLanes == null || GraphTree.Nodes.Count == 0)
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
                GraphBitmap = new Bitmap(width, height);
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
                Lanes.LaneRow row = GetRow(rowIndex);
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
                    DrawItem(GraphWorkArea, GetRow(rowIndex - 1));
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
                        brushLine = new HatchBrush(HatchStyle.DarkDownwardDiagonal, nextColor, curColor);
                        //brushLine = new SolidBrush(curColor);
                        //brushLine = new SolidBrush(nextColor);
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
                            //wa.DrawLine
                            //    (
                            //    penLine,
                            //    new Point(mid, top),
                            //    new Point(mid + (parent.ConnectsTo - lane) * LANE_WIDTH, top + RowHeight)
                            //    );
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
            if (row.Node.IsActive)
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

        private void DvcsGraph_Paint(object sender, PaintEventArgs e)
        {
            //DrawGraph(e.Graphics);
            e.Graphics.DrawImageUnscaled(GraphBitmap, 0, 0);
            for (int i = 0; i < CacheCount; i++)
            {

                e.Graphics.DrawLine(Pens.Black, 0, i * RowHeight, Width, i * RowHeight);
            }
        }

        private void dataGrid_Resize(object sender, EventArgs e)
        {
            RowHeight = RowTemplate.Height;
            CacheCount = Height * 3 / RowHeight;
        }


        public class Node
        {
            static int DebugIdNext = 1;
            int DebugId;

            public IComparable Id;
            public List<Junction> Ancestors = new List<Junction>();
            public List<Junction> Descendants = new List<Junction>();
            public bool IsActive = false;
            public bool IsSpecial = false;

            public object Data;

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

        public class Junction
        {
            static char DebugIdNext = 'A';
            char DebugId;

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

        public class Graph
        {
            public List<Junction> Junctions = new List<Junction>();
            public Dictionary<IComparable, Node> Nodes = new Dictionary<IComparable, Node>();
            public Comparison<object> Sorter = null;

            public Node Add(IComparable aId, IComparable[] aParentIds, object aData)
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
                return node;
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

        internal class Lanes : IEnumerable<Lanes.LaneRow>
        {
            private Graph Graph;
            private JunctionHierarchy JunctionLanes;

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

            private class JunctionHierarchy
            {
                private List<List<Junction>> mData = new List<List<Junction>>();

                public JunctionHierarchy()
                {
                }

                public JunctionHierarchy(JunctionHierarchy aOther)
                {
                    // Deep copy
                    mData = new List<List<Junction>>();
                    foreach (List<Junction> row in aOther.mData)
                    {
                        List<Junction> newRow = new List<Junction>();
                        foreach (Junction item in row)
                        {
                            newRow.Add(item);
                        }
                        mData.Add(newRow);
                    }
                }

                public int Rows()
                {
                    return mData.Count;
                }

                public int Columns(int aRow)
                {
                    if (mData.Count <= aRow)
                    {
                        return 0;
                    }
                    else
                    {
                        return mData[aRow].Count;
                    }
                }

                public Junction this[int row, int col]
                {
                    get
                    {
                        if (mData.Count > row)
                        {
                            if (mData[row].Count > col)
                            {
                                return mData[row][col];
                            }
                        }
                        return null;
                    }
                    set
                    {
                        while (mData.Count <= row)
                        {
                            mData.Add(new List<Junction>());
                        }
                        while (mData[row].Count <= col)
                        {
                            mData[row].Add(null);
                        }
                        mData[row][col] = value;
                    }
                }
                public List<Junction> this[int row]
                {
                    get
                    {
                        while (mData.Count <= row)
                        {
                            mData.Add(new List<Junction>());
                        }
                        return mData[row];
                    }
                }

                public int Find(Junction aItem)
                {
                    int aCol = 0;
                    for (int aRow = 0; aRow < mData.Count; aRow++)
                    {
                        for (aCol = 0; aCol < mData[aRow].Count; aCol++)
                        {
                            if (aItem.Equals(mData[aRow][aCol]))
                            {
                                return aRow;
                            }
                        }
                    }
                    return -1;
                }

                public void InsertRow(int aRow)
                {
                    mData.Insert(aRow, new List<Junction>());
                }

                public override string ToString()
                {
                    string s = "";
                    foreach (List<Junction> row in mData)
                    {
                        foreach (Junction item in row)
                        {
                            string itemName;
                            if (item == null)
                            {
                                itemName = "<null>";
                            }
                            else
                            {
                                itemName = item.ToString();
                            }
                            s += string.Format("{0, -30}", itemName);
                        }
                        s += "\n";
                    }

                    return s;
                }
            } // end of class LaneArray

            private class LaneEnumerator : IEnumerator<LaneRow>
            {
                private Lanes Lanes;
                private Comparison<object> Sorter;

                private JunctionHierarchy JunctionLanes;
                private List<List<Node>> LaneNodes = new List<List<Node>>();
                private Dictionary<Junction, List<Node>> JunctionNodes = new Dictionary<Junction, List<Node>>();

                private ActiveLaneRow CurrentRow = new ActiveLaneRow();

                public LaneEnumerator(Lanes aLanes)
                {
                    Lanes = aLanes;
                    Sorter = Lanes.Graph.Sorter;
                    if (Sorter == null)
                    {
                        Sorter = delegate(object a, object b)
                        {
                            IComparable left = (IComparable) a;
                            IComparable right = (IComparable) b;
                            return left.CompareTo( right );
                        };
                    }
                    Reset();
                }

                public void Reset()
                {
                    JunctionLanes = new JunctionHierarchy(Lanes.JunctionLanes);

                    // Update current lanes with the first row of cliques
                    for (int i = 0; i < Lanes.JunctionLanes.Columns(0); i++)
                    {
                        Junction j = Lanes.JunctionLanes[0, i];
                        if (j != null)
                        {
                            List<Node> nodes = new List<Node>(j.Bunch);
                            LaneNodes.Add(nodes);
                            JunctionNodes[j] = nodes;
                        }
                        else
                        {
                            LaneNodes.Add(new List<Node>());
                        }
                    }
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
                        return CurrentRow;
                    }
                }

                public bool MoveNext()
                {
                    // If there are no lanes, there is nothing more to draw
                    if (LaneNodes.Count == 0)
                    {
                        return false;
                    }

                    // Make sure that lanes that have been merged are active and the branched 
                    // lanes are cleared. Also remove any empty lanes from LaneNodes
                    #region Keep merge active & cleanup LaneNodes
                    int curLaneCount = Math.Max(CurrentRow.NodeLane, CurrentRow.Count);
                    for (int curLane = 0; curLane <= curLaneCount; curLane++)
                    {
                        if (CurrentRow[curLane].Length == 1)
                        {
                            int rowLane = CurrentRow[curLane][0];
                            if (rowLane != curLane)
                            {
                                // We've pointed to the lane that has a node. That means this is a branch 
                                // from that lane. Keep the branch alive in it's new lane
                                CurrentRow[rowLane] = new LaneRow.LaneInfo[1] { CurrentRow[curLane][0] };
                                CurrentRow[curLane] = new LaneRow.LaneInfo[0];
                            }
                        }
                        else if (CurrentRow[curLane].Length == 2)
                        {
                            LaneRow.LaneInfo[] mergeLanes = CurrentRow[curLane];
                            CurrentRow[mergeLanes[0]] = new LaneRow.LaneInfo[1] { mergeLanes[0] };
                            CurrentRow[mergeLanes[1]] = new LaneRow.LaneInfo[1] { mergeLanes[1] };

                            // This lane just did a merge. Mark both merge lanes as active
                            if (CurrentRow[curLane][0] != curLane && CurrentRow[curLane][1] != curLane)
                            {
                                // I'm ok with this...with the code that pushes everything as far left
                                // as possible, sometimes you'll get a merge into a new lane on the right.

                                //Console.WriteLine("Odd...We had a merge that came from 2 different lanes...");
                                //if (Debugger.IsAttached)
                                //    Debugger.Break();
                                CurrentRow[curLane] = new LaneRow.LaneInfo[0];
                            }
                        }
                    }
                    #endregion

                    // Find the new current row's node (newest item in the row)
                    #region Find current node & index
                    CurrentRow.Node = null;
                    for (int curLane = 0; curLane < LaneNodes.Count; curLane++)
                    {
                        List<Node> lane = LaneNodes[curLane];
                        // Only check if there is more than 1 item in the lane. If there is just 1
                        // item, the remaining item is the child node of the connecting junction.
                        // We will wait to draw it until then.
                        if (lane.Count > 1 || (lane.Count != 0 && lane[lane.Count - 1].Ancestors.Count == 0))
                        {
                            if (CurrentRow.Node == null || Sorter.Invoke(lane[0].Data, CurrentRow.Node.Data) > 0)
                            {
                                CurrentRow.Node = lane[0];
                                CurrentRow.NodeLane = curLane;
                            }
                        }
                    }
                    #endregion

                    // Check to see if there are available lanes that could be used
                    // that are better than the current row node lane
                    #region Don't skip lanes
                    // Remove unused lanes from the end of CurrentRow
                    for (int i = CurrentRow.Count - 1; i >= 0; --i)
                    {
                        if (CurrentRow[i].Length == 0)
                        {
                            CurrentRow.RemoveAt(i);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (CurrentRow.NodeLane > CurrentRow.Count)
                    {
                        List<Node> temp = LaneNodes[CurrentRow.NodeLane];
                        LaneNodes[CurrentRow.NodeLane] = LaneNodes[CurrentRow.Count];
                        LaneNodes[CurrentRow.Count] = temp;
                        CurrentRow.NodeLane = CurrentRow.Count;
                    }
                    #endregion

                    // Check for multiple junctions with this node at the top. Remove the 
                    // node from that junction as well. This will happen when there is a branch 
                    #region Check for branches
                    List<LaneRow.LaneInfo> CurRowParents = new List<LaneRow.LaneInfo>();
                    for (int curLane = 0; curLane < LaneNodes.Count; curLane++)
                    {
                        List<Node> lane = LaneNodes[curLane];
                        if (lane.Count == 0 || CurrentRow.Node != lane[0])
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
                            foreach (KeyValuePair<Junction, List<Node>> j in JunctionNodes)
                            {
                                if (j.Value == LaneNodes[curLane])
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
                    CurrentRow[CurrentRow.NodeLane] = CurRowParents.ToArray();
                    #endregion

                    // Advance the LaneNodes
                    #region Check to see if we can move up ancestor(s) into LaneNodes
                    // Check to see if any of the lanes are fully processed and a ancestor
                    // can be brought in for processing.
                    for (int curLane = 0; curLane < LaneNodes.Count; curLane++)
                    {
                        // If 1 item left in the row, see if we can start to draw any of the 
                        // parents. We can only do it if all of the parent's descendants are 
                        // fully drawn.
                        if (LaneNodes[curLane].Count != 1)
                        {
                            continue;
                        }

                        Node node = LaneNodes[curLane][0];

                        if (node.Ancestors.Count == 0)
                        {
                            // If we have multiple lanes with this same ancestor on the top,
                            // we need to consolidate them. This happens when the first node in the
                            // graph has a branch
                            for (int nextLane = curLane + 1; nextLane < LaneNodes.Count; nextLane++)
                            {
                                if (LaneNodes[nextLane].Count == 1 && LaneNodes[nextLane][0] == node)
                                {
                                    LaneNodes[nextLane].Clear();
                                    CurrentRow[nextLane][0].ConnectsTo = curLane;
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
                            if (JunctionNodes.ContainsKey(parent))
                            {
                                // We've already merged this junction in
                                continue;
                            }

                            // If all of this junction's descendants are processed, we can
                            // move the parent into the current lane nodes.
                            bool canMerge = true;
                            foreach (Junction sibling in parent.Child.Descendants)
                            {
                                if (!JunctionNodes.ContainsKey(sibling) || JunctionNodes[sibling].Count > 1)
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
                                LaneNodes[curLane].Clear();
                            }

                            // Find available lane to merge into. 
                            int laneIndex;
                            for (laneIndex = 0; laneIndex < LaneNodes.Count; laneIndex++)
                            {
                                if (LaneNodes[laneIndex].Count == 0)
                                {
                                    break;
                                }
                            }
                            if (laneIndex == LaneNodes.Count)
                            {
                                LaneNodes.Add(new List<Node>());
                            }
                            //Console.WriteLine("\tMerge {0} into lane {1}", parent, laneIndex);

                            // Clear first so JunctionNodes count goes to 0
                            LaneNodes[laneIndex].Clear();

                            // Check each merged sibling to see if there were any nodes
                            // remaining. If so, we need to point it to the new row. If not
                            // it has been taken care of already.
                            foreach (Junction sibling in parent.Child.Descendants)
                            {
                                // If the sibling still has nodes, we need to update
                                // any references to it to point to the newly merged
                                // location instead
                                if (JunctionNodes[sibling].Count == 0)
                                {
                                    continue;
                                }
                                int siblingLane = LaneNodes.IndexOf(JunctionNodes[sibling]);
                                CurrentRow.Replace(siblingLane, laneIndex);

                                if (!CurrentRow.Active(siblingLane))
                                {
                                    // First give preference to something already
                                    // referenced by this lane (this will avoid messy
                                    // merges where neither parent is in the same lane
                                    // as the merge
                                    foreach (LaneRow.LaneInfo lane in CurrentRow[siblingLane])
                                    {
                                        if (lane > siblingLane)
                                        {
                                            CurrentRow.Replace(lane, siblingLane);
                                            List<Node> temp = LaneNodes[siblingLane];
                                            LaneNodes[siblingLane] = LaneNodes[lane];
                                            LaneNodes[lane] = temp;
                                            break;
                                        }
                                    }

                                    // TODO: This lane is no longer active. Lets merge somebody
                                    // in to it, even if it wasn't a split merge
                                }
                            }

                            List<Node> nodes = new List<Node>(parent.Bunch);
                            LaneNodes[laneIndex] = nodes;
                            JunctionNodes[parent] = nodes;
                        }
                    }
                    #endregion

                    // Clean up any unused LaneNodes at the end (needed so that MoveNext 
                    // check is easy) and remove unused lanes from the end of CurrentRow
                    // (needed so we know if we are leaving empty lanes the next time 
                    // we call MoveNext)
                    #region Trim empty lanes
                    for (int curLane = 0; curLane < LaneNodes.Count; curLane++)
                    {
                        if (!CurrentRow.Active(curLane))
                        {
                            if (LaneNodes[curLane].Count == 0)
                            {
                                // Clear the empty lane
                                CurrentRow.Clear(curLane);
                                LaneNodes.RemoveAt(curLane);
                                curLane--;
                            }
                            else
                            {
                                // Swap the lane with one that is in use
                                for (int j = curLane + 1; j < LaneNodes.Count; j++)
                                {
                                    if (CurrentRow.Active(j))
                                    {
                                        CurrentRow.Replace(j, curLane);
                                        List<Node> temp = LaneNodes[curLane];
                                        LaneNodes[curLane] = LaneNodes[j];
                                        LaneNodes[j] = temp;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    for (int i = CurrentRow.Count - 1; i >= 0; --i)
                    {
                        if (i > CurrentRow.NodeLane && CurrentRow[i].Length == 0)
                        {
                            CurrentRow.RemoveAt(i);
                        }
                        else
                        {
                            break;
                        }
                    }
                    #endregion

                    // DEBUG: Spit out the information about the lane
                    //Console.WriteLine(CurrentRow);

                    // Return that there are more items left
                    return true;
                }

            }

            public Lanes(Graph aGraph)
            {
                Graph = aGraph;

                foreach (Node n in Graph.Nodes.Values)
                {
                    if (n.Data == null)
                    {
                        string errMsg = string.Format("Incomplete graph. Node \"{0}\" has no data", n.Id);
                        throw new ArgumentException(errMsg);
                    }
                }

                JunctionLanes = new JunctionHierarchy();
                // Add the heads
                foreach (Node h in Graph.GetHeads())
                {
                    foreach (Junction j in h.Ancestors)
                    {
                        AddJunction(null, j);
                    }
                }

                // Also add any single node graphs
                foreach (Junction j in Graph.Junctions)
                {
                    if (j.Child == j.Parent)
                    {
                        AddJunction(null, j);
                    }
                }

                //Console.WriteLine(this.ToString());
            }

            private void AddJunction(Junction aChild, Junction aParent)
            {
                //Console.WriteLine("Lanes: Add Junction {0} -> {1}", aChild == null ? "*" : aChild.ToString().Substring(0, 1), aParent.ToString().Substring(0, 1));

                if (aChild == null)
                {
                    // Heads always go on the top row
                    JunctionLanes[0].Add(aParent);
                }
                else
                {
                    // Get the row/col of the child
                    int childRow = JunctionLanes.Find(aChild);
                    if (childRow == -1)
                    {
                        // Since we're growing from the children to the parents, 
                        // descendants should always be in the graph before their 
                        // ancestors.
                        Console.WriteLine("Lanes.AddJunction: Bad graph?");
                        if (Debugger.IsAttached) Debugger.Break();
                    }

                    int parentRow = JunctionLanes.Find(aParent);
                    if (parentRow != -1)
                    {
                        // The child parent exists in the graph. Make sure it is
                        // on a higher row than the parent.
                        if (parentRow <= childRow)
                        {
                            JunctionLanes[parentRow].Remove(aParent);
                            JunctionLanes[childRow + 1].Add(aParent);
                        }
                    }
                    else
                    {
                        // We haven't seen the parent node yet
                        JunctionLanes[childRow + 1].Add(aParent);
                    }
                }

                // Add our parents to the lane table
                foreach (Junction j in aParent.Parent.Ancestors)
                {
                    AddJunction(aParent, j);
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

            public override string ToString()
            {
                return JunctionLanes.ToString();
            }
        } // end of class Lanes

    } // end of class DvcsGraph
}
