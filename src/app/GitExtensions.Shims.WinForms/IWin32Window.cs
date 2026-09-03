namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.IWin32Window</c>: an opaque owner-window marker
///  passed through dialog APIs.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtensions.Extensibility/Git/IGitUICommands.cs</c>,
///  <c>GitExtensions.Extensibility/MessageBoxes.cs</c>.
/// </remarks>
public interface IWin32Window
{
    /// <summary>
    ///  Gets the platform window handle; <c>0</c> when the implementation has none.
    /// </summary>
    nint Handle { get; }
}
