using Avalonia;
using Avalonia.Styling;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using WinFormsApplication = GitExtensions.Shims.WinForms.Application;
using WinFormsSystemColorMode = GitExtensions.Shims.WinForms.SystemColorMode;

namespace GitUI.Compat;

/// <summary>
///  Maps the built-in Git Extensions appearance setting to Avalonia's theme variants.
/// </summary>
public static class AvaloniaThemeSettings
{
    private static Application? _subscribedApplication;

    /// <summary>Loads and applies the configured Git Extensions theme.</summary>
    public static void ApplyAppSettings()
    {
        Application application = Application.Current
            ?? throw new InvalidOperationException("The Avalonia application was not created.");

        ThemeId configuredThemeId = AppSettings.ThemeId;
        application.RequestedThemeVariant = configuredThemeId == ThemeId.DefaultDark
            ? ThemeVariant.Dark
            : configuredThemeId == ThemeId.DefaultLight
                ? ThemeVariant.Light
                : ThemeVariant.Default;

        UpdateSystemColorMode(application);
        ThemeModule.Load();

        application.RequestedThemeVariant = AppSettings.ThemeId == ThemeId.WindowsAppColorModeId
            ? ThemeVariant.Default
            : ThemeModule.Settings.Theme.SystemColorMode == WinFormsSystemColorMode.Dark
                ? ThemeVariant.Dark
                : ThemeVariant.Light;

        UpdateSystemColorMode(application);
        SubscribeToThemeChanges(application);
    }

    private static void SubscribeToThemeChanges(Application application)
    {
        if (ReferenceEquals(_subscribedApplication, application))
        {
            return;
        }

        if (_subscribedApplication is not null)
        {
            _subscribedApplication.ActualThemeVariantChanged -= Application_ActualThemeVariantChanged;
        }

        _subscribedApplication = application;
        _subscribedApplication.ActualThemeVariantChanged += Application_ActualThemeVariantChanged;
    }

    private static void Application_ActualThemeVariantChanged(object? sender, EventArgs e)
    {
        if (sender is Application application)
        {
            UpdateSystemColorMode(application);
        }
    }

    private static void UpdateSystemColorMode(Application application)
    {
        WinFormsApplication.SystemColorMode = application.ActualThemeVariant == ThemeVariant.Dark
            ? WinFormsSystemColorMode.Dark
            : WinFormsSystemColorMode.Classic;
    }
}
