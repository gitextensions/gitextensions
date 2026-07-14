namespace GitExtUtils.GitUI;

/// <summary>
///  Twin of the WinForms <c>DpiUtil</c> for code linked/ported into GitUI.Avalonia.
///  Avalonia layouts and renders in device-independent pixels and applies the display scale
///  itself, so scaling here is the identity function.
/// </summary>
public static class DpiUtil
{
    public static int Scale(int value) => value;

    public static Size Scale(Size value) => value;

    public static Point Scale(Point value) => value;
}
