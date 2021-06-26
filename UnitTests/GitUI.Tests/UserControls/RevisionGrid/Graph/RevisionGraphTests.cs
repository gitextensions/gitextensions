using System.Collections.Generic;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitUITests.UserControls.RevisionGrid
{
    [TestFixture]
    public class RevisionGraphTests
    {
        private RevisionGraph _revisionGraph;

        [SetUp]
        public void Setup()
        {
            _revisionGraph = new RevisionGraph();

            foreach (var revision in Revisions)
            {
                // Mark the first revision as the current checkout
                _revisionGraph.Add(revision, _revisionGraph.Count == 0 ? RevisionNodeFlags.CheckedOut : RevisionNodeFlags.None);
            }
        }

        [Test]
        public void ShouldBeAbleToCacheGraphTo()
        {
            Assert.AreEqual(0, _revisionGraph.GetCachedCount());
            _revisionGraph.CacheTo(4, 2);
            Assert.AreEqual(3, _revisionGraph.GetCachedCount());
            _revisionGraph.CacheTo(4, 4);
            Assert.AreEqual(5, _revisionGraph.GetCachedCount());
            _revisionGraph.CacheTo(400, 400);
            Assert.AreEqual(6, _revisionGraph.GetCachedCount());
            _revisionGraph.LoadingCompleted();
            Assert.AreEqual(6 + LookAhead, _revisionGraph.GetCachedCount());
        }

        [Test]
        public void ShouldBeAbleToClear()
        {
            Assert.AreEqual(6 + LookAhead, _revisionGraph.Count);
            _revisionGraph.Clear();
            Assert.AreEqual(0, _revisionGraph.Count);
        }

        [Test]
        public void ShouldBeAbleToHighlightBranch()
        {
            _revisionGraph.CacheTo(_revisionGraph.Count, _revisionGraph.Count);
            Assert.IsTrue(_revisionGraph.GetNodeForRow(0).IsRelative);
            Assert.IsTrue(_revisionGraph.GetNodeForRow(1).IsRelative);
            Assert.IsTrue(_revisionGraph.GetNodeForRow(4).IsRelative);
            _revisionGraph.HighlightBranch(_revisionGraph.GetNodeForRow(1).Objectid);
            Assert.IsFalse(_revisionGraph.GetNodeForRow(0).IsRelative);
            Assert.IsTrue(_revisionGraph.GetNodeForRow(1).IsRelative);
            Assert.IsTrue(_revisionGraph.GetNodeForRow(4).IsRelative);
        }

        [Test]
        public void ShouldBeAbleToGetLaneCount()
        {
            _revisionGraph.CacheTo(_revisionGraph.Count, _revisionGraph.Count);
            Assert.AreEqual(1, _revisionGraph.GetSegmentsForRow(0).GetLaneCount());
            Assert.AreEqual(1, _revisionGraph.GetSegmentsForRow(1).GetLaneCount());
            Assert.AreEqual(2, _revisionGraph.GetSegmentsForRow(2).GetLaneCount());
            Assert.AreEqual(2, _revisionGraph.GetSegmentsForRow(3).GetLaneCount());
            Assert.AreEqual(1, _revisionGraph.GetSegmentsForRow(4).GetLaneCount());
            Assert.AreEqual(1, _revisionGraph.GetSegmentsForRow(5).GetLaneCount());
        }

        [Test]
        public void ShouldReorderInTopoOrder()
        {
            _revisionGraph.CacheTo(_revisionGraph.Count, _revisionGraph.Count);
            Assert.IsTrue(_revisionGraph.GetTestAccessor().ValidateTopoOrder());

            GitRevision commit1 = new(ObjectId.Random());

            GitRevision commit2 = new(ObjectId.Random());
            commit1.ParentIds = new ObjectId[] { commit2.ObjectId };
            commit2.ParentIds = new ObjectId[] { _revisionGraph.GetNodeForRow(4).Objectid };

            _revisionGraph.Add(commit2, RevisionNodeFlags.None); // This commit is now dangling

            _revisionGraph.CacheTo(_revisionGraph.Count, _revisionGraph.Count);
            Assert.IsTrue(_revisionGraph.GetTestAccessor().ValidateTopoOrder());

            _revisionGraph.Add(commit1, RevisionNodeFlags.None); // Add the connecting commit

            _revisionGraph.CacheTo(_revisionGraph.Count, _revisionGraph.Count);
            Assert.IsTrue(_revisionGraph.GetTestAccessor().ValidateTopoOrder());

            // Add a new head
            GitRevision newHead = new(ObjectId.Random());
            newHead.ParentIds = new ObjectId[] { _revisionGraph.GetNodeForRow(0).Objectid };
            _revisionGraph.Add(newHead, RevisionNodeFlags.None); // Add commit that has the current top node as parent.

            _revisionGraph.CacheTo(_revisionGraph.Count, _revisionGraph.Count); // Call to cache fix the order
            Assert.IsTrue(_revisionGraph.GetTestAccessor().ValidateTopoOrder());
        }

        [Test] // github issue #6193
        public void CacheEmptyGraph()
        {
            _revisionGraph.Clear();
            _revisionGraph.CacheTo(100, 100);
            _revisionGraph.CacheTo(100, 100);
        }

        [Test] // github issue #6210
        public void DetachedSingleRevision()
        {
            _revisionGraph.Clear();

            /* Visualization of commit graph:
             *     Commit1
             *        |
             *        |       Commit2 (detached)
             *        |
             *     Commit3
             */

            GitRevision commit1 = new(ObjectId.Random());

            GitRevision commit2 = new(ObjectId.Random());

            GitRevision commit3 = new(ObjectId.Random());
            commit1.ParentIds = new ObjectId[] { commit3.ObjectId };

            _revisionGraph.Add(commit1, RevisionNodeFlags.None);
            _revisionGraph.Add(commit2, RevisionNodeFlags.None);
            _revisionGraph.Add(commit3, RevisionNodeFlags.None);

            _revisionGraph.CacheTo(_revisionGraph.Count, _revisionGraph.Count);

            Assert.AreEqual(1, _revisionGraph.GetSegmentsForRow(1).GetCurrentRevisionLane());
        }

        private static int LookAhead => 20;

        private static IEnumerable<GitRevision> Revisions
        {
            get
            {
                /* Visualization of commit graph:
                 *     Commit1
                 *        |
                 *     Commit2
                 *    /       \
                 * Commit3     |
                 *   |         |
                 *   |       Commit4
                 *    \       /
                 *     Commit5
                 *        |
                 *     Commit6
                 *        |
                 *       ...
                 *        |
                 *     Commit(6 + LookAhead)
                 */
                GitRevision commit1 = new(ObjectId.Random());

                GitRevision commit2 = new(ObjectId.Random());
                commit1.ParentIds = new ObjectId[] { commit2.ObjectId };

                GitRevision commit3 = new(ObjectId.Random());
                GitRevision commit4 = new(ObjectId.Random());
                commit2.ParentIds = new ObjectId[] { commit3.ObjectId, commit4.ObjectId };

                GitRevision commit5 = new(ObjectId.Random());
                commit3.ParentIds = new ObjectId[] { commit5.ObjectId };
                commit4.ParentIds = new ObjectId[] { commit5.ObjectId };

                yield return commit1;
                yield return commit2;
                yield return commit3;
                yield return commit4;

                GitRevision prevCommit = commit5;
                for (int i = 0; i < 1 + LookAhead; ++i)
                {
                    GitRevision currCommit = new(ObjectId.Random());
                    prevCommit.ParentIds = new ObjectId[] { currCommit.ObjectId };
                    yield return prevCommit;
                    prevCommit = currCommit;
                }

                prevCommit.ParentIds = new ObjectId[] { };
                yield return prevCommit;
            }
        }
    }
}
