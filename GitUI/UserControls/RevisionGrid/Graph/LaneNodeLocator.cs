using System;
using System.Linq;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal interface ILaneNodeLocator
    {
        (RevisionGraphRevision, bool isAtNode) FindPrevNode(int rowIndex, int lane);
    }

    internal sealed class LaneNodeLocator : ILaneNodeLocator
    {
        private readonly IRevisionGraphRowProvider _revisionGraphRowProvider;

        public static readonly (RevisionGraphRevision, bool) NotFoundResult = (null, false);

        public LaneNodeLocator(IRevisionGraphRowProvider revisionGraphRowProvider)
        {
            _revisionGraphRowProvider = revisionGraphRowProvider;
        }

        public (RevisionGraphRevision, bool isAtNode) FindPrevNode(int rowIndex, int lane)
        {
            if (rowIndex < 0 || lane < 0)
            {
                // as unlikely as it may be...
                // don't throw, just pretend we couldn't find it
                return NotFoundResult;
            }

            IRevisionGraphRow row = _revisionGraphRowProvider.GetSegmentsForRow(rowIndex);
            if (row == null)
            {
                return NotFoundResult;
            }

            if (row.GetCurrentRevisionLane() == lane)
            {
                return (row.Revision, isAtNode: true);
            }

            var segmentsForLane = row.GetSegmentsForIndex(lane);
            if (segmentsForLane.Count() > 0)
            {
                var firstParent = segmentsForLane.First().Parent;
#if DEBUG
                if (segmentsForLane.Any(segment => segment.Parent != firstParent))
                {
                    throw new Exception(string.Format("All segments for a lane should have the same parent.\n"
                                                      + "Not fulfilled for rowIndex {0} lane {1} with {2} segments.",
                                                      rowIndex, lane, segmentsForLane.Count()));
                }
#endif
                return (firstParent, isAtNode: false);
            }

            return NotFoundResult;
        }
    }
}