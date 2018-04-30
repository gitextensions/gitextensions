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

            #endregion

            public readonly List<Node> AddedNodes = new List<Node>();

            private readonly List<Junction> _junctions = new List<Junction>();
            public readonly Dictionary<string, Node> Nodes = new Dictionary<string, Node>();
            private readonly Lanes _lanes;
            private int _filterNodeCount;

            private bool _isFilter;
            private int _nodeCount;
            private int _processedNodes;

            public Graph()
            {
                _lanes = new Lanes(this);
            }

            public bool IsFilter
            {
                get { return _isFilter; }
                set
                {
                    _isFilter = value;
                    _lanes.Clear();
                    foreach (Node n in Nodes.Values)
                    {
                        n.InLane = int.MaxValue;
                    }

                    foreach (Junction j in _junctions)
                    {
                        j.CurrentState = Junction.State.Unprocessed;
                    }

                    // We need to signal the DvcsGraph object that it needs to
                    // redraw everything.
                    Updated?.Invoke(this);
                }
            }

            public int Count
            {
                get
                {
                    if (IsFilter)
                    {
                        return _filterNodeCount;
                    }

                    return _nodeCount;
                }
            }

            public ILaneRow this[int col] => _lanes[col];

            public int CachedCount => _lanes.CachedCount;

            public void Filter(string id)
            {
                Node node = Nodes[id];

                if (!node.IsFiltered)
                {
                    _filterNodeCount++;
                    node.IsFiltered = true;
                }

                // Clear the filtered lane data.
                // TODO: We could be smart and only clear items after Node[id]. The check
                // below isn't valid, since it could be either the filtered or unfiltered
                // lane...
                ////if (node.InLane != int.MaxValue)
                ////{
                ////   filteredLanes.Clear();
                ////}
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

            public void HighlightBranch(string id)
            {
                ClearHighlightBranch();
                HighlightBranchRecursive(id);
            }

            public bool IsRevisionRelative(string guid)
            {
                if (Nodes.TryGetValue(guid, out var startNode))
                {
                    return startNode.Ancestors.Any(a => a.IsRelative);
                }

                return false;
            }

            public void HighlightBranchRecursive(string id)
            {
                if (Nodes.TryGetValue(id, out var startNode))
                {
                    foreach (Junction junction in startNode.Ancestors)
                    {
                        if (junction.HighLight)
                        {
                            continue;
                        }

                        junction.HighLight = true;

                        HighlightBranchRecursive(junction.Oldest.Id);
                    }
                }
            }

            public event GraphUpdatedHandler Updated;

            public void Add(string id, string[] parentIds, DataType type, GitRevision data)
            {
                // If we haven't seen this node yet, create a new junction.
                if (!GetNode(id, out var node) && (parentIds == null || parentIds.Length == 0))
                {
                    var newJunction = new Junction(node, node);
                    _junctions.Add(newJunction);
                }

                _nodeCount++;
                node.Data = data;
                node.DataType = type;
                node.Index = AddedNodes.Count;
                AddedNodes.Add(node);

                foreach (string parentId in parentIds)
                {
                    GetNode(parentId, out var parent);

                    if (parent.Index < node.Index)
                    {
                        // TODO: We might be able to recover from this with some work, but
                        // since we build the graph async it might be tough to figure out.
                        Debug.WriteLine("The nodes must be added such that all children are added before their parents");
                        continue;
                    }

                    if (node.Descendants.Count == 1 && node.Ancestors.Count <= 1
                        && node.Descendants[0].Oldest == node
                        && parent.Ancestors.Count == 0

                        // If this is true, the current revision is in the middle of a branch
                        // and is about to start a new branch. This will also mean that the last
                        // revisions are non-relative. Make sure a new junction is added and this
                        // is the start of a new branch (and color!)
                        && (type & DataType.Active) != DataType.Active)
                    {
                        // The node isn't a junction point. Just the parent to the node's
                        // (only) ancestor junction.
                        node.Descendants[0].Add(parent);
                    }
                    else if (node.Ancestors.Count == 1 && node.Ancestors[0].Youngest != node)
                    {
                        // The node is in the middle of a junction. We need to split it.
                        Junction splitNode = node.Ancestors[0].Split(node);
                        _junctions.Add(splitNode);

                        // The node is a junction point. We are a new junction
                        var junction = new Junction(node, parent);
                        _junctions.Add(junction);
                    }
                    else if (parent.Descendants.Count == 1 && parent.Descendants[0].Oldest != parent)
                    {
                        // The parent is in the middle of a junction. We need to split it.
                        Junction splitNode = parent.Descendants[0].Split(parent);
                        _junctions.Add(splitNode);

                        // The node is a junction point. We are a new junction
                        var junction = new Junction(node, parent);
                        _junctions.Add(junction);
                    }
                    else
                    {
                        // The node is a junction point. We are a new junction
                        var junction = new Junction(node, parent);
                        _junctions.Add(junction);
                    }
                }

                bool isRelative = (type & DataType.Active) == DataType.Active;
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
                        int resetTo = d.Oldest.Descendants.Aggregate(d.Oldest.InLane, (current, dd) => Math.Min(current, dd.Youngest.InLane));
                        Debug.WriteLine("We have to start over at lane {0} because of {1}", resetTo, node);
                        isRebuild = true;
                        break;
                    }
                }

                if (isRebuild)
                {
                    // TODO: It would be nice if we didn't have to start completely over...but it wouldn't
                    // be easy since we don't keep around all of the necessary lane state for each step.
                    int lastLane = _lanes.Count - 1;
                    _lanes.Clear();
                    _lanes.CacheTo(lastLane);

                    // We need to signal the DvcsGraph object that it needs to redraw everything.
                    Updated?.Invoke(this);
                }
                else
                {
                    _lanes.Update(node);
                }
            }

            public void Clear()
            {
                AddedNodes.Clear();
                _junctions.Clear();
                Nodes.Clear();
                _lanes.Clear();
                _nodeCount = 0;
                _filterNodeCount = 0;
            }

            public void ProcessNode(Node node)
            {
                if (_isFilter)
                {
                    return;
                }

                for (int i = _processedNodes; i < AddedNodes.Count; i++)
                {
                    if (AddedNodes[i] == node)
                    {
                        bool isChanged = false;
                        while (i > _processedNodes)
                        {
                            // This only happens if we weren't in topo order
                            if (Debugger.IsAttached)
                            {
                                Debugger.Break();
                            }

                            Node temp = AddedNodes[i];
                            AddedNodes[i] = AddedNodes[i - 1];
                            AddedNodes[i - 1] = temp;
                            i--;
                            isChanged = true;
                        }

                        // Signal that these rows have changed
                        if (isChanged)
                        {
                            Updated?.Invoke(this);
                        }

                        _processedNodes++;
                        break;
                    }
                }
            }

            public void Prune()
            {
                Node[] nodesToRemove = Nodes.Values.Where(n => n.Data == null).ToArray();

                // Remove all nodes that don't have a value associated with them.
                foreach (Node n in nodesToRemove)
                {
                    Nodes.Remove(n.Id);

                    // This guy should have been at the end of some junctions
                    foreach (Junction j in n.Descendants)
                    {
                        j.Remove(n);
                    }
                }
            }

            public IEnumerable<Node> GetRefs()
            {
                var nodes = new List<Node>();
                foreach (Junction j in _junctions)
                {
                    if (j.Youngest.Descendants.Count == 0 && !nodes.Contains(j.Youngest))
                    {
                        nodes.Add(j.Youngest);
                    }
                }

                return nodes;
            }

            public bool CacheTo(int idx)
            {
                return _lanes.CacheTo(idx);
            }

