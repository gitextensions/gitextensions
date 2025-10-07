using System.Diagnostics;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;

namespace GitUI.UserControls.RevisionGrid.Graph;

public static class RevisionGraphLaneColor
{
    public static int GetColorForLane(int seed)
    {
        return Math.Abs(seed) % PresetGraphBrushes.Count;
    }

    public static Color NonRelativeColor { get; } = AppColor.GraphNonRelativeBranch.GetThemeColor();

    internal static Brush NonRelativeBrush { get; }

    internal static readonly List<Brush> PresetGraphBrushes = [];

    static RevisionGraphLaneColor()
    {
        Color[] branchColors = Enum.GetNames(typeof(AppColor))
            .Where(name => name.StartsWith(nameof(AppColor.GraphBranch1)[..^1]))
            .Select(name => ((AppColor)Enum.Parse(typeof(AppColor), name)).GetThemeColor())
            .Where(color => !color.IsEmpty)
            .Distinct()
            .ToArray();

        const int minBranchColors = 4;
        if (branchColors.Length < minBranchColors)
        {
            Trace.WriteLine(@"At least {minBranchColors} different graph colors must be configured - using crying fallback");
            branchColors = [Color.Cyan, Color.Magenta, Color.Yellow, Color.Lime];
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
