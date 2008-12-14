using System;
using System.Collections.Generic;

using System.Text;

namespace GitCommands
{
    public class Lane
    {
        public Lane(int laneNumber)
        {
            Points = new List<LanePoint>();
            LaneNumber = laneNumber;
        }

        public List<LanePoint> Points { get; set; }
        public int LaneNumber { get; set; }

        public LanePoint AddPoint(Lane lane, int pointNumber)
        {
            LanePoint point = new LanePoint(pointNumber);
            point.Lane = this;
            Points.Add(point);
            return point;
        }

        public bool Contains(string guid)
        {
            foreach (LanePoint point in Points)
            {
                if (point.BranchFrom == null && point.Revision.Guid == guid)
                    return true;
            }
            return false;
        }

        public bool EndingWith(string guid)
        {
            if (Points.Count > 0)
            {
                if (Points[Points.Count - 1].BranchFrom == null && Points[Points.Count - 1].Revision.Guid == guid)
                    return true;
            }
            return false;
        }
    }
}
