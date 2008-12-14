using System;
using System.Collections.Generic;

using System.Text;

namespace GitCommands
{
    public class LaneGraph
    {
        public LaneGraph()
        {
            Lanes = new List<Lane>();
            Points = new List<LanePoint>();
        }

        public List<Lane> Lanes { get; set; }
        public List<LanePoint> Points { get; set; }

        public List<Lane> GetLanesForPointnumber(int pointNumber)
        {
            List<Lane> lanes = new List<Lane>();
            foreach (Lane lane in Lanes)
            {
                if (lane.Points.Count > 0 && lane.Points[0].PointNumber <= pointNumber && lane.Points[lane.Points.Count - 1].PointNumber >= pointNumber)
                    lanes.Add(lane);

            }
            return lanes;
        }

        public int GetOptimalLaneNumber(Lane lane)
        {
            return lane.LaneNumber;
            //return Math.Max(GetLanesForPointnumber(lane.Points[0].PointNumber).Count(), GetLanesForPointnumber(lane.Points[lane.Points.Count-1].PointNumber).Count()-1);
                            //GetLanesForPointnumber(lane.Points[lane.Points.Count-1].PointNumber).Count())-1;
        }

        public Lane AddLane()
        {
            Lane lane = new Lane(Lanes.Count);
            Lanes.Add(lane);

            return lane;
        }

        public List<Lane> FindLanesContaining(string guid)
        {
            List<Lane> lanes = new List<Lane>();
            foreach (Lane lane in Lanes)
            {
                if (lane.Contains(guid))
                    lanes.Add(lane);
            }
            return lanes;
        }

        public List<Lane> FindLanesEndingWith(string guid)
        {
            List<Lane> lanes = new List<Lane>();
            foreach (Lane lane in Lanes)
            {
                if (lane.EndingWith(guid))
                    lanes.Add(lane);
            }
            return lanes;
        }

        public LanePoint AddPoint(Lane lane, GitRevision revision)
        {
            LanePoint point = lane.AddPoint(lane, Points.Count);
            Points.Add(point);
            point.Revision = revision;
            return point;
        }
    }
}
