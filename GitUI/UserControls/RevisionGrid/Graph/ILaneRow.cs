namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal interface ILaneRow
    {
        // Node information
        int NodeLane { get; }
        Node Node { get; }

        // Lane information
        int Count { get; }
        LaneInfo this[int lane, int item] { get; }
        int LaneInfoCount(int lane);
    }
}