using System;
using System.Collections.Generic;
using System.Drawing;
using GitCommands;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal interface IJunctionColorProvider
    {
        Color NonRelativeColor { get; }
        Color GetColor(Junction junction, RevisionGraphDrawStyleEnum revisionGraphDrawStyle);
    }

    internal sealed class JunctionColorProvider : IJunctionColorProvider
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
        private readonly HashSet<int> _adjacentColors = new HashSet<int>();
        private readonly Random _random = new Random();

        public Color NonRelativeColor { get; } = Color.LightGray;

        public Color GetColor(Junction junction, RevisionGraphDrawStyleEnum revisionGraphDrawStyle)
        {
            // Non relatives or non-highlighted in empty color
            switch (revisionGraphDrawStyle)
            {
                case RevisionGraphDrawStyleEnum.DrawNonRelativesGray when !junction.IsRelative:
                case RevisionGraphDrawStyleEnum.HighlightSelected when !junction.IsHighlighted:
                    {
                        return NonRelativeColor;
                    }
            }

            if (!AppSettings.MulticolorBranches)
            {
                return AppSettings.GraphColor;
            }

            // See if this junction's colour has already been calculated
            if (junction.ColorIndex > -1 && junction.ColorIndex < PresetGraphColors.Count)
            {
                return PresetGraphColors[junction.ColorIndex];
            }

            var colorIndex = FindDistinctColour(junction);
            junction.ColorIndex = colorIndex;

            return PresetGraphColors[colorIndex];
        }

        private int FindDistinctColour(Junction junction)
        {
            // NOTE we reuse _adjacentColors to avoid allocating lists during UI painting.
            // This is safe as we are always on the UI thread here.
            _adjacentColors.Clear();
            AddAdjacentColors(junction.Youngest.Ancestors);
            AddAdjacentColors(junction.Youngest.Descendants);
            AddAdjacentColors(junction.Oldest.Ancestors);
            AddAdjacentColors(junction.Oldest.Descendants);

            if (_adjacentColors.Count == 0)
            {
                // This is an end-point. Use the first colour.
                return 0;
            }

            // This is a parent branch, calculate new color based on parent branch
            for (var i = 0; i < PresetGraphColors.Count; i++)
            {
                if (!_adjacentColors.Contains(i))
                {
                    return i;
                }
            }

            // All colours are adjacent (highly uncommon!) so just pick one at random
            return _random.Next(PresetGraphColors.Count);

            void AddAdjacentColors(IReadOnlyList<Junction> peers)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < peers.Count; i++)
                {
                    var peer = peers[i];
                    var peerColorIndex = peer.ColorIndex;
                    if (peerColorIndex != -1)
                    {
                        _adjacentColors.Add(peerColorIndex);
                    }
                }
            }
        }
    }
}