using System.Collections.Generic;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal struct LaneInfo
    {
        private readonly List<Junction> _junctions;

        public int ConnectLane { get; set; }

        public LaneInfo(int connectLane, Junction junction)
            : this(connectLane, new List<Junction> { junction })
        {
        }

        private LaneInfo(int connectLane, List<Junction> junctions)
        {
            ConnectLane = connectLane;
            _junctions = junctions;
        }

        public IReadOnlyList<Junction> Junctions => _junctions;

        public LaneInfo Clone()
        {
            return new LaneInfo(ConnectLane, new List<Junction>(_junctions));
        }

        public void UnionWith(LaneInfo other)
        {
            foreach (Junction otherJunction in other._junctions)
            {
                if (!_junctions.Contains(otherJunction))
                {
                    _junctions.Add(otherJunction);
                }
            }

            _junctions.TrimExcess();
        }

#if DEBUG
        public override string ToString() => ConnectLane.ToString();
#endif
    }
}