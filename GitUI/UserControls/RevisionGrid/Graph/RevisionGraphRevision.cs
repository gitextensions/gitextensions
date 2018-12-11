﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    // This class represents a revision, or node.
    //     *  <- child revision
    //     |
    //     *  <- parent revision
    public class RevisionGraphRevision
    {
        public RevisionGraphRevision(ObjectId objectId, int guessScore)
        {
            Objectid = objectId;

            Parents = new ConcurrentBag<RevisionGraphRevision>();
            Children = new ConcurrentBag<RevisionGraphRevision>();
            StartSegments = new SynchronizedCollection<RevisionGraphSegment>();

            Score = guessScore;

            LaneColor = -1;
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

        /// <summary>
        /// The score is used to order the revisions in topo-order. The initial score will be assigned when a revision is loaded
        /// from the commit log (the result of git.exe). The score will be adjusted, if required, when this revision is added as a parent
        /// to a revision with a higher score.
        /// </summary>
        public int Score { get; private set; }

        public int LaneColor { get; set; }

        // This method is called to ensure that the score is higher than a given score.
        // E.g. the score needs to be higher that the score of its children.
        public int EnsureScoreIsAbove(int minimalScore)
        {
            if (minimalScore <= Score)
            {
                return Score;
            }

            Score = minimalScore;

            if (!Parents.Any())
            {
                return Score;
            }

            int maxScore = Score;

            var stack = new Stack<RevisionGraphRevision>();
            stack.Push(this);
            while (stack.Count > 0)
            {
                var revision = stack.Pop();

                foreach (var parent in revision.Parents.Where(r => r.Score <= revision.Score))
                {
                    parent.Score = revision.Score + 1;

                    Debug.Assert(parent.Score > revision.Score, "Reorder score failed.");

                    maxScore = Math.Max(parent.Score, maxScore);
                    stack.Push(parent);
                }
            }

            return maxScore;
        }

        public GitRevision GitRevision { get; set; }

        public ObjectId Objectid { get; set; }

        public ConcurrentBag<RevisionGraphRevision> Parents { get; }
        public ConcurrentBag<RevisionGraphRevision> Children { get; }
        public SynchronizedCollection<RevisionGraphSegment> StartSegments { get; }

        // Mark this commit, and all its parents, as relative. Used for branch highlighting.
        // By default, the current checkout will be marked relative.
        public void MakeRelative()
        {
            if (IsRelative)
            {
                return;
            }

            if (!Parents.Any())
            {
                IsRelative = true;
                return;
            }

            var stack = new Stack<RevisionGraphRevision>();
            stack.Push(this);

            while (stack.Count > 0)
            {
                var revision = stack.Pop();

                revision.IsRelative = true;

                foreach (var parent in revision.Parents.Where(r => !r.IsRelative))
                {
                    stack.Push(parent);
                }
            }
        }

        public void AddParent(RevisionGraphRevision parent, out int maxScore)
        {
            // Generate a LaneColor used for rendering
            if (Parents.Any())
            {
                parent.LaneColor = parent.Score;
            }
            else
            {
                if (parent.LaneColor == -1)
                {
                    parent.LaneColor = LaneColor;
                }
            }

            if (IsRelative)
            {
                parent.MakeRelative();
            }

            Parents.Add(parent);
            parent.AddChild(this);

            maxScore = parent.EnsureScoreIsAbove(Score + 1);

            StartSegments.Add(new RevisionGraphSegment(parent, this));
        }

        private void AddChild(RevisionGraphRevision child)
        {
            Children.Add(child);
        }
    }
}
