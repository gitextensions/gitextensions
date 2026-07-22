using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using GitExtUtils.GitUI.Theming;
using DrawingColor = System.Drawing.Color;
using MediaColor = Avalonia.Media.Color;

namespace GitUI.Compat;

/// <summary>
///  Projects the framework-neutral Git Extensions theme into Avalonia resources.
/// </summary>
internal static class AvaloniaThemeResources
{
    internal const string AppColorPrefix = "GitExtensionsAppColor";
    internal const string KnownColorPrefix = "GitExtensionsKnownColor";

    public static void Apply(Application application, ThemeSettings settings)
    {
        bool isDark = settings.Theme.SystemColorMode == GitExtensions.Shims.WinForms.SystemColorMode.Dark;
        ResourceDictionary resources = GetThemeResources(application, isDark ? ThemeVariant.Dark : ThemeVariant.Light);

        foreach (AppColor name in Enum.GetValues<AppColor>())
        {
            DrawingColor color = GetAppColor(settings, name);
            PublishColor(resources, AppColorPrefix + name, color);
        }

        foreach (KnownColor name in Enum.GetValues<KnownColor>())
        {
            if (!DrawingColor.FromKnownColor(name).IsSystemColor)
            {
                continue;
            }

            PublishColor(resources, KnownColorPrefix + name, GetKnownColor(settings, name, isDark));
        }

        DrawingColor panel = GetAppColor(settings, AppColor.PanelBackground);
        DrawingColor editor = GetAppColor(settings, AppColor.EditorBackground);
        DrawingColor selection = GetAppColor(settings, AppColor.Selection);
        DrawingColor windowText = GetKnownColor(settings, KnownColor.WindowText, isDark);
        DrawingColor grayText = GetKnownColor(settings, KnownColor.GrayText, isDark);
        DrawingColor sectionBorder = Lerp(panel, windowText, isDark ? 0.14f : 0.18f);
        DrawingColor treeConnector = Lerp(panel, windowText, isDark ? 0.38f : 0.46f);
        DrawingColor refLabelBackground = isDark ? Lerp(panel, DrawingColor.Black, 0.36f) : panel;
        DrawingColor removedBackground = GetAppColor(settings, AppColor.AnsiTerminalRedBackNormal);
        DrawingColor addedBackground = GetAppColor(settings, AppColor.AnsiTerminalGreenBackNormal);
        DrawingColor removedForeground = GetAppColor(settings, AppColor.AnsiTerminalRedForeNormal);
        DrawingColor addedForeground = GetAppColor(settings, AppColor.AnsiTerminalGreenForeNormal);
        DrawingColor movedRemovedForeground = GetAppColor(settings, AppColor.AnsiTerminalMagentaForeNormal);
        DrawingColor movedAddedForeground = GetAppColor(settings, AppColor.AnsiTerminalBlueForeNormal);
        DrawingColor dimmedRemovedBackground = Dim(Dim(removedBackground, editor), editor);
        DrawingColor dimmedAddedBackground = Dim(Dim(addedBackground, editor), editor);
        DrawingColor resetSoft = DrawingColor.FromArgb(128, 255, 128);
        DrawingColor resetMixed = DrawingColor.FromArgb(255, 255, 128);
        DrawingColor resetHard = DrawingColor.FromArgb(255, 128, 128);
        if (isDark)
        {
            resetSoft = Dim(resetSoft, editor);
            resetMixed = Dim(resetMixed, editor);
            resetHard = Dim(resetHard, editor);
        }

        SetBrush(resources, "ThemeBackgroundBrush", panel);
        SetBrush(resources, "ThemeForegroundBrush", windowText);
        SetBrush(resources, "ThemeBorderLowBrush", sectionBorder);
        SetBrush(resources, "GitExtensionsPanelBackgroundBrush", panel);
        SetBrush(resources, "GitExtensionsWindowTextBrush", windowText);
        SetBrush(resources, "GitExtensionsPaneBorderBrush", sectionBorder);
        SetBrush(resources, "GitExtensionsSectionBorderBrush", sectionBorder);
        SetBrush(resources, "GitExtensionsRefLabelBackgroundBrush", refLabelBackground);
        SetBrush(resources, "GitExtensionsBranchRefBrush", GetAppColor(settings, AppColor.Branch));
        SetBrush(resources, "GitExtensionsRemoteBranchRefBrush", GetAppColor(settings, AppColor.RemoteBranch));
        SetBrush(resources, "GitExtensionsTagRefBrush", GetAppColor(settings, AppColor.Tag));
        SetBrush(resources, "GitExtensionsOtherRefBrush", GetAppColor(settings, AppColor.OtherTag));
        SetBrush(resources, "GitExtensionsTreeConnectorBrush", treeConnector);
        SetBrush(resources, "GitExtensionsInactiveSelectionBackgroundBrush", GetAppColor(settings, AppColor.InactiveSelectionHighlight));

        SetBrush(resources, "GitExtensionsSelectionBackgroundBrush", selection);
        SetBrush(resources, "GitExtensionsSelectionPointerOverBackgroundBrush", Lerp(selection, windowText, 0.08f));
        SetBrush(resources, "GitExtensionsSelectionForegroundBrush", windowText);

        SetBrush(resources, "GitExtensionsValidFilterBackgroundBrush", isDark ? dimmedAddedBackground : addedBackground);
        SetBrush(resources, "GitExtensionsInvalidFilterBackgroundBrush", isDark ? dimmedRemovedBackground : removedBackground);
        SetBrush(resources, "GitExtensionsResetSoftBackgroundBrush", resetSoft);
        SetBrush(resources, "GitExtensionsResetMixedBackgroundBrush", resetMixed);
        SetBrush(resources, "GitExtensionsResetHardBackgroundBrush", resetHard);
        SetBrush(resources, "GitExtensionsResetSoftForegroundBrush", GetTextColor(resetSoft, windowText));
        SetBrush(resources, "GitExtensionsResetMixedForegroundBrush", GetTextColor(resetMixed, windowText));
        SetBrush(resources, "GitExtensionsResetHardForegroundBrush", GetTextColor(resetHard, windowText));
        SetBrush(resources, "GitExtensionsDiffEditorBackgroundBrush", editor);
        SetBrush(resources, "GitExtensionsDiffTextBrush", windowText);
        SetBrush(resources, "GitExtensionsDiffLineNumberBackgroundBrush", GetAppColor(settings, AppColor.LineNumberBackground));
        SetBrush(resources, "GitExtensionsDiffLineNumberBrush", grayText);
        SetBrush(resources, "GitExtensionsDiffLineNumberSelectedBrush", windowText);
        SetBrush(resources, "GitExtensionsDiffSectionBrush", GetAppColor(settings, AppColor.DiffSection));
        SetBrush(resources, "GitExtensionsDiffRemovedBrush", removedBackground);
        SetBrush(resources, "GitExtensionsDiffAddedBrush", addedBackground);
        SetBrush(resources, "GitExtensionsDiffRemovedDimBrush", dimmedRemovedBackground);
        SetBrush(resources, "GitExtensionsDiffAddedDimBrush", dimmedAddedBackground);
        SetBrush(resources, "GitExtensionsDiffMovedRemovedBrush", GetAppColor(settings, AppColor.AnsiTerminalMagentaBackNormal));
        SetBrush(resources, "GitExtensionsDiffMovedAddedBrush", GetAppColor(settings, AppColor.AnsiTerminalBlueBackNormal));
        SetBrush(resources, "GitExtensionsDiffRemovedForegroundBrush", removedForeground);
        SetBrush(resources, "GitExtensionsDiffAddedForegroundBrush", addedForeground);
        SetBrush(resources, "GitExtensionsDiffRemovedDimForegroundBrush", Dim(removedForeground, editor));
        SetBrush(resources, "GitExtensionsDiffAddedDimForegroundBrush", Dim(addedForeground, editor));
        SetBrush(resources, "GitExtensionsDiffMovedRemovedForegroundBrush", movedRemovedForeground);
        SetBrush(resources, "GitExtensionsDiffMovedAddedForegroundBrush", movedAddedForeground);

        DrawingColor blameHighlight = isDark
            ? Lerp(editor, DrawingColor.White, 0.08f)
            : GetKnownColor(settings, KnownColor.ControlLight, isDark: false);
        SetBrush(resources, "GitExtensionsBlameHighlightBrush", blameHighlight);
        SetBrush(resources, "GitExtensionsBlameAuthorBrush", grayText);
    }

