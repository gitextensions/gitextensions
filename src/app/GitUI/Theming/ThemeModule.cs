using System.Diagnostics;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtUtils.GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Theming;

public static class ThemeModule
{
    public static ThemeSettings Settings { get; private set; } = ThemeSettings.Default;

    private static ThemeRepository Repository { get; } = new();

    public static void Load()
    {
        new ThemeMigration(Repository).Migrate();
        Settings = LoadThemeSettings(Repository);
        bool isDarkMode = IsDarkColor(Settings.Theme.GetColor(AppColor.PanelBackground));
        SystemColorMode mode = isDarkMode ? SystemColorMode.Dark : SystemColorMode.Classic;
        Application.SetColorMode(mode);
        UpdateEditorSettings();
        ColorHelper.ThemeSettings = Settings;
        ThemeFix.ThemeSettings = Settings;

        static bool IsDarkColor(Color color) => new HslColor(color).L < 0.5;
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

        string oldThemeName = AppSettings.ThemeIdName_v1;
        if (oldThemeName is not null)
        {
            // Migrate to default v4 theme for v3
            AppSettings.ThemeIdName_v1 = null;
            AppSettings.ThemeId = ThemeId.Default;
            if (oldThemeName != ThemeId.Default.Name)
            {
                MessageBox.Show($"The theme ({oldThemeName}) is reset to default as the theme support is limited in this Git Extensions version.",
                    TranslatedStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        ThemeId themeId = AppSettings.ThemeId;
        string[] variations = AppSettings.ThemeVariations;
        if (string.IsNullOrEmpty(themeId.Name))
        {
            return CreateFallbackSettings(invariantTheme, variations);
        }

        Theme theme;
        try
        {
            theme = repository.GetTheme(themeId, AppSettings.ThemeVariations);
        }
        catch (ThemeException ex)
        {
            Trace.WriteLine($"Failed to load {(themeId.IsBuiltin ? "preinstalled" : "user-defined")} theme {themeId.Name}: {ex}");
            MessageBoxes.ShowError(null, $"Failed to load {(themeId.IsBuiltin ? "preinstalled" : "user-defined")} theme {themeId.Name}: {ex.Message}"
                    + $"{Environment.NewLine}{Environment.NewLine}See also https://github.com/gitextensions/gitextensions/wiki/Dark-Mode");
            AppSettings.ThemeId = ThemeId.Default;
            return CreateFallbackSettings(invariantTheme, variations);
        }

        return new ThemeSettings(theme, invariantTheme, AppSettings.ThemeVariations, AppSettings.UseSystemVisualStyle);
    }

    private static ThemeSettings CreateFallbackSettings(Theme invariantTheme, string[] variations) =>
        new(Theme.CreateDefaultTheme(variations), invariantTheme, variations, useSystemVisualStyle: true);

    internal static class TestAccessor
    {
        public static void ReloadThemeSettings(IThemeRepository repository) =>
            Settings = LoadThemeSettings(repository);
    }
}
