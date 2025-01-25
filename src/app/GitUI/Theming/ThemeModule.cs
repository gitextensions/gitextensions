using GitCommands;
using GitExtUtils.GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Theming
{
    public static class ThemeModule
    {
        public static ThemeSettings Settings { get; private set; } = ThemeSettings.Default;

        private static ThemeRepository Repository { get; } = new();
        public static bool IsDarkTheme { get; private set; }

        public static void Load()
        {
            new ThemeMigration(Repository).Migrate();
            Settings = LoadThemeSettings(Repository);
            IsDarkTheme = Settings.Theme.GetNonEmptyColor(KnownColor.Window).GetBrightness() < 0.5;
            UpdateEditorSettings();
            ColorHelper.ThemeSettings = Settings;
            ThemeFix.ThemeSettings = Settings;
        }

        private static void UpdateEditorSettings()
        {
            DefaultHighlightingStrategy strategy = HighlightingManager.Manager.DefaultHighlighting;
            strategy.SetColorFor("Default",
                new HighlightColor(SystemColors.WindowText, AppColor.EditorBackground.GetThemeColor(), false, false, adaptable: false));
            strategy.SetColorFor("LineNumbers",
                new HighlightColor(SystemColors.GrayText, AppColor.LineNumberBackground.GetThemeColor(), false, false, adaptable: false));
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
                MessageBoxes.ShowError(null, $"Failed to load invariant theme: {ex}");
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
                MessageBoxes.ShowError(null, $"Failed to load {(themeId.IsBuiltin ? "preinstalled" : "user-defined")} theme {themeId.Name}: {ex}");
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
}
