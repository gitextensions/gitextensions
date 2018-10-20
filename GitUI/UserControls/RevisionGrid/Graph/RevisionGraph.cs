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

            Score = guessScore;
        }

        public int Score { get; private set; }

        public int LaneIndex { get; set; }

        private void IncreaseScore(int delta)
        {
            if (delta + 1 > Score)
            {
                Score = Math.Max(delta + 1, Score);

                foreach (RevisionGraphRevision parent in Parents)
                {
                    parent.IncreaseScore(Score);
                }
            }
        }

        public GitRevision GitRevision { get; set; }

        public ObjectId Objectid { get; set; }

        private ConcurrentBag<RevisionGraphRevision> Parents { get; set; }
        private ConcurrentBag<RevisionGraphRevision> Children { get; set; }

        public RevisionGraphSegment AddParent(RevisionGraphRevision parent)
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

            parent.IncreaseScore(Score);

            return new RevisionGraphSegment(parent, this);
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

    public class RevisionGraph
    {
        private ConcurrentDictionary<ObjectId, RevisionGraphRevision> _nodeByObjectId = new ConcurrentDictionary<ObjectId, RevisionGraphRevision>();
        private ConcurrentBag<RevisionGraphRevision> _nodes = new ConcurrentBag<RevisionGraphRevision>();
        private ConcurrentBag<RevisionGraphSegment> _segments = new ConcurrentBag<RevisionGraphSegment>();

        private bool _reorder = true;
        private List<RevisionGraphRevision> _orderedNodesCache = null;

        public void Clear()
        {
            _nodeByObjectId = new ConcurrentDictionary<ObjectId, RevisionGraphRevision>();
            _nodes = new ConcurrentBag<RevisionGraphRevision>();
            _segments = new ConcurrentBag<RevisionGraphSegment>();
            _orderedNodesCache = null;
        }

        public RevisionGraphRevision GetNodeForRow(int row)
        {
            if (_orderedNodesCache == null || _reorder)
            {
                _orderedNodesCache = _nodes.OrderBy(n => n.Score).ToList();
                _reorder = false;
            }

            if (row >= _orderedNodesCache.Count)
            {
                return null;
            }

            return _orderedNodesCache.ElementAt(row);
        }

        public Point GetNodePosition(RevisionGraphRevision node)
        {
            if (_orderedNodesCache == null || _reorder)
            {
                _orderedNodesCache = _nodes.OrderBy(n => n.Score).ToList();
                _reorder = false;
            }

            int row = _orderedNodesCache.IndexOf(node);
            int lane = GetSegmentsForRow(row, out RevisionGraphRevision unused).IndexOf(s => s.Parent == node || s.Child == node);

            return new Point(lane, row);
        }

        public IEnumerable<RevisionGraphSegment> GetSegmentsForRow(int row, out RevisionGraphRevision node)
        {
            int scoreToSearch;
            node = GetNodeForRow(row);
            if (node != null)
            {
                scoreToSearch = node.Score;
            }
            else
            {
                scoreToSearch = -10;
            }

            return _segments.Where(s => s.StartScore <= scoreToSearch && s.EndScore >= scoreToSearch).OrderBy(s => s.StartScore);
        }

        public int Count
        {
            get
            {
                return _nodes.Count;
            }
        }

        public void Add(GitRevision revision)
        {
            if (!_nodeByObjectId.TryGetValue(revision.ObjectId, out RevisionGraphRevision revisionGraphRevision))
            {
                int score = 0;
                if (_nodes.Any())
                {
                    score = _nodes.Max(r => r.Score) + 1;
                }

                revisionGraphRevision = new RevisionGraphRevision(revision.ObjectId, score);
                revisionGraphRevision.LaneIndex = score;
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
                    parentRevisionGraphRevision = new RevisionGraphRevision(parentObjectId, _nodes.Max(r => r.Score) + 1);
                    _nodes.Add(parentRevisionGraphRevision);
                    _nodeByObjectId.TryAdd(parentObjectId, parentRevisionGraphRevision);

                    _segments.Add(revisionGraphRevision.AddParent(parentRevisionGraphRevision));
                }
                else
                {
                    _segments.Add(revisionGraphRevision.AddParent(parentRevisionGraphRevision));
                }
            }

            _reorder = true;
        }
    }
}
