using System.Diagnostics;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Theming;

public static class ThemeModule
{
    public static ThemeSettings Settings { get; private set; } = ThemeSettings.Default;

    private static ThemeRepository Repository { get; } = new();

    public static void Load()
    {
        Settings = LoadThemeSettings(Repository);
        Application.SetColorMode(Settings.Theme.SystemColorMode);
        UpdateEditorSettings();
        ColorHelper.ThemeSettings = Settings;
        ThemeFix.ThemeSettings = Settings;
    }

    private static void UpdateEditorSettings()
    {
        DefaultHighlightingStrategy strategy = HighlightingManager.Manager.DefaultHighlighting;
        strategy.SetColorFor("Default",
            new HighlightColor(SystemColors.WindowText, AppColor.EditorBackground.GetThemeColor(), bold: false, italic: false, adaptable: false));
        strategy.SetColorFor("LineNumbers",
            new HighlightColor(SystemColors.GrayText, AppColor.LineNumberBackground.GetThemeColor(), bold: false, italic: false, adaptable: false));
        strategy.SetColorFor("LineNumberSelected",
            new HighlightColor(SystemColors.WindowText, AppColor.LineNumberBackground.GetThemeColor(), bold: true, italic: false, adaptable: false));
        if (Application.IsDarkModeEnabled)
        {
            strategy.SetColorFor("EOLMarkers",
                new HighlightColor(nameof(SystemColors.ControlDarkDark), bold: false, italic: false));
            strategy.SetColorFor("SpaceMarkers",
                new HighlightColor(nameof(SystemColors.ControlDarkDark), bold: false, italic: false));
            strategy.SetColorFor("TabMarkers",
                new HighlightColor(nameof(SystemColors.ControlDarkDark), bold: false, italic: false));
        }
    }

    private static ThemeSettings LoadThemeSettings(IThemeRepository repository)
    {
        Theme invariantTheme;
        try
        {
            invariantTheme = repository.GetInvariantTheme();
        }
        catch (ThemeException ex)
        {
            // Not good, ColorHelper needs actual InvariantTheme to correctly transform colors.
            MessageBoxes.ShowError(null, $"Failed to load invariant theme: {ex.Message}"
                    + $"{Environment.NewLine}{Environment.NewLine}See also https://github.com/gitextensions/gitextensions/wiki/Dark-Mode");
            return ThemeSettings.Default;
        }

        ThemeId themeId = AppSettings.ThemeId;
        if (string.IsNullOrEmpty(themeId.Name))
        {
            // Migrate from default invariant/light to Windows default color mode
            themeId = ThemeId.WindowsAppColorModeId;
            AppSettings.ThemeId = themeId;
        }

        bool systemVisualStyle = AppSettings.UseSystemVisualStyle;
        if (themeId == ThemeId.WindowsAppColorModeId)
        {
            // fix systemVisualStyle for WindowsAppColorModeId mode (always for DefaultLight)
            // This is also how it is presented in Settings
            themeId = ThemeId.ColorModeThemeId;
            systemVisualStyle = themeId == ThemeId.DefaultLight;
        }

        string[] variations = AppSettings.ThemeVariations;
        if (themeId == ThemeId.DefaultLight)
        {
            // default/invariant/light theme
            return CreateFallbackSettings(invariantTheme, variations);
        }

        Theme theme;
        try
        {
            theme = repository.GetTheme(themeId, variations);
        }
        catch (ThemeException ex)
        {
            Trace.WriteLine($"Failed to load {(themeId.IsBuiltin ? "preinstalled" : "user-defined")} theme {themeId.Name}: {ex}");
            MessageBoxes.ShowError(null, $"Failed to load {(themeId.IsBuiltin ? "preinstalled" : "user-defined")} theme {themeId.Name}: {ex.Message}"
                    + $"{Environment.NewLine}{Environment.NewLine}See also https://github.com/gitextensions/gitextensions/wiki/Dark-Mode");
            AppSettings.ThemeId = ThemeId.DefaultLight;
            return CreateFallbackSettings(invariantTheme, variations);
        }

        return new ThemeSettings(theme, invariantTheme, variations, systemVisualStyle);
    }

    private static ThemeSettings CreateFallbackSettings(Theme invariantTheme, string[] variations) =>
        new(Theme.CreateDefaultTheme(variations), invariantTheme, variations, useSystemVisualStyle: true);

    internal static class TestAccessor
    {
        public static void ReloadThemeSettings(IThemeRepository repository) =>
            Settings = LoadThemeSettings(repository);
    }
}
