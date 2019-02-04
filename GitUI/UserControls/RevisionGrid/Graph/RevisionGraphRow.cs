using System;
using System.Collections.Generic;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    public interface IRevisionGraphRow
    {
        RevisionGraphRevision Revision { get; }
        IReadOnlyList<RevisionGraphSegment> Segments { get; }
        int GetCurrentRevisionLane();
        int GetLaneCount();
        IEnumerable<RevisionGraphSegment> GetSegmentsForIndex(int index);
        int GetLaneIndexForSegment(RevisionGraphSegment revisionGraphRevision);
    }

    // The RevisionGraphRow contains an ordered list of Segments that crosses the row or connects to the revision in the row.
    // The segments can be returned in the order how it is stored.
    // Segments are not the same as lanes.A crossing segment is a lane, but multiple segments can connect to the revision.
    // Therefor, a single lane can have multiple segments.
    public class RevisionGraphRow : IRevisionGraphRow
    {
        public RevisionGraphRow(RevisionGraphRevision revision, IReadOnlyList<RevisionGraphSegment> segments)
        {
            Revision = revision;
            Segments = segments;
        }

        public RevisionGraphRevision Revision { get; private set; }
        public IReadOnlyList<RevisionGraphSegment> Segments { get; private set; }

        // This dictonary contains a cached list of all segments and the lane index the segment is in for this row.
        private IReadOnlyDictionary<RevisionGraphSegment, int> _segmentLanes;

        // The cached lanecount
        private int _laneCount;

        // The cached revisionlane
        private int _revisionLane;

        // The row contains ordered segments. This method sorts the segments per lane.
        // Segments that cross this row (start above and end below) get there own private lane.
        // Segments that connect to the revision (node) for this row, share the same lane.
        // Building the segment lane cache is not very expensive, but since only a fraction of the rows will be rendered
        // we cache on demand.
        private void BuildSegmentLanes()
        {
            if (_segmentLanes != null)
            {
                return;
            }

            // We do not want SegementLanes to be build multiple times. Lock it.
            lock (Revision)
            {
                // Another thread could be waiting for the lock, while the segmentlanes where being build. Check again if segmentslanes is null.
                if (_segmentLanes != null)
                {
                    return;
                }

                Dictionary<RevisionGraphSegment, int> newSegmentLanes = new Dictionary<RevisionGraphSegment, int>();

                int currentRevisionLane = -1;
                int laneIndex = 0;
                foreach (var segment in Segments)
                {
                    if (segment.Child == Revision || segment.Parent == Revision)
                    {
                        // The current segment connects to the revision of this row. Store the revision lane.
                        if (currentRevisionLane < 0)
                        {
                            currentRevisionLane = laneIndex;
                            laneIndex++;
                        }

                        // All segments that connect to the current revision are in the same lane.
                        newSegmentLanes[segment] = currentRevisionLane;
                    }
                    else
                    {
                        // This is a crossing lane. We could not merge it in the lane with this row's revision.

                        // Try to detect this:
                        // *
                        // |
                        // | *
                        // | |
                        // | |  <- this is the row we are processing
                        // |/
                        // *    <- same parent, not on current row
                        //
                        // And change it into this, by merging the segments in a singe lane:
                        // *
                        // |
                        // | *
                        // |/
                        // |    <- merge into a singe lane to simplify graph
                        // |
                        // *
                        bool added = false;
                        foreach (var searchParent in newSegmentLanes)
                        {
                            // If there is another segment with the same parent, and its not this row's revision, merge into 1 lane.
                            if (searchParent.Value != currentRevisionLane && searchParent.Key.Parent == segment.Parent)
                            {
                                // Use indexer to overwrite if segments was already added. This shouldn't happen, but it does.
                                newSegmentLanes[segment] = searchParent.Value;
                                added = true;
                                break;
                            }
                        }

                        // Segment has not been assigned a lane yet
                        if (!added)
                        {
                            newSegmentLanes[segment] = laneIndex;
                            laneIndex++;
                        }
                    }
                }

                _segmentLanes = newSegmentLanes;
                _laneCount = laneIndex;
                if (currentRevisionLane >= 0)
                {
                    _revisionLane = currentRevisionLane;
                }
                else
                {
                    _revisionLane = _laneCount;
                    _laneCount++;
                }
            }
        }

        public int GetCurrentRevisionLane()
        {
            BuildSegmentLanes();
            return Math.Max(0, _revisionLane);
        }

        public int GetLaneCount()
        {
            BuildSegmentLanes();
            return _laneCount;
        }

        public IEnumerable<RevisionGraphSegment> GetSegmentsForIndex(int index)
        {
            BuildSegmentLanes();
            foreach (var keyValye in _segmentLanes)
            {
                if (keyValye.Value == index)
                {
                    yield return keyValye.Key;
                }
            }
        }

        public int GetLaneIndexForSegment(RevisionGraphSegment revisionGraphRevision)
        {
            BuildSegmentLanes();
            if (_segmentLanes.TryGetValue(revisionGraphRevision, out int index))
            {
                return index;
            }

            return -1;
        }
    }
}
