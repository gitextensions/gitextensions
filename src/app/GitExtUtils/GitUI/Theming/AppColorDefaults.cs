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
                { AppColor.AnsiTerminalBlackNormal, Color.Black },
                { AppColor.AnsiTerminalBlackBold, Color.FromArgb(96, 96, 96) },
                { AppColor.AnsiTerminalBlackDim, Color.FromArgb(127, 127, 127) },
                { AppColor.AnsiTerminalRedNormal, Color.FromArgb(212, 44, 58) },
                { AppColor.AnsiTerminalRedBold, Color.FromArgb(255, 118, 118) },
                { AppColor.AnsiTerminalRedDim, Color.FromArgb(208, 142, 147) },
                { AppColor.AnsiTerminalGreenNormal, Color.FromArgb(28, 168, 0) },
                { AppColor.AnsiTerminalGreenBold, Color.FromArgb(0, 242, 0) },
                { AppColor.AnsiTerminalGreenDim, Color.FromArgb(137, 190, 127) },
                { AppColor.AnsiTerminalYellowNormal, Color.FromArgb(192, 160, 0) },
                { AppColor.AnsiTerminalYellowBold, Color.FromArgb(242, 242, 0) },
                { AppColor.AnsiTerminalYellowDim, Color.FromArgb(199, 187, 127) },
                { AppColor.AnsiTerminalBlueNormal, Color.FromArgb(0, 93, 255) },
                { AppColor.AnsiTerminalBlueBold, Color.FromArgb(125, 151, 255) },
                { AppColor.AnsiTerminalBlueDim, Color.FromArgb(127, 143, 233) },
                { AppColor.AnsiTerminalMagentaNormal, Color.FromArgb(177, 72, 198) },
                { AppColor.AnsiTerminalMagentaBold, Color.FromArgb(255, 112, 255) },
                { AppColor.AnsiTerminalMagentaDim, Color.FromArgb(194, 154, 202) },
                { AppColor.AnsiTerminalCyanNormal, Color.FromArgb(0, 168, 154) },
                { AppColor.AnsiTerminalCyanBold, Color.FromArgb(0, 240, 240) },
                { AppColor.AnsiTerminalCyanDim, Color.FromArgb(127, 190, 184) },
                { AppColor.AnsiTerminalWhiteNormal, Color.FromArgb(191, 191, 191) },
                { AppColor.AnsiTerminalWhiteBold, Color.FromArgb(255, 255, 255) },
                { AppColor.AnsiTerminalWhiteDim, Color.FromArgb(222, 222, 222) }
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
