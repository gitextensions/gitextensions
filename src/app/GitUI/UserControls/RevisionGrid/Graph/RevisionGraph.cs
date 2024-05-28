using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
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
        private const int _orderSegmentsLookAhead = 50;
        private const int _straightenLanesLookAhead = 20;

        internal RevisionGraphConfig Config { get; private set; } = new();

        /// <summary>
        /// GitRevision which can be displayed.
        /// This information is added from the update thread but can be accessed by multiple threads.
        /// </summary>
        private readonly ConcurrentDictionary<ObjectId, RevisionGraphRevision> _revisionByObjectId = new();

        /// <summary>
        /// Partial revisions that are seen as parents and have no GitRevision yet.
        /// These revisions are only accessed from the update thread and are normally empty
        /// when the loading is completed.
        /// </summary>
        private readonly Dictionary<ObjectId, RevisionGraphRevision> _incompleteRevisionByObjectId = [];

        /// <summary>
        /// The loading is completed when all revisions are loaded from the log,
        /// then <see cref="_revisionByObjectId"/> is completely updated.
        /// </summary>
        private bool _loadingCompleted;

        /// <summary>
        /// The max score is used to keep a chronological order during the graph building.
        /// It is cheaper than doing <c>_nodeByObjectId.Values.Max(n => n.Score)</c>.
        /// </summary>
        private int _maxScore;

        /// <summary>
        /// The node cache is an ordered list of <see cref="_revisionByObjectId"/>.
        /// This is used so we can draw commits before the graph building is complete.
        /// </summary>
        /// <remarks>
        /// This cache is normally built twice, first time when a few revisions are loaded
        /// and the first page is to be displayed, very quick.
        /// The second time is when all revisions are loaded, that may require hundred ms to build.
        /// </remarks>
        private ImmutableArray<RevisionGraphRevision> _orderedNodesCache = ImmutableArray<RevisionGraphRevision>.Empty;

        /// <summary>
        /// Mark the <see cref="_orderedNodesCache"/> as dirty. This will trigger a rebuild of the cache.
        /// </summary>
        private bool _orderedNodesCacheInvalid = true;

        /// <summary>
        /// The ordered row cache contains rows with segments stored in lanes.
        /// </summary>
        /// <remarks>This cache is very expensive to build, done only for displayed rows - in a background task.</remarks>
        private readonly List<RevisionGraphRow> _orderedRowCache = [];

        /// <summary>
        /// <see cref="_orderedRowCache"/> is invalid if it contains scores above this value.
        /// </summary>
        private int _orderedRowCacheInvalidFromScore = int.MaxValue;

        private int _straightenDiagonalsLookAhead => Config.StraightenGraphDiagonals ? _straightenLanesLookAhead / 2 : 0;

        /// <summary>
        /// The number of rows needed to look ahead for straightening diagonals and lanes.
        /// Refer to the <cref>CacheTo</cref> function.
        /// </summary>
        private int _straightenLookAhead => 2 * (_straightenDiagonalsLookAhead + _straightenLanesLookAhead);

        public void Clear()
        {
            _loadingCompleted = false;
            _maxScore = 0;
            _revisionByObjectId.Clear();
            Count = 0;
            _incompleteRevisionByObjectId.Clear();
            _orderedNodesCache = ImmutableArray<RevisionGraphRevision>.Empty;
            _orderedNodesCacheInvalid = true;
            _orderedRowCache.Clear();
            _orderedRowCacheInvalidFromScore = int.MaxValue;
            Config = new();
        }

        public void LoadingCompleted()
        {
            _loadingCompleted = true;

            Debug.WriteLineIf(_incompleteRevisionByObjectId.Count > 0, $"Info: RevisionGraph loading completed, {_incompleteRevisionByObjectId.Count} revisions still incomplete. Occurs if git-log is limiting commits.");
        }

        /// <summary>
        /// The number of revisions in <see cref="_revisionByObjectId"/>, potentially nodes in the graph.
        /// "Cache" to prevent concurrent dictionary access.
        /// </summary>
        public int Count { get; private set; }

        public bool OnlyFirstParent { get; set; }
        public ObjectId HeadId { get; set; }

        /// <summary>
        /// Checks whether the given hash is present in the graph.
        /// </summary>
        /// <param name="objectId">The hash to find.</param>
        /// <returns><see langword="true"/>, if the given hash if found; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="objectId"/> is <see langword="null"/>.</exception>
        public bool Contains(ObjectId objectId) => _revisionByObjectId.ContainsKey(objectId);

        /// <summary>
        /// Get the index for the last cached row with with 'straightened' lanes.
        /// </summary>
        /// <returns>Index for the last row in the row cache with 'straightened' lanes.</returns>
        public int GetCachedCount()
        {
            int cachedCount = _orderedRowCache.Count;
            if (cachedCount == 0 || IsRowCacheDirty(BuildOrderedNodesCache(int.MaxValue)))
            {
                return 0;
            }

            // _loadingCompleted is true when all revisions have been added to _revisionByObjectId.
            // Return the full number of rows only if the straightening of segments has finished, too.
            // Else do not show rows yet which might be affected by the straightening of segments.
            return _loadingCompleted && cachedCount == Count ? cachedCount : Math.Max(0, cachedCount - _straightenLookAhead);
        }

        /// <summary>
        /// Builds the revision graph cache. There are two caches that are built in this method.
        /// <para>Cache 1: an ordered list of the revisions. This is cheap to build. (<see cref="_orderedNodesCache"/>).</para>
        /// <para>Cache 2: an ordered list of all prepared graph rows. This is expensive to build. (<see cref="_orderedRowCache"/>).</para>
        /// </summary>
        /// <param name="currentRowIndex">
        /// The row that needs to be displayed. This ensures the ordered revisions are available up to this index.
        /// </param>
        /// <param name="lastToCacheRowIndex">
        /// The graph can be built per x rows. This defines the last row index that the graph will build cache to.
        /// </param>
        public void CacheTo(int currentRowIndex, int lastToCacheRowIndex, CancellationToken cancellationToken = default)
        {
            // Graph segments shall be straightened. For this, we need to look ahead some rows.
            // If lanes of a row are moved, go back the same number of rows as for the look-ahead
            // because then the previous rows could benefit from segment straightening, too.
            // Afterwards straighten diagonals. There is no significant need to look back for straightening lanes again - keeping it easy.
            //
            // row 0
            // row 1
            // ...
            // last finally straightened row                    <-- GetCachedCount()
            // first partially straightened diagonals row       <-- go back not further than here for straightening diagonals
            // ...
            // last partially straightened diagonals row
            // first look-ahead row for diagonals straightening
            // ...
            // last look-ahead row for diagonals straightening  <-- GetCachedCount() + 2 * _straightenDiagonalsLookAhead
            // first partially straightened lanes row           <-- go back not further than here for straightening lanes
            // ...
            // last partially straightened lanes row
            // row to continue lane straightening               <-- GetCachedCount() + 2 * _straightenDiagonalsLookAhead + _straightenLanesLookAhead
            // first look-ahead row for lane straightening
            // ...
            // last look-ahead row for lane straightening       <-- GetCachedCount() + 2 * _straightenDiagonalsLookAhead + 2 * _straightenLanesLookAhead
            //
            int lookAhead = _straightenLookAhead;
            currentRowIndex += lookAhead;
            lastToCacheRowIndex += lookAhead;

            if (_loadingCompleted)
            {
                int maxRowIndex = Count - 1;
                currentRowIndex = Math.Min(currentRowIndex, maxRowIndex);
                lastToCacheRowIndex = Math.Min(lastToCacheRowIndex, maxRowIndex);
            }

            ImmutableArray<RevisionGraphRevision> orderedNodesCache = BuildOrderedNodesCache(currentRowIndex);
            BuildOrderedRowCache(orderedNodesCache, lastToCacheRowIndex, cancellationToken);
        }

        public bool IsRowRelative(int row)
            => GetNodeForRow(row)?.IsRelative is true;

        public bool TryGetNode(ObjectId objectId, [NotNullWhen(true)] out RevisionGraphRevision? revision)
            => _revisionByObjectId.TryGetValue(objectId, out revision);

        public bool TryGetRowIndex(ObjectId objectId, out int index)
        {
            if (!TryGetNode(objectId, out RevisionGraphRevision revision))
            {
                index = 0;
                return false;
            }

            if (!_loadingCompleted)
            {
                // Try to use the existing _orderedNodesCache, to avoid rebuilding it if not needed.
                index = BuildOrderedNodesCache(0).IndexOf(revision);
                if (index >= 0)
                {
                    return true;
                }
            }

            index = BuildOrderedNodesCache(int.MaxValue).IndexOf(revision);
            return index >= 0;
        }

        public RevisionGraphRevision? GetNodeForRow(int row)
        {
            // Use a local variable, because the cached list can be reset
            ImmutableArray<RevisionGraphRevision> localOrderedNodesCache = BuildOrderedNodesCache(row);
            if (row < 0 || row >= localOrderedNodesCache.Length)
            {
                return null;
            }

            return localOrderedNodesCache[row];
        }

        public IRevisionGraphRow? GetSegmentsForRow(int row)
        {
            ImmutableArray<RevisionGraphRevision> localOrderedNodesCache = BuildOrderedNodesCache(row);
            if (IsRowCacheDirty(localOrderedNodesCache) || row < 0 || row >= _orderedRowCache.Count)
            {
                return null;
            }

            return _orderedRowCache[row];
        }

        public void HighlightBranch(ObjectId id)
        {
            // Clear current highlighting
            foreach (RevisionGraphRevision revision in _revisionByObjectId.Values)
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
        /// Add a single revision from the git log to the graph, including segments to parents.
        /// </summary>
        /// <param name="revision">The revision to add.</param>
        /// <returns><see langword="true"/> if the row cache is invalidated; otherwise <see langword="false"/>.</returns>
        public bool Add(GitRevision revision)
        {
            int minExistingScore;

            // The commits will be sorted by the score (not continuous numbering, there may be gaps).
            // This revision will be ordered after existing, 1+_maxScore is a preliminary score.
            if (_incompleteRevisionByObjectId.TryGetValue(revision.ObjectId, out RevisionGraphRevision revisionGraphRevision))
            {
                // This revision was added as a parent, but is now found in the log.
                // Increase the score for this revision to keep the order intact.
                minExistingScore = revisionGraphRevision.Score;
                revisionGraphRevision.OverrideScore(++_maxScore);
                _incompleteRevisionByObjectId.Remove(revision.ObjectId);
            }
            else
            {
                // This revision is added from the log, but not seen before. This is probably a root node (new branch)
                // or the revisions are not in topo order. If this the case, we deal with it later.
                revisionGraphRevision = new RevisionGraphRevision(revision.ObjectId, ++_maxScore);
                minExistingScore = revisionGraphRevision.Score;
            }

            revisionGraphRevision.GitRevision = revision;
            revisionGraphRevision.ApplyFlags(isCheckedOut: HeadId == revision.ObjectId);

            // Build the parent/child structure. The parent revisions need to added here.
            if (revision.ParentIds is not null)
            {
                foreach (ObjectId parentObjectId in revision.ParentIds)
                {
                    if (_incompleteRevisionByObjectId.TryGetValue(parentObjectId, out RevisionGraphRevision parentRevisionGraphRevision))
                    {
                        // Node seen as parent but not in grid (revisionGraphRevision is likely a separate branch).
                        // Set a new preliminary score (checked in AddParents), will be updated when seen in grid.
                        parentRevisionGraphRevision.OverrideScore(++_maxScore);
                    }
                    else if (_revisionByObjectId.TryGetValue(parentObjectId, out parentRevisionGraphRevision))
                    {
                        // Already in grid. If in the grid, caches must be invalidated (including possible parents).
                        // This node is out of order, which should be uncommon.
                        minExistingScore = Math.Min(minExistingScore, parentRevisionGraphRevision.Score);
                        _maxScore = parentRevisionGraphRevision.EnsureScoreIsAbove(++_maxScore);
                    }
                    else
                    {
                        // This parent is not seen before. (This is the common path).
                        // Create a new (partial) revision.
                        // Complete the info in the revision when this revision is loaded from the log.
                        parentRevisionGraphRevision = new RevisionGraphRevision(parentObjectId, ++_maxScore);
                        _incompleteRevisionByObjectId.TryAdd(parentObjectId, parentRevisionGraphRevision);
                    }

                    // Store the newly created segment (connection between 2 revisions)
                    revisionGraphRevision.AddParent(parentRevisionGraphRevision);

                    if (OnlyFirstParent)
                    {
                        break;
                    }
                }
            }

            _revisionByObjectId.TryAdd(revision.ObjectId, revisionGraphRevision);
            Count++;
            return MarkCacheAsInvalidIfNeeded(minExistingScore);
        }

        /// <summary>
        /// Insert workTree and index revisions to the graph, offset existing revisions.
        /// </summary>
        /// <param name="workTreeRev">The workTree revision to add.</param>
        /// <param name="indexRev">The index revision to add.</param>
        /// <param name="parents">Parent ids for the revision to find (and insert before).</param>
        /// <returns><see langword="true"/> if the row cache is invalidated; otherwise <see langword="false"/>.</returns>
        public bool Insert(GitRevision workTreeRev, GitRevision indexRev, IReadOnlyList<ObjectId> parents)
        {
            DebugHelpers.Assert(!_revisionByObjectId.ContainsKey(workTreeRev.ObjectId), "Worktree revision was unexpectedly already added to the RevisionGraph.");
            DebugHelpers.Assert(!_revisionByObjectId.ContainsKey(indexRev.ObjectId), "Index revision was unexpectedly already added to the RevisionGraph.");

            // Where to insert the revision, negative is before existing in the graph.
            int insertScore = int.MinValue;

            // Find the first parent that is already in the graph.
            foreach (ObjectId parentId in parents!)
            {
                if (TryGetNode(parentId, out RevisionGraphRevision? parentRev))
                {
                    // Insert before this ancestor
                    const int insertRange = 2;
                    int limitScore = parentRev.Score;
                    insertScore = limitScore - insertRange;
                    foreach (RevisionGraphRevision graphRevision in _revisionByObjectId.Values)
                    {
                        if (graphRevision.Score < limitScore)
                        {
                            // Lower existing scores to reserve the inserted range
                            graphRevision.OverrideScore(graphRevision.Score - insertRange);
                        }
                    }

                    break;
                }
            }

            // Add graph revisions including segment.
            RevisionGraphRevision workTreeGraphRevision = new(workTreeRev.ObjectId, insertScore)
            {
                GitRevision = workTreeRev
            };

            RevisionGraphRevision indexGraphRevision = new(indexRev.ObjectId, ++insertScore)
            {
                GitRevision = indexRev
            };

            workTreeGraphRevision.AddParent(indexGraphRevision);
            _revisionByObjectId.TryAdd(workTreeRev.ObjectId, workTreeGraphRevision);
            _revisionByObjectId.TryAdd(indexRev.ObjectId, indexGraphRevision);
            Count += 2;

            // Invalidate caches
            return MarkCacheAsInvalidIfNeeded(workTreeGraphRevision.Score);
        }

        /// <summary>
        /// Check if the row cache is dirty by comparing to the ordered node cache.
        /// </summary>
        /// <param name="orderedNodesCache">The ordered nodes cache.</param>
        /// <returns><see langword="true"/> if the row cache is dirty; otherwise <see langword="false"/>.</returns>
        private bool IsRowCacheDirty(ImmutableArray<RevisionGraphRevision> orderedNodesCache)
        {
            if (_orderedRowCache.Count == 0)
            {
                _orderedRowCacheInvalidFromScore = int.MaxValue;
                return false;
            }

            // We need bounds checking on orderedNodesCache. It should be always larger then the orderedRowCache,
            // but another thread could clear the orderedNodesCache while another is building orderedRowCache.
            // This is not a problem, since all methods use local instances of those caches. We do need to invalidate.
            if (_orderedRowCacheInvalidFromScore <= _orderedRowCache[^1].Revision.Score || _orderedRowCache.Count > orderedNodesCache.Length)
            {
                return true;
            }

            // It is very easy to check if the orderedRowCache is dirty or not. If the last revision added to the orderedRowCache
            // is not in the same index in the orderedNodesCache, the order has been changed. Only then rebuilding is
            // required. If the order is changed after this revision, we do not care since it wasn't processed yet.
            // But when filtering revisions, this can mismatch. So check the first revision in addition.
            int indexToCompare = _orderedRowCache.Count - 1;
            return _orderedRowCache[indexToCompare].Revision != orderedNodesCache[indexToCompare]
                || _orderedRowCache[0].Revision != orderedNodesCache[0];
        }

        private void BuildOrderedRowCache(ImmutableArray<RevisionGraphRevision> orderedNodesCache, int lastToCacheRowIndex, CancellationToken cancellationToken)
        {
            bool orderSegments = Config.ReduceGraphCrossings;

            int orderedNodesCount = orderedNodesCache.Length;
            int lastOrderedNodeIndex = orderedNodesCount - 1;
            bool loadingCompleted = _loadingCompleted;
            _orderedRowCache.Capacity = orderedNodesCount;
            if (IsRowCacheDirty(orderedNodesCache))
            {
                _orderedRowCacheInvalidFromScore = int.MaxValue;
                _orderedRowCache.Clear();
            }

            int maxLastToCacheRowIndex = lastOrderedNodeIndex - (loadingCompleted || !orderSegments ? 0 : _orderSegmentsLookAhead);
            if (lastToCacheRowIndex > maxLastToCacheRowIndex)
            {
                lastToCacheRowIndex = maxLastToCacheRowIndex;
                loadingCompleted = false;
            }

            int startIndex = _orderedRowCache.Count;
            if (startIndex > lastToCacheRowIndex)
            {
                return;
            }

            for (int nextIndex = startIndex; nextIndex <= lastToCacheRowIndex; ++nextIndex)
            {
                cancellationToken.ThrowIfCancellationRequested();
                RevisionGraphRevision revision = orderedNodesCache[nextIndex];
                RevisionGraphSegment[] revisionStartSegments = revision.GetStartSegments();
                if (orderSegments)
                {
                    revisionStartSegments = Order(revisionStartSegments, orderedNodesCache, nextIndex);
                }

                // The list containing the segments is created later. We can set the correct capacity then, to prevent resizing
                List<RevisionGraphSegment> segments;

                if (nextIndex == 0)
                {
                    // This is the first row. Start with only the startsegments of this row
                    segments = new List<RevisionGraphSegment>(revisionStartSegments);

                    int revisionSegmentsCount = revisionStartSegments.Length;
                    for (int i = 0; i < revisionSegmentsCount; i++)
                    {
                        RevisionGraphSegment startSegment = revisionStartSegments[i];
                        startSegment.LaneInfo = new LaneInfo(startSegment);
                    }
                }
                else
                {
                    // Copy lanes from last row
                    RevisionGraphRow previousRevisionGraphRow = _orderedRowCache[nextIndex - 1];

                    // Create segments list with the correct capacity
                    segments = new List<RevisionGraphSegment>(previousRevisionGraphRow.Segments.Count + revisionStartSegments.Length);

                    bool startSegmentsAdded = false;

                    // Loop through all segments that do not end in the previous row
                    int prevRevisionSegmentCount = previousRevisionGraphRow.Segments.Count;
                    for (int i = 0; i < prevRevisionSegmentCount; i++)
                    {
                        RevisionGraphSegment segment = previousRevisionGraphRow.Segments[i];
                        if (segment.Parent == previousRevisionGraphRow.Revision)
                        {
                            continue;
                        }

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

                            int startSegmentsCount = revisionStartSegments.Length;
                            for (int j = 0; j < startSegmentsCount; j++)
                            {
                                RevisionGraphSegment startSegment = revisionStartSegments[j];
                                if (startSegment == revisionStartSegments[0])
                                {
                                    if (startSegment.LaneInfo is null || startSegment.LaneInfo.StartScore > segment.LaneInfo?.StartScore)
                                    {
                                        startSegment.LaneInfo = segment.LaneInfo;
                                    }
                                }
                                else
                                {
                                    startSegment.LaneInfo ??= segment.LaneInfo is null
                                            ? new LaneInfo(startSegment)
                                            : new LaneInfo(startSegment, derivedFrom: segment.LaneInfo);
                                }
                            }
                        }
                    }

                    // The startsegments do not connect to any previous row. This means that this is a new branch.
                    if (!startSegmentsAdded)
                    {
                        // Add new segments started by this revision to the end
                        segments.AddRange(revisionStartSegments);

                        int revisionStartSegmentsCount = revisionStartSegments.Length;
                        for (int i = 0; i < revisionStartSegmentsCount; i++)
                        {
                            RevisionGraphSegment startSegment = revisionStartSegments[i];
                            startSegment.LaneInfo = new LaneInfo(startSegment);
                        }
                    }
                }

                _orderedRowCache.Add(new RevisionGraphRow(revision, segments, Config.MergeGraphLanesHavingCommonParent));
            }

            // Straightening does not apply to the first and the last row. The single node there shall not be moved.
            // So the straightening algorithm can presume that a previous and a next row do exist.
            // Straighten only lines for which the full look-ahead is loaded.
            loadingCompleted = loadingCompleted && lastToCacheRowIndex == lastOrderedNodeIndex;
            int straightenLanesStartIndex = Math.Max(1, startIndex - _straightenLanesLookAhead);
            int straightenLanesLastIndex = loadingCompleted ? lastToCacheRowIndex - 1 : lastToCacheRowIndex - _straightenLanesLookAhead;
            StraightenLanes(straightenLanesStartIndex, straightenLanesLastIndex, lastLookAheadIndex: lastToCacheRowIndex, _orderedRowCache, Config.StraightenGraphSegmentsLimit);

            int straightenDiagonalsLookAhead = _straightenDiagonalsLookAhead;
            if (straightenDiagonalsLookAhead > 0)
            {
                int straightenDiagonalsStartIndex = Math.Max(1, startIndex - _straightenLanesLookAhead - straightenDiagonalsLookAhead);
                int straightenDiagonalsLastIndex = loadingCompleted ? lastToCacheRowIndex - 1 : lastToCacheRowIndex - _straightenLanesLookAhead - straightenDiagonalsLookAhead;
                StraightenDiagonals(straightenDiagonalsStartIndex, straightenDiagonalsLastIndex, lastLookAheadIndex: lastToCacheRowIndex, straightenDiagonalsLookAhead, _orderedRowCache, Config.StraightenGraphSegmentsLimit);
            }

            return;

            static RevisionGraphSegment[] Order(RevisionGraphSegment[] segments, ImmutableArray<RevisionGraphRevision> orderedNodesCache, int nextIndex)
            {
                // Define local function GetRowIndex with precalculated limit here
                int endIndex = Math.Min(nextIndex + _orderSegmentsLookAhead, orderedNodesCache.Length);
                int GetRowIndex(RevisionGraphRevision revision)
                {
                    for (int index = nextIndex + 1; index < endIndex; ++index)
                    {
                        if (orderedNodesCache[index] == revision)
                        {
                            return index - nextIndex;
                        }
                    }

                    return int.MaxValue;
                }

                return segments.OrderBy(s => s, (a, b) =>
                    {
                        int rowA = GetRowIndex(a.Parent);
                        int rowB = GetRowIndex(b.Parent);

                        // Prefer the one which is the ancestor of the other
                        if (rowA != int.MaxValue && rowB != int.MaxValue)
                        {
                            if (rowA > rowB && IsAncestorOf(a.Parent, b.Parent, rowA))
                            {
                                return -1;
                            }
                            else if (rowB > rowA && IsAncestorOf(b.Parent, a.Parent, rowB))
                            {
                                return 1;
                            }
                        }

                        return Score(a, rowA).CompareTo(Score(b, rowB));

                        int Score(RevisionGraphSegment segment, int row)
                        {
                            int grandParentCount = segment.Parent.ParentCount;
                            return grandParentCount == 0 ? row // initial revision
                                : grandParentCount >= 2 ? -2_000_000_000 + row // merged into
                                : !segment.Parent.Children.Pop().IsEmpty ? -1_000_000_000 + row // branched from
                                : row; // just a commit
                        }

                        bool IsAncestorOf(RevisionGraphRevision ancestor, RevisionGraphRevision child, int stopRow)
                        {
                            if (child.Parents.Contains(ancestor))
                            {
                                return true;
                            }

                            foreach (RevisionGraphRevision parent in child.Parents)
                            {
                                if (GetRowIndex(parent) < stopRow && IsAncestorOf(ancestor, parent, stopRow))
                                {
                                    return true;
                                }
                            }

                            return false;
                        }
                    })
                    .ToArray();
            }

            static void StraightenLanes(int startIndex, int lastStraightenIndex, int lastLookAheadIndex, IReadOnlyList<RevisionGraphRow> localOrderedRowCache, int straightenGraphSegmentsLimit)
            {
                // Try to detect this:
                // | | |<-- previous lane
                // |/ /
                // * |<---- current lane
                // | |
                // | |
                // | |
                // |\ \
                // | | |<-- look-ahead lane
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

                    RevisionGraphRow currentRow = localOrderedRowCache[currentIndex];
                    if (currentRow.Segments.Count > straightenGraphSegmentsLimit)
                    {
                        ++currentIndex;
                        continue;
                    }

                    bool moved = false;
                    RevisionGraphRow previousRow = localOrderedRowCache[currentIndex - 1];
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
                        int lookAheadLane = currentLane;
                        RevisionGraphSegment segmentOrAncestor = currentRow.FirstParentOrSelf(revisionGraphSegment);
                        for (int lookAheadIndex = currentIndex + 1; lookAheadLane == currentLane && lookAheadIndex <= Math.Min(currentIndex + _straightenLanesLookAhead, lastLookAheadIndex); ++lookAheadIndex)
                        {
                            RevisionGraphRow lookAheadRow = localOrderedRowCache[lookAheadIndex];
                            lookAheadLane = lookAheadRow.GetLaneForSegment(segmentOrAncestor).Index;
                            if ((lookAheadLane == straightenedCurrentLane) || (lookAheadLane > straightenedCurrentLane && previousLane == straightenedCurrentLane))
                            {
                                for (int moveIndex = currentIndex; moveIndex < lookAheadIndex; ++moveIndex)
                                {
                                    localOrderedRowCache[moveIndex].MoveLanesRight(currentLane);
                                }

                                moved = true;
                                break; // from for lookAheadIndex
                            }

                            segmentOrAncestor = lookAheadRow.FirstParentOrSelf(segmentOrAncestor);
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

            static void StraightenDiagonals(int startIndex, int lastStraightenIndex, int lastLookAheadIndex, int straightenDiagonalsLookAhead, IList<RevisionGraphRow> localOrderedRowCache, int straightenGraphSegmentsLimit)
            {
                List<MoveLaneBy> moveLaneBy = new(capacity: straightenDiagonalsLookAhead);
                int goBackLimit = 1;
                for (int currentIndex = startIndex; currentIndex <= lastStraightenIndex;)
                {
                    goBackLimit = Math.Max(goBackLimit, currentIndex - straightenDiagonalsLookAhead);
                    int currentLastLookAheadIndex = Math.Min(currentIndex + straightenDiagonalsLookAhead, lastLookAheadIndex);

                    IRevisionGraphRow currentRow = localOrderedRowCache[currentIndex];
                    if (currentRow.Segments.Count > straightenGraphSegmentsLimit)
                    {
                        ++currentIndex;
                        continue;
                    }

                    bool moved = false;
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

                        // Unfold one-lane shift to diagonal (cannot be done together with TurnMultiLaneCrossingIntoDiagonal(diagonalDelta: -1))
                        // Try to detect this:
                        // * | | <-- previous lane
                        // |/ /
                        // * | <---- current lane
                        // | |
                        // * | <---- next lane
                        // |/
                        // * <------ end lane
                        //
                        // And change it into this:
                        // * | | <-- previous lane
                        // |/  |
                        // *   | <-- current lane
                        // |  /
                        // * / <---- next lane
                        // |/
                        // * <------ end lane
                        if (currentLane == previousLane - 1 && currentIndex + 2 <= currentLastLookAheadIndex)
                        {
                            RevisionGraphSegment segmentOrAncestor = currentRow.FirstParentOrSelf(revisionGraphSegment);
                            IRevisionGraphRow nextRow = localOrderedRowCache[currentIndex + 1];
                            int nextLane = nextRow.GetLaneForSegment(segmentOrAncestor).Index;
                            if (nextLane == currentLane)
                            {
                                segmentOrAncestor = nextRow.FirstParentOrSelf(segmentOrAncestor);
                                int endLane = localOrderedRowCache[currentIndex + 2].GetLaneForSegment(segmentOrAncestor).Index;
                                if (endLane >= 0 && endLane == nextLane - 1 && !IsPrevLaneDiagonal())
                                {
                                    currentRow.MoveLanesRight(currentLane);
                                    ++currentLane;
                                    moved = true;
                                    break; // from for revisionGraphSegment
                                }
                            }
                        }

                        moved = TurnMultiLaneCrossingIntoDiagonal(diagonalDelta: +1)
                              || TurnMultiLaneCrossingIntoDiagonal(diagonalDelta: -1);
                        if (moved)
                        {
                            break; // from for revisionGraphSegment
                        }

                        // Join multi-lane crossings
                        // Try to detect this:
                        // | | * | <-- previous lane (not diagonal)
                        // | |/ /
                        // | * | <---- current lane
                        // |,-´
                        // | <-------- next lane (not diagonal)
                        //
                        // And change it into this:
                        // | | * | <-- previous lane
                        // | |/  |
                        // | *   | <-- current lane
                        // |,---´
                        // | <-------- next lane
                        int deltaPrev = previousLane - currentLane;
                        if (previousLane >= 0 && Math.Abs(deltaPrev) >= 1)
                        {
                            RevisionGraphSegment segmentOrAncestor = currentRow.FirstParentOrSelf(revisionGraphSegment);
                            IRevisionGraphRow nextRow = localOrderedRowCache[currentIndex + 1];
                            int nextLane = nextRow.GetLaneForSegment(segmentOrAncestor).Index;
                            int deltaNext = currentLane - nextLane;
                            if (nextLane >= 0 && Math.Sign(deltaNext) == Math.Sign(deltaPrev) && Math.Abs(deltaNext + deltaPrev) >= 3 && !IsPrevLaneDiagonal(Math.Sign(deltaPrev)) && !IsNextLaneDiagonal())
                            {
                                int moveBy = deltaNext < 0 ? -deltaNext : deltaPrev;
                                currentRow.MoveLanesRight(currentLane, moveBy);
                                currentLane += moveBy;
                                moved = true;
                                break; // from for revisionGraphSegment
                            }

                            bool IsNextLaneDiagonal()
                            {
                                if (currentIndex + 2 > currentLastLookAheadIndex)
                                {
                                    return false;
                                }

                                segmentOrAncestor = nextRow.FirstParentOrSelf(segmentOrAncestor);
                                int nextNextLane = localOrderedRowCache[currentIndex + 2].GetLaneForSegment(segmentOrAncestor).Index;
                                return nextNextLane >= 0 && nextNextLane == nextLane - Math.Sign(deltaNext);
                            }
                        }

                        continue; // for revisionGraphSegment (added just as a separator for the local function)

                        bool IsPrevLaneDiagonal(int diagonalDelta = 1)
                        {
                            if (currentIndex < 2)
                            {
                                return false;
                            }

                            int prevPrevLane = localOrderedRowCache[currentIndex - 2].GetLaneForSegment(revisionGraphSegment).Index;
                            return prevPrevLane >= 0 && prevPrevLane == previousLane + diagonalDelta;
                        }

                        // Turn multi-lane crossings into diagonals
                        // Try to detect this:
                        // * | <------ previous lane
                        // | |
                        // * | <------ current lane
                        // |\-.-.
                        // * | | | <-- end lane
                        //
                        // And change it into this:
                        // * | <------ previous lane
                        // |  \
                        // *   \ <---- current lane
                        // |\-. \
                        // * | | | <-- end lane
                        //
                        // Try to detect this:
                        // * | | | <-- previous lane
                        // |/-´-´
                        // * | <------ current lane
                        // | |
                        // * | <------ end lane
                        //
                        // And change it into this:
                        // * | | | <-- previous lane
                        // |/-´ /
                        // *   / <---- current lane
                        // |  /
                        // * | <------ end lane
                        bool TurnMultiLaneCrossingIntoDiagonal(int diagonalDelta)
                        {
                            moveLaneBy.Clear();
                            RevisionGraphSegment segmentOrAncestor = revisionGraphSegment;
                            int diagonalLane = previousLane >= 0 ? previousLane : currentLane;
                            for (int lookAheadIndex = currentIndex; lookAheadIndex <= currentLastLookAheadIndex; ++lookAheadIndex)
                            {
                                diagonalLane += diagonalDelta;
                                IRevisionGraphRow endRow = localOrderedRowCache[lookAheadIndex];
                                Lane endLane = endRow.GetLaneForSegment(segmentOrAncestor);
                                int moveBy = diagonalLane - endLane.Index;
                                bool lastChance = endLane.Sharing == LaneSharing.DifferentStart;
                                if (moveBy < 0 || endLane.Index < 0 || !(endLane.Sharing == LaneSharing.ExclusiveOrPrimary || lastChance))
                                {
                                    return false;
                                }

                                // Unfold one-lane shift to diagonal, too
                                // Try to detect this:
                                // * <------ previous lane
                                // |\
                                // * | <---- current lane
                                // | |
                                // * | <---- move lane
                                // |\ \
                                // * | | <-- keep lane
                                // | | |
                                // * | | <-- end lane
                                //
                                // And change it into this:
                                // * <------ previous lane
                                // |\
                                // * \ <---- current lane
                                // |  \
                                // *   | <-- move lane
                                // |\  |
                                // * | | <-- keep lane with moveBy == 1
                                // | | |
                                // * | | <-- end lane with moveBy >= 2
                                if (moveBy >= 2 && moveLaneBy.Count == 2 && lookAheadIndex == currentIndex + 3 && moveLaneBy[1].By == 1)
                                {
                                    MoveLanes(moveLaneBy.Take(1));
                                    return true;
                                }

                                // Diagonal ends
                                if (moveBy == 0 && moveLaneBy.Count > 0)
                                {
                                    MoveLanes(moveLaneBy);
                                    return true;
                                }

                                if (lastChance)
                                {
                                    return false;
                                }

                                if (moveBy > 0)
                                {
                                    moveLaneBy.Add(new MoveLaneBy(endRow, endLane.Index, moveBy));
                                }

                                segmentOrAncestor = endRow.FirstParentOrSelf(segmentOrAncestor);
                            }

                            return false;

                            static void MoveLanes(IEnumerable<MoveLaneBy> moveLaneBy)
                            {
                                foreach ((IRevisionGraphRow row, int lane, int by) in moveLaneBy)
                                {
                                    row.MoveLanesRight(lane, by);
                                }
                            }
                        }
                    }

                    // if moved, check again whether the lanes of previous rows can be moved, too
                    currentIndex = moved ? Math.Max(currentIndex - straightenDiagonalsLookAhead, goBackLimit) : currentIndex + 1;
                }
            }
        }

        /// <summary>
        /// Build the ordered list of nodes from the revisions.
        /// </summary>
        /// <param name="currentRowIndex">The min node index for the row; is automatically limited using <see cref="Count"/>.</param>
        /// <returns>The ordered revision cache.</returns>
        private ImmutableArray<RevisionGraphRevision> BuildOrderedNodesCache(int currentRowIndex)
        {
            if (!_orderedNodesCacheInvalid && _orderedNodesCache.Length > Math.Min(Count - 1, currentRowIndex))
            {
                return _orderedNodesCache;
            }

            // The Score of the referenced nodes can be rewritten by another thread.
            // Then the cache is invalidated by setting _orderedNodesCache.
            // So if Sort() complains in this case, try again.
            while (true)
            {
                try
                {
                    // Reset flags to ensure the cache isn't marked dirty before we even got to rebuilding it.
                    _orderedNodesCacheInvalid = false;
                    _orderedNodesCache = [];

                    _orderedNodesCache = _revisionByObjectId.Values.OrderBy(n => n.Score).ToImmutableArray();

                    return _orderedNodesCache;
                }
                catch (ArgumentException ex) when (_orderedNodesCacheInvalid && ex.Message.Contains("IComparer.Compare()"))
                {
                    // ignore and try again
                }
            }
        }

        /// <summary>
        /// Mark the node and row cache as invalid if the node scores in the ordered node cache have been changed.
        /// </summary>
        /// <param name="minScore">The minimum score of the modified nodes.</param>
        /// <returns><see langword="true"/> if the row cache is invalidated; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MarkCacheAsInvalidIfNeeded(int minScore)
        {
            if (!_orderedNodesCacheInvalid)
            {
                ImmutableArray<RevisionGraphRevision> localOrderedNodesCache = _orderedNodesCache;
                if (localOrderedNodesCache.Length > 0 && minScore <= localOrderedNodesCache[^1].Score)
                {
                    _orderedNodesCacheInvalid = true;
                }
            }

            // The row cache is expensive to build, find if the built index is affected.
            // This is not expected to be a common issue (primarily inserting artificial,
            // topo order and tests).
            // Not fully optimized, invalidate the complete cache instead of only the rows after the index.
            _orderedRowCacheInvalidFromScore = minScore;

            return _orderedNodesCacheInvalid;
        }

        private readonly record struct MoveLaneBy(IRevisionGraphRow Row, int Lane, int By);

        internal TestAccessor GetTestAccessor() => new(this);

        internal readonly struct TestAccessor
        {
            private readonly RevisionGraph _revisionGraph;

            public TestAccessor(RevisionGraph revisionGraph)
            {
                _revisionGraph = revisionGraph;
            }

            /// <summary>
            /// Validate the topo order in brute force.
            /// </summary>
            /// <returns><see langword="true"/> if scores for the revisions are validated; otherwise <see langword="false"/>.</returns>
            public bool ValidateTopoOrder()
            {
                int nodeIndex = -1;
                foreach (RevisionGraphRevision node in _revisionGraph._revisionByObjectId.Values)
                {
                    ++nodeIndex;

                    int parentIndex = -1;
                    foreach (RevisionGraphRevision parent in node.Parents)
                    {
                        ++parentIndex;
                        if (parent.Score <= node.Score)
                        {
                            Trace.WriteLine($"Node {nodeIndex}:{node.Score} parent {parentIndex}:{parent.Score} has a lower score");
                            return false;
                        }
                    }

                    int childIndex = -1;
                    foreach (RevisionGraphRevision child in node.Children)
                    {
                        ++childIndex;
                        if (node.Score <= child.Score)
                        {
                            Trace.WriteLine($"Node {nodeIndex}:{node.Score} child {childIndex}:{child.Score} has a higher score");
                            return false;
                        }
                    }
                }

                return true;
            }
        }
    }
}
