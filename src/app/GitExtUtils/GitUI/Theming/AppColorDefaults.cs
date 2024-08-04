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
                { AppColor.AnsiTerminalBlackNormal, Color.FromArgb(0x00, 0x00, 0x00) },
                { AppColor.AnsiTerminalBlackBold, Color.FromArgb(0x60, 0x60, 0x60) },
                { AppColor.AnsiTerminalBlackDim, Color.FromArgb(0x7f, 0x7f, 0x7f) },
                { AppColor.AnsiTerminalRedNormal, Color.FromArgb(0xd4, 0x2c, 0x3a) },
                { AppColor.AnsiTerminalRedBold, Color.FromArgb(0xff, 0x76, 0x76) },
                { AppColor.AnsiTerminalRedDim, Color.FromArgb(0xd0, 0x8e, 0x93) },
                { AppColor.AnsiTerminalGreenNormal, Color.FromArgb(0x1c, 0xa8, 0x00) },
                { AppColor.AnsiTerminalGreenBold, Color.FromArgb(0x00, 0xf2, 0x00) },
                { AppColor.AnsiTerminalGreenDim, Color.FromArgb(0x89, 0xbe, 0x7f) },
                { AppColor.AnsiTerminalYellowNormal, Color.FromArgb(0xc0, 0xa0, 0x00) },
                { AppColor.AnsiTerminalYellowBold, Color.FromArgb(0xf2, 0xf2, 0x00) },
                { AppColor.AnsiTerminalYellowDim, Color.FromArgb(0xc7, 0xbb, 0x7f) },
                { AppColor.AnsiTerminalBlueNormal, Color.FromArgb(0x00, 0x5d, 0xff) },
                { AppColor.AnsiTerminalBlueBold, Color.FromArgb(0x7d, 0x97, 0xff) },
                { AppColor.AnsiTerminalBlueDim, Color.FromArgb(0x7f, 0x8f, 0xe9) },
                { AppColor.AnsiTerminalMagentaNormal, Color.FromArgb(0xb1, 0x48, 0xc6) },
                { AppColor.AnsiTerminalMagentaBold, Color.FromArgb(0xff, 0x70, 0xff) },
                { AppColor.AnsiTerminalMagentaDim, Color.FromArgb(0xc2, 0x9a, 0xca) },
                { AppColor.AnsiTerminalCyanNormal, Color.FromArgb(0x00, 0xa8, 0x9a) },
                { AppColor.AnsiTerminalCyanBold, Color.FromArgb(0x00, 0xf0, 0xf0) },
                { AppColor.AnsiTerminalCyanDim, Color.FromArgb(0x7f, 0xbe, 0xb8) },
                { AppColor.AnsiTerminalWhiteNormal, Color.FromArgb(0xbf, 0xbf, 0xbf) },
                { AppColor.AnsiTerminalWhiteBold, Color.FromArgb(0xff, 0xff, 0xff) },
                { AppColor.AnsiTerminalWhiteDim, Color.FromArgb(0xde, 0xde, 0xde) }
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
