using Avalonia;
using Avalonia.Styling;
using GitCommands;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Compat;

/// <summary>
///  Maps the built-in Git Extensions appearance setting to Avalonia's theme variants.
/// </summary>
public static class AvaloniaThemeSettings
{
    /// <summary>Applies the loaded built-in theme while preserving platform mode for other themes.</summary>
    public static void ApplyAppSettings()
    {
        Application application = Application.Current
            ?? throw new InvalidOperationException("The Avalonia application was not created.");

        ThemeId themeId = AppSettings.ThemeId;
        application.RequestedThemeVariant = themeId == ThemeId.DefaultDark
            ? ThemeVariant.Dark
            : themeId == ThemeId.DefaultLight
                ? ThemeVariant.Light
                : ThemeVariant.Default;
    }
}
