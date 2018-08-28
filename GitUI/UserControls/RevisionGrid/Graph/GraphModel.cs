using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal sealed class GraphModel
    {
        public event Action Updated;

        private readonly List<Junction> _junctions = new List<Junction>();
        private readonly ActiveLaneRow _currentRow = new ActiveLaneRow();
        private readonly List<LaneJunctionDetail> _laneNodes = new List<LaneJunctionDetail>();
        private readonly List<ILaneRow> _laneRows = new List<ILaneRow>();
        private readonly Dictionary<ObjectId, Node> _nodeByObjectId = new Dictionary<ObjectId, Node>();
        private readonly List<Node> _nodes = new List<Node>();

        private int _processedNodes;
        public int Count { get; private set; }

        [CanBeNull]
        public ILaneRow GetLaneRow(int row)
        {
            if (row < 0)
            {
                return null;
            }

            if (row < _laneRows.Count)
            {
                return _laneRows[row];
            }

            if (row < _nodes.Count)
            {
                #region refresh the cache and recalculate nodes' edges
                // Whenever we refresh we trigger clear sequence that clears the model and
                // all claculated information about rows edges.
                // Without this information we are unable to correctly render the graph...
                // The recalculation is typically done on a background thread by the RevisionDataGridView,
                // however here for us it is too late, and as a work around we must kick off the recalculation manually.
                // This feels dirty but it addresses a major regression https://github.com/gitextensions/gitextensions/issues/5167
                //
                // The method seems to be pretty snappy, when measured in Debug build with StopWatch the timings were
                // typically in vicinity of ns:
                //
                //  Loaded GitExtensions repo:
                //
                //     00:00:00.0005014 : 1
                //     00:00:00.0016211 : 6
                //     00:00:00.0000095 : 7
                //     00:00:00.0000079 : 8
                //     00:00:00.0000167 : 9
                //     00:00:00.0001133 : 10
                //     00:00:00.0000553 : 11
                //     00:00:00.0000402 : 12
                //     00:00:00.0000388 : 13
                //     00:00:00.0000362 : 14
                //
                // Fast scroll with mouse to an arbitrary commit:
                //
                //     00:00:00.0229141 : 1057
                //     00:00:00.0000355 : 1058
                //     00:00:00.0000332 : 1068
                //     00:00:00.0000330 : 1069
                //     00:00:00.0000320 : 1070
                //     00:00:00.0000328 : 1071
                //     ...snip few brewity...
                //     00:00:00.0632790 : 2959
                //     00:00:00.0001009 : 2960
                //     00:00:00.0000907 : 2961
                //     ...snip few brewity...
                //     00:00:00.0000684 : 2971
                //     00:00:00.0001053 : 2972
                //     00:00:00.0000941 : 2973
                //     00:00:00.2486322 : 8245
                //     00:00:00.0000229 : 8246
                //     ...snip few brewity...
                //     00:00:00.0000206 : 8258
                //     00:00:00.0000165 : 8259
                //     00:00:00.0108505 : 9038
                //     00:00:00.0000314 : 9039
                //     00:00:00.0000178 : 9040
                //     ...snip few brewity...
                //     00:00:00.0000215 : 9050
                //     00:00:00.0000211 : 9051
                //     00:00:00.0000180 : 9052
                //
                // Refresh at commit #9049:
                //
                //     00:00:00.0000180 : 0
                //     00:00:00.0000117 : 1
                //     00:00:00.0000827 : 2
                //     00:00:00.0000681 : 3
                //     00:00:00.0001763 : 4
                //     00:00:00.0000702 : 5
                //     00:00:00.0000783 : 8
                //     00:00:00.0000236 : 11
                //     00:00:00.0000189 : 12
                //     00:00:00.0000250 : 13
                //     00:00:00.0000112 : 14
                //     00:00:00.7471912 : 9048
                //     00:00:00.0000723 : 9049
                //
                //
                CacheTo(row);
                #endregion

                return new SavedLaneRow(_nodes[row]);
            }

            return null;
        }

        [ContractAnnotation("=>true,node:notnull")]
        [ContractAnnotation("=>false,node:null")]
        public bool TryGetNode(ObjectId objectId, out Node node) => _nodeByObjectId.TryGetValue(objectId, out node);

        public int CachedCount => _laneRows.Count;

        public void HighlightBranch(ObjectId startId)
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
                var stack = new Stack<ObjectId>();
                stack.Push(startId);

                while (stack.Count != 0)
                {
                    var id = stack.Pop();

                    if (!_nodeByObjectId.TryGetValue(id, out var node))
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

        public bool IsRevisionRelative(ObjectId objectId)
        {
            return _nodeByObjectId.TryGetValue(objectId, out var startNode)
                   && startNode.Ancestors.Any(a => a.IsRelative);
        }

        public void Add(GitRevision revision, RevisionNodeFlags flags)
        {
            var parentIds = revision.ParentIds;

            // If we haven't seen this node yet, create a new junction.
            // We process revisions in reverse order.
            // For each revision we create a node for the revision and its parents.
            // Most of the time we will have a node for this revision, created for
            // a parent of revision processed beforehand.
            if (!GetOrCreateNode(revision.ObjectId, out var node) && (parentIds == null || parentIds.Count == 0))
            {
                // The revision has not been seen yet -- it must be a leaf node.
                // Create a junction for it.
                _junctions.Add(new Junction(node));
            }

            Count++;
            node.Revision = revision;
            node.Flags = flags;
            node.Index = _nodes.Count;
            _nodes.Add(node);

            var isCheckedOut = flags.HasFlag(RevisionNodeFlags.CheckedOut);

            foreach (var parentId in parentIds ?? Array.Empty<ObjectId>())
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
                ancestor.IsRelative |= isRelative;

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
                var lastLaneIndex = Count - 1;

                ClearLanes();
                CacheTo(lastLaneIndex);

                // We need to redraw everything
                Updated?.Invoke();
            }
            else
            {
                Update(node);
            }

            bool GetOrCreateNode(ObjectId objectId, out Node n)
            {
                if (!_nodeByObjectId.TryGetValue(objectId, out n))
                {
                    n = new Node(objectId);
                    _nodeByObjectId.Add(objectId, n);
                    return false;
                }

                return true;
            }
        }

        public void Clear()
        {
            _nodes.Clear();
            _junctions.Clear();
            _nodeByObjectId.Clear();
            ClearLanes();

            Count = 0;
        }

        private void ClearLanes()
        {
            _laneRows.Clear();
            _laneNodes.Clear();
            _currentRow.Clear();

            foreach (var node in GetLeafNodes())
            {
                Update(node);
            }

            return;

            IEnumerable<Node> GetLeafNodes()
            {
                var nodes = new HashSet<Node>();

                foreach (var junction in _junctions)
                {
                    if (junction.Youngest.Descendants.Count == 0)
                    {
                        nodes.Add(junction.Youngest);
                    }
                }

                return nodes;
            }
        }

        public bool CacheTo(int row)
        {
            while (row >= CachedCount)
            {
                if (!MoveNext())
                {
                    return false;
                }
            }

            return true;
        }

        private void Update(Node node)
        {
            if (node.Descendants.Count != 0)
            {
                return;
            }

            // This node is a head, create a new lane for it
            Node head = node;
            if (head.Ancestors.Count != 0)
            {
                foreach (Junction j in head.Ancestors)
                {
                    _laneNodes.Add(new LaneJunctionDetail(j));
                }
            }
            else
            {
                // This is a single entry with no parents or children.
                _laneNodes.Add(new LaneJunctionDetail(head));
            }
        }

        private bool MoveNext()
        {
            // If there are no lanes, there is nothing more to draw
            if (_laneNodes.Count == 0 || Count <= _laneRows.Count)
            {
                return false;
            }

            // Find the new current row's node (newest item in the row)

            #region Find current node & index

            _currentRow.Node = null;
            for (int curLane = 0; curLane < _laneNodes.Count; curLane++)
            {
                LaneJunctionDetail lane = _laneNodes[curLane];
                if (lane.Count == 0)
                {
                    continue;
                }

                // NOTE: We could also compare with sourceGraph sourceGraph.AddedNodes[sourceGraph.processedNodes],
                // since it should always be the same value
                if (_currentRow.Node?.Revision == null ||
                    (lane.Current.Revision != null && lane.Current.Index < _currentRow.Node.Index))
                {
                    _currentRow.Node = lane.Current;
                    _currentRow.NodeLane = curLane;
                    ////break;
                }
            }

            if (_currentRow.Node == null)
            {
                // DEBUG: The check above didn't find anything, but should have
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                ////Node[] topo = this.sourceGraph.TopoSortedNodes();
                return false;
            }

            // If this row doesn't contain data, we're to the end of the valid entries.
            if (_currentRow.Node.Revision == null)
            {
                return false;
            }

            ProcessNode(_currentRow.Node);

            #endregion

            // Check for multiple junctions with this node at the top. Remove the
            // node from that junction as well. This will happen when there is a branch

            #region Check for branches

            _currentRow.Clear(_currentRow.NodeLane);
            for (int curLane = 0; curLane < _laneNodes.Count; curLane++)
            {
                LaneJunctionDetail lane = _laneNodes[curLane];
                if (lane.Count == 0)
                {
                    continue;
                }

                if (_currentRow.Node != lane.Current)
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
            if (_currentRow.Count < 10)
            {
                for (int lane = 0; lane < _currentRow.Count; lane++)
                {
                    for (int item = 0; item < _currentRow.LaneInfoCount(lane); item++)
                    {
                        LaneInfo laneInfo = _currentRow[lane, item];
                        if (laneInfo.ConnectLane <= lane)
                        {
                            continue;
                        }

                        // Lane is moving to the right, check to see if it intersects
                        // with any lanes moving to the left.
                        for (int otherLane = lane + 1; otherLane <= laneInfo.ConnectLane; otherLane++)
                        {
                            if (_currentRow.LaneInfoCount(otherLane) != 1)
                            {
                                continue;
                            }

                            LaneInfo otherLaneInfo = _currentRow[otherLane, 0];
                            if (otherLaneInfo.ConnectLane < otherLane)
                            {
                                _currentRow.Swap(otherLaneInfo.ConnectLane, otherLane);
                                LaneJunctionDetail temp = _laneNodes[otherLane];
                                _laneNodes[otherLane] = _laneNodes[otherLaneInfo.ConnectLane];
                                _laneNodes[otherLaneInfo.ConnectLane] = temp;
                            }
                        }
                    }
                }
            }

            // Keep the merge lanes next to each other
            ////int mergeFromCount = currentRow.LaneInfoCount(currentRow.NodeLane);
            ////if (mergeFromCount > 1)
            ////{
            ////   for (int i = 0; i < mergeFromCount; i++)
            ////   {
            ////       Graph.LaneInfo laneInfo = currentRow[currentRow.NodeLane, i];
            ////       // Check to see if the lane is currently next to us
            ////       if (laneInfo.ConnectLane - currentRow.NodeLane > mergeFromCount)
            ////       {
            ////           // Only move the lane if it isn't already being drawn.
            ////           if (currentRow.LaneInfoCount(laneInfo.ConnectLane) == 0)
            ////           {
            ////               // Remove the row laneInfo.ConnectLane and insert
            ////               // it at currentRow.NodeLane+1.
            ////               // Then start over searching for others if i != mergeFromCount-1?
            ////               int adjacentLane = currentRow.NodeLane + 1;
            ////               if (adjacentLane >= laneNodes.Count) Debugger.Break();
            ////               currentRow.Expand(adjacentLane);
            ////               currentRow.Replace(laneInfo.ConnectLane + 1, adjacentLane);
            ////
            ////               LaneJunctionDetail temp = laneNodes[laneInfo.ConnectLane];
            ////               laneNodes.RemoveAt(laneInfo.ConnectLane);
            ////               laneNodes.Insert(adjacentLane, temp);
            ////           }
            ////       }
            ////   }
            ////}

            #endregion

            if (_currentRow.Node != null)
            {
                ILaneRow row = _currentRow.Advance();

                // This means there is a node that got put in the graph twice...
                if (row.Node.InLane != int.MaxValue)
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }

                row.Node.InLane = _laneRows.Count;
                _laneRows.Add(row);
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
            LaneJunctionDetail lane = _laneNodes[curLane];
            int minLane = curLane;

            // Advance the lane
            lane.Next();

            // See if we can pull up ancestors
            if (lane.Count == 0 && lane.Junction == null)
            {
                // Handle a single node branch.
                _currentRow.Collapse(curLane);
                _laneNodes.RemoveAt(curLane);
            }
            else if (lane.Count == 0)
            {
                Node node = lane.Junction.Oldest;
                foreach (Junction parent in node.Ancestors)
                {
                    if (parent.ProcessingState != JunctionProcessingState.Unprocessed)
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
                        for (int i = 0; i < _laneNodes.Count; i++)
                        {
                            if (_laneNodes[i].Current == addedLane.Current)
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
                            _laneNodes[curLane] = lane;
                            addedLaneLane = curLane;
                        }
                        else
                        {
                            addedLaneLane = curLane + 1;
                            _laneNodes.Insert(addedLaneLane, addedLane);
                            _currentRow.Expand(addedLaneLane);
                        }
                    }

                    _currentRow.Add(curLane, new LaneInfo(addedLaneLane, parent));
                }

                // If the lane count after processing is still 0
                // this is a root node of the graph
                if (lane.Count == 0)
                {
                    _currentRow.Collapse(curLane);
                    _laneNodes.RemoveAt(curLane);
                }
            }
            else if (lane.Count == 1)
            {
                // If any other lanes have this node on top, merge them together
                for (int i = 0; i < _laneNodes.Count; i++)
                {
                    if (i == curLane || curLane >= _laneNodes.Count)
                    {
                        continue;
                    }

                    if (_laneNodes[i].Current == _laneNodes[curLane].Current)
                    {
                        int left;
                        int right;
                        Junction junction = _laneNodes[curLane].Junction;
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

                        _currentRow.Replace(right, left);
                        _currentRow.Collapse(right);
                        _laneNodes[right].Clear();
                        _laneNodes.RemoveAt(right);

                        _currentRow.Add(_currentRow.NodeLane, new LaneInfo(left, junction));
                        minLane = Math.Min(minLane, left);
                    }
                }

                // If the current lane is still active, add it. It might not be active
                // if it got merged above.
                if (!lane.IsClear)
                {
                    _currentRow.Add(_currentRow.NodeLane, new LaneInfo(curLane, lane.Junction));
                }
            }
            else
            {
                // lane.Count > 1
                _currentRow.Add(_currentRow.NodeLane, new LaneInfo(curLane, lane.Junction));
            }

            return curLane;
        }

        private void ProcessNode(Node node)
        {
            for (int i = _processedNodes; i < _nodes.Count; i++)
            {
                if (_nodes[i] != node)
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

                    _nodes.Swap(i - 1, i);
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
            var nodesToRemove = _nodeByObjectId.Values.Where(n => n.Revision == null).ToList();

            // Remove all nodes that don't have a value associated with them.
            foreach (var node in nodesToRemove)
            {
                _nodeByObjectId.Remove(node.ObjectId);

                // This guy should have been at the end of some junctions
                foreach (var descendant in node.Descendants)
                {
                    descendant.Remove(node);
                }
            }
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
