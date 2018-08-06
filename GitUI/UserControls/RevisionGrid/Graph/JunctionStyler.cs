using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using GitCommands;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal sealed class JunctionStyler
    {
        private readonly IJunctionColorProvider _junctionColorProvider;
        private readonly List<Color> _junctionColors = new List<Color>(capacity: 2);

        public JunctionStyler(IJunctionColorProvider junctionColorProvider)
        {
            _junctionColorProvider = junctionColorProvider;
        }

        public Brush GetLaneBrush()
        {
            if (_junctionColors.Count < 1)
            {
                return Brushes.Black;
            }

            if (_junctionColors.Count == 1 || !AppSettings.StripedBranchChange)
            {
                Color color;
                if (_junctionColors[0] != _junctionColorProvider.NonRelativeColor)
                {
                    color = GetAdjustedLineColor(_junctionColors[0]);
                }
                else if (_junctionColors.Count > 1 && _junctionColors[1] != _junctionColorProvider.NonRelativeColor)
                {
                    color = GetAdjustedLineColor(_junctionColors[1]);
                }
                else
                {
                    color = GetAdjustedLineColor(_junctionColorProvider.NonRelativeColor);
                }

                return new SolidBrush(color);
            }

            Color lastRealColor = _junctionColors.LastOrDefault(c => c != _junctionColorProvider.NonRelativeColor);
            if (lastRealColor.IsEmpty)
            {
                return new SolidBrush(GetAdjustedLineColor(_junctionColorProvider.NonRelativeColor));
            }

            return new HatchBrush(HatchStyle.DarkDownwardDiagonal, GetAdjustedLineColor(_junctionColors[0]), lastRealColor);
        }

        public Brush GetNodeBrush(Rectangle nodeRect, bool isHighlight)
        {
            if (_junctionColors.Count < 1)
            {
                return Brushes.Black;
            }

            if (_junctionColors.Count == 1)
            {
                var nodeColor = isHighlight ? _junctionColors[0] : _junctionColorProvider.NonRelativeColor;
                return new SolidBrush(nodeColor);
            }

            return new LinearGradientBrush(nodeRect, _junctionColors[0], _junctionColors[1], LinearGradientMode.Horizontal);
        }

        public void UpdateJunctionColors(IReadOnlyList<Junction> junctions, RevisionGraphDrawStyleEnum revisionGraphDrawStyle)
        {
            _junctionColors.Clear();

            // Select one or two colours to use when rendering this junction
            if (junctions.Count == 0)
            {
                _junctionColors.Add(Color.Black);
                return;
            }

            for (var i = 0; i < 2 && i < junctions.Count; i++)
            {
                var color = _junctionColorProvider.GetColor(junctions[i], revisionGraphDrawStyle);
                _junctionColors.Add(color);
            }
        }

        // unit test helper
        internal IEnumerable<Color> GetJunctionColors() => _junctionColors.AsEnumerable();

        private static Color GetAdjustedLineColor(Color c) => ColorHelper.MakeColorDarker(c, amount: 0.1);
    }
}