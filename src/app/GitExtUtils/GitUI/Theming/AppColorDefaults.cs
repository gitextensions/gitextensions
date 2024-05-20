namespace GitExtUtils.GitUI.Theming
{
    public static class AppColorDefaults
    {
        public static readonly Color FallbackColor = Color.Magenta;

        private static readonly Dictionary<AppColor, Color> Values =
            new()
            {
                { AppColor.OtherTag, Color.Gray },
                { AppColor.AuthoredHighlight, Color.FromArgb(0xea, 0xf1, 0xff) },
                { AppColor.HighlightAllOccurences, Color.FromArgb(0xe8, 0xe8, 0xff) },
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

        private static readonly Dictionary<string, Dictionary<AppColor, Color>> Variations = new()
        {
            {
                "colorblind", new()
                {
                   { AppColor.Graph, Color.FromArgb(0x06, 0x00, 0xa8) },
                   { AppColor.RemoteBranch, Color.FromArgb(0x06, 0x00, 0xa8) },
                   { AppColor.DiffRemoved, Color.FromArgb(0x94, 0xcb, 0xff) },
                   { AppColor.DiffRemovedExtra, Color.FromArgb(0x4d, 0xa6, 0xff) },
                }
            }
        };

        public static Color GetBy(AppColor name, string[]? variations = null)
        {
            if (!Values.TryGetValue(name, out Color result))
            {
                result = FallbackColor;
            }

            if (variations == null)
            {
                return result;
            }

            foreach (string variation in variations)
            {
                if (!Variations.TryGetValue(variation, out Dictionary<AppColor, Color> colorOverrides))
                {
                    continue;
                }

                if (colorOverrides.TryGetValue(name, out Color colorOverride))
                {
                    result = colorOverride;
                }
            }

            return result;
        }
    }
}
