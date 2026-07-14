namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.CheckState</c>; values match WinForms.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtensions.Extensibility/Extensions/UIExtensions.cs</c>.
/// </remarks>
public enum CheckState
{
    Unchecked = 0,
    Checked = 1,
    Indeterminate = 2,
}
