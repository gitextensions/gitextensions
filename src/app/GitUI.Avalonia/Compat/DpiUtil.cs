using Avalonia.Controls.ApplicationLifetimes;
using Application = Avalonia.Application;

namespace GitExtUtils.GitUI;

/// <summary>
///  Twin of the WinForms <c>DpiUtil</c> for code linked/ported into GitUI.Avalonia.
///  Avalonia layouts and renders in device-independent pixels and applies the display scale
///  itself, so scaling here is the identity function.
/// </summary>
public static class DpiUtil
{
    public static int DpiX => (int)Math.Round(96 * ScaleX);

    public static int DpiY => (int)Math.Round(96 * ScaleY);

    public static float ScaleX => (float)GetDesktopScale();

    public static float ScaleY => (float)GetDesktopScale();

    public static int Scale(int value) => value;

    public static Size Scale(Size value) => value;

    public static Point Scale(Point value) => value;

    private static double GetDesktopScale()
        => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?
            .MainWindow?.Screens.Primary?.Scaling ?? 1;
}
