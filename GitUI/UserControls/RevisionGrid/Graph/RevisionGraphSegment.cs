namespace GitUI.UserControls.RevisionGrid.Graph
{
    // This class represents the connection between 2 revisions.
    //     *    <- Child
    //     |    <- segment connects two commits
    //     *    <- Parent
    // A segment can span multiple rows when rendered as a graph.
    // Example: This graph has 6 segements.
    //     *    <- Child
    //   / | \  <- Child.StartSegments ("start" although they are merged here)
    //  |  *  |
    //  |  |  |
    //  |  *  |
    //   \ |  |
    //     *  |
    //     | /
    //     *    <- Parent
    public class RevisionGraphSegment
    {
        public RevisionGraphSegment(RevisionGraphRevision parent, RevisionGraphRevision child)
        {
            Parent = parent;
            Child = child;
        }

        public int StartScore => Child.Score;

        public int EndScore => Parent.Score;

        public RevisionGraphRevision Parent { get; private set; }
        public RevisionGraphRevision Child { get; private set; }
    }
}
