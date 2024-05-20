using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    // This class represents a revision, or node.
    //     *  <- child revision
    //     |
    //     *  <- parent revision
    [DebuggerDisplay("{Objectid} {GitRevision.Subject}")]
    public class RevisionGraphRevision
    {
        // Enough as initial the majority of times because commits nearly never have more than 2 parents.
        private const int _initialParentStackCapacity = 2;
        private ImmutableStack<RevisionGraphRevision> _children = ImmutableStack<RevisionGraphRevision>.Empty;
        private readonly ConcurrentQueue<RevisionGraphSegment> _startSegments = new();

        public RevisionGraphRevision(ObjectId objectId, int guessScore)
        {
            Objectid = objectId;
            Score = guessScore;
        }

        public void ApplyFlags(bool isCheckedOut)
        {
            IsRelative |= isCheckedOut;
        }

        public bool IsRelative { get; set; }

        /// <summary>
        /// The score is used to order the revisions in topo-order. The initial score will be assigned when a revision is loaded
        /// from the commit log (the result of git.exe). The score will be adjusted, if required, when this revision is added as a parent
        /// to a revision with a higher score.
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// Override the score for the revision.
        /// </summary>
        /// <param name="score">The new score.</param>
        public void OverrideScore(int score) => Score = score;

        /// <summary>
        /// This method is called to ensure that the score is higher than a given score.
        /// E.g. the score needs to be higher that the score of its children.
        /// Warning: On big repositories, this method could be **really** costly.
        /// </summary>
        /// <param name="minimalScore">The minimal score to set.</param>
        /// <returns>true if Score was updated.</returns>
        public int EnsureScoreIsAbove(int minimalScore)
        {
            if (minimalScore <= Score)
            {
                return Score;
            }

            Score = minimalScore;

            if (ParentCount == 0)
            {
                return Score;
            }

            int traversalCount = 0;
            int maxScore = Score;

            Stack<RevisionGraphRevision> stack = new(_initialParentStackCapacity);
            stack.Push(this);
            while (stack.Count > 0)
            {
                RevisionGraphRevision revision = stack.Pop();

                RevisionGraphRevision previous = null;
                foreach (RevisionGraphSegment segment in revision._startSegments)
                {
                    RevisionGraphRevision parent = segment.Parent;
                    if (parent.Score > revision.Score)
                    {
                        continue;
                    }

                    traversalCount++;
                    parent.Score = revision.Score + 1;

                    maxScore = Math.Max(parent.Score, maxScore);

                    // Without using a collection (due to performance cost),
                    // try to queue first parent with a more complex history (i.e. more parents)
                    // by comparing **only** with previous sibling.
                    if (previous is null)
                    {
                        previous = parent;
                    }
                    else
                    {
                        if (previous._startSegments.Count >= parent._startSegments.Count)
                        {
                            stack.Push(previous);
                            previous = parent;
                        }
                        else
                        {
                            stack.Push(parent);
                        }
                    }
                }

                if (previous is not null)
                {
                    stack.Push(previous);
                }
            }

            if (traversalCount > 1_000_000)
            {
                Debug.WriteLine($"performance: Consider enabling git log commit sorting... (CommitId: {Objectid} / Traversal count: {traversalCount})");
            }

            return maxScore;
        }

        public GitRevision? GitRevision { get; set; }

        public ObjectId Objectid { get; }

        public int ParentCount { get; private set; }
        public IEnumerable<RevisionGraphRevision> Parents => _startSegments.Select(segment => segment.Parent);
        public ImmutableStack<RevisionGraphRevision> Children => _children;
        public RevisionGraphSegment[] GetStartSegments() => _startSegments.ToArray();

        /// <summary>
        /// Mark this commit, and all its parents, as relative. Used for branch highlighting.
        /// By default, the current checkout will be marked relative.
        /// </summary>
        public void MakeRelative()
        {
            if (IsRelative)
            {
                return;
            }

            if (ParentCount == 0)
            {
                IsRelative = true;
                return;
            }

            Stack<RevisionGraphRevision> stack = new(_initialParentStackCapacity);
            stack.Push(this);

            while (stack.Count > 0)
            {
                RevisionGraphRevision revision = stack.Pop();

                revision.IsRelative = true;

                foreach (RevisionGraphSegment segment in revision._startSegments)
                {
                    RevisionGraphRevision parent = segment.Parent;
                    if (parent.IsRelative)
                    {
                        continue;
                    }

                    stack.Push(parent);
                }
            }
        }

        /// <summary>
        /// Add a parent to this revision.
        /// </summary>
        /// <param name="parent">The parent to add.</param>
        public void AddParent(RevisionGraphRevision parent)
        {
            DebugHelpers.Assert(parent.Score > Score, "Parent score must be higher than for the child.");

            if (IsRelative)
            {
                parent.MakeRelative();
            }

            parent.AddChild(this);

            _startSegments.Enqueue(new RevisionGraphSegment(parent, this));
            ++ParentCount;
        }

        private void AddChild(RevisionGraphRevision child)
        {
            ImmutableInterlocked.Push(ref _children, child);
        }

        internal TestAccessor GetTestAccessor() => new(this);

        internal readonly struct TestAccessor
        {
            private readonly RevisionGraphRevision _form;

            public TestAccessor(RevisionGraphRevision form)
            {
                _form = form;
            }

            /// <summary>
            /// Add a parent to this revision.
            /// The parent score is checked to be higher than the score of this revision,
            /// but siblings are not considered (which is OK in tests).
            /// </summary>
            /// <param name="parent">The parent to add.</param>
            public void AddParent(RevisionGraphRevision parent)
            {
                // This adjustment was previously in AddParent(), moved out for performance reasons.
                parent.EnsureScoreIsAbove(_form.Score + 1);

                _form.AddParent(parent);
            }
        }
    }
}
