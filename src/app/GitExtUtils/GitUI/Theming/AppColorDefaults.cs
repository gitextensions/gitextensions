namespace GitExtUtils.GitUI.Theming;

public static class AppColorDefaults
{
    public static readonly Color FallbackColor = Color.Magenta;

    private static readonly Dictionary<AppColor, Color> _values =
        new()
        {
            { AppColor.PanelBackground, SystemColors.Window },
            { AppColor.EditorBackground, SystemColors.Window },
            { AppColor.LineNumberBackground, SystemColors.Window },
            { AppColor.AuthoredHighlight, Color.FromArgb(0xea, 0xf1, 0xff) },
            { AppColor.HighlightAllOccurences, Color.FromArgb(0xe8, 0xe8, 0xff) },
            { AppColor.InactiveSelectionHighlight, Color.FromArgb(0xe6, 0xe6, 0xe6) },
            { AppColor.GraphBranch1, Color.FromArgb(0xf0, 0x64, 0xa0) },
            { AppColor.GraphBranch2, Color.FromArgb(0x78, 0xb4, 0xe6) },
            { AppColor.GraphBranch3, Color.FromArgb(0x24, 0xc2, 0x21) },
            { AppColor.GraphBranch4, Color.FromArgb(0xa0, 0x78, 0xf0) },
            { AppColor.GraphBranch5, Color.FromArgb(0xdd, 0x32, 0x28) },
            { AppColor.GraphBranch6, Color.FromArgb(0x1a, 0xc6, 0xa6) },
            { AppColor.GraphBranch7, Color.FromArgb(0xe7, 0xb0, 0x0f) },
            { AppColor.GraphBranch8, Color.Empty },
            { AppColor.GraphNonRelativeBranch, Color.LightGray },
            { AppColor.Branch, Color.FromArgb(0x00, 0x80, 0x00) },
            { AppColor.RemoteBranch, Color.FromArgb(0x8b, 0x00, 0x09) },
            { AppColor.Tag, Color.DarkBlue },
            { AppColor.OtherTag, Color.Gray },
            { AppColor.DiffSection, Color.FromArgb(230, 230, 230) },
            { AppColor.AnsiTerminalBlackForeNormal, Color.FromArgb(0x00, 0x00, 0x00) },
            { AppColor.AnsiTerminalBlackBackNormal, Color.FromArgb(0x60, 0x60, 0x60) },
            { AppColor.AnsiTerminalBlackForeBold, Color.FromArgb(0x40, 0x40, 0x40) },
            { AppColor.AnsiTerminalBlackBackBold, Color.FromArgb(0x40, 0x40, 0x40) },
            { AppColor.AnsiTerminalRedForeNormal, Color.FromArgb(0xd3, 0x00, 0x0B) },
            { AppColor.AnsiTerminalRedBackNormal, Color.FromArgb(0xff, 0xc8, 0xc8) },
            { AppColor.AnsiTerminalRedForeBold, Color.FromArgb(0xff, 0x5e, 0x5e) },
            { AppColor.AnsiTerminalRedBackBold, Color.FromArgb(0xff, 0xa5, 0xa5) },
            { AppColor.AnsiTerminalGreenForeNormal, Color.FromArgb(0x18, 0x91, 0x00) },
            { AppColor.AnsiTerminalGreenBackNormal, Color.FromArgb(0xc8, 0xff, 0xc8) },
            { AppColor.AnsiTerminalGreenForeBold, Color.FromArgb(0x21, 0xc8, 0x00) },
            { AppColor.AnsiTerminalGreenBackBold, Color.FromArgb(0x87, 0xff, 0x87) },
            { AppColor.AnsiTerminalYellowForeNormal, Color.FromArgb(0xc0, 0xa0, 0x00) },
            { AppColor.AnsiTerminalYellowBackNormal, Color.FromArgb(0xf2, 0xf2, 0x93) },
            { AppColor.AnsiTerminalYellowForeBold, Color.FromArgb(0xe2, 0xc7, 0x3f) },
            { AppColor.AnsiTerminalYellowBackBold, Color.FromArgb(0xff, 0xff, 0x00) },
            { AppColor.AnsiTerminalBlueForeNormal, Color.FromArgb(0x00, 0x5d, 0xff) },
            { AppColor.AnsiTerminalBlueBackNormal, Color.FromArgb(0xc8, 0xd0, 0xf4) },
            { AppColor.AnsiTerminalBlueForeBold, Color.FromArgb(0x60, 0x80, 0xff) },
            { AppColor.AnsiTerminalBlueBackBold, Color.FromArgb(0x93, 0xab, 0xff) },
            { AppColor.AnsiTerminalMagentaForeNormal, Color.FromArgb(0xb1, 0x48, 0xc6) },
            { AppColor.AnsiTerminalMagentaBackNormal, Color.FromArgb(0xf6, 0xce, 0xff) },
            { AppColor.AnsiTerminalMagentaForeBold, Color.FromArgb(0xff, 0x6b, 0xff) },
            { AppColor.AnsiTerminalMagentaBackBold, Color.FromArgb(0xeb, 0x9b, 0xff) },
            { AppColor.AnsiTerminalCyanForeNormal, Color.FromArgb(0x00, 0xa8, 0x9a) },
            { AppColor.AnsiTerminalCyanBackNormal, Color.FromArgb(0xc9, 0xff, 0xf9) },
            { AppColor.AnsiTerminalCyanForeBold, Color.FromArgb(0x00, 0xe0, 0xe0) },
            { AppColor.AnsiTerminalCyanBackBold, Color.FromArgb(0x8c, 0xff, 0xef) },
            { AppColor.AnsiTerminalWhiteForeNormal, Color.FromArgb(0xbf, 0xbf, 0xbf) },
            { AppColor.AnsiTerminalWhiteBackNormal, Color.FromArgb(0xe0, 0xe0, 0xe0) },
            { AppColor.AnsiTerminalWhiteForeBold, Color.FromArgb(0xff, 0xff, 0xff) },
            { AppColor.AnsiTerminalWhiteBackBold, Color.FromArgb(0xff, 0xff, 0xff) },
        };

    private static readonly Dictionary<string, Dictionary<AppColor, Color>> _variations = new()
    {
        {
            "colorblind", new()
            {
               { AppColor.RemoteBranch, Color.FromArgb(0x06, 0x00, 0xa8) },
               { AppColor.AnsiTerminalRedBackNormal, Color.FromArgb(0x94, 0xcb, 0xff) },
               { AppColor.AnsiTerminalRedBackBold, Color.FromArgb(0x4d, 0xa6, 0xff) },
            }
        }
    };

    public static Color GetBy(AppColor name, string[]? variations = null)
    {
        if (!_values.TryGetValue(name, out Color result))
        {
            result = FallbackColor;
        }

        if (variations == null)
        {
            return result;
        }

        foreach (string variation in variations)
        {
            if (!_variations.TryGetValue(variation, out Dictionary<AppColor, Color> colorOverrides))
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
