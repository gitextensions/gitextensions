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
    private const string WinFormsDefaultMonospaceFontName = "Consolas";

    /// <summary>Replaces the Windows design-time monospace defaults before any view is created.</summary>
    public static void InstallPlatformDefaults(Application application)
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        FontFamily fontFamily = new(GetPlatformMonospaceFontName());
        application.Resources["GitExtensionsFixedWidthFontFamily"] = fontFamily;
        application.Resources["GitExtensionsMonospaceFontFamily"] = fontFamily;
    }

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
        string fontFamilyName = !OperatingSystem.IsWindows()
            && IsMonospaceResource(resourcePrefix)
            && string.Equals(font.Name, WinFormsDefaultMonospaceFontName, StringComparison.OrdinalIgnoreCase)
                ? GetPlatformMonospaceFontName()
                : font.Name;

        application.Resources[$"{resourcePrefix}FontFamily"] = new FontFamily(fontFamilyName);
        application.Resources[$"{resourcePrefix}FontSize"] = font.Size * DeviceIndependentPixelsPerPoint;
        application.Resources[$"{resourcePrefix}FontStyle"] = font.Italic ? FontStyle.Italic : FontStyle.Normal;
        application.Resources[$"{resourcePrefix}FontWeight"] = font.Bold ? FontWeight.Bold : FontWeight.Normal;
    }

    private static bool IsMonospaceResource(string resourcePrefix)
        => resourcePrefix is "GitExtensionsFixedWidth" or "GitExtensionsMonospace";

    private static string GetPlatformMonospaceFontName()
        => OperatingSystem.IsMacOS() ? "Menlo" : "DejaVu Sans Mono";
}
