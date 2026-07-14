namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.MessageBox</c>: routes to the installed
///  <see cref="IMessageBoxHost"/>.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtensions.Extensibility/MessageBoxes.cs</c> (the approved wrapper).
/// </remarks>
public static class MessageBox
{
    /// <summary>
    ///  Shows a message box and blocks until the user dismisses it.
    /// </summary>
    /// <returns>The <see cref="DialogResult"/> selected by the user.</returns>
    public static DialogResult Show(IWin32Window? owner, string? text, string? caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        => ShimHost.MessageBoxHost.Show(owner, text, caption, buttons, icon, defaultButton);

    /// <summary>
    ///  Shows a message box with the first button as default.
    /// </summary>
    /// <returns>The <see cref="DialogResult"/> selected by the user.</returns>
    public static DialogResult Show(IWin32Window? owner, string? text, string? caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        => Show(owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);

    /// <summary>
    ///  Shows a message box without an owner window.
    /// </summary>
    /// <returns>The <see cref="DialogResult"/> selected by the user.</returns>
    public static DialogResult Show(string? text, string? caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        => Show(owner: null, text, caption, buttons, icon);
}
