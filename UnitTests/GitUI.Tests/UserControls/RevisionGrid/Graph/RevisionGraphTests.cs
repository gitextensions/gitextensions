using FluentAssertions.Execution;
using GitCommands;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;

namespace GitUITests.UserControls.RevisionGrid
{
    [TestFixture]
    public class RevisionGraphTests
    {
        private RevisionGraph _revisionGraph;

        [SetUp]
        public void Setup()
        {
            AppSettings.MergeGraphLanesHavingCommonParent.Value = true;
            AppSettings.ReduceGraphCrossings.Value = false;
            AppSettings.StraightenGraphDiagonals.Value = false;

            _revisionGraph = new RevisionGraph();

            foreach (GitRevision revision in Revisions)
            {
                // Mark the first revision as the current checkout
                if (_revisionGraph.Count == 0)
                {
                    _revisionGraph.HeadId = revision.ObjectId;
                }

                _revisionGraph.Add(revision);
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

            _revisionGraph.Add(commit2); // This commit is now dangling

            _revisionGraph.CacheTo(_revisionGraph.Count, _revisionGraph.Count);
            Assert.IsTrue(_revisionGraph.GetTestAccessor().ValidateTopoOrder());

            _revisionGraph.Add(commit1); // Add the connecting commit

            _revisionGraph.CacheTo(_revisionGraph.Count, _revisionGraph.Count);
            Assert.IsTrue(_revisionGraph.GetTestAccessor().ValidateTopoOrder());

            // Add a new head
            GitRevision newHead = new(ObjectId.Random());
            newHead.ParentIds = new ObjectId[] { _revisionGraph.GetNodeForRow(0).Objectid };
            _revisionGraph.Add(newHead); // Add commit that has the current top node as parent.

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

            _revisionGraph.Add(commit1);
            _revisionGraph.Add(commit2);
            _revisionGraph.Add(commit3);
            _revisionGraph.LoadingCompleted();

            _revisionGraph.CacheTo(_revisionGraph.Count, _revisionGraph.Count);

            Assert.AreEqual(1, _revisionGraph.GetSegmentsForRow(1).GetCurrentRevisionLane());

            AssertGraphLayout(_revisionGraph, @"
                *
                |
                | *
                |
                *
            ");
        }

        [Test]
        public void SegmentsAreStraightened()
        {
            RevisionGraph revisionGraph = CreateGraph(" 1  2:1  3:1  4:1,3  5:4  6:5  7:5,6  8:7,2 ");

            AssertGraphLayout(revisionGraph, @"
                8
                |\
                7 |
                |\ \
                | 6 |
                |/  |
                5   |
                |   |
                4   |
                |\  |
                | 3 |
                |/ /
                | 2
                |/
                1
            ");
        }

        [Test]
        public void SegmentsWithCommitsAreStraightened()
        {
            RevisionGraph revisionGraph = CreateGraph(" 1  2:1  3:1  4:1,3  5:2  6:5  7:4  8:4,7  9:8,6 ");

            AssertGraphLayout(revisionGraph, @"
                9
                |\
                8 |
                |\ \
                | 7 |
                |/  |
                |   6
                |   |
                |   5
                |   |
                4   |
                |\  |
                | 3 |
                |/ /
                | 2
                |/
                1
            ");
        }

        [Test]
        public void SegmentsWithOutgoingSecondaryMergesAreNotStraightened()
        {
            RevisionGraph revisionGraph = CreateGraph(" 1  2:1  3:1  4:1,3  5:2  6:4,5  7:6  8:6,7  9:8,5 ");

            AssertGraphLayout(revisionGraph, @"
                9
                |\
                8 |
                |\ \
                | 7 |
                |/ /
                6 |
                |\|
                | 5
                | |
                4 |
                |\ \
                | 3 |
                |/ /
                | 2
                |/
                1
            ");
        }

        [Test]
        public void SegmentsWithIncomingMergesAreStraightened()
        {
            RevisionGraph revisionGraph = CreateGraph(" 1  2:1  3:1  4:1,3  5:2,4  6:4  7:4,6  8:7,5 ");

            AssertGraphLayout(revisionGraph, @"
                8
                |\
                7 |
                |\ \
                | 6 |
                |/  |
                |   5
                |--´|
                4   |
                |\  |
                | 3 |
                |/ /
                | 2
                |/
                1
            ");
        }

        [Test]
        public void SegmentsAreStraightenedAlthoughThisCausesWidthIncrease()
        {
            RevisionGraph revisionGraph = CreateGraph(" 1  2:1  3:1  4:1  5:1,4  6:2  7:2,6  8:5  9:5,8,3,7 ");

            AssertGraphLayout(revisionGraph, @"
                9
                |\----ˎ
                | 8 | |
                |/  | |
                |   | 7
                |   | |\
                |   | | 6
                |   | |/
                5   | |
                |\  | |
                | 4 | |
                |/ / /
                | 3 |
                |/ /
                | 2
                |/
                1
            ");
        }

        [Test]
        public void SegmentsWithOutgoingPrimaryMergesAreStraightened()
        {
            RevisionGraph revisionGraph = CreateGraph(" 1  2:1  3:1  6:1  7:1,6  8:3,2  9:7  0:7,9,8 ");

            AssertGraphLayout(revisionGraph, @"
                0
                |\--ˎ
                | 9 |
                |/  |
                |   8
                |   |\
                7   | |
                |\  | |
                | 6 | |
                |/ / /
                | 3 |
                |/ /
                | 2
                |/
                1
            ");
        }

        [Test]
        public void SegmentsAreNotStraightenedIfThisCausesAShiftForPrimarySegment()
        {
            RevisionGraph revisionGraph = CreateGraph(" 1  a:1  b:1  2:1  3:1  4:1  5:4,1  6:3  7:5,6  8:7,2,6  c:8  d:8  e:8  9:8,e,d,c,b,a ");

            // Two segments cross at the 'X'. The one going '/' could be straightened,
            // but then it would shift the parent node causing an unwanted gap.

            AssertGraphLayout(revisionGraph, @"
                9
                |\--------ˎ
                | e | | | |
                |/ / /  | |
                | d |   | |
                |/ /    | |
                | c     | |
                |/      | |
                8       | |
                |\--ˎ   | |
                7 | |   | |
                |\ X    | |
                | 6 |   | |
                | | |   | |
                5 | |   | |
                |\ \ \  | |
                4 | | | | |
                |/ / / / /
                | 3 | | |
                |/ / / /
                | 2 | |
                |/ / /
                | b |
                |/ /
                | a
                |/
                1
            ");
        }

        [Test]
        public void SegmentsAreNotStraightenedOverMultiLaneCrossings()
        {
            const string graphWithMultiLaneCrossings = "0:C,1,2,3,4,5,6,7,8,9,A,B 1:R 2:R 3:R 4:C 5:C 6:C 7:R 8:C 9:R A:C B:R C:D D:E E:F F:G G:H,K,R H:I,R I:J,R J:R K:R R";

            RevisionGraph revisionGraph = CreateGraphTopDown(graphWithMultiLaneCrossings);

            // No need to straighten the segments 7:R, 9:R, B:R at rows C to G because shared.
            AssertGraphLayout(revisionGraph, @"
                0
                |\--------------------ˎ
                | 1 | | | | | | | | | |
                | | | | | | | | | | | |
                | | 2 | | | | | | | | |
                | |/ / / / / / / / / /
                | | 3 | | | | | | | |
                | |/ / / / / / / / /
                | | 4 | | | | | | |
                |-|´ / / / / / / /
                | | 5 | | | | | |
                |-|´ / / / / / /
                | | 6 | | | | |
                |-|´ / / / / /
                | | 7 | | | |
                | |/ / / / /
                | | 8 | | |
                |-|´ / / /
                | | 9 | |
                | |/ / /
                | | A |
                |-|´ /
                | | B
                | |/
                C |
                | |
                D |
                | |
                E |
                | |
                F |
                | |
                G |
                |\-\ˎ
                H | |
                |\ /
                I | |
                |\| |
                J | |
                |/ /
                | K
                |/
                R
            ");

            AppSettings.MergeGraphLanesHavingCommonParent.Value = false;

            revisionGraph = CreateGraphTopDown(graphWithMultiLaneCrossings);

            // Do not move the segments 7:R, 9:R, B:R at rows C to G.
            AssertGraphLayout(revisionGraph, @"
                0
                |\--------------------ˎ
                | 1 | | | | | | | | | |
                | | | | | | | | | | | |
                | | 2 | | | | | | | | |
                | | | | | | | | | | | |
                | | | 3 | | | | | | | |
                | | | | | | | | | | | |
                | | | | 4 | | | | | | |
                | | | | | | | | | | | |
                | | | | | 5 | | | | | |
                | | | | | | | | | | | |
                | | | | | | 6 | | | | |
                | | | | | | | | | | | |
                | | | | | | | 7 | | | |
                | | | | | | | | | | | |
                | | | | | | | | 8 | | |
                | | | | | | | | | | | |
                | | | | | | | | | 9 | |
                | | | | | | | | | | | |
                | | | | | | | | | | A |
                | | | | | | | | | | | |
                | | | | | | | | | | | B
                |-|-|-|-----,--------´
                C | | | | | |
                | | | | | | |
                D | | | | | |
                | | | | | | |
                E | | | | | |
                | | | | | | |
                F | | | | | |
                | | | | | | |
                G | | | | | |
                |\-`-`-`-`-`-`--ˎ
                H | | | | | | | |
                |\ \ \ \ \ \ \ \ \
                I | | | | | | | | |
                |\ \ \ \ \ \ \ \ \ \
                J | | | | | | | | | |
                | | | | | | | | | | |
                | | | K | | | | | | |
                |/-----------------´
                R
            ");

            AppSettings.StraightenGraphDiagonals.Value = true;

            revisionGraph = CreateGraphTopDown(graphWithMultiLaneCrossings);

            // Do not straighten the segments 7:R, 9:R, B:R at rows C to G but create diagonals.
            AssertGraphLayout(revisionGraph, @"
                0
                |\--------------------ˎ
                | 1 | | | | | | | | | |
                | | | | | | | | | | | |
                | | 2 | | | | | | | | |
                | | | | | | | | | | | |
                | | | 3 | | | | | | | |
                | | | | | | | | | | | |
                | | | | 4 | | | | | | |
                | | | | | | | | | | | |
                | | | | | 5 | | | | | |
                | | | | | | | | | | | |
                | | | | | | 6 | | | | |
                | | | | | | | | | | | |
                | | | | | | | 7 | | | |
                | | | | | | | | | | | |
                | | | | | | | | 8 | | |
                | | | | | | | | | | | |
                | | | | | | | | | 9 | |
                | | | | | | | | | | | |
                | | | | | | | | | | A |
                | | | | | | | | | | | |
                | | | | | | | | | | | B
                |-|-|-|------/---/-´ /
                C | | |     |   |   |
                | | | |    /   /   /
                D | | |   |   |   |
                | | | |  /   /   /
                E | | | |   |   |
                | | | | |  /   /
                F | | | | |   |
                |  \ \ \ \ \  |
                G   | | | | | |
                |\--ˎ\ \ \ \ \ \
                H | | | | | | | |
                |\ \ \ \ \ \ \ \ \
                I | | | | | | | | |
                |\ \ \ \ \ \ \ \ \ \
                J | | | | | | | | | |
                | | | | | | | | | | |
                | | | K | | | | | | |
                |/-----------------´
                R
            ");
        }

        [Test]
        public void TurnMultiLaneCrossingsIntoDiagonals()
        {
            AppSettings.MergeGraphLanesHavingCommonParent.Value = false;
            AppSettings.StraightenGraphDiagonals.Value = true;

            RevisionGraph revisionGraph = CreateGraph("R 5:R 4:R 3:R 2:R,5,4 1:2 0:1,3");

            AssertGraphLayout(revisionGraph, @"
                0
                |\
                1 |
                |  \
                2   |
                |\--ˎ\
                | | | 3
                | | | |
                | | 4 |
                | | | |
                | 5 | |
                |/---´
                R
            ");

            revisionGraph = CreateGraph("R 7:R 6:R 5:R 4:R 3:R,7,6,5 2:3 1:2 0:1,4");

            AssertGraphLayout(revisionGraph, @"
                0
                |\
                1 |
                |  \
                2   |
                |    \
                3     |
                |\----ˎ\
                | | | | 4
                | | | | |
                | | | 5 |
                | | | | |
                | | 6 | |
                | | | | |
                | 7 | | |
                |/-----´
                R
            ");

            revisionGraph = CreateGraph("R 8:R 7:R 6:R 5:R 4:R 3:R,8,7,6,5 2:3 1:2 0:1,4");

            AssertGraphLayout(revisionGraph, @"
                0
                |\
                1 |
                | |
                2 |
                | |
                3 |
                |\-`------ˎ
                | | | | | 4
                | | | | | |
                | | | | 5 |
                | | | | | |
                | | | 6 | |
                | | | | | |
                | | 7 | | |
                | | | | | |
                | 8 | | | |
                |/-------´
                R
            ");
        }

        [Test]
        public void UnfoldOneLaneShiftsToDiagonals()
        {
            AppSettings.MergeGraphLanesHavingCommonParent.Value = false;
            AppSettings.StraightenGraphDiagonals.Value = true;

            using (new AssertionScope())
            {
                RevisionGraph revisionGraph = CreateGraph("R 4:R 3:R 2:R,3 1:2 0:1,4");

                AssertGraphLayout(revisionGraph, @"
                    0
                    |\
                    1 |
                    |  \
                    2   |
                    |\  |
                    | 3 |
                    | | |
                    | | 4
                    |/-´
                    R
                ");

                revisionGraph = CreateGraph("R 9:R 8:9 7:R,R 6:7,R 5:6 4:5,8 3:R 2:4,3 1:2,R 0:1,R");

                AssertGraphLayout(revisionGraph, @"
                    0
                    |\
                    1 |
                    |\ \
                    2 | |
                    |\ \ \
                    | 3 | |
                    |  \ \ \
                    4   | | |
                    |\  | | |
                    5 | | | |
                    | | | | |
                    6 | | | |
                    |\ \ \ \ \
                    7 | | | | |
                    |\ \ \ \ \ \
                    | | | 8 | | |
                    | | | | | | |
                    | | | 9 | | |
                    |/---------´
                    R
                ");

                revisionGraph = CreateGraph("R H:R G:H F:R E:F D:R C:F B:C A:B 9:A 8:9 7:G 6:C 5:C 4:E 3:E 2:D 1:D 0:D,1,2,3,4,5,6,B,8,9,7");

                AssertGraphLayout(revisionGraph, @"
                    0
                    |\------------------ˎ
                    | 1 | | | | | | | | |
                    | | | | | | | | | | |
                    | | 2 | | | | | | | |
                    | | | | | | | | | | |
                    | | | 3 | | | | | | |
                    | | | | | | | | | | |
                    | | | | 4 | | | | | |
                    | | | | | | | | | | |
                    | | | | | 5 | | | | |
                    | | | | | | | | | | |
                    | | | | | | 6 | | | |
                    | | | | | | | | | | |
                    | | | | | | | | | | 7
                    | | | | | | | | | | |
                    | | | | | | | | 8 | |
                    | | | | | | | | |/  |
                    | | | | | | | | 9   |
                    | | | | | | | | |  /
                    | | | | | | | | A |
                    | | | | | | | |/ /
                    | | | | | | | B |
                    | | | | | |/-´ /
                    | | | | | C   |
                    |/-´ /  | |  /
                    D   |   | | |
                    | ,/---´  | |
                    | E       | |
                    | |------´ /
                    | F       |
                    | |      /
                    | |     G
                    | |     |
                    | |     H
                    |/-----´
                    R
                ");

                revisionGraph = CreateGraphTopDown("0:1 1:2,4 2:3 3:R,5 4:5,R 5:R R");

                AssertGraphLayout(revisionGraph, @"
                    0
                    |
                    1
                    |\
                    2 |
                    |  \
                    3   |
                    |\  |
                    | | 4
                    | |/|
                    | 5 |
                    |/-´
                    R
                ");

                revisionGraph = CreateGraphTopDown("0:4 1:3,2 2:6 3:8,B 4:5 5:A,6 6:7 7:8 8:C,R 9:A A:B,R B:R C:R R");

                AssertGraphLayout(revisionGraph, @"
                    0
                    |
                    | 1
                    | |\
                    | | 2
                    | |  \
                    | 3   |
                    | |\   \
                    4 | |   |
                    | |  \  |
                    5 |   | |
                    |\,\--|´
                    | 6 | |
                    | | | |
                    | 7 | |
                    | |/  |
                    | 8   |
                    | |\  |
                    | | | | 9
                    |-|--\-\
                    A |   | |
                    |\-\--|´
                    B | | |
                    | | | |
                    | | C |
                    |/---´
                    R
                ");
            }
        }

        [Test]
        public void DoNotUnfoldOneLaneShiftFollowedByDiagonal()
        {
            AppSettings.MergeGraphLanesHavingCommonParent.Value = false;
            AppSettings.StraightenGraphDiagonals.Value = true;

            RevisionGraph revisionGraph = CreateGraph("R 5:R 4:R 3:R,4 2:R,3 1:2 0:1,5");

            // Do not move the right lane of row 2.
            AssertGraphLayout(revisionGraph, @"
                0
                |\
                1 |
                | |
                2 |
                |\ \
                | 3 |
                | |\ \
                | | 4 |
                | | | |
                | | | 5
                |/---´
                R
            ");

            revisionGraph = CreateGraph("R 9:R 8:9 7:8,R 6:7 5:6 4:5,R 3:R 2:4,3 1:2,R 0:1,R");

            // Do not move the right lanes of row 6.
            AssertGraphLayout(revisionGraph, @"
                0
                |\
                1 |
                |\ \
                2 | |
                |\ \ \
                | 3 | |
                |  \ \ \
                4   | | |
                |\  | | |
                5 | | | |
                | | | | |
                6 | | | |
                | | | | |
                7 | | | |
                |\ \ \ \ \
                8 | | | | |
                | | | | | |
                9 | | | | |
                |/-------´
                R
            ");

            revisionGraph = CreateGraph("R 9:R 8:9 7:R,R 6:7,R 5:6,R 4:5,8 3:R 2:4,3 1:2,R 0:1,R");

            // Do not move the right lanes of row 4.
            AssertGraphLayout(revisionGraph, @"
                0
                |\
                1 |
                |\ \
                2 | |
                |\ \ \
                | 3 | |
                | | | |
                4 | | |
                |\ \ \ \
                5 | | | |
                |\ \ \ \ \
                6 | | | | |
                |\ \ \ \ \ \
                7 | | | | | |
                |\ \ \ \ \ \ \
                | | | | 8 | | |
                | | | | | | | |
                | | | | 9 | | |
                |/-----------´
                R
            ");

            revisionGraph = CreateGraphTopDown("0:5,1,2,3,4 1:5 2:5 3:6 4:8 5:R 6:7 7:8,B 8:9 9:R,A A:R B:R R");

            // Do not move segment 4:8 at row 6 & 5.
            AssertGraphLayout(revisionGraph, @"
                0
                |\------ˎ
                | 1 | | |
                | | | | |
                | | 2 | |
                | | | | |
                | | | 3 |
                | | | | |
                | | | | 4
                |/-´ / /
                5   | |
                |  / /
                | 6 |
                | | |
                | 7 |
                | |X
                | 8 |
                | |  \
                | 9   |
                | |\  |
                | | A |
                | | | |
                | | | B
                |/---´
                R
            ");

            revisionGraph = CreateGraphTopDown("0:5,1,2,3,4 1:5 2:5 3:6 4:8 5:R 6:7 7:8,B 8:9 9:R,A A:R,C B:R C:R R");

            // Do not move segment 7:B at row 9.
            AssertGraphLayout(revisionGraph, @"
                0
                |\------ˎ
                | 1 | | |
                | | | | |
                | | 2 | |
                | | | | |
                | | | 3 |
                | | | | |
                | | | | 4
                |/-´ / /
                5   | |
                |  / /
                | 6 |
                | | |
                | 7 |
                | |X
                | 8 |
                | | |
                | 9 |
                | |\ \
                | | A |
                | | |\ \
                | | | | B
                | | | | |
                | | | C |
                |/-----´
                R
            ");
        }

        [Test]
        public void JoinMultiLaneCrossings()
        {
            AppSettings.MergeGraphLanesHavingCommonParent.Value = false;
            AppSettings.StraightenGraphDiagonals.Value = true;

            RevisionGraph revisionGraph = CreateGraphTopDown("0:3,2 1:5 2:5 3:5 4:R 5:R R");

            AssertGraphLayout(revisionGraph, @"
                0
                |\
                | | 1
                | | |
                | 2 |
                | | |
                3 | |
                | | |
                | | | 4
                |/-´  |
                5     |
                |----´
                R
            ");

            revisionGraph = CreateGraphTopDown("0:3,2 1:5 2:5 3:5 4:6 5:6 6:7,8 7:R 8:R R");

            AssertGraphLayout(revisionGraph, @"
                0
                |\
                | | 1
                | | |
                | 2 |
                | | |
                3 | |
                | | |
                | | | 4
                |/-´  |
                5     |
                |----´
                6
                |\
                7 |
                | |
                | 8
                |/
                R
            ");

            revisionGraph = CreateGraph("R 7:R 6:R 5:7 4:7 3:R 2:6 1:7 0:5,4");

            AssertGraphLayout(revisionGraph, @"
                0
                |\
                | | 1
                | | |
                | | | 2
                | | | |
                | | | | 3
                | | | | |
                | 4 | | |
                | | | | |
                5 | | | |
                | | | | |
                | | | 6 |
                |/-´  | |
                7     | |
                |------´
                R
            ");

            revisionGraph = CreateGraphTopDown("0:R,3 1:2 2:8,7 3:4,5,6 4:R 5:R 6:R 7:8 8:R R");

            AssertGraphLayout(revisionGraph, @"
                0
                |\
                | | 1
                | | |
                | | 2
                | |  \----ˎ
                | 3   |   |
                | |\--ˎ\  |
                | 4 | | | |
                | | | | | |
                | | 5 | | |
                | | | | | |
                | | | 6 | |
                | | | | | |
                | | | | | 7
                | | | | |/
                | | | | 8
                |/-----´
                R
            ");
        }

        [Test]
        public void DoNotJoinMultiLaneCrossings()
        {
            AppSettings.MergeGraphLanesHavingCommonParent.Value = false;
            AppSettings.StraightenGraphDiagonals.Value = true;

            RevisionGraph revisionGraph = CreateGraph("R 7:R 6:R 5:7 4:R 3:6 2:6 1:6 0:6");

            // Do not move the multi-lane crossing 5:7 at row 6.
            AssertGraphLayout(revisionGraph, @"
                0
                |
                | 1
                | |
                | | 2
                | | |
                | | | 3
                | | | |
                | | | | 4
                | | | | |
                | | | | | 5
                |/,-,----´
                6 | |
                | | |
                | | 7
                |/-´
                R
            ");

            revisionGraph = CreateGraphTopDown("0:1,4,R 1:2,R 2:3,R 3:R 4:R R");

            // Do not move the multi-lane crossing to 0:R at row 1.
            AssertGraphLayout(revisionGraph, @"
                0
                |\--ˎ
                1 | |
                |\ \ \
                2 | | |
                |\ \ \ \
                3 | | | |
                | | | | |
                | | | 4 |
                |/-----´
                R
            ");
        }

        private const int LookAhead = 20 * 2;

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

        /// <summary>
        /// Creates a revision graph from a text description that lists commits, space separated, from oldest to newest.
        /// Each commit spec is {id}:{parent1 id},{parent2 id},...
        /// or just {id} if it has no parent.
        ///
        /// E.g.: " 1   2:1   3:1   4:2,3 "
        /// </summary>
        private static RevisionGraph CreateGraph(string commitSpecs)
        {
            List<GitRevision> commits = new();
            Dictionary<string, GitRevision> commitsById = new();

            // Parse and create commits
            foreach (string spec in commitSpecs.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                string[] parts = spec.Split(':');
                string id = parts[0];

                GitRevision commit = new(ObjectId.Random());
                commit.Subject = id;
                commits.Add(commit);
                commitsById.Add(id, commit);

                if (parts.Length > 1)
                {
                    string[] parentIds = parts[1].Split(',');
                    commit.ParentIds = parentIds.Select(id => commitsById[id].ObjectId).ToList();
                }
            }

            // Add commits to graph from newest to oldest
            RevisionGraph graph = new();
            commits.Reverse();

            foreach (GitRevision commit in commits)
            {
                graph.Add(commit);
            }

            graph.LoadingCompleted();

            int lastRowIndex = graph.Count - 1;
            graph.CacheTo(lastRowIndex, lastRowIndex);

            return graph;
        }

        private RevisionGraph CreateGraphTopDown(string commitSpecs)
        {
            return CreateGraph(commitSpecs.Split(' ').Reverse().Join(" "));
        }

        /// <summary>
        /// Asserts that the ascii graph of <paramref name="revisionGraph"/> matches <paramref name="expectedLayout"/>.
        /// For examples of how to specify <paramref name="expectedLayout"/>, see existing usage in tests.
        /// </summary>
        private static void AssertGraphLayout(RevisionGraph revisionGraph, string expectedLayout)
        {
            string expectedGraph = expectedLayout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Join("\n");
            string actualGraph = AsciiGraphFor(revisionGraph).Join("\n");
            try
            {
                Assert.AreEqual(expectedGraph, actualGraph);
            }
            catch
            {
                Console.WriteLine(actualGraph);
            }
        }

        /// <summary>
        /// Creates an ascii art representation of the <paramref name="revisionGraph"/> with the same layout
        /// as the actual GE GUI.
        /// </summary>
        private static List<string> AsciiGraphFor(RevisionGraph revisionGraph)
        {
            List<string> graph = new();

            int rowIndex = 0;
            while (true)
            {
                // Create a line for commit

                IRevisionGraphRow row = revisionGraph.GetSegmentsForRow(rowIndex);
                char[] line = Enumerable.Repeat(' ', (row.GetLaneCount() * 2) + 1).ToArray();

                // Show '|' in lanes passing through
                foreach (RevisionGraphSegment segment in row.Segments)
                {
                    line[row.GetLaneForSegment(segment).Index * 2] = '|';
                }

                // Show '*' in lane of actual commit
                string? subject = row.Revision.GitRevision.Subject;
                line[row.GetCurrentRevisionLane() * 2] = subject?.Length is 1 ? subject[0] : '*';

                graph.Add(new string(line).Trim());

                IRevisionGraphRow nextRow = revisionGraph.GetSegmentsForRow(rowIndex + 1);
                if (nextRow == null)
                {
                    break;
                }

                // Create a line between commits

                line = Enumerable.Repeat(' ', (Math.Max(row.GetLaneCount(), nextRow.GetLaneCount()) * 2) + 1).ToArray();

                // These drawing actions are done last, to appear on top
                List<Action> actions = new();

                foreach (RevisionGraphSegment segment in row.Segments)
                {
                    int fromPos = row.GetLaneForSegment(segment).Index * 2;
                    int toPos = nextRow.GetLaneForSegment(segment).Index * 2;
                    if (toPos == -2)
                    {
                        // Segment does not continue to next commit
                        continue;
                    }

                    if (toPos == fromPos)
                    {
                        // Segment stays in lane
                        actions.Add(() => line[fromPos] = '|');
                    }
                    else if (toPos == fromPos + 2)
                    {
                        // Segment shifts one lane to the right
                        actions.Add(() =>
                        {
                            if (line[fromPos + 1] == '/')
                            {
                                // , crossing another shifting left
                                line[fromPos + 1] = 'X';
                            }
                            else
                            {
                                line[fromPos + 1] = '\\';
                            }
                        });
                    }
                    else if (toPos == fromPos - 2)
                    {
                        // Segment shifts one lane to the left
                        actions.Add(() =>
                        {
                            if (line[fromPos - 1] == '\\')
                            {
                                // , crossing another shifting right
                                line[fromPos - 1] = 'X';
                            }
                            else
                            {
                                line[fromPos - 1] = '/';
                            }
                        });
                    }
                    else if (toPos > fromPos)
                    {
                        // Segment shifts multiple lanes to the right
                        line[fromPos + 1] = '`';
                        line[toPos] = 'ˎ';
                        for (int pos = fromPos + 2; pos < toPos; ++pos)
                        {
                            line[pos] = '-';
                        }
                    }
                    else if (toPos < fromPos)
                    {
                        // Segment shifts multiple lanes to the left
                        line[fromPos - 1] = '´';
                        line[toPos] = ',';
                        for (int pos = toPos + 1; pos < fromPos - 1; ++pos)
                        {
                            line[pos] = '-';
                        }
                    }
                }

                foreach (Action action in actions)
                {
                    action();
                }

                graph.Add(new string(line).Trim());
                ++rowIndex;
            }

            return graph;
        }
    }
}
