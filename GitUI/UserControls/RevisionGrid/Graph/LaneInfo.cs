namespace GitUI.UserControls.RevisionGrid.Graph
{
    public class LaneInfo
    {
        public LaneInfo(RevisionGraphSegment startSegment, LaneInfo? derivedFrom)
        {
            StartRevision = derivedFrom is null ? startSegment.Child : startSegment.Parent;

            int colorSeed = StartRevision.Objectid.GetHashCode();
            if (derivedFrom is null)
            {
                colorSeed ^= startSegment.Parent.Objectid.GetHashCode();
            }

            do
            {
                Color = RevisionGraphLaneColor.GetColorForLane(colorSeed);
                ++colorSeed;
            }
            while (Color == derivedFrom?.Color);
        }

        public int Color { get; private set; }

        public RevisionGraphRevision StartRevision { get; private set; }

        public int StartScore => StartRevision.Score;
    }
}
