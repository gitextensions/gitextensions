using System;
using System.Linq;
using System.Text;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal sealed class SavedLaneRow : ILaneRow
    {
        private readonly Edge[] _edges;

        public int NodeLane { get; }

        public Node Node { get; }

        public SavedLaneRow(Node node)
        {
            Node = node;
            NodeLane = -1;
            _edges = null;
        }

        public SavedLaneRow(ActiveLaneRow activeRow)
        {
            NodeLane = activeRow.NodeLane;
            Node = activeRow.Node;
            _edges = activeRow.EdgeList;
        }

        public LaneInfo this[int col, int row]
        {
            get
            {
                int count = 0;
                foreach (Edge edge in _edges)
                {
                    if (edge.Start == col)
                    {
                        if (count == row)
                        {
                            return edge.Data;
                        }

                        count++;
                    }
                }

                throw new Exception("Bad lane");
            }
        }

        public int Count
        {
            get
            {
                if (_edges == null)
                {
                    return 0;
                }

                int count = -1;
                foreach (Edge edge in _edges)
                {
                    if (edge.Start > count)
                    {
                        count = edge.Start;
                    }
                }

                return count + 1;
            }
        }

        public int LaneInfoCount(int lane)
        {
            return _edges.Count(edge => edge.Start == lane);
        }

#if DEBUG
        public override string ToString()
        {
            var s = new StringBuilder()
                .Append(NodeLane).Append('/').Append(Count).Append(": ");

            for (var i = 0; i < Count; i++)
            {
                if (i == NodeLane)
                {
                    s.Append('*');
                }

                s.Append('{');

                for (var j = 0; j < LaneInfoCount(i); j++)
                {
                    s.Append(' ').Append(this[i, j]);
                }

                s.Append(" }, ");
            }

            s.Append(Node);

            return s.ToString();
        }
#endif
    }
}