using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using Control = System.Windows.Forms.Control;

namespace GitUI.Theming
{
    public static class ThemeModule
    {
        public static ThemeSettings Settings { get; private set; } = ThemeSettings.Default;

        private static ThemeRepository Repository { get; } = new ThemeRepository(new ThemePersistence());

        public static void Load()
        {
            new ThemeMigration(Repository).Migrate();
            Settings = TryLoadTheme();
            ColorHelper.ThemeSettings = Settings;
            ThemeFix.ThemeSettings = Settings;
            Win32ThemeHooks.ThemeSettings = Settings;
        }

        private static bool TryInstallHooks(Theme theme)
        {
            Win32ThemeHooks.WindowCreated += Handle_WindowCreated;

            try
            {
                Win32ThemeHooks.InstallHooks(theme, new SystemDialogDetector());
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to install Win32 theming hooks: {ex}");
                Win32ThemeHooks.Uninstall();
                return false;
            }

            ResetGdiCaches();
            return true;
        }

        private static ThemeSettings TryLoadTheme()
        {
            var invariantTheme = Repository.GetInvariantTheme();
            if (invariantTheme == null)
            {
                // Not good, ColorHelper needs actual InvariantTheme to correctly transform colors.
                // Still not a mission-critical failure, do not raise.
                return ThemeSettings.Default;
            }

            ThemeId themeId = AppSettings.ThemeId;
            if (string.IsNullOrEmpty(themeId.Name))
            {
                return new ThemeSettings(Theme.Default, invariantTheme, AppSettings.UseSystemVisualStyle);
            }

            var theme = Repository.GetTheme(themeId);
            if (theme == null || !TryInstallHooks(theme))
            {
                return new ThemeSettings(Theme.Default, invariantTheme, AppSettings.UseSystemVisualStyle);
            }

            return new ThemeSettings(theme, invariantTheme, AppSettings.UseSystemVisualStyle);
        }

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

        private static void Handle_WindowCreated(IntPtr hwnd)
        {
            switch (Control.FromHandle(hwnd))
            {
                case Form form:
                    form.Load += (s, e) => ((Form)s).FixVisualStyle();
                    break;
            }
        }
    }
}
