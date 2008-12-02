using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCommands
{
    public class LaneGraphManager
    {
        public static LaneGraph CreateLaneGraph(List<GitRevision> revisions)
        {
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
                            currentPoint = graph.AddPoint(lane, revision.Guid);
                        }
                        else
                            graph.AddPoint(lane, revision.Guid).BranchFrom = currentPoint.Lane;
                    }
                    {
                        //If this revision is not on a lane yet, add a new lane
                        if (currentPoint == null)
                        {
                            Lane newLane = graph.AddLane();
                            currentPoint = graph.AddPoint(newLane, revision.Guid);

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
