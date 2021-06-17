using System.Collections.Generic;
using System.Drawing;

namespace GitExtUtils.GitUI.Theming
{
    public static class AppColorDefaults
    {
        public static readonly Color FallbackColor = Color.Magenta;

        private static readonly Dictionary<AppColor, Color> Values =
            new()
            {
                { AppColor.OtherTag, Color.Gray },
                { AppColor.AuthoredHighlight, Color.LightYellow },
                { AppColor.HighlightAllOccurences, Color.LightYellow },
                { AppColor.Tag, Color.DarkBlue },
                { AppColor.Graph, Color.DarkRed },
                { AppColor.Branch, Color.FromArgb(0x00, 0x80, 0x00) },
                { AppColor.RemoteBranch, Color.FromArgb(0x8b, 0x00, 0x09) },
                { AppColor.DiffSection, Color.FromArgb(230, 230, 230) },
                { AppColor.DiffRemoved, Color.FromArgb(255, 200, 200) },
                { AppColor.DiffRemovedExtra, Color.FromArgb(255, 165, 165) },
                { AppColor.DiffAdded, Color.FromArgb(200, 255, 200) },
                { AppColor.DiffAddedExtra, Color.FromArgb(135, 255, 135) }
            };

        public static Color GetBy(AppColor name)
        {
            return Values.TryGetValue(name, out var result)
                ? result
                : FallbackColor;
        }
    }
}
