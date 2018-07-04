using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal sealed class Lanes
    {
        private readonly ActiveLaneRow _currentRow = new ActiveLaneRow();
        private readonly List<LaneJunctionDetail> _laneNodes = new List<LaneJunctionDetail>();
        private readonly List<ILaneRow> _laneRows = new List<ILaneRow>();
        private readonly GraphModel _sourceGraph;

        public Lanes(GraphModel graph)
        {
            _sourceGraph = graph;
        }

        [CanBeNull]
        public ILaneRow this[int row]
        {
            get
            {
                if (row < 0)
                {
                    return null;
                }

                if (row < _laneRows.Count)
                {
                    return _laneRows[row];
                }

                if (row < _sourceGraph.AddedNodes.Count)
                {
                    return new SavedLaneRow(_sourceGraph.AddedNodes[row]);
                }

                return null;
            }
        }

        public int Count => _sourceGraph.Count;

        public int CachedCount => _laneRows.Count;

        public void Clear()
        {
            _laneRows.Clear();
            _laneNodes.Clear();
            _currentRow.Clear();

            foreach (Node node in _sourceGraph.GetRefs())
            {
                Update(node);
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

        public void Update(Node node)
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
            if (_laneNodes.Count == 0 || _sourceGraph.Count <= _laneRows.Count)
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

            _sourceGraph.ProcessNode(_currentRow.Node);

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
                    if (parent.State != JunctionState.Unprocessed)
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
    }
}
