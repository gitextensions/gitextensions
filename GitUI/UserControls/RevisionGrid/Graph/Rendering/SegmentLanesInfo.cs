namespace GitUI.UserControls.RevisionGrid.Graph.Rendering
{
    internal readonly ref struct SegmentLanesInfo
    {
        public readonly int StartLane;
        public readonly int CenterLane;
        public readonly int EndLane;
        public readonly bool DrawFromStart;
        public readonly bool DrawToEnd;

        public SegmentLanesInfo(int startLane, int centerLane, int endLane, bool drawFromStart, bool drawToEnd)
        {
            StartLane = startLane;
            CenterLane = centerLane;
            EndLane = endLane;
            DrawFromStart = drawFromStart;
            DrawToEnd = drawToEnd;
        }
    }
}
