using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using Control = System.Windows.Forms.Control;

namespace GitUI.Theming
{
    public static class ThemeModule
    {
        // TODO: cleanup
        //      there is no need for all lazy allocations since this class is ONLY used to hook and unhook global themeing.

        private static readonly Lazy<ThemeDeployment> DeploymentLazy =
            new Lazy<ThemeDeployment>(() => new ThemeDeployment(Controller));

        private static readonly Lazy<FormThemeEditorController> FormEditorControllerLazy =
            new Lazy<FormThemeEditorController>(CreateEditorController);

        private static ThemePersistence Persistence { get; } = new ThemePersistence();

        private static readonly Lazy<Theme> DefaultThemeLazy =
            new Lazy<Theme>(() => new DefaultTheme());

        private static readonly Lazy<ThemeManager> ControllerLazy =
            new Lazy<ThemeManager>(() => new ThemeManager(DefaultTheme));

        private static readonly Lazy<SystemDialogDetector> SystemDialogDetectorLazy =
            new Lazy<SystemDialogDetector>(() => new SystemDialogDetector());

        public static void Load()
        {
            if (TryInitialize())
            {
                return;
            }

            // Load default theme to prevent mixing dark AppColors with bright SystemColors
            // when AppSettings.UIThemeName points to dark theme
            Controller.SetInitialTheme(Controller.InvariantThemeName, useSystemVisualStyle: true);
        }

        public static void Unload()
        {
            Win32ThemeHooks.Uninstall();
            Win32ThemeHooks.WindowCreated -= Handle_WindowCreated;
        }

        private static ThemeDeployment Deployment =>
            DeploymentLazy.Value;

        private static FormThemeEditorController Controller =>
            FormEditorControllerLazy.Value;

        private static Theme DefaultTheme =>
            DefaultThemeLazy.Value;

        private static ThemeManager ThemeManager =>
            ControllerLazy.Value;

        private static SystemDialogDetector SystemDialogDetector =>
            SystemDialogDetectorLazy.Value;

        private static bool TryInitialize()
        {
            try
            {
                Deployment.DeployThemesToUserDirectory();
            }
            catch (Exception ex)
            {
                // non mission-critical, proceed
                Trace.WriteLine($"Failed to deploy schemes to user directory: {ex}");
            }

            var invariantTheme = Controller.LoadInvariantTheme();
            if (invariantTheme == null)
            {
                return false;
            }

            if (!Controller.SetInitialTheme(AppSettings.UIThemeName, AppSettings.UseSystemVisualStyle))
            {
                return false;
            }

            ThemeFix.UseSystemVisualStyle = Controller.UseSystemVisualStyleInitial;

            try
            {
                Win32ThemeHooks.InstallHooks(ThemeManager, SystemDialogDetector);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to install Win32 theming hooks: {ex}");
                Win32ThemeHooks.Uninstall();
                return false;
            }

            Win32ThemeHooks.WindowCreated += Handle_WindowCreated;
            ColorHelper.SetUITheme(ThemeManager, invariantTheme);
            return true;
        }

        private static FormThemeEditorController CreateEditorController()
        {
            var editor = new FormThemeEditorController(ThemeManager, Persistence);
            AppSettings.Saved += editor.SaveCurrentTheme;
            return editor;
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
