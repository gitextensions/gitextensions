using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    public static class RevisionGraphLaneColor
    {
        internal static readonly IReadOnlyList<Color> PresetGraphColors = new[]
        {
            Color.FromArgb(230, 36, 107), // red-pink
            Color.FromArgb(120, 180, 230), // light blue
            Color.FromArgb(36, 194, 33), // green
            Color.FromArgb(142, 108, 193), // light violet
            Color.FromArgb(221, 76, 60), // red
            Color.FromArgb(60, 120, 220), // dark blue
            Color.FromArgb(26, 198, 166), // cyan-green
            Color.FromArgb(231, 176, 15) // orange
        };

        public static Color NonRelativeColor { get; } = Color.LightGray;

        internal static Brush NonRelativeBrush { get; private set; }

        internal static readonly List<Brush> PresetGraphBrushes = new List<Brush>();

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
            return PresetGraphBrushes[Math.Abs(laneColor) % PresetGraphBrushes.Count];
        }
    }
}
