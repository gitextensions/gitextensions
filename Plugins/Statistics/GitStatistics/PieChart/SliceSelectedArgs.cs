using System;

namespace GitExtensions.Plugins.GitStatistics.PieChart
{
    public class SliceSelectedArgs : EventArgs
    {
        public object? Tag { get; }
        public string ToolTip { get; }
        public decimal Value { get; }

        public SliceSelectedArgs(decimal value, string toolTip)
        {
            Value = value;
            ToolTip = toolTip;
        }

        public SliceSelectedArgs(decimal value, string toolTip, object? tag)
        {
            Value = value;
            ToolTip = toolTip;
            Tag = tag;
        }
    }
}
