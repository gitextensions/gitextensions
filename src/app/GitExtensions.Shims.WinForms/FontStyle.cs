namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Drawing.FontStyle</c>; values match GDI+.
/// </summary>
[Flags]
public enum FontStyle
{
    Regular = 0,
    Bold = 1,
    Italic = 2,
    Underline = 4,
    Strikeout = 8,
}
