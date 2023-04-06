namespace GitUI.UserControls.RevisionGrid.Graph
{
    public interface IRevisionGraphRow
    {
        RevisionGraphRevision Revision { get; }
        IReadOnlyList<RevisionGraphSegment> Segments { get; }
        int GetCurrentRevisionLane();
        int GetLaneCount();
        IEnumerable<RevisionGraphSegment> GetSegmentsForIndex(int index);
        int GetLaneIndexForSegment(RevisionGraphSegment revisionGraphRevision);
        void MoveLanesRight(int fromLane);
    }
}
