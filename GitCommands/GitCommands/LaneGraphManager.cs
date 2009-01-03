using System;
using System.Collections.Generic;

using System.Text;

namespace GitCommands
{
    public class LaneGraphManager
    {
        public static LaneGraph CreateLaneGraph()
        {
            LaneGraph graph = new LaneGraph();

            //Create all seperate lanes
            foreach (GitHead head in GitCommands.GetHeads(false))
            {
                Lane lane = graph.AddLane();

                foreach (GitRevision revision in GitCommands.GitRevisions(head.Name))
                {
                    graph.AddPoint(lane, revision);
                }
            }

            //Merge all lanes
            foreach (Lane lane in graph.Lanes)
            {
                //LanePoint lastPo
                for (int n = lane.Points.Count - 1; n >= 0; n--)
                {
                    LanePoint point = lane.Points[n];
                    foreach (Lane searchLane in graph.Lanes)
                    {
                        if (searchLane != lane && searchLane.LaneNumber < lane.LaneNumber)
                        foreach (LanePoint searchPoint in searchLane.Points)
                        {
                            if (searchPoint != point)
                            {
                                if (searchPoint.Revision.Guid.Equals(point.Revision.ParentGuids[0]))
                                {
                                    point.BranchFrom = searchLane;
                                    point.BranchFromPoint = searchPoint;
                                }

                                if (searchPoint.Revision.Guid.Equals(point.Revision.Guid))
                                {
                                    lane.Points.Remove(point);
                                    graph.Points.Remove(point);
                                    break;
                                }
                            }
                        }

                    }
                }
            }

            //f:\dev>git.cmd log --topo-order --no-color --parents --boundary -z --pretty="for
            //mat:%m%HX%PX%n%an<%ae>%n%at%n%s%n" master



            return graph;
        }

        public static LaneGraph CreateLaneGraph(List<GitRevision> revisions)
        {
            return CreateLaneGraph();

            LaneGraph graph = new LaneGraph();

            //foreach (GitRevision revision in revisions)
            for (int n = revisions.Count-1; n >= 0; n--)
            {
                GitRevision revision = revisions[n];
                foreach (string parentGuid in revision.ParentGuids)
                {
                    LanePoint currentPoint = null;

                    //Add point to all lanes
                    foreach (Lane lane in graph.FindLanesEndingWith(parentGuid))
                    {
                        //Add point to lane
                        if (currentPoint == null)
                        {
                            currentPoint = graph.AddPoint(lane, revision);
                        }
                        else
                            graph.AddPoint(lane, revision).BranchFrom = currentPoint.Lane;
                    }
                    {
                        //If this revision is not on a lane yet, add a new lane
                        if (currentPoint == null)
                        {
                            Lane newLane = graph.AddLane();
                            currentPoint = graph.AddPoint(newLane, revision);

                            foreach (Lane searchLane in graph.Lanes)
                            {
                                if (searchLane.Contains(parentGuid) && searchLane != newLane)
                                {
                                    currentPoint.BranchFrom = searchLane;
                                }
                            }
                        }
                    }
                }
            }
            
            return graph;
        }

    }
}
