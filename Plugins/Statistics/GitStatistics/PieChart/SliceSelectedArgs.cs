using System;

namespace GitStatistics.PieChart
{
    public class SliceSelectedArgs : EventArgs
    {
        public object Tag;
        public string Tooltip;
        public decimal Value;

        public SliceSelectedArgs(decimal val, string hint)
        {
            Value = val;
            Tooltip = hint;
        }

        public SliceSelectedArgs(decimal val, string hint, object t)
        {
            Value = val;
            Tooltip = hint;
            Tag = t;
        }
    }
}