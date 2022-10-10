﻿using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    // This class represents a revision, or node.
    //     *  <- child revision
    //     |
    //     *  <- parent revision
    [DebuggerDisplay("{Objectid}")]
    public class RevisionGraphRevision
    {
        private ImmutableStack<RevisionGraphRevision> _parents = ImmutableStack<RevisionGraphRevision>.Empty;
        private ImmutableStack<RevisionGraphRevision> _children = ImmutableStack<RevisionGraphRevision>.Empty;
        private ConcurrentQueue<RevisionGraphSegment> _startSegments = new();

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
        /// Offset the score for the revision.
        /// If negative decreasing the score, i.e. increasing priority.
        /// </summary>
        /// <param name="offset">The offset to the current score.</param>
        public void OffsetScore(int offset) => Score += offset;

        // This method is called to ensure that the score is higher than a given score.
        // E.g. the score needs to be higher that the score of its children.
        public int EnsureScoreIsAbove(int minimalScore)
        {
            if (minimalScore <= Score)
            {
                return Score;
            }

            Score = minimalScore;

            if (Parents.IsEmpty)
            {
                return Score;
            }

            int maxScore = Score;

            Stack<RevisionGraphRevision> stack = new();
            stack.Push(this);
            while (stack.Count > 0)
            {
                var revision = stack.Pop();

                foreach (var parent in revision.Parents)
                {
                    if (parent.Score > revision.Score)
                    {
                        continue;
                    }

                    parent.Score = revision.Score + 1;

                    Debug.Assert(parent.Score > revision.Score, "Reorder score failed.");

                    maxScore = Math.Max(parent.Score, maxScore);
                    stack.Push(parent);
                }
            }

            return maxScore;
        }

        public GitRevision? GitRevision { get; set; }

        public ObjectId Objectid { get; }

        public ImmutableStack<RevisionGraphRevision> Parents => _parents;
        public ImmutableStack<RevisionGraphRevision> Children => _children;
        public RevisionGraphSegment[] GetStartSegments() => _startSegments.ToArray();

        // Mark this commit, and all its parents, as relative. Used for branch highlighting.
        // By default, the current checkout will be marked relative.
        public void MakeRelative()
        {
            if (IsRelative)
            {
                return;
            }

            if (Parents.IsEmpty)
            {
                IsRelative = true;
                return;
            }

            Stack<RevisionGraphRevision> stack = new();
            stack.Push(this);

            while (stack.Count > 0)
            {
                var revision = stack.Pop();

                revision.IsRelative = true;

                foreach (var parent in revision.Parents)
                {
                    if (parent.IsRelative)
                    {
                        continue;
                    }

                    stack.Push(parent);
                }
            }
        }

        public void AddParent(RevisionGraphRevision parent, out int maxScore)
        {
            if (IsRelative)
            {
                parent.MakeRelative();
            }

            ImmutableInterlocked.Push(ref _parents, parent);
            parent.AddChild(this);

            maxScore = parent.EnsureScoreIsAbove(Score + 1);

            _startSegments.Enqueue(new RevisionGraphSegment(parent, this));
        }

        private void AddChild(RevisionGraphRevision child)
        {
            ImmutableInterlocked.Push(ref _children, child);
        }
    }
}
