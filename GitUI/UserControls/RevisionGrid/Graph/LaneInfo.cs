namespace GitUI.UserControls.RevisionGrid.Graph
{
    public class LaneInfo
    {
        public LaneInfo(RevisionGraphSegment startSegment)
        {
            StartRevision = startSegment.Child;

            int colorSeed = StartRevision.Objectid.GetHashCode() ^ startSegment.Parent.Objectid.GetHashCode();
            Color = RevisionGraphLaneColor.GetColorForLane(colorSeed);
        }

        public LaneInfo(RevisionGraphSegment startSegment, LaneInfo derivedFrom)
        {
            StartRevision = startSegment.Parent;
            int colorSeed = StartRevision.Objectid.GetHashCode();

            do
            {
                Color = RevisionGraphLaneColor.GetColorForLane(colorSeed);
                ++colorSeed;
            }
            while (Color == derivedFrom.Color);
        }

        public int Color { get; private set; }

        public RevisionGraphRevision StartRevision { get; private set; }

        public int StartScore => StartRevision.Score;
    }
}
