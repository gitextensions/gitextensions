using System.Diagnostics;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    // This class represents the connection between 2 revisions.
    //     *    <- Child
    //     |    <- segment connects two commits
    //     *    <- Parent
    // A segment can span multiple rows when rendered as a graph.
    // Example: This graph has 6 segments.
    //     *    <- Child
    //   / | \  <- Child.StartSegments ("start" although they are merged here)
    //  |  *  |
    //  |  |  |
    //  |  *  |
    //   \ |  |
    //     *  |
    //     | /
    //     *    <- Parent
    [DebuggerDisplay("Child: {Child} - Parent: {Parent}")]
    public class RevisionGraphSegment
    {
        public RevisionGraphSegment(RevisionGraphRevision parent, RevisionGraphRevision child)
        {
            Parent = parent;
            Child = child;
        }

        public LaneInfo? LaneInfo { get; set; }

        public int StartScore => Child.Score;

        public int EndScore => Parent.Score;

        public RevisionGraphRevision Parent { get; }
        public RevisionGraphRevision Child { get; }
    }
}
