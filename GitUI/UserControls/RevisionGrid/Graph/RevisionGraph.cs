using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    // The RevisionGraph contains all the basic structures needed to render the graph.
    public class RevisionGraph
    {
        // Some unordered collections with raw data
        private ConcurrentDictionary<ObjectId, RevisionGraphRevision> _nodeByObjectId = new ConcurrentDictionary<ObjectId, RevisionGraphRevision>();
        private ConcurrentBag<RevisionGraphRevision> _nodes = new ConcurrentBag<RevisionGraphRevision>();

        /// <summary>
        /// The max score is used to keep a chronological order during the graph building.
        /// It is cheaper than doing <c>_nodes.Max(n => n.Score)</c>.
        /// </summary>
        private int _maxScore;

        /// <summary>
        /// The node cache is an ordered list with the nodes.
        /// This is used so we can draw commits before the graph building is complete.
        /// </summary>
        /// <remarks>This cache is very cheap to build.</remarks>
        private List<RevisionGraphRevision> _orderedNodesCache;
        private bool _reorder = true;
        private int _orderedUntilScore = -1;

        /// <summary>
        /// The ordered row cache contains rows with segments stored in lanes.
        /// </summary>
        /// <remarks>This cache is very expensive to build.</remarks>
        private List<RevisionGraphRow> _orderedRowCache;
        private bool _rebuild = true;
        private int _buildUntilScore = -1;

        // When the cache is updated, this action can be used to invalidate the UI
        public event Action Updated;

        public void Clear()
        {
            _maxScore = 0;
            _nodeByObjectId = new ConcurrentDictionary<ObjectId, RevisionGraphRevision>();
            _nodes = new ConcurrentBag<RevisionGraphRevision>();
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

        /// <summary>
        /// Builds the revision graph cache. There are two caches that are build in this method.
        /// <para>Cache 1: an ordered list of the revisions. This is very cheap to build. (_orderedNodesCache).</para>
        /// <para>Cache 2: an ordered list of all prepared graph rows. This is expensive to build. (_orderedRowCache)</para>
        /// </summary>
        /// <param name="currentRowIndex">
        /// The row that needs to be displayed. This ensures the ordered revisions are available up to this index.
        /// </param>
        /// <param name="lastToCacheRowIndex">
        /// The graph can be build per x rows. This defines the last row index that the graph will build cache to.
        /// </param>
        public void CacheTo(int currentRowIndex, int lastToCacheRowIndex)
        {
            BuildOrderedNodesCache(currentRowIndex);

            BuildOrderedRowCache(currentRowIndex, lastToCacheRowIndex);
        }

        public bool IsRowRelative(int row)
        {
            var node = GetNodeForRow(row);
            return node != null && node.IsRelative;
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
            BuildOrderedNodesCache(Count);

            if (!TryGetNode(objectId, out RevisionGraphRevision revision))
            {
                index = 0;
                return false;
            }

            index = _orderedNodesCache.IndexOf(revision);
            return index >= 0;
        }

        public RevisionGraphRevision GetNodeForRow(int row)
        {
            // Use a local variable, because the cached list can be reset
            var localOrderedNodesCache = _orderedNodesCache;
            if (localOrderedNodesCache == null || row >= localOrderedNodesCache.Count)
            {
                return null;
            }

            return localOrderedNodesCache.ElementAt(row);
        }

        public RevisionGraphRow GetSegmentsForRow(int row)
        {
            // Use a local variable, because the cached list can be reset
            var localOrderedRowCache = _orderedRowCache;
            if (localOrderedRowCache == null || row >= localOrderedRowCache.Count)
            {
                return null;
            }

            return localOrderedRowCache[row];
        }

        public void HighlightBranch(ObjectId id)
        {
            // Clear current higlighting
            foreach (var revision in _nodes)
            {
                revision.IsRelative = false;
            }

            // Highlight revision
            if (TryGetNode(id, out RevisionGraphRevision revisionGraphRevision))
            {
                revisionGraphRevision.MakeRelative();
            }
        }

        /// <summary>
        /// Add a single revision from the git log.
        /// </summary>
        public void Add(GitRevision revision, RevisionNodeFlags types)
        {
            if (!_nodeByObjectId.TryGetValue(revision.ObjectId, out RevisionGraphRevision revisionGraphRevision))
            {
                // This revision is added from the log, but not seen before. This is probably a root node (new branch) OR the revisions
                // are not in topo order. If this the case, we deal with it later.
                revisionGraphRevision = new RevisionGraphRevision(revision.ObjectId, ++_maxScore);
                revisionGraphRevision.LaneColor = revisionGraphRevision.IsCheckedOut ? 0 : _maxScore;

                _nodeByObjectId.TryAdd(revision.ObjectId, revisionGraphRevision);
            }
            else
            {
                // This revision was added earlier, but is now found in the log.
                // Increase the score to the current maxScore to keep the order in tact.
                revisionGraphRevision.EnsureScoreIsAbove(++_maxScore);
            }

            // This revision may have been added as a parent before. Probably only the ObjectId is known. Set all the other properties.
            revisionGraphRevision.GitRevision = revision;
            revisionGraphRevision.ApplyFlags(types);
            _nodes.Add(revisionGraphRevision);

            // No build the revisions parent/child structure. The parents need to added here. The child structure is kept in synch in
            // the RevisionGraphRevision class.
            if (revision.ParentIds != null)
            {
                foreach (ObjectId parentObjectId in revision.ParentIds)
                {
                    if (!_nodeByObjectId.TryGetValue(parentObjectId, out RevisionGraphRevision parentRevisionGraphRevision))
                    {
                        // This parent is not loaded before. Create a new (partial) revision. We will complete the info in the revision
                        // when this revision is loaded from the log.
                        parentRevisionGraphRevision = new RevisionGraphRevision(parentObjectId, ++_maxScore);
                        _nodeByObjectId.TryAdd(parentObjectId, parentRevisionGraphRevision);
                    }
                    else
                    {
                        // This revision is already loaded, add the existing revision to the parents list of new revision.
                        // If the current score is lower, cache is invalid. The new score will (probably) be higher.
                        MarkCacheAsInvalidIfNeeded(parentRevisionGraphRevision);
                    }

                    // Store the newly created segment (connection between 2 revisions)
                    revisionGraphRevision.AddParent(parentRevisionGraphRevision, out int newMaxScore);
                    _maxScore = Math.Max(_maxScore, newMaxScore);
                }
            }

            MarkCacheAsInvalidIfNeeded(revisionGraphRevision);
        }

        private void BuildOrderedRowCache(int currentRowIndex, int lastToCacheRowIndex)
        {
            if (_orderedRowCache == null || _rebuild)
            {
                _orderedRowCache = new List<RevisionGraphRow>(currentRowIndex);
                _rebuild = false;
            }

            int nextIndex = _orderedRowCache.Count;
            if (nextIndex > lastToCacheRowIndex)
            {
                return;
            }

            int cacheCount = _orderedNodesCache.Count;
            while (nextIndex <= lastToCacheRowIndex && cacheCount > nextIndex)
            {
                bool startSegmentsAdded = false;

                RevisionGraphRevision revision = _orderedNodesCache[nextIndex];

                // The list containing the segments is created later. We can set the correct capacity then, to prevent resizing
                List<RevisionGraphSegment> segments;

                if (nextIndex > 0)
                {
                    // Copy lanes from last row
                    RevisionGraphRow previousRevisionGraphRow = _orderedRowCache[nextIndex - 1];

                    // Create segments list with te correct capacity
                    segments = new List<RevisionGraphSegment>(previousRevisionGraphRow.Segments.Count + revision.StartSegments.Count);

                    // Loop through all segments that do not end in this row
                    foreach (var segment in previousRevisionGraphRow.Segments.Where(s => s.Parent != previousRevisionGraphRow.Revision))
                    {
                        segments.Add(segment);

                        // This segments continues in the next row. Copy all other segments that start from this revision to this lane.
                        if (revision == segment.Parent && !startSegmentsAdded)
                        {
                            startSegmentsAdded = true;
                            segments.AddRange(revision.StartSegments);
                        }
                    }
                }
                else
                {
                    // Create segments list with te correct capacity
                    segments = new List<RevisionGraphSegment>(revision.StartSegments.Count);
                }

                if (!startSegmentsAdded)
                {
                    // Add new segments started by this revision to the end
                    segments.AddRange(revision.StartSegments);
                }

                _orderedRowCache.Add(new RevisionGraphRow(revision, segments));
                _buildUntilScore = revision.Score;
                nextIndex++;
            }

            Updated?.Invoke();
        }

        private void BuildOrderedNodesCache(int currentRowIndex)
        {
            if (_orderedNodesCache != null && !_reorder && _orderedNodesCache.Count >= currentRowIndex)
            {
                return;
            }

            _orderedNodesCache = _nodes.OrderBy(n => n.Score).ToList();
            if (_orderedNodesCache.Count > 0)
            {
                _orderedUntilScore = _orderedNodesCache.Last().Score;
            }

            _reorder = false;
        }

        private void MarkCacheAsInvalidIfNeeded(RevisionGraphRevision revisionGraphRevision)
        {
            if (revisionGraphRevision.Score <= _orderedUntilScore)
            {
                _reorder = true;
            }

            if (revisionGraphRevision.Score <= _buildUntilScore)
            {
                _rebuild = true;
            }
        }

        internal TestAccessor GetTestAccessor() => new TestAccessor(this);

        internal readonly struct TestAccessor
        {
            private readonly RevisionGraph _revisionGraph;

            public TestAccessor(RevisionGraph revisionGraph)
            {
                _revisionGraph = revisionGraph;
            }

            // This method will validate the topo order in brute force.
            // Only used for unit testing.
            public bool ValidateTopoOrder()
            {
                int currentIndex = 0;
                foreach (var node in _revisionGraph._orderedNodesCache)
                {
                    foreach (var parent in node.Parents)
                    {
                        if (!_revisionGraph.TryGetRowIndex(parent.Objectid, out int parentIndex) || parentIndex < currentIndex)
                        {
                            return false;
                        }
                    }

                    foreach (var child in node.Children)
                    {
                        if (!_revisionGraph.TryGetRowIndex(child.Objectid, out int childIndex) || childIndex > currentIndex)
                        {
                            return false;
                        }
                    }

                    currentIndex++;
                }

                return true;
            }
        }
    }
}
