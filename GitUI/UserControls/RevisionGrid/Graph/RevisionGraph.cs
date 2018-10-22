using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    public class RevisionGraph
    {
        // Some unordered collections with raw data
        private ConcurrentDictionary<ObjectId, RevisionGraphRevision> _nodeByObjectId = new ConcurrentDictionary<ObjectId, RevisionGraphRevision>();
        private ConcurrentBag<RevisionGraphRevision> _nodes = new ConcurrentBag<RevisionGraphRevision>();
        private ConcurrentBag<RevisionGraphSegment> _segments = new ConcurrentBag<RevisionGraphSegment>();

        // maxscore is used during graph buiding. It is cheaper than doing .Max(.score)
        private int _maxScore = 0;

        // Some properties to hold the cached row data. The nodecache is an ordered list with the nodes. This is used to be able to draw commits before the graph is completed.
        // The orderedrowcache contains the rows with the segments stored in lanes.
        private bool _reorder = true;
        private List<RevisionGraphRevision> _orderedNodesCache = null;
        private List<RevisionGraphRow> _orderedRowCache = null;
        private int _cachedUntillScore = -1;

        // When the cache is updated, this action can be used to invalidate the UI
        public event Action Updated;

        public void Clear()
        {
            _maxScore = 0;
            _nodeByObjectId = new ConcurrentDictionary<ObjectId, RevisionGraphRevision>();
            _nodes = new ConcurrentBag<RevisionGraphRevision>();
            _segments = new ConcurrentBag<RevisionGraphSegment>();
            _orderedNodesCache = null;
            _orderedRowCache = null;
        }

        public int Count => _nodes.Count;

        public int GetCachedCount()
        {
            if (_orderedRowCache == null)
            {
                return 0;
            }

            return _orderedRowCache.Count;
        }

        public void CacheTo(int untillRow, int graphUntillRow)
        {
            if (_orderedNodesCache == null || _reorder || _orderedNodesCache.Count <= untillRow)
            {
                _orderedNodesCache = _nodes.Where(n => n.GitRevision != null).OrderBy(n => n.Score).ToList();
                if (_orderedNodesCache.Count > 0)
                {
                    _cachedUntillScore = _nodes.Last().Score;
                }

                if (_reorder || _orderedRowCache == null)
                {
                    _orderedRowCache = new List<RevisionGraphRow>(untillRow);
                }

                _reorder = false;
                Updated?.Invoke();
            }

            int nextIndex = _orderedRowCache.Count;
            if (nextIndex <= graphUntillRow)
            {
                int cacheCount = _orderedNodesCache.Count;
                while (nextIndex <= graphUntillRow && cacheCount > nextIndex)
                {
                    RevisionGraphRow revisionGraphRow = new RevisionGraphRow(_orderedNodesCache[nextIndex]);

                    if (nextIndex > 0)
                    {
                        // Copy lanes from last row
                        RevisionGraphRow previousRevisionGraphRow = _orderedRowCache[nextIndex - 1];
                        foreach (var segment in previousRevisionGraphRow.Segments)
                        {
                            // This segment ends here
                            if (!previousRevisionGraphRow.Revision.EndSegments.Any(s => s == segment))
                            {
                                revisionGraphRow.Segments.Add(segment);

                                foreach (var startSegment in revisionGraphRow.Revision.StartSegments)
                                {
                                    // This segments continues in the next row
                                    if (startSegment.Child == segment.Parent)
                                    {
                                        if (!revisionGraphRow.Segments.Contains(startSegment))
                                        {
                                            revisionGraphRow.Segments.Add(startSegment);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Add new segments started by this revision to the end
                    foreach (var segment in revisionGraphRow.Revision.StartSegments)
                    {
                        if (!revisionGraphRow.Segments.Contains(segment))
                        {
                            revisionGraphRow.Segments.Add(segment);
                        }
                    }

                    _orderedRowCache.Add(revisionGraphRow);
                    nextIndex++;
                }

                Updated?.Invoke();
            }
        }

        public bool IsRowRelative(int row)
        {
            if (_orderedNodesCache == null || _orderedNodesCache.Count < row)
            {
                return false;
            }

            return _orderedNodesCache[row].IsRelative;
        }

        public bool IsRevisionRelative(ObjectId objectId)
        {
            if (_nodeByObjectId.TryGetValue(objectId, out RevisionGraphRevision revision))
            {
                return revision.IsRelative;
            }

            return false;
        }

        public bool TryGetNode(ObjectId objectId, out RevisionGraphRevision revision)
        {
            return _nodeByObjectId.TryGetValue(objectId, out revision);
        }

        public bool TryGetRowIndex(ObjectId objectId, out int index)
        {
            if (_orderedNodesCache == null || !TryGetNode(objectId, out RevisionGraphRevision revision))
            {
                index = 0;
                return false;
            }

            index = _orderedNodesCache.IndexOf(revision);
            return index >= 0;
        }

        public RevisionGraphRevision GetNodeForRow(int row)
        {
            if (_orderedNodesCache == null || row >= _orderedNodesCache.Count)
            {
                return null;
            }

            return _orderedNodesCache.ElementAt(row);
        }

        public RevisionGraphRow GetSegmentsForRow(int row)
        {
            if (_orderedRowCache == null || row >= _orderedRowCache.Count)
            {
                return null;
            }

            return _orderedRowCache[row];
        }

        public void Add(GitRevision revision, RevisionNodeFlags types)
        {
            if (!_nodeByObjectId.TryGetValue(revision.ObjectId, out RevisionGraphRevision revisionGraphRevision))
            {
                int score = _maxScore + 1;
                _maxScore = score;

                revisionGraphRevision = new RevisionGraphRevision(revision.ObjectId, score);
                revisionGraphRevision.ApplyFlags(types);
                revisionGraphRevision.LaneColor = _nodes.Count;

                revisionGraphRevision.GitRevision = revision;
                _nodeByObjectId.TryAdd(revision.ObjectId, revisionGraphRevision);
                _nodes.Add(revisionGraphRevision);
            }
            else
            {
                // This revision was added as a parent. Probably only the objectid is known.
                revisionGraphRevision.GitRevision = revision;
                revisionGraphRevision.ApplyFlags(types);
                revisionGraphRevision.IncreaseScore(_maxScore);
                _nodes.Add(revisionGraphRevision);
            }

            // Invalidate cache if the new score is lower then the cached result
            if (revisionGraphRevision.Score <= _cachedUntillScore)
            {
                _reorder = true;
            }

            foreach (ObjectId parentObjectId in revision.ParentIds)
            {
                if (!_nodeByObjectId.TryGetValue(parentObjectId, out RevisionGraphRevision parentRevisionGraphRevision))
                {
                    parentRevisionGraphRevision = new RevisionGraphRevision(parentObjectId, ++_maxScore);
                    _nodeByObjectId.TryAdd(parentObjectId, parentRevisionGraphRevision);

                    int newMaxScore;
                    _segments.Add(revisionGraphRevision.AddParent(parentRevisionGraphRevision, out newMaxScore));
                    _maxScore = Math.Max(_maxScore, newMaxScore);

                    // Invalidate cache if the new score is lower then the cached result
                    if (parentRevisionGraphRevision.Score <= _cachedUntillScore)
                    {
                        _reorder = true;
                    }
                }
                else
                {
                    // If the current score is lower, cache is invalid. The new score will (probably) be higher.
                    if (parentRevisionGraphRevision.Score <= _cachedUntillScore)
                    {
                        _reorder = true;
                    }

                    int newMaxScore;
                    _segments.Add(revisionGraphRevision.AddParent(parentRevisionGraphRevision, out newMaxScore));
                    _maxScore = Math.Max(_maxScore, newMaxScore);
                }
            }
        }
    }
}
