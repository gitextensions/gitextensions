using System.Collections.Generic;
using System.Linq;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal sealed class Edges
    {
        private readonly List<int> _countEnd = new List<int>();
        private readonly List<int> _countStart = new List<int>();

        public List<Edge> EdgeList { get; } = new List<Edge>();

        public LaneInfo Current(int lane, int item)
        {
            int found = 0;
            foreach (Edge e in EdgeList)
            {
                if (e.Start == lane)
                {
                    if (item == found)
                    {
                        return e.Data;
                    }

                    found++;
                }
            }

            return default;
        }

        public LaneInfo Next(int lane, int item)
        {
            int found = 0;
            foreach (Edge e in EdgeList)
            {
                if (e.End == lane)
                {
                    if (item == found)
                    {
                        return e.Data;
                    }

                    found++;
                }
            }

            return default;
        }

        public LaneInfo RemoveNext(int lane, int item, out int start, out int end)
        {
            int found = 0;
            for (int i = 0; i < EdgeList.Count; i++)
            {
                if (EdgeList[i].End == lane)
                {
                    if (item == found)
                    {
                        LaneInfo data = EdgeList[i].Data;
                        start = EdgeList[i].Start;
                        end = EdgeList[i].End;
                        _countStart[start]--;
                        _countEnd[end]--;
                        EdgeList.RemoveAt(i);
                        return data;
                    }

                    found++;
                }
            }

            start = -1;
            end = -1;
            return default;
        }

        public void Add(int from, LaneInfo data)
        {
            var e = new Edge(data, from);
            EdgeList.Add(e);

            while (_countStart.Count <= e.Start)
            {
                _countStart.Add(0);
            }

            _countStart[e.Start]++;
            while (_countEnd.Count <= e.End)
            {
                _countEnd.Add(0);
            }

            _countEnd[e.End]++;
        }

        public void Clear(int lane)
        {
            for (int i = EdgeList.Count - 1; i >= 0; --i)
            {
                int start = EdgeList[i].Start;
                if (start == lane)
                {
                    int end = EdgeList[i].End;
                    _countStart[start]--;
                    _countEnd[end]--;
                    EdgeList.RemoveAt(i);
                }
            }
        }

        public int CountCurrent()
        {
            int count = _countStart.Count;
            while (count > 0 && _countStart[count - 1] == 0)
            {
                count--;
                _countStart.RemoveAt(count);
            }

            return count;
        }

        public int CountCurrent(int lane)
        {
            return EdgeList.Count(e => e.Start == lane);
        }

        public int CountNext()
        {
            int count = _countEnd.Count;
            while (count > 0 && _countEnd[count - 1] == 0)
            {
                count--;
                _countEnd.RemoveAt(count);
            }

            return count;
        }

        public int CountNext(int lane)
        {
            // This is called quite a bit (as much as 5% of background thread processing),
            // so avoid using Enumerable.Count(predicate).
            var count = 0;

            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var index = 0; index < EdgeList.Count; index++)
            {
                var e = EdgeList[index];

                if (e.End == lane)
                {
                    count++;
                }
            }

            return count;
        }
    }
}