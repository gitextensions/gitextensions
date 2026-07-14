namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Displays message boxes on behalf of the <see cref="MessageBox"/> stand-in.
///  Implemented by the Avalonia application (synchronously, via a nested dispatcher frame).
/// </summary>
public interface IMessageBoxHost
{
    /// <summary>
    ///  Shows a message box and blocks until the user dismisses it.
    /// </summary>
    /// <returns>The <see cref="DialogResult"/> selected by the user.</returns>
    DialogResult Show(IWin32Window? owner, string? text, string? caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton);
}
