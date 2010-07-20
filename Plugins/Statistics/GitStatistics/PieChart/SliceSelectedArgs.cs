using System;

namespace GitStatistics.PieChart
{
    public delegate void SliceSelectedHandler(object sender, SliceSelectedArgs e);

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

        public SliceSelectedArgs(decimal val, string hint, Object t)
        {
            Value = val;
            Tooltip = hint;
            Tag = t;
        }
    }
}