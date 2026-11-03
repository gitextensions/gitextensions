using Avalonia.Controls.ApplicationLifetimes;
using Application = Avalonia.Application;

namespace GitExtUtils.GitUI;

/// <summary>
/// Utility class related to DPI settings, primarily used for device-independent dimensions on high-DPI displays.
/// </summary>
public static class DpiUtil
{
    public static int DpiX => (int)Math.Round(96 * ScaleX);

    public static int DpiY => (int)Math.Round(96 * ScaleY);

    public static float ScaleX => (float)GetDesktopScale();

    public static float ScaleY => (float)GetDesktopScale();

    /// <summary>
    /// Returns a copy of measurement <paramref name="value"/> which has
    /// equivalent length on screen at the current DPI as the original would
    /// at 96 DPI.
    /// </summary>
    /// <param name="value">The value to scale.</param>
    public static int Scale(int value) => value;

    /// <summary>
    /// Returns a copy of <paramref name="value"/> which takes equivalent
    /// screen space at the current DPI as the original would at 96 DPI.
    /// </summary>
    public static Size Scale(Size value) => value;

    /// <summary>
    /// Returns <paramref name="value"/> in device-independent units so that it has equivalent physical
    /// screen position at the current DPI as the original value would at 96 DPI.
    /// </summary>
    public static Point Scale(Point value) => value;

    private static double GetDesktopScale()
        => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?
            .MainWindow?.Screens.Primary?.Scaling ?? 1;
}
