using System.Collections.Generic;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    public class RevisionGraphRow
    {
        public RevisionGraphRow(RevisionGraphRevision revision)
        {
            Segments = new SynchronizedCollection<RevisionGraphSegment>();
            Revision = revision;
        }

        public RevisionGraphRevision Revision { get; private set; }
        public SynchronizedCollection<RevisionGraphSegment> Segments { get; private set; }

        private IReadOnlyDictionary<RevisionGraphSegment, int> _segmentLanes;

        private int _laneCount;

        private void BuildSegmentLanes()
        {
            int currentRevisionLane = -1;
            if (_segmentLanes == null)
            {
                // We do not want SegementLanes to be build multiple times. Lock it.
                lock (Revision)
                {
                    if (_segmentLanes == null)
                    {
                        Dictionary<RevisionGraphSegment, int> newSegmentLanes = new Dictionary<RevisionGraphSegment, int>();

                        int laneIndex = 0;
                        foreach (var segment in Segments)
                        {
                            if (segment.Child == Revision || segment.Parent == Revision)
                            {
                                if (currentRevisionLane < 0)
                                {
                                    currentRevisionLane = laneIndex;
                                    laneIndex++;
                                }

                                newSegmentLanes.Add(segment, currentRevisionLane);
                            }
                            else
                            {
                                bool added = false;
                                foreach (var searchParent in newSegmentLanes)
                                {
                                    if (searchParent.Value != currentRevisionLane && searchParent.Key.Parent == segment.Parent)
                                    {
                                        newSegmentLanes.Add(segment, searchParent.Value);
                                        added = true;
                                        break;
                                    }
                                }

                                if (!added)
                                {
                                    newSegmentLanes.Add(segment, laneIndex);
                                    laneIndex++;
                                }
                            }
                        }

                        _segmentLanes = newSegmentLanes;
                        _laneCount = laneIndex;
                    }
                }
            }
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
