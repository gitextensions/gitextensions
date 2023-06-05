using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal interface IRevisionGraphRowProvider
    {
        IRevisionGraphRow? GetSegmentsForRow(int row);
    }

    // The RevisionGraph contains all the basic structures needed to render the graph.
    public class RevisionGraph : IRevisionGraphRowProvider
    {
        internal const int MaxLanes = 40;
        private const int _straightenLanesLookAhead = 20;

        // Some unordered collections with raw data
        private ConcurrentDictionary<ObjectId, RevisionGraphRevision> _nodeByObjectId = new();
        private ImmutableList<RevisionGraphRevision> _nodes = ImmutableList<RevisionGraphRevision>.Empty;

        private bool _loadingCompleted;

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
        private RevisionGraphRevision[]? _orderedNodesCache;
        private bool _reorder = true;
        private int _orderedUntilScore = -1;

        /// <summary>
        /// The ordered row cache contains rows with segments stored in lanes.
        /// </summary>
        /// <remarks>This cache is very expensive to build.</remarks>
        private IList<RevisionGraphRow>? _orderedRowCache;

        // When the cache is updated, this action can be used to invalidate the UI
        public event Action? Updated;

        public void Clear()
        {
            _loadingCompleted = false;
            _maxScore = 0;
            _nodeByObjectId.Clear();
            _nodes = ImmutableList<RevisionGraphRevision>.Empty;
            _orderedNodesCache = null;
            _orderedRowCache = null;
        }

        public void LoadingCompleted()
        {
            _loadingCompleted = true;
        }

        public int Count => _nodes.Count;

        public bool OnlyFirstParent { get; set; }
        public ObjectId HeadId { get; set; }

        /// <summary>
        /// Checks whether the given hash is present in the graph.
        /// </summary>
        /// <param name="objectId">The hash to find.</param>
        /// <returns><see langword="true"/>, if the given hash if found; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="objectId"/> is <see langword="null"/>.</exception>
        public bool Contains(ObjectId objectId) => _nodeByObjectId.ContainsKey(objectId);

        public int GetCachedCount()
        {
            if (_orderedRowCache is null)
            {
                return 0;
            }

            // _loadingCompleted is set already when all _nodes have been loaded.
            // Return the full number of rows only if the straightening of segments has finished, too.
            // Else do not show rows yet which might be affected by the straightening of segments (refer to the CacheTo function).
            int cachedCount = _orderedRowCache.Count;
            return _loadingCompleted && cachedCount == Count ? cachedCount : Math.Max(0, cachedCount - (2 * _straightenLanesLookAhead));
        }

        /// <summary>
        /// Builds the revision graph cache. There are two caches that are built in this method.
        /// <para>Cache 1: an ordered list of the revisions. This is very cheap to build. (_orderedNodesCache).</para>
        /// <para>Cache 2: an ordered list of all prepared graph rows. This is expensive to build. (_orderedRowCache).</para>
        /// </summary>
        /// <param name="currentRowIndex">
        /// The row that needs to be displayed. This ensures the ordered revisions are available up to this index.
        /// </param>
        /// <param name="lastToCacheRowIndex">
        /// The graph can be built per x rows. This defines the last row index that the graph will build cache to.
        /// </param>
        public void CacheTo(int currentRowIndex, int lastToCacheRowIndex)
        {
            // Graph segments shall be straightened. For this, we need to look ahead some rows.
            // If lanes of a row are moved, go back the same number of rows as for the lookahead
            // because then the previous rows could benefit from segment straightening, too.
            //
            // row 0
            // row 1
            // ...
            // last finally straightened row           <-- GetCachedCount()
            // first partially straightened row        <-- go back not further than here
            // ...
            // last partially straightened row
            // row to continue straightening           <-- GetCachedCount() + _straightenLanesLookAhead
            // first lookahead row for straightening
            // ...
            // last lookahead row for straightening    <-- GetCachedCount() + 2 * _straightenLanesLookAhead
            //
            currentRowIndex += 2 * _straightenLanesLookAhead;
            lastToCacheRowIndex += 2 * _straightenLanesLookAhead;

            if (_loadingCompleted)
            {
                int maxRowIndex = Count - 1;
                currentRowIndex = Math.Min(currentRowIndex, maxRowIndex);
                lastToCacheRowIndex = Math.Min(lastToCacheRowIndex, maxRowIndex);
            }

            RevisionGraphRevision[] orderedNodesCache = BuildOrderedNodesCache(currentRowIndex);

            BuildOrderedRowCache(orderedNodesCache, currentRowIndex, lastToCacheRowIndex);
        }

        public bool IsRowRelative(int row)
        {
            var node = GetNodeForRow(row);
            return node is not null && node.IsRelative;
        }

        public bool IsRevisionRelative(ObjectId objectId)
        {
            if (_nodeByObjectId.TryGetValue(objectId, out RevisionGraphRevision revision))
            {
                return revision.IsRelative;
            }

            return false;
        }

        public bool TryGetNode(ObjectId objectId, [NotNullWhen(true)] out RevisionGraphRevision? revision)
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

            index = Array.IndexOf(BuildOrderedNodesCache(Count), revision);
            return index >= 0;
        }

        public RevisionGraphRevision? GetNodeForRow(int row)
        {
            // Use a local variable, because the cached list can be reset
            var localOrderedNodesCache = BuildOrderedNodesCache(row);
            if (row >= localOrderedNodesCache.Length)
            {
                return null;
            }

            return localOrderedNodesCache.ElementAt(row);
        }

        public IRevisionGraphRow? GetSegmentsForRow(int row)
        {
            // Use a local variable, because the cached list can be reset
            var localOrderedRowCache = _orderedRowCache;
            if (localOrderedRowCache is null || row < 0 || row >= localOrderedRowCache.Count)
            {
                return null;
            }

            return localOrderedRowCache[row];
        }

        public void HighlightBranch(ObjectId id)
        {
            // Clear current highlighting
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
        /// Set HasNotes for all GitRevisions (marking Notes as fetched).
        /// This is used when no Git Notes at all exist and notes never need to be retrieved.
        /// </summary>
        public void SetHasNotesForRevisions()
        {
            foreach (RevisionGraphRevision revision in _nodes)
            {
                revision.GitRevision.HasNotes = true;
            }
        }

        /// <summary>
        /// Add a single revision from the git log to the graph, including segments to parents.
        /// </summary>
        /// <param name="revision">The revision to add.</param>
        /// <param name="insertScore">Insert the (artificial) revision before the node with this score.</param>
        /// <param name="insertRange">Number of scores "reserved" in the list when inserting.</param>
        public void Add(GitRevision revision, int? insertScore = null, int insertRange = 0)
        {
            // The commits are sorted by the score (not contiuous numbering there may be gaps)
            // This commit will be ordered after existing, _maxScore is a preliminary score
            _maxScore++;

            bool updateParents = true;
            if (!_nodeByObjectId.TryGetValue(revision.ObjectId, out RevisionGraphRevision revisionGraphRevision))
            {
                // This revision is added from the log, but not seen before. This is probably a root node (new branch)
                // OR the revisions are not in topo order. If this the case, we deal with it later.
                int score = _maxScore;

                if (insertScore is not null && _nodeByObjectId is not null)
                {
                    // This revision is to be inserted before a certain node
                    foreach (var (_, graphRevision) in _nodeByObjectId)
                    {
                        if (graphRevision.Score < insertScore)
                        {
                            // Lower existing scores to reserve the inserted range
                            graphRevision.OffsetScore(-insertRange);
                        }
                    }

                    score = insertScore.Value - insertRange;
                }

                revisionGraphRevision = new RevisionGraphRevision(revision.ObjectId, score);
                _nodeByObjectId.TryAdd(revision.ObjectId, revisionGraphRevision);
            }
            else
            {
                // This revision was added earlier, but is now found in the log.
                if (insertScore is null)
                {
                    // Increase the score to the current maxScore to keep the order intact.
                    revisionGraphRevision.EnsureScoreIsAbove(_maxScore);
                }
                else
                {
                    // Second artificial (Index), score already set
                    // No parent segment to be added (HEAD not in grid)
                    updateParents = false;
                }
            }

            // This revision may have been added as a parent before. Probably only the ObjectId is known. Set all the other properties.
            revisionGraphRevision.GitRevision = revision;
            revisionGraphRevision.ApplyFlags(isCheckedOut: HeadId == revision.ObjectId);

            // Build the revisions parent/child structure. The parents need to added here. The child structure is kept in synch in
            // the RevisionGraphRevision class.
            if (revision.ParentIds is not null && updateParents)
            {
                foreach (ObjectId parentObjectId in revision.ParentIds)
                {
                    if (!_nodeByObjectId.TryGetValue(parentObjectId, out RevisionGraphRevision parentRevisionGraphRevision))
                    {
                        int score = insertScore is not null

                            // Inserted after current revision
                            ? revisionGraphRevision.Score + 1 + revision.ParentIds.IndexOf(parentId => parentId == parentObjectId)

                            // This parent is not loaded before. Create a new (partial) revision. We will complete the info in the revision
                            // when this revision is loaded from the log.
                            : ++_maxScore;
                        parentRevisionGraphRevision = new RevisionGraphRevision(parentObjectId, score);
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

                    if (OnlyFirstParent)
                    {
                        break;
                    }
                }
            }

            // Ensure all parents are loaded before adding it to the _nodes list. This is important for ordering.
            ImmutableInterlocked.Update(ref _nodes, (list, revision) => list.Add(revision), revisionGraphRevision);

            if (!updateParents)
            {
                // The rows may already be cached, invalidate and request reload of "some" rows
                _reorder = true;
                CacheTo(0, 99);
            }
        }

        /// <summary>
        /// It is very easy to check if the rowcache is dirty or not. If the last revision added to the rowcache
        /// is not in the same index in the orderednodecache, the order has been changed. Only then rebuilding is
        /// required. If the order is changed after this revision, we do not care since it wasn't processed yet.
        /// </summary>
        private static bool CheckRowCacheIsDirty(IList<RevisionGraphRow> orderedRowCache, RevisionGraphRevision[] orderedNodesCache)
        {
            // We need bounds checking on orderedNodesCache. It should be always larger then the rowcache,
            // but another thread could clear the orderedNodesCache while another is building orderedRowCache.
            // This is not a problem, since all methods use local instances of those caches. We do need to invalidate.
            if (orderedRowCache.Count > orderedNodesCache.Length)
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

        private void BuildOrderedRowCache(RevisionGraphRevision[] orderedNodesCache, int currentRowIndex, int lastToCacheRowIndex)
        {
            int lastOrderedNodeIndex = orderedNodesCache.Length - 1;

            // Ensure we keep using the same instance of the rowcache from here on
            IList<RevisionGraphRow>? localOrderedRowCache = _orderedRowCache;

            if (localOrderedRowCache is null || CheckRowCacheIsDirty(localOrderedRowCache, orderedNodesCache))
            {
                localOrderedRowCache = new List<RevisionGraphRow>(capacity: Math.Max(currentRowIndex, lastOrderedNodeIndex) + 1);
            }

            lastToCacheRowIndex = Math.Min(lastToCacheRowIndex, lastOrderedNodeIndex);
            int startIndex = localOrderedRowCache.Count;
            if (startIndex > lastToCacheRowIndex)
            {
                return;
            }

            for (int nextIndex = startIndex; nextIndex <= lastToCacheRowIndex; ++nextIndex)
            {
                bool startSegmentsAdded = false;

                RevisionGraphRevision revision = orderedNodesCache[nextIndex];

                // The list containing the segments is created later. We can set the correct capacity then, to prevent resizing
                List<RevisionGraphSegment> segments;
                RevisionGraphSegment[] revisionStartSegments = revision.GetStartSegments();

                RevisionGraphRow? previousRevisionGraphRow;
                if (nextIndex == 0)
                {
                    previousRevisionGraphRow = null;

                    // This is the first row. Start with only the startsegments of this row
                    segments = new List<RevisionGraphSegment>(revisionStartSegments);

                    foreach (var startSegment in revisionStartSegments)
                    {
                        startSegment.LaneInfo = new LaneInfo(startSegment, derivedFrom: null);
                    }
                }
                else
                {
                    // Copy lanes from last row
                    previousRevisionGraphRow = localOrderedRowCache[nextIndex - 1];

                    // Create segments list with the correct capacity
                    segments = new List<RevisionGraphSegment>(previousRevisionGraphRow.Segments.Count + revisionStartSegments.Length);

                    // Loop through all segments that do not end in the previous row
                    foreach (var segment in previousRevisionGraphRow.Segments.Where(s => s.Parent != previousRevisionGraphRow.Revision))
                    {
                        segments.Add(segment);

                        // This segment that is copied from the previous row, connects to the node in this row.
                        // Copy all new segments that start from this node (revision) to this lane.
                        if (revision == segment.Parent)
                        {
                            if (!startSegmentsAdded)
                            {
                                startSegmentsAdded = true;
                                segments.AddRange(revisionStartSegments);
                            }

                            foreach (var startSegment in revisionStartSegments)
                            {
                                if (startSegment == revisionStartSegments[0])
                                {
                                    if (startSegment.LaneInfo is null || startSegment.LaneInfo.StartScore > segment.LaneInfo?.StartScore)
                                    {
                                        startSegment.LaneInfo = segment.LaneInfo;
                                    }
                                }
                                else
                                {
                                    if (startSegment.LaneInfo is null)
                                    {
                                        startSegment.LaneInfo = new LaneInfo(startSegment, derivedFrom: segment.LaneInfo);
                                    }
                                }
                            }
                        }
                    }

                    // The startsegments do not connect to any previous row. This means that this is a new branch.
                    if (!startSegmentsAdded)
                    {
                        // Add new segments started by this revision to the end
                        segments.AddRange(revisionStartSegments);

                        foreach (var startSegment in revisionStartSegments)
                        {
                            startSegment.LaneInfo = new LaneInfo(startSegment, derivedFrom: null);
                        }
                    }
                }

                localOrderedRowCache.Add(new RevisionGraphRow(revision, segments, previousRevisionGraphRow));
            }

            // Straightening does not apply to the first and the last row. The single node there shall not be moved.
            // So the straightening algorithm can presume that a previous and a next row do exist.
            // Straighten only lines for which the full lookahead is loaded.
            int straightenStartIndex = Math.Max(1, startIndex - _straightenLanesLookAhead);
            int straightenLastIndex = _loadingCompleted && lastToCacheRowIndex == lastOrderedNodeIndex ? lastToCacheRowIndex - 1 : lastToCacheRowIndex - _straightenLanesLookAhead;
            StraightenLanes(straightenStartIndex, straightenLastIndex, lastLookaheadIndex: lastToCacheRowIndex, localOrderedRowCache);

            // Overwrite the global instance at the end, to prevent flickering
            _orderedRowCache = localOrderedRowCache;

            Updated?.Invoke();

            return;

            static void StraightenLanes(int startIndex, int lastStraightenIndex, int lastLookaheadIndex, IList<RevisionGraphRow> localOrderedRowCache)
            {
                // Try to detect this:
                // | | |<-- previous lane
                // |/ /
                // * |<---- current lane
                // | |
                // | |
                // | |
                // |\ \
                // | | |<-- lookahead lane
                //
                // And change it into this:
                // | | |
                // |/  |
                // *   |
                // |   |
                // |   |
                // |   |
                // |\  |
                // | | |
                //
                // also if the distance is > 1 but only if the other distance is exactly 1

                int goBackLimit = 1;
                for (int currentIndex = startIndex; currentIndex <= lastStraightenIndex;)
                {
                    goBackLimit = Math.Max(goBackLimit, currentIndex - _straightenLanesLookAhead);
                    bool moved = false;
                    IRevisionGraphRow currentRow = localOrderedRowCache[currentIndex];
                    IRevisionGraphRow previousRow = localOrderedRowCache[currentIndex - 1];
                    foreach (RevisionGraphSegment revisionGraphSegment in currentRow.Segments.Take(MaxLanes))
                    {
                        Lane currentRowLane = currentRow.GetLaneForSegment(revisionGraphSegment);
                        if (currentRowLane.Sharing != LaneSharing.ExclusiveOrPrimary)
                        {
                            continue; // with next revisionGraphSegment
                        }

                        int currentLane = currentRowLane.Index;
                        int previousLane = previousRow.GetLaneForSegment(revisionGraphSegment).Index;
                        if (previousLane <= currentLane)
                        {
                            continue; // with next revisionGraphSegment
                        }

                        int straightenedCurrentLane = currentLane + 1;
                        int lookaheadLane = currentLane;
                        for (int lookaheadIndex = currentIndex + 1; lookaheadLane == currentLane && lookaheadIndex <= Math.Min(currentIndex + _straightenLanesLookAhead, lastLookaheadIndex); ++lookaheadIndex)
                        {
                            lookaheadLane = localOrderedRowCache[lookaheadIndex].GetLaneForSegment(revisionGraphSegment).Index;
                            if ((lookaheadLane == straightenedCurrentLane) || (lookaheadLane > straightenedCurrentLane && previousLane == straightenedCurrentLane))
                            {
                                for (int moveIndex = currentIndex; moveIndex < lookaheadIndex; ++moveIndex)
                                {
                                    localOrderedRowCache[moveIndex].MoveLanesRight(currentLane);
                                }

                                moved = true;
                                break; // from for lookaheadIndex
                            }
                        }

                        if (moved)
                        {
                            break; // from for revisionGraphSegment
                        }
                    }

                    // if moved, check again whether the lanes of previous rows can be moved, too
                    currentIndex = moved ? Math.Max(currentIndex - _straightenLanesLookAhead, goBackLimit) : currentIndex + 1;
                }
            }
        }

        private RevisionGraphRevision[] BuildOrderedNodesCache(int currentRowIndex)
        {
            if (_orderedNodesCache is not null && !_reorder && _orderedNodesCache.Length >= Math.Min(Count, currentRowIndex))
            {
                return _orderedNodesCache;
            }

            // The Score of the referenced nodes can be rewritten by another thread.
            // Then the cache is invalidated by setting _reorder.
            // So if Sort() complains in this case, try again.
            while (true)
            {
                try
                {
                    // Reset the reorder flag and the orderedUntilScore. This makes sure it isn't marked dirty before we even got to
                    // rebuilding it.
                    _orderedUntilScore = int.MinValue;
                    _reorder = false;

                    // Use a local variable, because the cached list can be reset.
                    RevisionGraphRevision[] localOrderedNodesCache = _nodes.ToArray();
                    Array.Sort(localOrderedNodesCache, (x, y) => x.Score.CompareTo(y.Score));
                    _orderedNodesCache = localOrderedNodesCache;
                    if (localOrderedNodesCache.Length > 0)
                    {
                        _orderedUntilScore = localOrderedNodesCache.Last().Score;
                    }

                    return localOrderedNodesCache;
                }
                catch (ArgumentException ex) when (_reorder && ex.Message.Contains("IComparer.Compare()"))
                {
                    // ignore and try again
                }
            }
        }

        private void MarkCacheAsInvalidIfNeeded(RevisionGraphRevision revisionGraphRevision)
        {
            if (revisionGraphRevision.Score <= _orderedUntilScore)
            {
                _reorder = true;
            }
        }

        internal TestAccessor GetTestAccessor() => new(this);

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
