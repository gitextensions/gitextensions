using System.Collections.Generic;
using FluentAssertions;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitUITests.UserControls.RevisionGrid.Graph
{
    [TestFixture]
    public class RevisionGraphRowTests
    {
        private RevisionGraphSegment _segment = new(parent: new(ObjectId.IndexId, guessScore: 0), child: new(ObjectId.WorkTreeId, guessScore: 0));
        private RevisionGraphSegment _segment1 = new(parent: new(ObjectId.Random(), guessScore: 0), child: new(ObjectId.Random(), guessScore: 0));
        private RevisionGraphSegment _segment2 = new(parent: new(ObjectId.Random(), guessScore: 0), child: new(ObjectId.Random(), guessScore: 0));

        [Test]
        public void MoveLanesRight_should_do_nothing_if_empty([Values(-1, 0, 1)] int fromLane)
        {
            List<RevisionGraphSegment> segments = new();
            IRevisionGraphRow revisionGraphRow = new RevisionGraphRow(_segment.Child, segments);
            revisionGraphRow.GetLaneCount().Should().Be(1);

            revisionGraphRow.MoveLanesRight(fromLane);

            revisionGraphRow.GetLaneCount().Should().Be(1);
            revisionGraphRow.Segments.Should().BeEmpty();
        }

        [TestCase(-1, 1)]
        [TestCase(0, 1)]
        [TestCase(1, 0)]
        [TestCase(2, 0)]
        public void MoveLanesRight_should_move_single_segment(int fromLane, int expectedLane)
        {
            List<RevisionGraphSegment> segments = new() { _segment };
            IRevisionGraphRow revisionGraphRow = new RevisionGraphRow(_segment.Child, segments);
            revisionGraphRow.GetLaneCount().Should().Be(segments.Count);

            revisionGraphRow.MoveLanesRight(fromLane);

            revisionGraphRow.GetLaneCount().Should().Be(fromLane >= segments.Count ? segments.Count : segments.Count + 1);
            revisionGraphRow.GetLaneIndexForSegment(_segment).Should().Be(expectedLane);
        }

        [TestCase(-1, 1, 2, 3)]
        [TestCase(0, 1, 2, 3)]
        [TestCase(1, 0, 2, 3)]
        [TestCase(2, 0, 1, 3)]
        [TestCase(3, 0, 1, 2)]
        [TestCase(4, 0, 1, 2)]
        public void MoveLanesRight_should_move_segments(int fromLane, int expectedLane, int expectedLane1, int expectedLane2)
        {
            List<RevisionGraphSegment> segments = new() { _segment, _segment1, _segment2 };
            IRevisionGraphRow revisionGraphRow = new RevisionGraphRow(_segment.Child, segments);
            revisionGraphRow.GetLaneCount().Should().Be(3);

            revisionGraphRow.MoveLanesRight(fromLane);

            revisionGraphRow.GetLaneCount().Should().Be(fromLane >= segments.Count ? segments.Count : segments.Count + 1);
            revisionGraphRow.GetLaneIndexForSegment(_segment).Should().Be(expectedLane);
            revisionGraphRow.GetLaneIndexForSegment(_segment1).Should().Be(expectedLane1);
            revisionGraphRow.GetLaneIndexForSegment(_segment2).Should().Be(expectedLane2);
        }

        [TestCase(-1, 4, 1, 2, 3)]
        [TestCase(0, 1, 2, 3, 4)]
        [TestCase(0, 2, 1, 3, 4)]
        [TestCase(0, 3, 1, 2, 4)]
        [TestCase(0, 4, 1, 2, 3)]
        [TestCase(1, 0, 1, 2, 3)]
        [TestCase(1, 1, 0, 3, 4)]
        [TestCase(1, 2, 0, 3, 4)]
        [TestCase(1, 3, 0, 2, 4)]
        [TestCase(1, 4, 0, 2, 3)]
        [TestCase(2, 0, 1, 2, 3)]
        [TestCase(2, 1, 0, 2, 3)]
        [TestCase(2, 2, 0, 1, 4)]
        [TestCase(2, 3, 0, 1, 4)]
        [TestCase(2, 4, 0, 1, 3)]
        [TestCase(3, 0, 1, 2, 3)]
        [TestCase(3, 1, 0, 2, 3)]
        [TestCase(3, 2, 0, 1, 3)]
        [TestCase(3, 3, 0, 1, 2)]
        [TestCase(4, 3, 0, 1, 2)]
        public void MoveLanesRight_should_move_segments_twice(int fromLane1, int fromLane2, int expectedLane, int expectedLane1, int expectedLane2)
        {
            List<RevisionGraphSegment> segments = new() { _segment, _segment1, _segment2 };
            IRevisionGraphRow revisionGraphRow = new RevisionGraphRow(_segment.Child, segments);
            revisionGraphRow.GetLaneCount().Should().Be(3);

            revisionGraphRow.MoveLanesRight(fromLane1);
            revisionGraphRow.MoveLanesRight(fromLane2);

            revisionGraphRow.GetLaneCount().Should().Be(expectedLane2 + 1);
            revisionGraphRow.GetLaneIndexForSegment(_segment).Should().Be(expectedLane);
            revisionGraphRow.GetLaneIndexForSegment(_segment1).Should().Be(expectedLane1);
            revisionGraphRow.GetLaneIndexForSegment(_segment2).Should().Be(expectedLane2);
        }
    }
}
