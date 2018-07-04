using System;
using System.Text;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal sealed class ActiveLaneRow : ILaneRow
    {
        private Edges _edges;

        public int NodeLane { get; set; } = -1;

        public Node Node { get; set; }

        public Edge[] EdgeList => _edges.EdgeList.ToArray();

        public int Count => _edges.CountCurrent();

        public int LaneInfoCount(int lane)
        {
            return _edges.CountCurrent(lane);
        }

        public LaneInfo this[int col, int row] => _edges.Current(col, row);

        public void Add(int lane, LaneInfo data)
        {
            _edges.Add(lane, data);
        }

        public void Clear()
        {
            _edges = new Edges();
        }

        public void Clear(int lane)
        {
            _edges.Clear(lane);
        }

        public void Collapse(int col)
        {
            int edgeCount = Math.Max(_edges.CountCurrent(), _edges.CountNext());
            for (int i = col; i < edgeCount; i++)
            {
                while (_edges.CountNext(i) > 0)
                {
                    LaneInfo info = _edges.RemoveNext(i, 0, out var start, out _);
                    info.ConnectLane--;
                    _edges.Add(start, info);
                }
            }
        }

        public void Expand(int col)
        {
            int edgeCount = Math.Max(_edges.CountCurrent(), _edges.CountNext());
            for (int i = edgeCount - 1; i >= col; --i)
            {
                while (_edges.CountNext(i) > 0)
                {
                    LaneInfo info = _edges.RemoveNext(i, 0, out var start, out _);
                    info.ConnectLane++;
                    _edges.Add(start, info);
                }
            }
        }

        public void Replace(int old, int @new)
        {
            for (int j = _edges.CountNext(old) - 1; j >= 0; --j)
            {
                LaneInfo info = _edges.RemoveNext(old, j, out var start, out _);
                info.ConnectLane = @new;
                _edges.Add(start, info);
            }
        }

        public void Swap(int old, int @new)
        {
            // TODO: There is a more efficient way to do this
            int temp = _edges.CountNext();
            Replace(old, temp);
            Replace(@new, old);
            Replace(temp, @new);
        }

        public ILaneRow Advance()
        {
            var newLaneRow = new SavedLaneRow(this);

            var newEdges = new Edges();
            for (int i = 0; i < _edges.CountNext(); i++)
            {
                int edgeCount = _edges.CountNext(i);
                if (edgeCount > 0)
                {
                    LaneInfo info = _edges.Next(i, 0).Clone();
                    for (int j = 1; j < edgeCount; j++)
                    {
                        LaneInfo edgeInfo = _edges.Next(i, j);
                        info.UnionWith(edgeInfo);
                    }

                    newEdges.Add(i, info);
                }
            }

            _edges = newEdges;

            return newLaneRow;
        }

#if DEBUG
        public override string ToString()
        {
            var str = new StringBuilder();

            str.Append(NodeLane).Append('/').Append(Count).Append(": ");

            for (int i = 0; i < Count; i++)
            {
                if (i == NodeLane)
                {
                    str.Append('*');
                }

                str.Append('{');

                var count = LaneInfoCount(i);

                for (var j = 0; j < count; j++)
                {
                    str.Append(' ').Append(this[i, j]);
                }

                str.Append(" }, ");
            }

            str.Append(Node);

            return str.ToString();
        }
#endif
    }
}