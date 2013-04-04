using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GitCommands;

namespace GitUI.RevisionGridClasses
{
    partial class DvcsGraph
    {
        private sealed class Graph
        {
            #region Delegates

            public delegate void GraphUpdatedHandler(object sender);

            private delegate bool Visit(Node n);

            #endregion

            public readonly List<Node> AddedNodes = new List<Node>();

            private readonly List<Junction> junctions = new List<Junction>();
            public readonly Dictionary<string, Node> Nodes = new Dictionary<string, Node>();
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
                    foreach (Junction j in junctions)
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

            public ILaneRow this[int col]
            {
                get { return lanes[col]; }
            }

            public int CachedCount
            {
                get { return lanes.CachedCount; }
            }

            public void Filter(string aId)
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

            public void ClearHighlightBranch()
            {
                foreach (Node node in Nodes.Values)
                {
                    foreach (Junction junction in node.Ancestors)
                    {
                        junction.HighLight = false;
                    }
                }
            }

            public void HighlightBranch(string aId)
            {
                ClearHighlightBranch();
                HighlightBranchRecursive(aId);
            }

            public void HighlightBranchRecursive(string aId)
            {
                Node startNode = Nodes[aId];

                foreach (Junction junction in startNode.Ancestors)
                {
                    if (junction.HighLight)
                        continue;

                    junction.HighLight = true;

                    HighlightBranchRecursive(junction.Parent.Id);
                }
            }

            public event GraphUpdatedHandler Updated;

            public void Add(string aId, string[] aParentIds, DataType aType, GitRevision aData)
            {
                // If we haven't seen this node yet, create a new junction.
                Node node;
                if (!GetNode(aId, out node) && (aParentIds == null || aParentIds.Length == 0))
                {
                    var newJunction = new Junction(node, node);
                    junctions.Add(newJunction);
                }
                nodeCount++;
                node.Data = aData;
                node.DataType = aType;
                node.Index = AddedNodes.Count;
                AddedNodes.Add(node);

                foreach (string parentId in aParentIds)
                {
                    Node parent;
                    GetNode(parentId, out parent);
                    if (parent.Index < node.Index)
                    {
                        // TODO: We might be able to recover from this with some work, but
                        // since we build the graph async it might be tough to figure out.
                        Debug.WriteLine("The nodes must be added such that all children are added before their parents");
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
                        junctions.Add(splitNode);

                        // The node is a junction point. We are a new junction
                        var junction = new Junction(node, parent);
                        junctions.Add(junction);
                    }
                    else if (parent.Descendants.Count == 1 && parent.Descendants[0].Parent != parent)
                    {
                        // The parent is in the middle of a junction. We need to split it.     
                        Junction splitNode = parent.Descendants[0].Split(parent);
                        junctions.Add(splitNode);

                        // The node is a junction point. We are a new junction
                        var junction = new Junction(node, parent);
                        junctions.Add(junction);
                    }
                    else
                    {
                        // The node is a junction point. We are a new junction
                        var junction = new Junction(node, parent);
                        junctions.Add(junction);
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
                    var parent = d.TryGetParent(node);
                    if (parent != null && parent.InLane != int.MaxValue)
                    {
                        int resetTo = d.Parent.Descendants.Aggregate(d.Parent.InLane, (current, dd) => Math.Min(current, dd.Child.InLane));
                        Debug.WriteLine("We have to start over at lane {0} because of {1}", resetTo, node);
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
                junctions.Clear();
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
                            j.Remove(n);
                        }
                        goto start_over;
                    }
                }
            }

            public IEnumerable<Node> GetRefs()
            {
                var nodes = new List<Node>();
                foreach (Junction j in junctions)
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
                //L ? Empty list that will contain the sorted nodes
                //S ? Set of all nodes with no incoming edges

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
                foreach (Node h in GetRefs())
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

                if (J.Count != junctions.Count)
                {
                    foreach (var junction in junctions)
                    {
                        if (!J.Contains(junction))
                        {
                            if (junction.Parent != junction.Child)
                            {
                                Debug.WriteLine("*** {0} *** {1} {2}", junction, Nodes.Count, junctions.Count);
                            }
                        }
                    }
                }

                return L.ToArray();
            }

            private bool GetNode(string aId, out Node aNode)
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
                    junctions = new List<Junction>(1) { aJunction };
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

            #region Nested type: ILaneRow

            public interface ILaneRow
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
    }
}
