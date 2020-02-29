using System;
using System.Collections.Generic;
using FluentAssertions;
using GitUI.UserControls.RevisionGrid.Graph;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.UserControls.RevisionGrid.Graph
{
    [TestFixture]
    public class LaneNodeLocatorTests
    {
        private IRevisionGraphRowProvider _revisionGraphRowProvider;
        private LaneNodeLocator _laneNodeLocator;

        [SetUp]
        public void Setup()
        {
            _revisionGraphRowProvider = Substitute.For<IRevisionGraphRowProvider>();
            _laneNodeLocator = new LaneNodeLocator(_revisionGraphRowProvider);
        }

        private RevisionGraphRevision SetupLaneRow(int row, int lane, int laneCount, int nodeLane = -1, RevisionGraphSegment firstSegment = null)
        {
            var node = new RevisionGraphRevision(GitUIPluginInterfaces.ObjectId.WorkTreeId, 0);
            var revisionGraphRow = Substitute.For<IRevisionGraphRow>();

            var segments = new List<RevisionGraphSegment>();
            if (firstSegment != null)
            {
                segments.Add(firstSegment);
            }

            if (lane < laneCount)
            {
                revisionGraphRow.GetSegmentsForIndex(lane).Returns(segments);
            }

            revisionGraphRow.GetCurrentRevisionLane().Returns(nodeLane);
            if (lane == nodeLane)
            {
                revisionGraphRow.Revision.Returns(node);
            }
            else
            {
                segments.Add(new RevisionGraphSegment(node, null));
            }

            _revisionGraphRowProvider.GetSegmentsForRow(row).Returns(x => revisionGraphRow);
            return node;
        }

        [Test]
        public void FindPrevNode_should_return_null_if_lane_negative()
        {
            _laneNodeLocator.FindPrevNode(0, -1).Should().Be((null, false));
        }

        [Test]
        public void FindPrevNode_should_return_null_if_rowIndex_negative()
        {
            _laneNodeLocator.FindPrevNode(-1, 0).Should().Be((null, false));
        }

        [Test]
        public void FindPrevNode_should_return_null_if_model_does_not_have_row()
        {
            const int row = 100;
            _revisionGraphRowProvider.GetSegmentsForRow(row).Returns(x => null);
            _laneNodeLocator.FindPrevNode(row, 0).Should().Be((null, false));
        }

        [Test]
        public void FindPrevNode_should_return_null_if_rowIndex_exceeds_model_row_count()
        {
            // It is not up to FindPrevNode() to check this, because:
            // The model does not provide a property "Count". Tough it returns null in this case.
            FindPrevNode_should_return_null_if_model_does_not_have_row();
        }

        [Test]
        public void FindPrevNode_should_return_the_node_if_it_is_at_the_node_lane_although_lane_count_is_0()
        {
            const int row = 100;
            const int lane = 0;
            var node = SetupLaneRow(row, lane, laneCount: 0, nodeLane: lane);

            // row.GetCurrentRevisionLane() == lane
            _laneNodeLocator.FindPrevNode(row, lane).Should().Be((node, true));
        }

        [Test]
        public void FindPrevNode_should_return_null_if_lane_exceeds_lane_count_and_lane_is_not_the_node_lane()
        {
            const int row = 100;
            const int lane = 10;
            SetupLaneRow(row, lane, laneCount: lane);

            // lane >= _revisionGraphRowProvider.GetSegmentsForRow(rowIndex).Count
            _laneNodeLocator.FindPrevNode(row, lane).Should().Be((null, false));
        }

        [Test]
        public void FindPrevNode_should_return_null_if_there_is_no_lane_info()
        {
            const int row = 100;
            const int lane = 3;
            SetupLaneRow(row, lane, laneCount: lane + 1);
            _revisionGraphRowProvider.GetSegmentsForRow(row).GetSegmentsForIndex(lane).Returns(x => new List<RevisionGraphSegment>());

            // segmentsForLane.Count() <= 0
            _laneNodeLocator.FindPrevNode(row, lane).Should().Be((null, false));
        }

        [Test]
        public void FindPrevNode_should_return_the_parent_node_of_the_single_segment()
        {
            const int row = 100;
            const int lane = 3;
            var laneNode = SetupLaneRow(row, lane, laneCount: lane + 1);

            // innermost "return"
            _laneNodeLocator.FindPrevNode(row, lane).Should().Be((laneNode, false));
        }

        [Test]
        public void FindPrevNode_should_return_the_parent_node_of_the_first_segment_and_throw_in_debug_build()
        {
            const int row = 100;
            const int lane = 3;
            var parentNode = new RevisionGraphRevision(GitUIPluginInterfaces.ObjectId.WorkTreeId, 0);
            var childNode = new RevisionGraphRevision(GitUIPluginInterfaces.ObjectId.WorkTreeId, 0);
            var segment = new RevisionGraphSegment(parentNode, childNode);
            var laneNode = SetupLaneRow(row, lane, laneCount: lane + 1, firstSegment: segment);

#if !DEBUG
            // innermost "return" in RELEASE build
            _laneNodeLocator.FindPrevNode(row, lane).Should().Be((parentNode, false));
#else
            try
            {
                // Exception before innermost "return" in DEBUG build
                _laneNodeLocator.FindPrevNode(row, lane).Should().Be((parentNode, false));
                throw new AssertionException("The debug build should throw an exception!");
            }
            catch (Exception x)
            {
                x.Message.Should().Be(string.Format("All segments for a lane should have the same parent.\n"
                                                    + "Not fulfilled for rowIndex {0} lane {1} with {2} segments.",
                                                    row, lane, 2));
            }
#endif
        }
    }
}