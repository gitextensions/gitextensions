using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    public class RevisionGraphSegment
    {
        public RevisionGraphSegment(RevisionGraphRevision parent, RevisionGraphRevision child)
        {
            Parent = parent;
            Child = child;
        }

        public int StartScore
        {
            get
            {
                return Child.Score;
            }
        }

        public int EndScore
        {
            get
            {
                return Parent.Score;
            }
        }

        public RevisionGraphRevision Parent { get; private set; }
        public RevisionGraphRevision Child { get; private set; }
    }

    public class RevisionGraphRevision
    {
        public RevisionGraphRevision(ObjectId objectId, int guessScore)
        {
            Objectid = objectId;

            Parents = new ConcurrentBag<RevisionGraphRevision>();
            Children = new ConcurrentBag<RevisionGraphRevision>();
            StartSegments = new ConcurrentBag<RevisionGraphSegment>();
            EndSegments = new ConcurrentBag<RevisionGraphSegment>();

            Score = guessScore;
        }

        public void ApplyFlags(RevisionNodeFlags types)
        {
            IsRelative |= (types & RevisionNodeFlags.CheckedOut) != 0;
            HasRef = (types & RevisionNodeFlags.HasRef) != 0;
            IsCheckedOut = (types & RevisionNodeFlags.CheckedOut) != 0;
        }

        public bool IsRelative { get; set; }
        public bool HasRef { get; set; }
        public bool IsCheckedOut { get; set; }

        public int Score { get; private set; }

        public int LaneIndex { get; set; }

        private int IncreaseScore(int delta)
        {
            int maxScore = Score;
            if (delta + 1 > Score)
            {
                Score = Math.Max(delta + 1, Score);

                foreach (RevisionGraphRevision parent in Parents)
                {
                    maxScore = Math.Max(parent.IncreaseScore(Score), maxScore);
                }
            }

            return maxScore;
        }

        public GitRevision GitRevision { get; set; }

        public ObjectId Objectid { get; set; }

        public ConcurrentBag<RevisionGraphRevision> Parents { get; private set; }
        public ConcurrentBag<RevisionGraphRevision> Children { get; private set; }
        public ConcurrentBag<RevisionGraphSegment> StartSegments { get; private set; }
        public ConcurrentBag<RevisionGraphSegment> EndSegments { get; private set; }

        public void MakeRelative()
        {
            if (!IsRelative)
            {
                IsRelative = true;

                foreach (RevisionGraphRevision parent in Parents)
                {
                    parent.MakeRelative();
                }
            }
        }

        public RevisionGraphSegment AddParent(RevisionGraphRevision parent, out int maxScore)
        {
            if (Parents.Any())
            {
                parent.LaneIndex = Parents.Max(p => p.LaneIndex) + 1;
            }
            else
            {
                parent.LaneIndex = LaneIndex;
            }

            if (IsRelative)
            {
                parent.MakeRelative();
            }

            Parents.Add(parent);
            parent.AddChild(this);
            maxScore = parent.IncreaseScore(Score);

            RevisionGraphSegment revisionGraphSegment = new RevisionGraphSegment(parent, this);
            parent.EndSegments.Add(revisionGraphSegment);
            StartSegments.Add(revisionGraphSegment);

            return revisionGraphSegment;
        }

        private void AddChild(RevisionGraphRevision child)
        {
            Children.Add(child);
        }

        public override string ToString()
        {
            return Score + " -- " + GitRevision?.ToString();
        }
    }

    public class RevisionGraphRow
    {
        public RevisionGraphRow(RevisionGraphRevision revision)
        {
            Segments = new SynchronizedCollection<RevisionGraphSegment>();
            Revision = revision;
        }

        public RevisionGraphRevision Revision { get; private set; }
        public SynchronizedCollection<RevisionGraphSegment> Segments { get; private set; }

        private Dictionary<RevisionGraphSegment, int> _segmentLanes;

        private int _laneCount;

        private void BuildSegmentLanes()
        {
            int currentRevisionLane = -1;
            if (_segmentLanes == null)
            {
                _segmentLanes = new Dictionary<RevisionGraphSegment, int>();

                int laneIndex = 0;
                foreach (var segment in Segments.OrderByDescending(s => s.Child.LaneIndex))
                {
                    if (segment.Child == Revision || segment.Parent == Revision)
                    {
                        if (currentRevisionLane < 0)
                        {
                            currentRevisionLane = laneIndex;
                            laneIndex++;
                        }

                        _segmentLanes.Add(segment, currentRevisionLane);
                    }
                    else
                    {
                        bool added = false;
                        foreach (var searchParent in _segmentLanes)
                        {
                            if (searchParent.Key.Parent == segment.Parent)
                            {
                                _segmentLanes.Add(segment, searchParent.Value);
                                added = true;
                                break;
                            }
                        }

                        if (!added)
                        {
                            _segmentLanes.Add(segment, laneIndex);
                            laneIndex++;
                        }
                    }
                }

                _laneCount = laneIndex;
            }
        }

        public int GetLaneCount()
        {
            BuildSegmentLanes();
            return _laneCount;
        }

        public int GetLaneIndexForSegment(RevisionGraphSegment revisionGraphRevision)
        {
            BuildSegmentLanes();
            if (_segmentLanes.TryGetValue(revisionGraphRevision, out int index))
            {
                return index;
            }

            return -1;
        }
    }

    public class RevisionGraph
    {
        private ConcurrentDictionary<ObjectId, RevisionGraphRevision> _nodeByObjectId = new ConcurrentDictionary<ObjectId, RevisionGraphRevision>();
        private ConcurrentBag<RevisionGraphRevision> _nodes = new ConcurrentBag<RevisionGraphRevision>();
        private ConcurrentBag<RevisionGraphSegment> _segments = new ConcurrentBag<RevisionGraphSegment>();

        private bool _reorder = true;
        private List<RevisionGraphRevision> _orderedNodesCache = null;
        private List<RevisionGraphRow> _orderedRowCache = null;
        private int _cachedUntillScore = -1;

        public event Action Updated;

        public void Clear()
        {
            _nodeByObjectId = new ConcurrentDictionary<ObjectId, RevisionGraphRevision>();
            _nodes = new ConcurrentBag<RevisionGraphRevision>();
            _segments = new ConcurrentBag<RevisionGraphSegment>();
            _orderedNodesCache = null;
            _orderedRowCache = null;
        }

        public int Count
        {
            get
            {
                return _nodes.Count;
            }
        }

        public int CachedCount
        {
            get
            {
                if (_orderedRowCache == null)
                {
                    return 0;
                }

                return _orderedRowCache.Count;
            }
        }

        public void CacheTo(int untillRow)
        {
            if (_orderedNodesCache == null || _reorder || _orderedNodesCache.Count <= untillRow)
            {
                _orderedNodesCache = _nodes.OrderBy(n => n.Score).ToList();
                if (_orderedNodesCache.Count > 0)
                {
                    _cachedUntillScore = _nodes.Last().Score;
                }

                _orderedRowCache = new List<RevisionGraphRow>();
                _reorder = false;
            }

            int nextIndex = _orderedRowCache.Count;
            if (nextIndex <= untillRow)
            {
                int cacheCount = _orderedNodesCache.Count;
                while (nextIndex <= untillRow + 50 && cacheCount > nextIndex)
                {
                    RevisionGraphRow revisionGraphRow = new RevisionGraphRow(_orderedNodesCache[nextIndex]);

                    if (nextIndex > 0)
                    {
                        // Copy lanes from last row
                        RevisionGraphRow previousRevisionGraphRow = _orderedRowCache[nextIndex - 1];
                        foreach (var segment in previousRevisionGraphRow.Segments)
                        {
                            if (!previousRevisionGraphRow.Revision.EndSegments.Any(s => s == segment))
                            {
                                revisionGraphRow.Segments.Add(segment);
                            }
                        }
                    }

                    // Add new segments started by this revision
                    foreach (var segment in revisionGraphRow.Revision.StartSegments)
                    {
                        revisionGraphRow.Segments.Add(segment);
                    }

                    _orderedRowCache.Add(revisionGraphRow);
                    nextIndex++;
                }

                Updated?.Invoke();
            }
        }

        public bool TryGetNode(ObjectId objectId, out RevisionGraphRevision revision)
        {
            return _nodeByObjectId.TryGetValue(objectId, out revision);
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
            if (row >= _orderedRowCache.Count)
            {
                return null;
            }

            return _orderedRowCache[row];
        }

        private int _maxScore = 0;

        public void Add(GitRevision revision, RevisionNodeFlags types)
        {
            if (!_nodeByObjectId.TryGetValue(revision.ObjectId, out RevisionGraphRevision revisionGraphRevision))
            {
                int score = _maxScore + 1;
                _maxScore = score;

                revisionGraphRevision = new RevisionGraphRevision(revision.ObjectId, score);
                revisionGraphRevision.ApplyFlags(types);

                revisionGraphRevision.GitRevision = revision;
                _nodeByObjectId.TryAdd(revision.ObjectId, revisionGraphRevision);
                _nodes.Add(revisionGraphRevision);
            }
            else
            {
                // This revision was added as a parent. Probably only the objectid is known.
                revisionGraphRevision.GitRevision = revision;
                revisionGraphRevision.ApplyFlags(types);

                // Invalidate cache if the new score is lower then the cached result
                if (revisionGraphRevision.Score < _cachedUntillScore)
                {
                    _reorder = true;
                }
            }

            foreach (ObjectId parentObjectId in revision.ParentIds)
            {
                if (!_nodeByObjectId.TryGetValue(parentObjectId, out RevisionGraphRevision parentRevisionGraphRevision))
                {
                    parentRevisionGraphRevision = new RevisionGraphRevision(parentObjectId, ++_maxScore);
                    _nodes.Add(parentRevisionGraphRevision);
                    _nodeByObjectId.TryAdd(parentObjectId, parentRevisionGraphRevision);

                    int newMaxScore;
                    _segments.Add(revisionGraphRevision.AddParent(parentRevisionGraphRevision, out newMaxScore));
                    _maxScore = Math.Max(_maxScore, newMaxScore);

                    // Invalidate cache if the new score is lower then the cached result
                    if (parentRevisionGraphRevision.Score < _cachedUntillScore)
                    {
                        _reorder = true;
                    }
                }
                else
                {
                    // If the current score is lower, cache is invalid. The new score will (probably) be higher.
                    if (parentRevisionGraphRevision.Score < _cachedUntillScore)
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
