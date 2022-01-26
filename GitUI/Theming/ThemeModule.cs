using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using Control = System.Windows.Forms.Control;

namespace GitUI.Theming
{
    public static class ThemeModule
    {
        private static bool _suppressWin32HooksForTests;

        public static ThemeSettings Settings { get; private set; } = ThemeSettings.Default;

        private static ThemeRepository Repository { get; } = new();
        public static bool IsDarkTheme { get; private set; }

        public static void Load()
        {
            new ThemeMigration(Repository).Migrate();
            Settings = LoadThemeSettings(Repository);
            IsDarkTheme = Settings.Theme.GetNonEmptyColor(KnownColor.Window).GetBrightness() < 0.5;
            ColorHelper.ThemeSettings = Settings;
            ThemeFix.ThemeSettings = Settings;
            Win32ThemeHooks.ThemeSettings = Settings;
        }

#if SUPPORT_THEME_HOOKS
        private static void InstallHooks(Theme theme)
        {
            Win32ThemeHooks.WindowCreated += Handle_WindowCreated;

            try
            {
                Win32ThemeHooks.InstallHooks(theme, new SystemDialogDetector());
            }
            catch (Exception)
            {
                Win32ThemeHooks.Uninstall();
                throw;
            }

            ResetGdiCaches();
        }
#endif

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
                // Migrate to default theme
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

            if (!AppSettings.UseSystemVisualStyle && !_suppressWin32HooksForTests)
            {
                try
                {
#if SUPPORT_THEME_HOOKS
                    InstallHooks(theme);
#endif
                }
                catch (Exception ex)
                {
                    MessageBoxes.ShowError(null, $"Failed to install Win32 theming hooks: {ex}");
                    return CreateFallbackSettings(invariantTheme, variations);
                }
            }

            return new ThemeSettings(theme, invariantTheme, AppSettings.ThemeVariations, AppSettings.UseSystemVisualStyle);
        }

#if SUPPORT_THEME_HOOKS
        private static void ResetGdiCaches()
        {
            var systemDrawingAssembly = typeof(Color).Assembly;

            var colorTableField =
                systemDrawingAssembly.GetType("System.Drawing.KnownColorTable")
                    .GetField("colorTable", BindingFlags.Static | BindingFlags.NonPublic) ??
                throw new NotSupportedException();

            var threadDataProperty =
                systemDrawingAssembly.GetType("System.Drawing.SafeNativeMethods")
                    .GetNestedType("Gdip", BindingFlags.NonPublic)
                    .GetProperty("ThreadData", BindingFlags.Static | BindingFlags.NonPublic) ??
                throw new NotSupportedException();

            var systemBrushesKeyField =
                    typeof(SystemBrushes).GetField("SystemBrushesKey", BindingFlags.Static | BindingFlags.NonPublic) ??
                    throw new NotSupportedException();

            var systemBrushesKey = systemBrushesKeyField.GetValue(null);

            FieldInfo systemPensKeyField = typeof(SystemPens)
                .GetField("SystemPensKey", BindingFlags.Static | BindingFlags.NonPublic) ??
                throw new NotSupportedException();

            var systemPensKey = systemPensKeyField
                .GetValue(null);

            var threadData = (IDictionary)threadDataProperty.GetValue(null, null);
            colorTableField.SetValue(null, null);

            threadData[systemBrushesKey] = null;
            threadData[systemPensKey] = null;
        }

        public static void Unload()
        {
            Win32ThemeHooks.Uninstall();
            Win32ThemeHooks.WindowCreated -= Handle_WindowCreated;
        }

        public static void ReloadWin32ThemeData()
        {
            Win32ThemeHooks.LoadThemeData();
        }

        private static void Handle_WindowCreated(IntPtr hwnd)
        {
            switch (Control.FromHandle(hwnd))
            {
                case Form form:
                    form.Load += (s, e) => ((Form)s!).FixVisualStyle();
                    break;
            }
        }
#endif

        private static ThemeSettings CreateFallbackSettings(Theme invariantTheme, string[] variations) =>
            new(Theme.CreateDefaultTheme(variations), invariantTheme, variations, useSystemVisualStyle: true);

        internal static class TestAccessor
        {
            public static void ReloadThemeSettings(IThemeRepository repository) =>
                Settings = LoadThemeSettings(repository);

            public static bool SuppressWin32Hooks
            {
                get => _suppressWin32HooksForTests;
                set => _suppressWin32HooksForTests = value;
            }
        }
    }
}
