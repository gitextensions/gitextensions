using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GitUI.RevisionGridClasses
{
    partial class DvcsGraph
    {
        private sealed class Lanes : IEnumerable<Graph.ILaneRow>
        {
            private readonly ActiveLaneRow currentRow = new ActiveLaneRow();
            private readonly List<LaneJunctionDetail> laneNodes = new List<LaneJunctionDetail>();
            private readonly List<Graph.ILaneRow> laneRows;
            private readonly Graph sourceGraph;

            public Lanes(Graph aGraph)
            {
                sourceGraph = aGraph;
                // Rebuild lanes
                laneRows = new List<Graph.ILaneRow>();
            }

            public Graph.ILaneRow this[int row]
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

            public IEnumerator<Graph.ILaneRow> GetEnumerator()
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

                foreach (Node aNode in sourceGraph.GetRefs())
                    Update(aNode);
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
                if (aNode.Descendants.Count != 0)
                    return;

                // This node is a head, create a new lane for it
                Node h = aNode;
                if (h.Ancestors.Count != 0)
                {
                    foreach (Junction j in h.Ancestors)
                    {
                        var detail = new LaneJunctionDetail(j);
                        laneNodes.Add(detail);
                    }
                }
                else
                {
                    // This is a single entry with no parents or children.
                    var detail = new LaneJunctionDetail(h);
                    laneNodes.Add(detail);
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
                        //break;
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
                    int intoLane = AdvanceLane(curLane);
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
                //   but only when there are not too many lanes taking up too much performance
                if (currentRow.Count < 10)
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
                    Graph.ILaneRow row = currentRow.Advance();

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
            private int AdvanceLane(int curLane)
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
                    Node node = lane.Junction.Oldest;
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

            private sealed class ActiveLaneRow : Graph.ILaneRow
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

                public Graph.ILaneRow Advance()
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

                private sealed class Edges
                {
                    private readonly List<int> countEnd = new List<int>();
                    private readonly List<int> countStart = new List<int>();
                    private readonly List<Edge> edges = new List<Edge>();
                    
                    #pragma warning disable 0649
                    private readonly Graph.LaneInfo emptyItem;
                    #pragma warning restore 0649

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
                        return edges.Count(e => e.Start == lane);
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
                        return edges.Count(e => e.End == lane);
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

            private sealed class LaneEnumerator : IEnumerator<Graph.ILaneRow>
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

                public Graph.ILaneRow Current
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

            private sealed class LaneJunctionDetail
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
                            return 1 - index;
                        return junction == null ? 0 : junction.NodesCount - index;
                    }
                }

                public Junction Junction
                {
                    get { return junction; }
                }

                public Node Current
                {
                    get { return node ?? (index < junction.NodesCount ? junction[index] : null); }
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

                public void Next()
                {
                    index++;

                    if (junction != null && index >= junction.NodesCount)
                        junction.CurrentState = Junction.State.Processed;
                }

                public override string ToString()
                {
                    if (junction != null)
                    {
                        string nodeName = "(null)";
                        if (index < junction.NodesCount)
                        {
                            nodeName = junction[index].ToString();
                        }
                        return index + "/" + junction.NodesCount + "~" + nodeName + "~" + junction;
                    }
                    return node != null
                        ? index + "/n~" + node + "~(null)"
                        : "X/X~(null)~(null)";
                }
            }

            #endregion

            #region Nested type: SavedLaneRow

            private sealed class SavedLaneRow : Graph.ILaneRow
            {
                private readonly Edge[] edges;
                private readonly Node node;
                private readonly int nodeLane = -1;

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
                }

                public Node Node
                {
                    get { return node; }
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
                    return edges.Count(edge => edge.Start == lane);
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
    }
}
