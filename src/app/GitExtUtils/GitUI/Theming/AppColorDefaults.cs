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
                { AppColor.DiffAddedExtra, Color.FromArgb(135, 255, 135) },
                { AppColor.AnsiTerminalBlackForeNormal, Color.FromArgb(0x00, 0x00, 0x00) },
                { AppColor.AnsiTerminalBlackBackNormal, Color.FromArgb(0x00, 0x00, 0x00) },
                { AppColor.AnsiTerminalBlackForeBold, Color.FromArgb(0x60, 0x60, 0x60) },
                { AppColor.AnsiTerminalBlackBackBold, Color.FromArgb(0x60, 0x60, 0x60) },
                { AppColor.AnsiTerminalRedForeNormal, Color.FromArgb(0xd4, 0x2c, 0x3a) },
                { AppColor.AnsiTerminalRedBackNormal, Color.FromArgb(0xd4, 0x2c, 0x3a) },
                { AppColor.AnsiTerminalRedForeBold, Color.FromArgb(0xff, 0x76, 0x76) },
                { AppColor.AnsiTerminalRedBackBold, Color.FromArgb(0xff, 0x76, 0x76) },
                { AppColor.AnsiTerminalGreenForeNormal, Color.FromArgb(0x1c, 0xa8, 0x00) },
                { AppColor.AnsiTerminalGreenBackNormal, Color.FromArgb(0x1c, 0xa8, 0x00) },
                { AppColor.AnsiTerminalGreenForeBold, Color.FromArgb(0x00, 0xf2, 0x00) },
                { AppColor.AnsiTerminalGreenBackBold, Color.FromArgb(0x00, 0xf2, 0x00) },
                { AppColor.AnsiTerminalYellowForeNormal, Color.FromArgb(0xc0, 0xa0, 0x00) },
                { AppColor.AnsiTerminalYellowBackNormal, Color.FromArgb(0xc0, 0xa0, 0x00) },
                { AppColor.AnsiTerminalYellowForeBold, Color.FromArgb(0xf2, 0xf2, 0x00) },
                { AppColor.AnsiTerminalYellowBackBold, Color.FromArgb(0xf2, 0xf2, 0x00) },
                { AppColor.AnsiTerminalBlueForeNormal, Color.FromArgb(0x00, 0x5d, 0xff) },
                { AppColor.AnsiTerminalBlueBackNormal, Color.FromArgb(0x00, 0x5d, 0xff) },
                { AppColor.AnsiTerminalBlueForeBold, Color.FromArgb(0x7d, 0x97, 0xff) },
                { AppColor.AnsiTerminalBlueBackBold, Color.FromArgb(0x7d, 0x97, 0xff) },
                { AppColor.AnsiTerminalMagentaForeNormal, Color.FromArgb(0xb1, 0x48, 0xc6) },
                { AppColor.AnsiTerminalMagentaBackNormal, Color.FromArgb(0xb1, 0x48, 0xc6) },
                { AppColor.AnsiTerminalMagentaForeBold, Color.FromArgb(0xff, 0x70, 0xff) },
                { AppColor.AnsiTerminalMagentaBackBold, Color.FromArgb(0xff, 0x70, 0xff) },
                { AppColor.AnsiTerminalCyanForeNormal, Color.FromArgb(0x00, 0xa8, 0x9a) },
                { AppColor.AnsiTerminalCyanBackNormal, Color.FromArgb(0x00, 0xa8, 0x9a) },
                { AppColor.AnsiTerminalCyanForeBold, Color.FromArgb(0x00, 0xf0, 0xf0) },
                { AppColor.AnsiTerminalCyanBackBold, Color.FromArgb(0x00, 0xf0, 0xf0) },
                { AppColor.AnsiTerminalWhiteForeNormal, Color.FromArgb(0xbf, 0xbf, 0xbf) },
                { AppColor.AnsiTerminalWhiteBackNormal, Color.FromArgb(0xbf, 0xbf, 0xbf) },
                { AppColor.AnsiTerminalWhiteForeBold, Color.FromArgb(0xff, 0xff, 0xff) },
                { AppColor.AnsiTerminalWhiteBackBold, Color.FromArgb(0xff, 0xff, 0xff) }
            };

        private static readonly Dictionary<string, Dictionary<AppColor, Color>> Variations = new()
        {
            {
                "colorblind", new()
                {
                   { AppColor.Graph, Color.FromArgb(0x06, 0x00, 0xa8) },
                   { AppColor.RemoteBranch, Color.FromArgb(0x06, 0x00, 0xa8) },
                   { AppColor.AnsiTerminalRedBackNormal, Color.FromArgb(0x94, 0xcb, 0xff) },
                   { AppColor.AnsiTerminalRedBackBold, Color.FromArgb(0x4d, 0xa6, 0xff) },
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
