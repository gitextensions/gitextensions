using System.Diagnostics;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    [DebuggerDisplay("{Index} {Sharing}")]
    public sealed class Lane
    {
        public int Index;
        public LaneSharing Sharing;

        public Lane(int index, LaneSharing sharing)
        {
            Index = index;
            Sharing = sharing;
        }
    }
}
