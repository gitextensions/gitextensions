namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.SystemColorMode</c>; values match WinForms.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtUtils/GitUI/Theming/Theme.cs</c>, <c>GitExtUtils/GitUI/Theming/ThemeId.cs</c>.
/// </remarks>
public enum SystemColorMode
{
    Classic = 0,
    System = 1,
    Dark = 2,
}
