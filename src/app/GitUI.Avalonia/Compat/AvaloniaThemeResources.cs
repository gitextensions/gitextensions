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

    // Keep this list explicit: adding an AppColor must fail the theming-capability tests until
    // the Avalonia resource boundary deliberately accepts it.
    internal static IReadOnlyList<AppColor> MappedAppColors { get; } =
    [
        AppColor.PanelBackground,
        AppColor.EditorBackground,
        AppColor.LineNumberBackground,
        AppColor.AuthoredHighlight,
        AppColor.Selection,
        AppColor.HighlightAllOccurences,
        AppColor.InactiveSelectionHighlight,
        AppColor.GraphBranch1,
        AppColor.GraphBranch2,
        AppColor.GraphBranch3,
        AppColor.GraphBranch4,
        AppColor.GraphBranch5,
        AppColor.GraphBranch6,
        AppColor.GraphBranch7,
        AppColor.GraphBranch8,
        AppColor.GraphNonRelativeBranch,
        AppColor.Branch,
        AppColor.RemoteBranch,
        AppColor.Tag,
        AppColor.OtherTag,
        AppColor.DiffSection,
        AppColor.AnsiTerminalBlackForeNormal,
        AppColor.AnsiTerminalBlackBackNormal,
        AppColor.AnsiTerminalBlackForeBold,
        AppColor.AnsiTerminalBlackBackBold,
        AppColor.AnsiTerminalRedForeNormal,
        AppColor.AnsiTerminalRedBackNormal,
        AppColor.AnsiTerminalRedForeBold,
        AppColor.AnsiTerminalRedBackBold,
        AppColor.AnsiTerminalGreenForeNormal,
        AppColor.AnsiTerminalGreenBackNormal,
        AppColor.AnsiTerminalGreenForeBold,
        AppColor.AnsiTerminalGreenBackBold,
        AppColor.AnsiTerminalYellowForeNormal,
        AppColor.AnsiTerminalYellowBackNormal,
        AppColor.AnsiTerminalYellowForeBold,
        AppColor.AnsiTerminalYellowBackBold,
        AppColor.AnsiTerminalBlueForeNormal,
        AppColor.AnsiTerminalBlueBackNormal,
        AppColor.AnsiTerminalBlueForeBold,
        AppColor.AnsiTerminalBlueBackBold,
        AppColor.AnsiTerminalMagentaForeNormal,
        AppColor.AnsiTerminalMagentaBackNormal,
        AppColor.AnsiTerminalMagentaForeBold,
        AppColor.AnsiTerminalMagentaBackBold,
        AppColor.AnsiTerminalCyanForeNormal,
        AppColor.AnsiTerminalCyanBackNormal,
        AppColor.AnsiTerminalCyanForeBold,
        AppColor.AnsiTerminalCyanBackBold,
        AppColor.AnsiTerminalWhiteForeNormal,
        AppColor.AnsiTerminalWhiteBackNormal,
        AppColor.AnsiTerminalWhiteForeBold,
        AppColor.AnsiTerminalWhiteBackBold,
    ];

    // This is the complete SystemColors inventory consumed by the WinForms GitUI source.
    // The source audit fails when that inventory changes without a matching semantic resource.
    internal static IReadOnlyList<KnownColor> MappedSystemColors { get; } =
    [
        KnownColor.ActiveCaption,
        KnownColor.AppWorkspace,
        KnownColor.ButtonFace,
        KnownColor.Control,
        KnownColor.ControlDark,
        KnownColor.ControlDarkDark,
        KnownColor.ControlLight,
        KnownColor.ControlLightLight,
        KnownColor.ControlText,
        KnownColor.GradientActiveCaption,
        KnownColor.GrayText,
        KnownColor.Highlight,
        KnownColor.HighlightText,
        KnownColor.HotTrack,
        KnownColor.InactiveCaption,
        KnownColor.Info,
        KnownColor.InfoText,
        KnownColor.Window,
        KnownColor.WindowFrame,
        KnownColor.WindowText,
    ];

    public static void Apply(Application application, ThemeSettings settings)
    {
        ColorHelper.ThemeSettings = settings;
        bool isDark = settings.Theme.SystemColorMode == GitExtensions.Shims.WinForms.SystemColorMode.Dark;
        ResourceDictionary resources = GetThemeResources(application, isDark ? ThemeVariant.Dark : ThemeVariant.Light);

        foreach (AppColor name in MappedAppColors)
        {
            DrawingColor color = ResolveAppColor(settings, name);
            PublishColor(resources, AppColorPrefix + name, color);
        }

        foreach (KnownColor name in MappedSystemColors)
        {
            PublishColor(resources, KnownColorPrefix + name, ResolveSystemColor(settings, name));
        }

        DrawingColor panel = ResolveAppColor(settings, AppColor.PanelBackground);
        DrawingColor editor = ResolveAppColor(settings, AppColor.EditorBackground);
        DrawingColor window = ResolveSystemColor(settings, KnownColor.Window);
        DrawingColor commitMessageBackground = window.MakeDarkerBy(0.04);
        DrawingColor selection = ResolveAppColor(settings, AppColor.Selection);
        DrawingColor windowText = ResolveSystemColor(settings, KnownColor.WindowText);
        DrawingColor grayText = ResolveSystemColor(settings, KnownColor.GrayText);
        DrawingColor control = ResolveSystemColor(settings, KnownColor.Control);
        DrawingColor controlText = ResolveSystemColor(settings, KnownColor.ControlText);
        DrawingColor controlDark = ResolveSystemColor(settings, KnownColor.ControlDark);
        DrawingColor controlLight = ResolveSystemColor(settings, KnownColor.ControlLight);
        DrawingColor commitEditorBackground = isDark ? controlLight : window;
        DrawingColor highlight = ResolveSystemColor(settings, KnownColor.Highlight);
        DrawingColor highlightText = ResolveSystemColor(settings, KnownColor.HighlightText);
        DrawingColor inactiveSelection = ResolveSystemColor(settings, KnownColor.InactiveCaption);
        DrawingColor inactiveSelectionText = ResolveSystemColor(settings, KnownColor.InactiveCaptionText);
        DrawingColor info = ResolveSystemColor(settings, KnownColor.Info);
        DrawingColor infoText = ResolveSystemColor(settings, KnownColor.InfoText);
        DrawingColor sectionBorder = isDark
            ? DrawingColor.FromArgb(47, 47, 47)
            : DrawingColor.FromArgb(224, 224, 224);
        DrawingColor treeConnector = ColorHelper.Lerp(panel, windowText, isDark ? 0.38f : 0.46f);
        DrawingColor refLabelBackground = isDark ? ColorHelper.Lerp(panel, DrawingColor.Black, 0.36f) : panel;
        DrawingColor removedBackground = ResolveAppColor(settings, AppColor.AnsiTerminalRedBackNormal);
        DrawingColor addedBackground = ResolveAppColor(settings, AppColor.AnsiTerminalGreenBackNormal);
        DrawingColor removedForeground = ResolveAppColor(settings, AppColor.AnsiTerminalRedForeNormal);
        DrawingColor addedForeground = ResolveAppColor(settings, AppColor.AnsiTerminalGreenForeNormal);
        DrawingColor movedRemovedForeground = ResolveAppColor(settings, AppColor.AnsiTerminalMagentaForeNormal);
        DrawingColor movedAddedForeground = ResolveAppColor(settings, AppColor.AnsiTerminalBlueForeNormal);
        DrawingColor dimmedRemovedBackground = removedBackground.DimColor().DimColor();
        DrawingColor dimmedAddedBackground = addedBackground.DimColor().DimColor();
        DrawingColor resetSoft = DrawingColor.FromArgb(128, 255, 128);
        DrawingColor resetMixed = DrawingColor.FromArgb(255, 255, 128);
        DrawingColor resetHard = DrawingColor.FromArgb(255, 128, 128);
        DrawingColor warningPanel = DrawingColor.FromArgb(230, 99, 99);
        DrawingColor interactiveAction = DrawingColor.LightSkyBlue.AdaptBackColor();
        DrawingColor interactiveConflict = DrawingColor.Orange.AdaptBackColor();
        if (isDark)
        {
            resetSoft = resetSoft.DimColor();
            resetMixed = resetMixed.DimColor();
            resetHard = resetHard.DimColor();
            warningPanel = warningPanel.DimColor();
        }

        DrawingColor alternatingRow = panel.MakeDarkerBy(isDark ? -0.018 : 0.025);

        // subject/body colors are hardcoded to be correct relative each other,
        // subject color should be emphasied compared to body, selected vs unselected etc.

        // relativeNonSelectedSubject: SystemColors.ControlText
        DrawingColor revisionNonRelativeSubject = isDark
            ? DrawingColor.FromArgb(192, 192, 192) // de-emphasised light grey on dark background
            : grayText;
        DrawingColor revisionNonRelativeSelectedSubject = isDark
            ? DrawingColor.FromArgb(235, 235, 215) // near-white with warm tint on blue selection
            : DrawingColor.FromArgb(188, 188, 188);

        // relativeNonSelectedBody: SystemColors.GrayText
        DrawingColor revisionSelectedBody = isDark
            ? DrawingColor.FromArgb(170, 170, 150) // warm mid-grey on blue selection
            : DrawingColor.FromArgb(188, 188, 188); // same as _nonRelativeSelectedSubjectColor
        DrawingColor revisionNonRelativeBody = isDark
            ? DrawingColor.FromArgb(130, 130, 130) // darker grey than subject, further de-emphasised
            : DrawingColor.FromArgb(152, 152, 152);
        DrawingColor revisionNonRelativeSelectedBody = isDark
            ? DrawingColor.FromArgb(170, 170, 150) // same as relativeSelectedBody — consistent on selection
            : DrawingColor.FromArgb(161, 161, 161);
        DrawingColor blameHighlight = isDark
            ? editor.MakeDarkerBy(-0.06)
            : ResolveSystemColor(settings, KnownColor.ControlLight);
        DrawingColor[] blameAges =
        [
            DrawingColor.FromArgb(247, 252, 245).AdaptBackColor(),
            DrawingColor.FromArgb(199, 233, 192).AdaptBackColor(),
            DrawingColor.FromArgb(161, 217, 155).AdaptBackColor(),
            DrawingColor.FromArgb(116, 196, 118).AdaptBackColor(),
            DrawingColor.FromArgb(65, 171, 93).AdaptBackColor(),
            DrawingColor.FromArgb(35, 139, 69).AdaptBackColor(),
            DrawingColor.FromArgb(0, 68, 27).AdaptBackColor(),
        ];

        SetBrush(resources, "ThemeBackgroundBrush", panel);
        SetBrush(resources, "ThemeForegroundBrush", windowText);
        SetBrush(resources, "ThemeBorderLowBrush", sectionBorder);
        SetBrush(resources, "GitExtensionsPanelBackgroundBrush", panel);
        SetBrush(resources, "GitExtensionsWindowTextBrush", windowText);
        SetBrush(resources, "GitExtensionsWindowBackgroundBrush", window);
        SetBrush(resources, "GitExtensionsCommitEditorBackgroundBrush", commitEditorBackground);
        SetBrush(resources, "GitExtensionsCommitMessageBackgroundBrush", commitMessageBackground);
        SetBrush(resources, "GitExtensionsControlBackgroundBrush", control);
        SetBrush(resources, "GitExtensionsControlForegroundBrush", controlText);
        SetBrush(resources, "GitExtensionsControlBorderBrush", controlDark);
        SetBrush(resources, "GitExtensionsControlPointerOverBackgroundBrush", controlLight);
        SetBrush(resources, "GitExtensionsControlPressedBackgroundBrush", controlDark);
        SetBrush(resources, "GitExtensionsDisabledForegroundBrush", grayText);
        SetBrush(resources, "GitExtensionsHighlightBackgroundBrush", highlight);
        SetBrush(resources, "GitExtensionsHighlightForegroundBrush", highlightText);
        SetBrush(resources, "GitExtensionsFileStatusSelectionForegroundBrush", isDark ? controlText : highlightText);
        SetBrush(resources, "GitExtensionsInactiveSelectionForegroundBrush", inactiveSelectionText);
        SetBrush(resources, "GitExtensionsSystemInactiveSelectionBackgroundBrush", inactiveSelection);
        SetBrush(resources, "GitExtensionsToolTipBackgroundBrush", info);
        SetBrush(resources, "GitExtensionsToolTipForegroundBrush", infoText);
        SetBrush(resources, "GitExtensionsPaneBorderBrush", sectionBorder);
        SetBrush(resources, "GitExtensionsSectionBorderBrush", sectionBorder);
        SetBrush(resources, "GitExtensionsRefLabelBackgroundBrush", refLabelBackground);
        SetBrush(resources, "GitExtensionsBranchRefBrush", ResolveAppColor(settings, AppColor.Branch));
        SetBrush(resources, "GitExtensionsRemoteBranchRefBrush", ResolveAppColor(settings, AppColor.RemoteBranch));
        SetBrush(resources, "GitExtensionsTagRefBrush", ResolveAppColor(settings, AppColor.Tag));
        SetBrush(resources, "GitExtensionsOtherRefBrush", ResolveAppColor(settings, AppColor.OtherTag));
        SetBrush(resources, "GitExtensionsTreeConnectorBrush", treeConnector);
        SetBrush(resources, "GitExtensionsInactiveSelectionBackgroundBrush", ResolveAppColor(settings, AppColor.InactiveSelectionHighlight));

        SetBrush(resources, "GitExtensionsSelectionBackgroundBrush", selection);
        SetBrush(resources, "GitExtensionsSelectionPointerOverBackgroundBrush", ColorHelper.Lerp(selection, windowText, 0.08f));
        SetBrush(resources, "GitExtensionsSelectionForegroundBrush", windowText);
        SetBrush(resources, "GitExtensionsRevisionAlternatingRowBrush", alternatingRow);
        SetBrush(resources, "GitExtensionsRevisionAuthoredBrush", ResolveAppColor(settings, AppColor.AuthoredHighlight));
        SetBrush(resources, "GitExtensionsRevisionSelectedSubjectBrush", isDark ? controlText : highlightText);
        SetBrush(resources, "GitExtensionsRevisionNonRelativeSubjectBrush", revisionNonRelativeSubject);
        SetBrush(resources, "GitExtensionsRevisionNonRelativeSelectedSubjectBrush", revisionNonRelativeSelectedSubject);
        SetBrush(resources, "GitExtensionsRevisionSelectedBodyBrush", revisionSelectedBody);
        SetBrush(resources, "GitExtensionsRevisionNonRelativeBodyBrush", revisionNonRelativeBody);
        SetBrush(resources, "GitExtensionsRevisionNonRelativeSelectedBodyBrush", revisionNonRelativeSelectedBody);

        SetBrush(resources, "GitExtensionsValidFilterBackgroundBrush", ResolveFilterBackground(isDark, isValid: true));
        SetBrush(resources, "GitExtensionsInvalidFilterBackgroundBrush", ResolveFilterBackground(isDark, isValid: false));
        SetBrush(resources, "GitExtensionsResetSoftBackgroundBrush", resetSoft);
        SetBrush(resources, "GitExtensionsResetMixedBackgroundBrush", resetMixed);
        SetBrush(resources, "GitExtensionsResetHardBackgroundBrush", resetHard);
        SetBrush(resources, "GitExtensionsResetSoftForegroundBrush", resetSoft.GetTextColor());
        SetBrush(resources, "GitExtensionsResetMixedForegroundBrush", resetMixed.GetTextColor());
        SetBrush(resources, "GitExtensionsResetHardForegroundBrush", resetHard.GetTextColor());
        SetBrush(resources, "GitExtensionsWarningPanelBackgroundBrush", warningPanel);
        SetBrush(resources, "GitExtensionsWarningPanelForegroundBrush", warningPanel.GetTextColor());
        SetBrush(resources, "GitExtensionsInteractiveActionBackgroundBrush", interactiveAction);
        SetBrush(resources, "GitExtensionsInteractiveActionForegroundBrush", interactiveAction.GetTextColor());
        SetBrush(resources, "GitExtensionsInteractiveConflictBackgroundBrush", interactiveConflict);
        SetBrush(resources, "GitExtensionsInteractiveConflictForegroundBrush", interactiveConflict.GetTextColor());
        SetBrush(resources, "GitExtensionsDiffEditorBackgroundBrush", editor);
        SetBrush(resources, "GitExtensionsDiffTextBrush", windowText);
        SetBrush(resources, "GitExtensionsDiffLineNumberBackgroundBrush", ResolveAppColor(settings, AppColor.LineNumberBackground));
        SetBrush(resources, "GitExtensionsDiffLineNumberBrush", grayText);
        SetBrush(resources, "GitExtensionsDiffLineNumberSelectedBrush", windowText);
        SetBrush(resources, "GitExtensionsDiffSectionBrush", ResolveAppColor(settings, AppColor.DiffSection));
        SetBrush(resources, "GitExtensionsDiffRemovedBrush", removedBackground);
        SetBrush(resources, "GitExtensionsDiffAddedBrush", addedBackground);
        SetBrush(resources, "GitExtensionsDiffRemovedDimBrush", dimmedRemovedBackground);
        SetBrush(resources, "GitExtensionsDiffAddedDimBrush", dimmedAddedBackground);
        SetBrush(resources, "GitExtensionsDiffMovedRemovedBrush", ResolveAppColor(settings, AppColor.AnsiTerminalMagentaBackNormal));
        SetBrush(resources, "GitExtensionsDiffMovedAddedBrush", ResolveAppColor(settings, AppColor.AnsiTerminalBlueBackNormal));
        SetBrush(resources, "GitExtensionsDiffRemovedForegroundBrush", removedForeground);
        SetBrush(resources, "GitExtensionsDiffAddedForegroundBrush", addedForeground);
        SetBrush(resources, "GitExtensionsDiffRemovedDimForegroundBrush", removedForeground.DimColor());
        SetBrush(resources, "GitExtensionsDiffAddedDimForegroundBrush", addedForeground.DimColor());
        SetBrush(resources, "GitExtensionsDiffMovedRemovedForegroundBrush", movedRemovedForeground);
        SetBrush(resources, "GitExtensionsDiffMovedAddedForegroundBrush", movedAddedForeground);

        SetBrush(resources, "GitExtensionsBlameHighlightBrush", blameHighlight);
        SetBrush(resources, "GitExtensionsBlameAuthorBrush", grayText);
        for (int index = 0; index < blameAges.Length; index++)
        {
            SetBrush(resources, $"GitExtensionsBlameAge{index}Brush", blameAges[index]);
        }
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

    internal static DrawingColor ResolveAppColor(ThemeSettings settings, AppColor name)
    {
        DrawingColor color = settings.Theme.GetColor(name);
        return color.IsEmpty ? settings.InvariantTheme.GetColor(name) : color;
    }

    internal static DrawingColor ResolveSystemColor(ThemeSettings settings, KnownColor name)
    {
        bool isDark = settings.Theme.SystemColorMode == GitExtensions.Shims.WinForms.SystemColorMode.Dark;
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

    internal static DrawingColor ResolveFilterBackground(bool isDark, bool isValid)
        => (isDark, isValid) switch
        {
            (true, true) => DrawingColor.FromArgb(0x00, 0x95, 0x00),
            (true, false) => DrawingColor.FromArgb(0x95, 0x00, 0x00),
            (false, true) => DrawingColor.FromArgb(0xC8, 0xFF, 0xC8),
            _ => DrawingColor.FromArgb(0xFF, 0xC8, 0xC8),
        };

    private static void SetBrush(ResourceDictionary resources, string key, DrawingColor color)
        => SetBrush(resources, key, ToMediaColor(color));

    private static void SetBrush(ResourceDictionary resources, string key, MediaColor color)
        => resources[key] = new SolidColorBrush(color);

    internal static MediaColor ToMediaColor(DrawingColor color)
        => MediaColor.FromArgb(color.A, color.R, color.G, color.B);
}
