using System;
using System.Collections.Generic;

using System.Text;

namespace GitCommands
{
    public class LanePoint
    {
        public LanePoint(int pointNumber)
        {
            PointNumber = pointNumber;
            BranchFrom = null;
            BranchFromPoint = null;
        }

        public int PointNumber { get; set; }
        public GitRevision Revision { get; set; }
        public LanePoint BranchFromPoint { get; set; }
        public Lane BranchFrom { get; set; }
        public Lane Lane { get; set; }
    }
}
