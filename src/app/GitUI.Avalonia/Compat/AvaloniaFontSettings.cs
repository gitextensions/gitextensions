using Avalonia;
using Avalonia.Media;
using GitCommands;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

/// <summary>
///  Bridges the point-based Git Extensions font settings to Avalonia typography resources.
/// </summary>
public static class AvaloniaFontSettings
{
    private const double DeviceIndependentPixelsPerPoint = 96d / 72d;
    private const float DefaultUiFontSizeInPoints = 9F;

    /// <summary>
    ///  Makes the platform's Avalonia font family the non-Windows equivalent of
    ///  <c>SystemFonts.MessageBoxFont</c> before <see cref="AppSettings"/> reads its defaults.
    /// </summary>
    public static void InstallSystemDefaults()
    {
        WinFormsShims.Font defaultFont = new(
            FontManager.Current.DefaultFontFamily.Name,
            DefaultUiFontSizeInPoints);
        WinFormsShims.SystemFonts.DefaultFont = defaultFont;
        WinFormsShims.SystemFonts.MessageBoxFont = defaultFont;
    }

    /// <summary>Publishes the loaded application fonts to the shared Avalonia styles.</summary>
    public static void ApplyAppSettings()
    {
        Application application = Application.Current
            ?? throw new InvalidOperationException("The Avalonia application was not created.");

        ApplyFont(application, "GitExtensionsUi", AppSettings.Font);
        ApplyFont(application, "GitExtensionsCommit", AppSettings.CommitFont);
        ApplyFont(application, "GitExtensionsFixedWidth", AppSettings.FixedWidthFont);
        ApplyFont(application, "GitExtensionsMonospace", AppSettings.MonospaceFont);
    }

    private static void ApplyFont(Application application, string resourcePrefix, WinFormsShims.Font font)
    {
        application.Resources[$"{resourcePrefix}FontFamily"] = new FontFamily(font.Name);
        application.Resources[$"{resourcePrefix}FontSize"] = font.Size * DeviceIndependentPixelsPerPoint;
        application.Resources[$"{resourcePrefix}FontStyle"] = font.Italic ? FontStyle.Italic : FontStyle.Normal;
        application.Resources[$"{resourcePrefix}FontWeight"] = font.Bold ? FontWeight.Bold : FontWeight.Normal;
    }
}