#if false
            // TopoSorting is an easy way to detect if something has gone wrong with the graph

            public Node[] TopoSortedNodes()
            {
                // http://en.wikipedia.org/wiki/Topological_ordering
                // L ? Empty list that will contain the sorted nodes
                // S ? Set of all nodes with no incoming edges

                // function visit(node n)
                //    if n has not been visited yet then
                //        mark n as visited
                //        for each node m with an edge from n to m do
                //            visit(m)
                //        add n to L

                // for each node n in S do
                //    visit(n)

                var l = new Queue<Node>();
                var s = new Queue<Node>();
                var p = new Queue<Node>();
                foreach (Node h in GetRefs())
                {
                    foreach (Junction aj in h.Ancestors)
                    {
                        if (!s.Contains(aj.Oldest))
                        {
                            s.Enqueue(aj.Oldest);
                        }

                        if (!s.Contains(aj.Youngest))
                        {
                            s.Enqueue(aj.Youngest);
                        }
                    }
                }

                Visit visit = null;
                Visit localVisit = visit;
                visit = (Node n) =>
                {
                    if (!p.Contains(n))
                    {
                        p.Enqueue(n);
                        foreach (Junction e in n.Ancestors)
                        {
                            if (localVisit != null)
                            {
                                localVisit(e.Oldest);
                            }
                        }

                        l.Enqueue(n);
                        return true;
                    }

                    return false;
                };
                foreach (Node n in s)
                {
                    visit(n);
                }

                // Sanity check
                var j = new Queue<Junction>();
                var x = new Queue<Node>();
                foreach (Node n in l)
                {
                    foreach (Junction e in n.Descendants)
                    {
                        if (x.Contains(e.Youngest))
                        {
                            Debugger.Break();
                        }

                        if (!j.Contains(e))
                        {
                            j.Enqueue(e);
                        }
                    }

                    x.Enqueue(n);
                }

                if (j.Count != _junctions.Count)
                {
                    foreach (var junction in _junctions)
                    {
                        if (!j.Contains(junction))
                        {
                            if (junction.Oldest != junction.Youngest)
                            {
                                Debug.WriteLine("*** {0} *** {1} {2}", junction, Nodes.Count, _junctions.Count);
                            }
                        }
                    }
                }

                return l.ToArray();
            }
#endif

            private bool GetNode(string id, out Node node)
            {
                if (!Nodes.TryGetValue(id, out node))
                {
                    node = new Node(id);
                    Nodes.Add(id, node);
                    return false;
                }

                return true;
            }

            #region Nested type: LaneInfo

            public struct LaneInfo
            {
                private List<Junction> _junctions;

                public LaneInfo(int connectLane, Junction junction)
                {
                    ConnectLane = connectLane;
                    _junctions = new List<Junction>(1) { junction };
                }

                public int ConnectLane { get; set; }

                public IEnumerable<Junction> Junctions => _junctions;

                public LaneInfo Clone()
                {
                    var other = new LaneInfo { ConnectLane = ConnectLane, _junctions = new List<Junction>(_junctions) };
                    return other;
                }

                public void UnionWith(LaneInfo other)
                {
                    foreach (Junction otherJunction in other._junctions)
                    {
                        if (!_junctions.Contains(otherJunction))
                        {
                            _junctions.Add(otherJunction);
                        }
                    }

                    _junctions.TrimExcess();
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
