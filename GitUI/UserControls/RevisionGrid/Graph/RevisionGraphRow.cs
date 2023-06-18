using Microsoft;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    public interface IRevisionGraphRow
    {
        RevisionGraphRevision Revision { get; }
        IReadOnlyList<RevisionGraphSegment> Segments { get; }

        int GetCurrentRevisionLane();
        int GetLaneCount();
        Lane GetLaneForSegment(RevisionGraphSegment revisionGraphRevision);
        IEnumerable<RevisionGraphSegment> GetSegmentsForIndex(int index);
        void MoveLanesRight(int fromLane);
    }

    // The RevisionGraphRow contains an ordered list of Segments that crosses the row or connects to the revision in the row.
    // The segments can be returned in the order how it is stored.
    // Segments are not the same as lanes.A crossing segment is a lane, but multiple segments can connect to the revision.
    // Therefore, a single lane can have multiple segments.
    public class RevisionGraphRow : IRevisionGraphRow
    {
        private static readonly Lane _noLane = new(Index: -1, LaneSharing.ExclusiveOrPrimary);

        public RevisionGraphRow(RevisionGraphRevision revision, IReadOnlyList<RevisionGraphSegment> segments, RevisionGraphRow previousRow)
        {
            Revision = revision;
            Segments = segments;
            _previousRow = previousRow;
        }

        public RevisionGraphRevision Revision { get; }

        public IReadOnlyList<RevisionGraphSegment> Segments { get; }

        private readonly RevisionGraphRow _previousRow;

        /// <summary>
        /// This dictionary contains a cached list of all segments and the lane index the segment is in for this row.
        /// </summary>
        private Dictionary<RevisionGraphSegment, Lane>? _segmentLanes;

        /// <summary>
        /// Contains the gaps created by. <cref>MoveLanesRight</cref>
        /// </summary>
        private HashSet<int>? _gaps;

        /// <summary>
        /// The cached lanecount.
        /// </summary>
        private int _laneCount;

        /// <summary>
        /// The cached revisionlane.
        /// </summary>
        private int _revisionLane;

        // The row contains ordered segments. This method sorts the segments per lane.
        // Segments that cross this row (start above and end below) get there own private lane.
        // Segments that connect to the revision (node) for this row, share the same lane.
        // Building the segment lane cache is not very expensive, but since only a fraction of the rows will be rendered
        // we cache on demand.
        private void BuildSegmentLanes()
        {
            if (_segmentLanes is not null)
            {
                return;
            }

            // We do not want SegmentLanes to be built multiple times. Lock it.
            lock (Revision)
            {
                // Another thread could be waiting for the lock, while the _segmentLanes were being built. Check again if _segmentLanes is null.
                if (_segmentLanes is not null)
                {
                    return;
                }

                _segmentLanes = new(capacity: Segments.Count);
                _laneCount = 0;
                _revisionLane = -1;
                bool hasStart = false;
                bool hasEnd = false;

                foreach (RevisionGraphSegment segment in Segments)
                {
                    _segmentLanes.Add(segment, CreateOrReuseLane(segment));
                }

                if (_revisionLane < 0)
                {
                    _revisionLane = CreateLane();
                }

                return;

                Lane CreateOrReuseLane(RevisionGraphSegment segment)
                {
                    // All segments that connect to the current revision are in the same lane.
                    //              | | * |                               prev StartLane:   0                   1                   2               3
                    //              | |/ /                                prev LaneSharing: ExclusiveOrPrimary, ExclusiveOrPrimary, DifferentStart, ExclusiveOrPrimary
                    // start        * | |                                      StartLane:   0                   1                   1               2
                    //              |/_/     <-- segment.Parent == Revision => LaneSharing: ExclusiveOrPrimary, DifferentStart,     Entire,         DifferentStart
                    // center       *____    <-- this.Revision                 CenterLane:  _revisionLane                                           ^^^ impossible: would also be shared with lane 1
                    //              |\ \ \   <-- segment.Child == Revision  => LaneSharing: ExclusiveOrPrimary, DifferentEnd, DifferentEnd, DifferentEnd (never entirely shared)
                    // end          | | * |                                    EndLane:     0                   1             2             3

                    if (segment.Child == Revision)
                    {
                        // The current segment starts at the revision of this row. Store the revision lane.
                        if (_revisionLane < 0)
                        {
                            _revisionLane = CreateLane();
                        }

                        LaneSharing laneSharing;
                        if (!hasStart)
                        {
                            hasStart = true;
                            laneSharing = LaneSharing.ExclusiveOrPrimary;
                        }
                        else
                        {
                            laneSharing = LaneSharing.DifferentEnd;
                        }

                        return new Lane(_revisionLane, laneSharing);
                    }

                    if (segment.Parent == Revision)
                    {
                        // The current segment ends at the revision of this row. Store the revision lane.
                        if (_revisionLane < 0)
                        {
                            _revisionLane = CreateLane();
                        }

                        // prev start   | | *                                 prev StartLane:   0                   1                   2
                        //              | |/                                  prev LaneSharing: ExclusiveOrPrimary, ExclusiveOrPrimary, DifferentStart
                        // prev center  * |                                        StartLane:   0                   1                   1
                        //              |/       <-- segment.Parent == Revision => LaneSharing: ExclusiveOrPrimary, DifferentStart,     Entire
                        // prev end     *        <-- this.Revision                 CenterLane:  _revisionLane
                        LaneSharing laneSharing;
                        if (!hasEnd)
                        {
                            hasEnd = true;
                            laneSharing = LaneSharing.ExclusiveOrPrimary;
                        }
                        else
                        {
                            laneSharing = GetSecondarySharingOfContinuedSegment();
                        }

                        return new Lane(_revisionLane, laneSharing);
                    }

                    // This is a crossing lane. We could not merge it in the lane with this row's revision.

                    // Try to detect this:
                    // | * | |
                    // | | | |
                    // * | | |
                    // | | | |
                    // | | | | <-- this is the row we are processing
                    // |/_/_/
                    // *       <-- same parent, not on current row
                    //
                    // And change it into this, by merging the segments in a singe lane:
                    // | * |
                    // | |/    <-- LaneSharing: ExclusiveOrPrimary, ExclusiveOrPrimary, DifferentStart, Entire
                    // * |     <-- (previous row: merge into a singe lane to simplify graph - added here just for LaneSharing value)
                    // |/      <-- LaneSharing: ExclusiveOrPrimary, DifferentStart,     Entire,         Entire
                    // |       <-- processed row: merge into a singe lane to simplify graph
                    // |
                    // *
                    foreach (KeyValuePair<RevisionGraphSegment, Lane> searchParent in _segmentLanes)
                    {
                        // If there is another segment with the same parent, and it is not this row's revision, merge into one lane.
                        if (searchParent.Value.Index != _revisionLane && searchParent.Key.Parent == segment.Parent)
                        {
                            return new Lane(searchParent.Value.Index, GetSecondarySharingOfContinuedSegment());
                        }
                    }

                    // Segment has not been assigned a lane yet
                    return new Lane(CreateLane(), LaneSharing.ExclusiveOrPrimary);

                    LaneSharing GetSecondarySharingOfContinuedSegment()
                    {
                        return _previousRow.GetLaneForSegment(segment).Sharing switch
                        {
                            LaneSharing.ExclusiveOrPrimary or LaneSharing.DifferentEnd => LaneSharing.DifferentStart,
                            LaneSharing.Entire or LaneSharing.DifferentStart => LaneSharing.Entire,
                            _ => throw new NotImplementedException()
                        };
                    }
                }

                int CreateLane()
                {
                    return _laneCount++;
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
            foreach (KeyValuePair<RevisionGraphSegment, Lane> keyValue in _segmentLanes!)
            {
                if (keyValue.Value.Index == index)
                {
                    yield return keyValue.Key;
                }
            }
        }

        public Lane GetLaneForSegment(RevisionGraphSegment revisionGraphRevision)
        {
            BuildSegmentLanes();
            if (_segmentLanes!.TryGetValue(revisionGraphRevision, out Lane lane))
            {
                return lane;
            }

            return _noLane;
        }

        public void MoveLanesRight(int fromLane)
        {
            int nextGap = _gaps?.Min(lane => lane > fromLane ? lane : null) ?? int.MaxValue;

            if (_revisionLane >= fromLane && _revisionLane < nextGap)
            {
                ++_revisionLane;
            }

            Validates.NotNull(_segmentLanes);
            RevisionGraphSegment[] segmentsToBeMoved = _segmentLanes.Where(keyValue => keyValue.Value.Index >= fromLane && keyValue.Value.Index < nextGap)
                                                                    .Select(keyValue => keyValue.Key)
                                                                    .ToArray();
            if (!segmentsToBeMoved.Any())
            {
                return;
            }

            _gaps ??= new();
            _gaps.Add(fromLane);
            if (nextGap < int.MaxValue)
            {
                _gaps.Remove(nextGap);
            }
            else
            {
                ++_laneCount;
            }

            foreach (RevisionGraphSegment segment in segmentsToBeMoved)
            {
                Lane lane = _segmentLanes[segment];
                _segmentLanes[segment] = new Lane(lane.Index + 1, lane.Sharing);
            }
        }
    }
}
