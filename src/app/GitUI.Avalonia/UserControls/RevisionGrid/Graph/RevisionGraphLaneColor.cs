using System.Diagnostics;
using Avalonia.Media;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using Color = System.Drawing.Color;

namespace GitUI.UserControls.RevisionGrid.Graph;

// Twin of GitUI/UserControls/RevisionGrid/Graph/RevisionGraphLaneColor.cs with Avalonia
// brushes instead of GDI brushes. Keep the lane-color selection logic in sync with upstream.
public static class RevisionGraphLaneColor
{
    public static int GetColorForLane(int seed)
    {
        return Math.Abs(seed) % PresetGraphBrushes.Count;
    }

    public static Color NonRelativeColor { get; } = AppColor.GraphNonRelativeBranch.GetThemeColor();

    internal static IBrush NonRelativeBrush { get; }

    internal static readonly List<IBrush> PresetGraphBrushes = [];

    static RevisionGraphLaneColor()
    {
        Color[] branchColors = [.. Enum.GetNames<AppColor>()
            .Where(name => name.StartsWith(nameof(AppColor.GraphBranch1)[..^1]))
            .Select(name => Enum.Parse<AppColor>(name).GetThemeColor())
            .Where(color => !color.IsEmpty)
            .Distinct()];

        const int minBranchColors = 4;
        if (branchColors.Length < minBranchColors)
        {
            Trace.WriteLine(@"At least {minBranchColors} different graph colors must be configured - using crying fallback");
            branchColors = [Color.Cyan, Color.Magenta, Color.Yellow, Color.Lime];
        }

        foreach (Color color in branchColors)
        {
            PresetGraphBrushes.Add(CreateBrush(color));
        }

        NonRelativeBrush = CreateBrush(NonRelativeColor);
    }

    public static IBrush GetBrushForLane(int laneColor)
    {
        return PresetGraphBrushes[laneColor];
    }

    private static IBrush CreateBrush(Color color)
    {
        SolidColorBrush brush = new(Avalonia.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        return brush.ToImmutable();
    }
}
