using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid.Graph;

namespace GitUI.UserControls.RevisionGrid.Columns;

/// <summary>
///  Computes the set of commit IDs to highlight when hovering a ref label in the revision graph.
///  Debounces computation and owns the highlight state and cancellation sequence.
/// </summary>
internal sealed class HoverHighlightCalculator : IDisposable
{
    private const int _debounceMs = 100;

    private readonly RevisionGraph _revisionGraph;
    private readonly Func<VisibleRowRange> _getCachedVisibleRange;
    private readonly CancellationTokenSequence _sequence = new();

    public HoverHighlightCalculator(RevisionGraph revisionGraph, Func<VisibleRowRange> getCachedVisibleRange)
    {
        _revisionGraph = revisionGraph;
        _getCachedVisibleRange = getCachedVisibleRange;
    }

    /// <summary>
    ///  The current set of highlighted commit IDs, or <see langword="null"/> if none are highlighted.
    /// </summary>
    public IReadOnlySet<ObjectId>? HighlightedIds { get; private set; }

    /// <summary>
    ///  <see langword="true"/> if the highlight changed since the last call to <see cref="ConsumeIsDirty"/>.
    /// </summary>
    public bool IsDirty { get; private set; }

    /// <summary>
    ///  Returns the current dirty state and resets it to <see langword="false"/>.
    /// </summary>
    public bool ConsumeIsDirty()
    {
        bool dirty = IsDirty;
        IsDirty = false;
        return dirty;
    }

    /// <summary>
    ///  Clears the hover highlight and marks the state as dirty.
    /// </summary>
    public void Clear()
    {
        if (HighlightedIds is null)
        {
            return;
        }

        HighlightedIds = null;
        IsDirty = true;
    }

    /// <summary>
    ///  Updates the hover highlight to show only the ancestry of the
    ///  <paramref name="gitRef"/> and tracked remote or the tracking local.
    ///  Debounces by <see cref="_debounceMs"/> ms before computing,
    ///  cancelling any prior pending computation when called again.
    ///  Set <see langword="null"/> to clear hover highlighting.
    /// </summary>
    /// <param name="gitRef">The ref to highlight, or <see langword="null"/> to clear.</param>
    /// <param name="rowIndex">
    ///  The row index of the hovered ref label, limits the search to the visible range.
    /// </param>
    public async Task SetAsync(IGitRef? gitRef, int rowIndex = -1)
    {
        CancellationToken cancellationToken = _sequence.Next();
        await Task.Delay(_debounceMs, cancellationToken);
        Compute(gitRef, rowIndex, cancellationToken);
    }

