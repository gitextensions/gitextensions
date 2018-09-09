using FluentAssertions;
using GitUI.UserControls.RevisionGrid.Graph;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.UserControls.RevisionGrid.Graph
{
    [TestFixture]
    public class LaneNodeLocatorTests
    {
        private ILaneRowProvider _laneRowProvider;
        private LaneNodeLocator _laneNodeLocator;

        [SetUp]
        public void Setup()
        {
            _laneRowProvider = Substitute.For<ILaneRowProvider>();
            _laneNodeLocator = new LaneNodeLocator(_laneRowProvider);
        }

        private Node SetupLaneRow(int row, int laneCount, int nodeLane = -1)
        {
            var node = new Node(GitUIPluginInterfaces.ObjectId.WorkTreeId);
            var laneRow = Substitute.For<ILaneRow>();
            laneRow.Count.Returns(laneCount);
            laneRow.NodeLane.Returns(nodeLane);
            laneRow.Node.Returns(node);
            _laneRowProvider.GetLaneRow(row).Returns(x => laneRow);
            return node;
        }

        [Test]
        public void FindPrevNode_should_return_null_if_x_negative()
        {
            _laneNodeLocator.FindPrevNode(-1, 0, 1).Should().Be((null, false));
        }

        [Test]
        public void FindPrevNode_should_return_null_if_rowIndex_negative()
        {
            _laneNodeLocator.FindPrevNode(0, -1, 1).Should().Be((null, false));
        }

        [Test]
        public void FindPrevNode_should_return_null_if_laneWidth_less_than_one()
        {
            _laneNodeLocator.FindPrevNode(0, 0, 0).Should().Be((null, false));
        }

        [Test]
        public void FindPrevNode_should_return_null_if_model_does_not_have_row()
        {
            const int row = 100;
            _laneRowProvider.GetLaneRow(row).Returns(x => null);
            _laneNodeLocator.FindPrevNode(0, row, 1).Should().Be((null, false));
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
            const int width = 5;
            var node = SetupLaneRow(row, laneCount: 0, lane);

            // lane == laneRow.NodeLane
            _laneNodeLocator.FindPrevNode(lane * width, row, width).Should().Be((node, true));
        }

        [Test]
        public void FindPrevNode_should_return_null_if_lane_exceeds_lane_count_and_lane_is_not_the_node_lane()
        {
            const int row = 100;
            const int lane = 10;
            const int width = 5;
            SetupLaneRow(row, laneCount: lane);

            // lane >= _laneRowProvider.GetLaneRow(rowIndex).Count
            _laneNodeLocator.FindPrevNode(lane * width, row, width).Should().Be((null, false));
        }

        [Test]
        public void FindPrevNode_should_return_null_if_there_is_no_lane_info()
        {
            const int row = 100;
            const int lane = 3;
            const int width = 5;
            SetupLaneRow(row, laneCount: lane + 1);
            _laneRowProvider.GetLaneRow(row).LaneInfoCount(lane).Returns(x => 0);

            // empty loop "for (laneInfoIndex)"
            _laneNodeLocator.FindPrevNode(lane * width, row, width).Should().Be((null, false));
        }

        [Test]
        public void FindPrevNode_should_return_null_if_the_junction_is_empty()
        {
            // const int row = 100;
            // const int lane = 3;
            // const int width = 5;

            // empty loop "for (nodeIndex)"
            // not testable since Junction cannot be mocked up without nodes
            // _laneNodeLocator.FindPrevNode(lane * width, row, width).Should().Be((null, false));
        }

        [Test]
        public void FindPrevNode_should_return_the_first_junction_node_of_the_first_LaneInfo()
        {
            const int row = 100;
            const int lane = 3;
            const int width = 5;
            var laneNode = SetupLaneRow(row, laneCount: lane + 1);
            var junctionNode0 = new Node(GitUIPluginInterfaces.ObjectId.WorkTreeId);
            var junctionNode1 = new Node(GitUIPluginInterfaces.ObjectId.WorkTreeId);
            var junction = new Junction(junctionNode0, junctionNode1);
            var laneInfo = new LaneInfo(lane, junction);
            var laneRow = _laneRowProvider.GetLaneRow(row);
            laneRow.LaneInfoCount(lane).Returns(x => 2);
            laneRow[lane, 0].Returns(x => laneInfo);

            // match in the first iteration of the loop "for (laneInfoIndex)"
            // match in the first iteration of the loop "for (nodeIndex)"
            _laneNodeLocator.FindPrevNode(lane * width, row, width).Should().Be((junctionNode0, false));
        }

        [Test]
        public void FindPrevNode_should_return_the_second_junction_node_of_the_first_LaneInfo()
        {
            const int row = 100;
            const int lane = 3;
            const int width = 5;
            var laneNode = SetupLaneRow(row, laneCount: lane + 1);
            var junctionNode0 = new Node(GitUIPluginInterfaces.ObjectId.WorkTreeId)
            {
                Index = row
            };
            var junctionNode1 = new Node(GitUIPluginInterfaces.ObjectId.WorkTreeId);
            var junction = new Junction(junctionNode0, junctionNode1);
            var laneInfo = new LaneInfo(lane, junction);
            var laneRow = _laneRowProvider.GetLaneRow(row);
            laneRow.LaneInfoCount(lane).Returns(x => 2);
            laneRow[lane, 0].Returns(x => laneInfo);

            // match in the first iteration of the loop "for (laneInfoIndex)"
            // match in the second iteration of the loop "for (nodeIndex)"
            _laneNodeLocator.FindPrevNode(lane * width, row, width).Should().Be((junctionNode1, false));
        }

        [Test]
        public void FindPrevNode_should_return_the_first_junction_node_of_the_second_LaneInfo()
        {
            const int row = 100;
            const int lane = 3;
            const int width = 5;
            var laneNode = SetupLaneRow(row, laneCount: lane + 1);
            var junctionNode0 = new Node(GitUIPluginInterfaces.ObjectId.WorkTreeId)
            {
                Index = row
            };
            var junctionNode1 = new Node(GitUIPluginInterfaces.ObjectId.WorkTreeId)
            {
                Index = row
            };
            var junctionNode2 = new Node(GitUIPluginInterfaces.ObjectId.WorkTreeId);
            var junction0 = new Junction(junctionNode0, junctionNode1);
            var junction1 = new Junction(junctionNode2);
            var laneInfo0 = new LaneInfo(lane, junction0);
            var laneInfo1 = new LaneInfo(lane, junction1);
            var laneRow = _laneRowProvider.GetLaneRow(row);
            laneRow.LaneInfoCount(lane).Returns(x => 2);
            laneRow[lane, 0].Returns(x => laneInfo0);
            laneRow[lane, 1].Returns(x => laneInfo1);

            // match in the second iteration of the loop "for (laneInfoIndex)"
            // match in the first iteration of the loop "for (nodeIndex)"
            _laneNodeLocator.FindPrevNode(lane * width, row, width).Should().Be((junctionNode2, false));
        }
    }
}