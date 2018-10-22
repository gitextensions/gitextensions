using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    public class RevisionGraphRevision
    {
        public RevisionGraphRevision(ObjectId objectId, int guessScore)
        {
            Objectid = objectId;

            Parents = new ConcurrentBag<RevisionGraphRevision>();
            Children = new ConcurrentBag<RevisionGraphRevision>();
            StartSegments = new SynchronizedCollection<RevisionGraphSegment>();
            EndSegments = new ConcurrentBag<RevisionGraphSegment>();

            Score = guessScore;
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

        public int Score { get; private set; }

        public int LaneColor { get; set; }

        public int IncreaseScore(int delta)
        {
            if (delta + 1 > Score)
            {
                Score = delta + 1;

                int maxScore = Score;
                foreach (RevisionGraphRevision parent in Parents)
                {
                    maxScore = Math.Max(parent.IncreaseScore(Score), maxScore);
                }

                return maxScore;
            }

            return Score;
        }

        public GitRevision GitRevision { get; set; }

        public ObjectId Objectid { get; set; }

        public ConcurrentBag<RevisionGraphRevision> Parents { get; private set; }
        public ConcurrentBag<RevisionGraphRevision> Children { get; private set; }
        public SynchronizedCollection<RevisionGraphSegment> StartSegments { get; private set; }
        public ConcurrentBag<RevisionGraphSegment> EndSegments { get; private set; }

        public void MakeRelative()
        {
            if (!IsRelative)
            {
                IsRelative = true;

                foreach (RevisionGraphRevision parent in Parents)
                {
                    parent.MakeRelative();
                }
            }
        }

        public RevisionGraphSegment AddParent(RevisionGraphRevision parent, out int maxScore)
        {
            // Generate a LaneColor used for rendering
            parent.LaneColor = Parents.Any() ? parent.Objectid.GetHashCode() : LaneColor;

            if (IsRelative)
            {
                parent.MakeRelative();
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
    }
}
