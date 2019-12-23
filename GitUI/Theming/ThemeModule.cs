﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using Control = System.Windows.Forms.Control;

namespace GitUI.Theming
{
    public static class ThemeModule
    {
        private static readonly Lazy<ThemeDeployment> DeploymentLazy =
            new Lazy<ThemeDeployment>(() => new ThemeDeployment(FormEditorController));

        private static readonly Lazy<FormThemeEditorController> FormEditorControllerLazy =
            new Lazy<FormThemeEditorController>(CreateEditorController);

        private static readonly Lazy<FormThemeEditor> FormEditorLazy =
            new Lazy<FormThemeEditor>(() => new FormThemeEditor(FormEditorController));

        private static ThemePersistence Persistence { get; } = new ThemePersistence();

        private static readonly Lazy<Theme> DefaultThemeLazy =
            new Lazy<Theme>(() => new DefaultTheme());

        private static readonly Lazy<ThemeManager> ControllerLazy =
            new Lazy<ThemeManager>(() => new ThemeManager(DefaultTheme));

        private static readonly Lazy<SystemDialogDetector> SystemDialogDetectorLazy =
            new Lazy<SystemDialogDetector>(() => new SystemDialogDetector());

        private static ThemeDeployment Deployment =>
            DeploymentLazy.Value;

        public static FormThemeEditorController FormEditorController =>
            FormEditorControllerLazy.Value;

        private static FormThemeEditor FormEditor =>
            FormEditorLazy.Value;

        private static Theme DefaultTheme =>
            DefaultThemeLazy.Value;

        private static ThemeManager ThemeManager =>
            ControllerLazy.Value;

        private static SystemDialogDetector SystemDialogDetector =>
            SystemDialogDetectorLazy.Value;

        public static void Load()
        {
            if (!TryInitialize())
            {
                // Load default theme to prevent mixing dark AppColors with bright SystemColors
                // when AppSettings.UIThemeName points to dark theme
                FormEditorController.ApplySavedTheme("win10default", quiet: true);
            }
        }

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

            if (!FormEditorController.TryLoadInvariantTheme(out var invariantTheme))
            {
                return false;
            }

            if (!FormEditorController.ApplyCurrentTheme(AppSettings.UIThemeName))
            {
                return false;
            }

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

        public static void Unload()
        {
            Win32ThemeHooks.Uninstall();
            Win32ThemeHooks.WindowCreated -= Handle_WindowCreated;
        }

        public static void ShowEditor()
        {
            FormEditor.Show();
            FormEditor.BringToFront();
        }

        private static FormThemeEditorController CreateEditorController()
        {
            var editor = new FormThemeEditorController(ThemeManager, DefaultTheme, Persistence);
            AppSettings.Saved += editor.SaveCurrentTheme;
            return editor;
        }

        private static void Handle_WindowCreated(IntPtr hwnd)
        {
            switch (Control.FromHandle(hwnd))
            {
                case Form form:
                    form.Load += (s, e) =>
                    {
                        if (!AppSettings.UseSystemVisualStyle)
                        {
                            ((Form)s).FixVisualStyle();
                        }
                    };
                    break;
            }
        }
    }
}