    private static ResourceDictionary GetThemeResources(Application application, ThemeVariant variant)
    {
        if (application.Resources.ThemeDictionaries.TryGetValue(variant, out IThemeVariantProvider? existing)
            && existing is ResourceDictionary resources)
        {
            return resources;
        }

        ResourceDictionary created = new();
        application.Resources.ThemeDictionaries[variant] = created;
        return created;
    }

    private static DrawingColor GetAppColor(ThemeSettings settings, AppColor name)
    {
        DrawingColor color = settings.Theme.GetColor(name);
        return color.IsEmpty ? settings.InvariantTheme.GetColor(name) : color;
    }

    private static DrawingColor GetKnownColor(ThemeSettings settings, KnownColor name, bool isDark)
    {
        DrawingColor color = settings.Theme.GetColor(name);
        if (!color.IsEmpty)
        {
            return color;
        }

        if (isDark && TryGetDarkSystemColor(name, out color))
        {
            return color;
        }

        color = settings.InvariantTheme.GetColor(name);
        return color.IsEmpty ? DrawingColor.FromKnownColor(name) : color;
    }

    private static bool TryGetDarkSystemColor(KnownColor name, out DrawingColor color)
    {
        string? value = name switch
        {
            KnownColor.ActiveBorder => "#464646",
            KnownColor.ActiveCaption => "#3C5F78",
            KnownColor.ActiveCaptionText => "#FFFFFF",
            KnownColor.AppWorkspace => "#3C3C3C",
            KnownColor.ButtonFace => "#202020",
            KnownColor.ButtonHighlight => "#101010",
            KnownColor.ButtonShadow => "#464646",
            KnownColor.Control => "#202020",
            KnownColor.ControlDark => "#4A4A4A",
            KnownColor.ControlDarkDark => "#5A5A5A",
            KnownColor.ControlLight => "#2E2E2E",
            KnownColor.ControlLightLight => "#1F1F1F",
            KnownColor.ControlText => "#FFFFFF",
            KnownColor.Desktop => "#101010",
            KnownColor.GradientActiveCaption => "#416482",
            KnownColor.GradientInactiveCaption => "#557396",
            KnownColor.GrayText => "#969696",
            KnownColor.Highlight => "#2864B4",
            KnownColor.HighlightText => "#000000",
            KnownColor.HotTrack => "#2D5FAF",
            KnownColor.InactiveBorder => "#3C3F41",
            KnownColor.InactiveCaption => "#374B5A",
            KnownColor.InactiveCaptionText => "#BEBEBE",
            KnownColor.Info => "#50503C",
            KnownColor.InfoText => "#BEBEBE",
            KnownColor.Menu => "#373737",
            KnownColor.MenuBar => "#373737",
            KnownColor.MenuHighlight => "#2A80D2",
            KnownColor.MenuText => "#F0F0F0",
            KnownColor.ScrollBar => "#505050",
            KnownColor.Window => "#323232",
            KnownColor.WindowFrame => "#282828",
            KnownColor.WindowText => "#F0F0F0",
            _ => null,
        };

        color = value is null ? DrawingColor.Empty : System.Drawing.ColorTranslator.FromHtml(value);
        return !color.IsEmpty;
    }

