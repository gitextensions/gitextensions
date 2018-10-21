using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    public static class RevisionGraphLaneColor
    {
        internal static readonly IReadOnlyList<Color> PresetGraphColors = new[]
{
            Color.FromArgb(240, 36, 117),
            Color.FromArgb(52, 152, 219),
            Color.FromArgb(46, 204, 113),
            Color.FromArgb(142, 68, 173),
            Color.FromArgb(231, 76, 60),
            Color.FromArgb(40, 40, 40),
            Color.FromArgb(26, 188, 156),
            Color.FromArgb(241, 196, 15)
        };

        public static Color NonRelativeColor { get; } = Color.LightGray;

        internal static Brush NonRelativeBrush { get; private set; }

        internal static readonly List<Brush> PresetGraphPens = new List<Brush>();

        static RevisionGraphLaneColor()
        {
            foreach (Color color in PresetGraphColors)
            {
                PresetGraphPens.Add(new SolidBrush(color));
            }

            NonRelativeBrush = new SolidBrush(NonRelativeColor);
        }

        public static Brush GetPenForLane(int laneIndex)
        {
            return PresetGraphPens[laneIndex % PresetGraphPens.Count];
        }
    }
}
