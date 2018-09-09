using System.Linq;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal interface ILaneNodeLocator
    {
        (Node, bool isAtNode) FindPrevNode(int x, int rowIndex, int laneWidth);
    }

    internal sealed class LaneNodeLocator : ILaneNodeLocator
    {
        private readonly ILaneRowProvider _laneRowProvider;

        public LaneNodeLocator(ILaneRowProvider laneRowProvider)
        {
            _laneRowProvider = laneRowProvider;
        }

        public (Node, bool isAtNode) FindPrevNode(int x, int rowIndex, int laneWidth)
        {
            if (x < 0 || rowIndex < 0 || laneWidth < 1)
            {
                // as unlikely as it may be...
                // don't throw, just pretend we couldn't find it
            }
            else
            {
                int lane = x / laneWidth;
                lock (_laneRowProvider)
                {
                    var laneRow = _laneRowProvider.GetLaneRow(rowIndex);
                    if (laneRow != null)
                    {
                        if (lane == laneRow.NodeLane)
                        {
                            return (laneRow.Node, isAtNode: true);
                        }

                        if (lane < laneRow.Count)
                        {
                            for (int laneInfoIndex = 0, laneInfoCount = laneRow.LaneInfoCount(lane); laneInfoIndex < laneInfoCount; ++laneInfoIndex)
                            {
                                // search for next node below this row
                                var laneInfo = laneRow[lane, laneInfoIndex];
                                var firstJunction = laneInfo.Junctions.First();
                                for (int nodeIndex = 0, nodeCount = firstJunction.NodeCount; nodeIndex < nodeCount; ++nodeIndex)
                                {
                                    var laneNode = firstJunction[nodeIndex];
                                    if (laneNode.Index > rowIndex)
                                    {
                                        return (laneNode, isAtNode: false);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return (null, false);
        }
    }
}