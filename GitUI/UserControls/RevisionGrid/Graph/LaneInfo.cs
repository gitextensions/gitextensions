using System.Linq;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    public class LaneInfo
    {
        public LaneInfo(RevisionGraphSegment startSegment)
        {
            StartRevision = startSegment.Parent;
            Color = StartRevision.Objectid.GetHashCode();
        }

        public int Color { get; private set; }

        public RevisionGraphRevision StartRevision { get; private set; }

        public int StartScore => StartRevision.Score;
    }
}
