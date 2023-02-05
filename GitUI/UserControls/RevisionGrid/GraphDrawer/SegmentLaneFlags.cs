namespace GitUI.UserControls.RevisionGrid.GraphDrawer
{
    internal struct SegmentLaneFlags
    {
        internal bool DrawFromStart;
        internal bool DrawToEnd;
        internal bool DrawCenterToStartPerpendicularly;
        internal bool DrawCenter;
        internal bool DrawCenterPerpendicularly;
        internal bool DrawCenterToEndPerpendicularly;
        internal bool IsTheRevisionLane;
        internal int HorizontalOffset;
    }
}
