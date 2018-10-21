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

        public int GetLaneIndexForSegment(RevisionGraphSegment revisionGraphRevision)
        {
            int laneIndex = 0;
            bool nodePassed = false;

            foreach (var segment in Segments.OrderByDescending(s => s.Child.LaneIndex))
            {
                if (segment == revisionGraphRevision)
                {
                    return laneIndex;
                }

                if (segment.Child == Revision || segment.Parent == Revision)
                {
                    if (revisionGraphRevision.Child == Revision || revisionGraphRevision.Parent == Revision)
                    {
                        return laneIndex;
                    }

                    if (!nodePassed)
                    {
                        laneIndex++;
                        nodePassed = true;
                    }
                }
                else
                {
                    laneIndex++;
                }
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

        public void Add(GitRevision revision)
        {
            if (!_nodeByObjectId.TryGetValue(revision.ObjectId, out RevisionGraphRevision revisionGraphRevision))
            {
                int score = _maxScore + 1;
                _maxScore = score;

                revisionGraphRevision = new RevisionGraphRevision(revision.ObjectId, score);
                ////revisionGraphRevision.LaneIndex = score;
                revisionGraphRevision.GitRevision = revision;
                _nodeByObjectId.TryAdd(revision.ObjectId, revisionGraphRevision);
                _nodes.Add(revisionGraphRevision);
            }
            else
            {
                // This revision was added as a parent. Probably only the objectid is known.
                revisionGraphRevision.GitRevision = revision;
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
                }
                else
                {
                    int newMaxScore;
                    _segments.Add(revisionGraphRevision.AddParent(parentRevisionGraphRevision, out newMaxScore));
                    _maxScore = Math.Max(_maxScore, newMaxScore);
                }
            }

            _reorder = true;
        }
    }
}
