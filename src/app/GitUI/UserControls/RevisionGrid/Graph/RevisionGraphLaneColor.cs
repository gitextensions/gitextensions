using System.Diagnostics;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    public static class RevisionGraphLaneColor
    {
        public static int GetColorForLane(int seed)
        {
            return Math.Abs(seed) % PresetGraphBrushes.Count;
        }

        public static Color NonRelativeColor { get; } = Color.LightGray;

        internal static Brush NonRelativeBrush { get; }

        internal static readonly List<Brush> PresetGraphBrushes = [];

        static RevisionGraphLaneColor()
        {
            Color[] branchColors = Enum.GetNames(typeof(AppColor))
                .Where(name => name.StartsWith(nameof(AppColor.GraphBranch1)[..^1]))
                .Select(name => ((AppColor)Enum.Parse(typeof(AppColor), name)).GetThemeColor())
                .Where(color => !color.IsEmpty)
                .ToArray();

            if (branchColors.Length < 2)
            {
                Trace.WriteLine("At least two graph colors must be configured");
                branchColors = [Color.Magenta, Color.Cyan];
            }

            foreach (Color color in branchColors)
            {
                PresetGraphBrushes.Add(new SolidBrush(color));
            }

            NonRelativeBrush = new SolidBrush(NonRelativeColor);
        }

        public static Brush GetBrushForLane(int laneColor)
        {
            return PresetGraphBrushes[laneColor];
        }
    }
}
