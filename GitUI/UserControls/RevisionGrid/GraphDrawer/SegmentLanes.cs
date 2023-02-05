namespace GitUI.UserControls.RevisionGrid.GraphDrawer
{
    internal struct SegmentLanes
    {
        internal int StartLane;
        internal int CenterLane;
        internal int EndLane;
        internal int PrimaryEndLane;
        internal bool IsTheRevisionLane;
        internal bool DrawFromStart;
        internal bool DrawToEnd;
    }
}
