﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GitCommands;
using JetBrains.Annotations;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal sealed class GraphModel
    {
        public event Action Updated;

        private readonly List<Junction> _junctions = new List<Junction>();
        private readonly Lanes _lanes;
        private int _processedNodes;

        public List<Node> AddedNodes { get; } = new List<Node>();
        public Dictionary<string, Node> Nodes { get; } = new Dictionary<string, Node>();
        public int Count { get; private set; }

        public GraphModel()
        {
            _lanes = new Lanes(this);
        }

        [CanBeNull]
        public ILaneRow this[int row] => _lanes[row];

        public int CachedCount => _lanes.CachedCount;

        public void HighlightBranch(string startId)
        {
            ClearHighlights();
            WalkBranchAndHighlightReachableNodes();
            return;

            void ClearHighlights()
            {
                foreach (var junction in _junctions)
                {
                    junction.IsHighlighted = false;
                }
            }

            void WalkBranchAndHighlightReachableNodes()
            {
                var stack = new Stack<string>();
                stack.Push(startId);

                while (stack.Count != 0)
                {
                    var id = stack.Pop();

                    if (!Nodes.TryGetValue(id, out var node))
                    {
                        continue;
                    }

                    foreach (var junction in node.Ancestors)
                    {
                        if (!junction.IsHighlighted)
                        {
                            junction.IsHighlighted = true;

                            stack.Push(junction.Oldest.ObjectId);
                        }
                    }
                }
            }
        }

        public bool IsRevisionRelative(string guid)
        {
            return Nodes.TryGetValue(guid, out var startNode)
                   && startNode.Ancestors.Any(a => a.IsRelative);
        }

        public void Add(GitRevision revision, RevisionNodeFlags flags)
        {
            var parentIds = revision.ParentGuids;

            // If we haven't seen this node yet, create a new junction.
            if (!GetOrCreateNode(revision.Guid, out var node) && (parentIds == null || parentIds.Count == 0))
            {
                _junctions.Add(new Junction(node));
            }

            Count++;
            node.Data = revision;
            node.Flags = flags;
            node.Index = AddedNodes.Count;
            AddedNodes.Add(node);

            var isCheckedOut = flags.HasFlag(RevisionNodeFlags.CheckedOut);

            foreach (var parentId in parentIds ?? Array.Empty<string>())
            {
                GetOrCreateNode(parentId, out var parent);

                if (parent.Index < node.Index)
                {
                    // TODO: We might be able to recover from this with some work, but
                    // since we build the graph async it might be tough to figure out.
                    Debug.WriteLine("The nodes must be added such that all children are added before their parents");
                    continue;
                }

                if (node.Descendants.Count == 1 &&
                    node.Ancestors.Count <= 1 &&
                    node.Descendants[0].Oldest == node &&
                    parent.Ancestors.Count == 0 &&

                    // If this is true, the current revision is in the middle of a branch
                    // and is about to start a new branch. This will also mean that the last
                    // revisions are non-relative. Make sure a new junction is added and this
                    // is the start of a new branch (and color!)
                    !isCheckedOut)
                {
                    // The node isn't a junction point. Just the parent to the node's
                    // (only) ancestor junction.
                    node.Descendants[0].Add(parent);
                }
                else if (node.Ancestors.Count == 1 && node.Ancestors[0].Youngest != node)
                {
                    // The node is in the middle of a junction. We need to split it.
                    _junctions.Add(node.Ancestors[0].SplitIntoJunctionWith(node));

                    // The node is a junction point. We are a new junction
                    _junctions.Add(new Junction(node, parent));
                }
                else if (parent.Descendants.Count == 1 && parent.Descendants[0].Oldest != parent)
                {
                    // The parent is in the middle of a junction. We need to split it.
                    _junctions.Add(parent.Descendants[0].SplitIntoJunctionWith(parent));

                    // The node is a junction point. We are a new junction
                    _junctions.Add(new Junction(node, parent));
                }
                else
                {
                    // The node is a junction point. We are a new junction
                    _junctions.Add(new Junction(node, parent));
                }
            }

            var isRelative = isCheckedOut || node.Descendants.Any(d => d.IsRelative);

            var needsRebuild = false;

            foreach (var ancestor in node.Ancestors)
            {
                ancestor.IsRelative = isRelative || ancestor.IsRelative;

                // Uh, oh, we've already processed this lane. We'll have to update some rows.
                var parent = ancestor.TryGetParent(node);

                if (parent != null && parent.InLane != int.MaxValue)
                {
                    Debug.WriteLine("We have to start over at lane {0} because of {1}",
                        ancestor.Oldest.Descendants.Aggregate(ancestor.Oldest.InLane, (current, dd) => Math.Min(current, dd.Youngest.InLane)),
                        node);

                    needsRebuild = true;
                    break;
                }
            }

            if (needsRebuild)
            {
                // TODO: It would be nice if we didn't have to start completely over...but it wouldn't
                // be easy since we don't keep around all of the necessary lane state for each step.
                var lastLaneIndex = _lanes.Count - 1;

                _lanes.Clear();
                _lanes.CacheTo(lastLaneIndex);

                // We need to redraw everything
                Updated?.Invoke();
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
            Count = 0;
        }

        public void ProcessNode(Node node)
        {
            for (int i = _processedNodes; i < AddedNodes.Count; i++)
            {
                if (AddedNodes[i] != node)
                {
                    continue;
                }

                bool isChanged = false;

                while (i > _processedNodes)
                {
                    // This only happens if we weren't in topo order
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }

                    AddedNodes.Swap(i - 1, i);
                    i--;
                    isChanged = true;
                }

                // Signal that these rows have changed
                if (isChanged)
                {
                    Updated?.Invoke();
                }

                _processedNodes++;
                break;
            }
        }

        public void Prune()
        {
            var nodesToRemove = Nodes.Values.Where(n => n.Data == null).ToList();

            // Remove all nodes that don't have a value associated with them.
            foreach (var node in nodesToRemove)
            {
                Nodes.Remove(node.ObjectId);

                // This guy should have been at the end of some junctions
                foreach (Junction j in node.Descendants)
                {
                    j.Remove(node);
                }
            }
        }

        public IReadOnlyList<Node> GetRefs()
        {
            var nodes = new List<Node>(capacity: _junctions.Count);

            foreach (var junction in _junctions)
            {
                if (junction.Youngest.Descendants.Count == 0 && !nodes.Contains(junction.Youngest))
                {
                    nodes.Add(junction.Youngest);
                }
            }

            return nodes;
        }

        public bool CacheTo(int idx)
        {
            return _lanes.CacheTo(idx);
        }

        private bool GetOrCreateNode(string objectId, [NotNull] out Node node)
        {
            if (!Nodes.TryGetValue(objectId, out node))
            {
                node = new Node(objectId);
                Nodes.Add(objectId, node);
                return false;
            }

            return true;
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
                foreach (var refNode in GetRefs())
                {
                    foreach (var junction in refNode.Ancestors)
                    {
                        if (!s.Contains(junction.Oldest))
                        {
                            s.Enqueue(junction.Oldest);
                        }

                        if (!s.Contains(junction.Youngest))
                        {
                            s.Enqueue(junction.Youngest);
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
    }
}