    private static void PublishColor(ResourceDictionary resources, string key, DrawingColor color)
    {
        if (color.IsEmpty)
        {
            resources.Remove(key);
            resources.Remove(key + "Brush");
            return;
        }

        MediaColor mediaColor = ToMediaColor(color);
        resources[key] = mediaColor;
        resources[key + "Brush"] = new SolidColorBrush(mediaColor);
    }

    private static void SetBrush(ResourceDictionary resources, string key, DrawingColor color)
        => SetBrush(resources, key, ToMediaColor(color));

    private static void SetBrush(ResourceDictionary resources, string key, MediaColor color)
        => resources[key] = new SolidColorBrush(color);

    private static MediaColor ToMediaColor(DrawingColor color)
        => MediaColor.FromArgb(color.A, color.R, color.G, color.B);

    private static DrawingColor Lerp(DrawingColor from, DrawingColor to, float amount)
    {
        byte a = (byte)Math.Round(from.A + ((to.A - from.A) * amount));
        byte r = (byte)Math.Round(from.R + ((to.R - from.R) * amount));
        byte g = (byte)Math.Round(from.G + ((to.G - from.G) * amount));
        byte b = (byte)Math.Round(from.B + ((to.B - from.B) * amount));
        return DrawingColor.FromArgb(a, r, g, b);
    }

    private static DrawingColor GetTextColor(DrawingColor background, DrawingColor preferred)
    {
        const double minimumContrastRatio = 4.5;

        if (GetContrastRatio(background, preferred) >= minimumContrastRatio)
        {
            return preferred;
        }

        DrawingColor black = DrawingColor.Black;
        DrawingColor white = DrawingColor.White;
        return GetContrastRatio(background, black) >= GetContrastRatio(background, white)
            ? black
            : white;
    }

    private static double GetContrastRatio(DrawingColor first, DrawingColor second)
    {
        double firstLuminance = GetRelativeLuminance(first);
        double secondLuminance = GetRelativeLuminance(second);
        return (Math.Max(firstLuminance, secondLuminance) + 0.05)
            / (Math.Min(firstLuminance, secondLuminance) + 0.05);
    }

    private static double GetRelativeLuminance(DrawingColor color)
        => (0.2126 * SrgbLinearize(color.R))
            + (0.7152 * SrgbLinearize(color.G))
            + (0.0722 * SrgbLinearize(color.B));

    private static DrawingColor Dim(DrawingColor color, DrawingColor background)
    {
        byte r = SrgbDelinearize((SrgbLinearize(color.R) + SrgbLinearize(background.R)) * 0.5);
        byte g = SrgbDelinearize((SrgbLinearize(color.G) + SrgbLinearize(background.G)) * 0.5);
        byte b = SrgbDelinearize((SrgbLinearize(color.B) + SrgbLinearize(background.B)) * 0.5);
        return DrawingColor.FromArgb(color.A, r, g, b);
    }

    private static double SrgbLinearize(byte channel)
    {
        double normalized = channel / 255.0;
        return normalized <= 0.04045 ? normalized / 12.92 : Math.Pow((normalized + 0.055) / 1.055, 2.4);
    }

    private static byte SrgbDelinearize(double linear)
    {
        double normalized = linear <= 0.0031308 ? 12.92 * linear : (1.055 * Math.Pow(linear, 1.0 / 2.4)) - 0.055;
        return (byte)Math.Round(Math.Clamp(normalized * 255.0, 0.0, 255.0));
    }
}
