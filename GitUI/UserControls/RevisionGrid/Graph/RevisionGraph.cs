using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal interface IRevisionGraphRowProvider
    {
        IRevisionGraphRow GetSegmentsForRow(int row);
    }

    // The RevisionGraph contains all the basic structures needed to render the graph.
    public class RevisionGraph : IRevisionGraphRowProvider
    {
        // Some unordered collections with raw data
        private ConcurrentDictionary<ObjectId, RevisionGraphRevision> _nodeByObjectId = new ConcurrentDictionary<ObjectId, RevisionGraphRevision>();
        private ImmutableList<RevisionGraphRevision> _nodes = ImmutableList<RevisionGraphRevision>.Empty;

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
        private IList<RevisionGraphRow> _orderedRowCache;

        // When the cache is updated, this action can be used to invalidate the UI
        public event Action Updated;

        public void Clear()
        {
            _maxScore = 0;
            _nodeByObjectId = new ConcurrentDictionary<ObjectId, RevisionGraphRevision>();
            _nodes = ImmutableList<RevisionGraphRevision>.Empty;
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
            List<RevisionGraphRevision> orderedNodesCache = BuildOrderedNodesCache(currentRowIndex);

            BuildOrderedRowCache(orderedNodesCache, currentRowIndex, lastToCacheRowIndex);
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
            if (!TryGetNode(objectId, out RevisionGraphRevision revision))
            {
                index = 0;
                return false;
            }

            index = BuildOrderedNodesCache(Count).IndexOf(revision);
            return index >= 0;
        }

        public RevisionGraphRevision GetNodeForRow(int row)
        {
            // Use a local variable, because the cached list can be reset
            var localOrderedNodesCache = BuildOrderedNodesCache(row);
            if (row >= localOrderedNodesCache.Count)
            {
                return null;
            }

            return localOrderedNodesCache.ElementAt(row);
        }

        public IRevisionGraphRow GetSegmentsForRow(int row)
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

                    if (types.HasFlag(RevisionNodeFlags.OnlyFirstParent))
                    {
                        break;
                    }
                }
            }

            // Ensure all parents are loaded before adding it to the _nodes list. This is important for ordering.
            ImmutableInterlocked.Update(ref _nodes, list => list.Add(revisionGraphRevision));
        }

        /// <summary>
        /// It is very easy to check if the rowcache is dirty or not. If the last revision added to the rowcache
        /// is not in the same index in the orderednodecache, the order has been changed. Only then rebuilding is
        /// required. If the order is changed after this revision, we do not care since it wasn't processed yet.
        /// </summary>
        private bool CheckRowCacheIsDirty(IList<RevisionGraphRow> orderedRowCache, IList<RevisionGraphRevision> orderedNodesCache)
        {
            // We need bounds checking on orderedNodesCache. It should be always larger then the rowcache,
            // but another thread could clear the orderedNodesCache while another is building orderedRowCache.
            // This is not a problem, since all methods use local instances of those caches. We do need to invalidate.
            if (orderedRowCache.Count > orderedNodesCache.Count)
            {
                return true;
            }

            if (orderedRowCache.Count == 0)
            {
                return false;
            }

            int indexToCompare = orderedRowCache.Count - 1;
            return orderedRowCache[indexToCompare].Revision != orderedNodesCache[indexToCompare];
        }

        private void BuildOrderedRowCache(IList<RevisionGraphRevision> orderedNodesCache, int currentRowIndex, int lastToCacheRowIndex)
        {
            // Ensure we keep using the same instance of the rowcache from here on
            var localOrderedRowCache = _orderedRowCache;

            if (localOrderedRowCache == null || CheckRowCacheIsDirty(localOrderedRowCache, orderedNodesCache))
            {
                localOrderedRowCache = new List<RevisionGraphRow>(currentRowIndex);
            }

            int nextIndex = localOrderedRowCache.Count;
            if (nextIndex > lastToCacheRowIndex)
            {
                return;
            }

            int cacheCount = orderedNodesCache.Count;
            while (nextIndex <= lastToCacheRowIndex && cacheCount > nextIndex)
            {
                bool startSegmentsAdded = false;

                RevisionGraphRevision revision = orderedNodesCache[nextIndex];

                // The list containing the segments is created later. We can set the correct capacity then, to prevent resizing
                List<RevisionGraphSegment> segments;

                if (nextIndex == 0)
                {
                    // This is the first row. Start with only the startsegments of this row
                    segments = new List<RevisionGraphSegment>(revision.StartSegments);
                }
                else
                {
                    // Copy lanes from last row
                    RevisionGraphRow previousRevisionGraphRow = localOrderedRowCache[nextIndex - 1];

                    // Create segments list with te correct capacity
                    segments = new List<RevisionGraphSegment>(previousRevisionGraphRow.Segments.Count + revision.StartSegments.Count);

                    // Loop through all segments that do not end in the previous row
                    foreach (var segment in previousRevisionGraphRow.Segments.Where(s => s.Parent != previousRevisionGraphRow.Revision))
                    {
                        segments.Add(segment);

                        // This segment that is copied from the previous row, connects to the node in this row.
                        // Copy all new segments that start from this node (revision) to this lane.
                        if (revision == segment.Parent && !startSegmentsAdded)
                        {
                            startSegmentsAdded = true;
                            segments.AddRange(revision.StartSegments);
                        }
                    }

                    // The startsegments do not connect to any previous row. This means that this is a new branch.
                    if (!startSegmentsAdded)
                    {
                        // Add new segments started by this revision to the end
                        segments.AddRange(revision.StartSegments);
                    }
                }

                localOrderedRowCache.Add(new RevisionGraphRow(revision, segments));
                nextIndex++;
            }

            // Overwrite the global instance at the end, to prevent flickering
            _orderedRowCache = localOrderedRowCache;

            Updated?.Invoke();
        }

        [NotNull]
        private List<RevisionGraphRevision> BuildOrderedNodesCache(int currentRowIndex)
        {
            if (_orderedNodesCache != null && !_reorder && _orderedNodesCache.Count >= Math.Min(Count, currentRowIndex))
            {
                return _orderedNodesCache;
            }

            // Reset the reorder flag and the orderedUntilScore. This makes sure it isn't marked dirty before we even got to
            // rebuilding it.
            _orderedUntilScore = 0;
            _reorder = false;

            // Use a local variable, because the cached list can be reset
            var localOrderedNodesCache = _nodes.ToList();
            localOrderedNodesCache.Sort((x, y) => x.Score.CompareTo(y.Score));
            _orderedNodesCache = localOrderedNodesCache;
            if (localOrderedNodesCache.Count > 0)
            {
                _orderedUntilScore = localOrderedNodesCache.Last().Score;
            }

            return localOrderedNodesCache;
        }

        private void MarkCacheAsInvalidIfNeeded(RevisionGraphRevision revisionGraphRevision)
        {
            if (revisionGraphRevision.Score <= _orderedUntilScore)
            {
                _reorder = true;
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
                foreach (var node in _revisionGraph._nodes)
                {
                    foreach (var parent in node.Parents)
                    {
                        if (parent.Score <= node.Score)
                        {
                            return false;
                        }
                    }

                    foreach (var child in node.Children)
                    {
                        if (node.Score <= child.Score)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }
    }
}
