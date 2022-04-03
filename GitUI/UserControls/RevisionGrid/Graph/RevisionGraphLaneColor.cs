using System;
using System.Collections.Generic;
using System.Drawing;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    public static class RevisionGraphLaneColor
    {
        internal static readonly IReadOnlyList<Color> PresetGraphColors = new[]
        {
            Color.FromArgb(240, 100, 160), // red-pink
            Color.FromArgb(120, 180, 230), // light blue
            Color.FromArgb(36, 194, 33), // green
            Color.FromArgb(160, 120, 240), // light violet
            Color.FromArgb(221, 50, 40), // red
            Color.FromArgb(26, 198, 166), // cyan-green
            Color.FromArgb(231, 176, 15) // orange
        };

        public static int GetColorForLane(int seed)
        {
            return Math.Abs(seed) % PresetGraphBrushes.Count;
        }

        public static Color NonRelativeColor { get; } = Color.LightGray;

        internal static Brush NonRelativeBrush { get; }

        internal static readonly List<Brush> PresetGraphBrushes = new();

        static RevisionGraphLaneColor()
        {
            foreach (Color color in PresetGraphColors)
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