    private void Compute(IGitRef? gitRef, int rowIndex, CancellationToken cancellationToken)
    {
        if (gitRef is null || rowIndex < 0)
        {
            Clear();
            return;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        VisibleRowRange visibleRange = _getCachedVisibleRange();
        HashSet<ObjectId> ancestorIds = [];

        // Build the set of currently visible ObjectIds below current row (and some parents/children).
        if (_revisionGraph.GetNodeForRow(rowIndex) is not { } hoveredRevision)
        {
            throw new OperationCanceledException();
        }

        int maxChildrenAbove = Math.Max(50, visibleRange.Count);
        HashSet<ObjectId> visibleIds = new(capacity: maxChildrenAbove + (2 * visibleRange.Count));

        // Add visible ids and their parents, find if a tracked branch is in the set
        AddIdAndParents(_revisionGraph, rowIndex, visibleIds);

        bool checkOtherRefs = gitRef is NestledVirtualRef nestledRef
            ? !nestledRef.TrackingBranchIsGone
            : (gitRef.IsRemote || (gitRef.IsHead && !string.IsNullOrEmpty(gitRef.MergeWith)))
                && !hoveredRevision.GitRevision?.Refs.Any(r => IsInBranchGroup(r, gitRef)) is true;

        RevisionGraphRevision? belowRev = null;
        int visibleTo = visibleRange.FromIndex + visibleRange.Count - 1;
        for (int row = rowIndex + 1; row <= visibleTo; row++)
        {
            RevisionGraphRevision rev = AddIdAndParents(_revisionGraph, row, visibleIds);
            if (checkOtherRefs && rev.GitRevision?.Refs.Any(r => IsInBranchGroup(r, gitRef)) is true)
            {
                belowRev = rev;
            }
        }

        WalkAncestors(hoveredRevision, ancestorIds, visibleIds);

        if (belowRev is not null)
        {
            WalkAncestors(belowRev, ancestorIds, visibleIds);
        }
        else if (checkOtherRefs)
        {
            // Rows above rowIndex are only needed for the upward search
            int searchFrom = Math.Max(0, visibleRange.FromIndex - maxChildrenAbove);
            for (int row = rowIndex - 1; row >= searchFrom; row--)
            {
                RevisionGraphRevision rev = AddIdAndParents(_revisionGraph, row, visibleIds);
                if (rev.GitRevision?.Refs.Any(r => IsInBranchGroup(r, gitRef)) is true)
                {
                    WalkAncestors(rev, ancestorIds, visibleIds);
                    break;
                }
            }
        }

        IReadOnlySet<ObjectId>? highlightedIds = ancestorIds.Count > 0 ? ancestorIds : null;

        if (HighlightIdsSameValues(HighlightedIds, highlightedIds))
        {
            throw new OperationCanceledException();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        HighlightedIds = highlightedIds;
        IsDirty = true;

        return;

        static RevisionGraphRevision AddIdAndParents(RevisionGraph revisionGraph, int row, HashSet<ObjectId> visibleIds)
        {
            RevisionGraphRevision? rev = revisionGraph.GetNodeForRow(row);
            if (rev is null)
            {
                throw new OperationCanceledException();
            }

            visibleIds.Add(rev.Objectid);

            // A segment whose Child is above the visible area still crosses visible rows and must
            // be highlighted. Include the Parent endpoint of every segment leaving below so that
            // WalkAncestors can reach them without storing all ancestors.
            if (revisionGraph.GetSegmentsForRow(row) is not { } graphRow)
            {
                return rev;
            }

            foreach (RevisionGraphSegment segment in graphRow.Segments)
            {
                visibleIds.Add(segment.Parent.Objectid);
            }

            return rev;
        }

        static bool IsInBranchGroup(IGitRef r, IGitRef gitRef)
            => gitRef.IsTrackingRemote(r)
            || r.IsTrackingRemote(gitRef)
            || (gitRef is NestledVirtualRef { TrackingBranchIsGone: false } && gitRef.CompleteName == r.CompleteName);

        static void WalkAncestors(RevisionGraphRevision revision, HashSet<ObjectId> result, IReadOnlySet<ObjectId> visibleIds)
        {
            Stack<RevisionGraphRevision> stack = [];
            HashSet<ObjectId> visited = [];
            stack.Push(revision);
            while (stack.Count > 0)
            {
                RevisionGraphRevision current = stack.Pop();
                if (!visited.Add(current.Objectid))
                {
                    continue;
                }

                // If already in result from a previous WalkAncestors call, all ancestors
                // of this commit were already processed — prune this entire subtree.
                if (result.Contains(current.Objectid))
                {
                    continue;
                }

                // Only store revisions that are in the visible set (commit nodes or segment
                // endpoints crossing the viewport boundary). The visible set is pre-extended
                // with one segment above and below, so spanning segments are handled.
                if (visibleIds.Contains(current.Objectid))
                {
                    result.Add(current.Objectid);
                }

                foreach (RevisionGraphRevision parent in current.Parents)
                {
                    stack.Push(parent);
                }
            }
        }

        static bool HighlightIdsSameValues(IReadOnlySet<ObjectId>? left, IReadOnlySet<ObjectId>? right)
        {
            if (left is null || right is null)
            {
                return left is null && right is null;
            }

            return left.SetEquals(right);
        }
    }

    public void Dispose() => _sequence.Dispose();

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly HoverHighlightCalculator _calculator;

        internal TestAccessor(HoverHighlightCalculator calculator)
        {
            _calculator = calculator;
        }

        internal IReadOnlySet<ObjectId>? HighlightedIds => _calculator.HighlightedIds;

        internal bool IsDirty => _calculator.IsDirty;

        internal void SetHoverHighlight(IGitRef? gitRef, int rowIndex = -1)
            => _calculator.Compute(gitRef, rowIndex, CancellationToken.None);
    }
}
