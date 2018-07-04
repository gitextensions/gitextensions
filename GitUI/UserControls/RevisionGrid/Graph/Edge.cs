namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal readonly struct Edge
    {
        public readonly int Start;
        public readonly LaneInfo Data;

        public Edge(LaneInfo data, int start)
        {
            Data = data;
            Start = start;
        }

        public int End => Data.ConnectLane;

#if DEBUG
        public override string ToString() => $"{Start}->{End}: {Data}";
#endif
    }
}