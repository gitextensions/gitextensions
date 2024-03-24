namespace GitUI.UserControls.RevisionGrid.Graph.Rendering
{
    internal readonly ref struct SegmentLanesInfo
    {
        public readonly int StartLane;
        public readonly int CenterLane;
        public readonly int EndLane;
        public readonly int PrimaryEndLane;
        public readonly bool IsTheRevisionLane;
        public readonly bool DrawFromStart;
        public readonly bool DrawToEnd;

        public SegmentLanesInfo(int startLane, int centerLane, int endLane, int primaryEndLane, bool isTheRevisionLane, bool drawFromStart, bool drawToEnd)
        {
            StartLane = startLane;
            CenterLane = centerLane;
            EndLane = endLane;
            PrimaryEndLane = primaryEndLane;
            IsTheRevisionLane = isTheRevisionLane;
            DrawFromStart = drawFromStart;
            DrawToEnd = drawToEnd;
        }
    }
}
