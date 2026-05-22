namespace GitExtensions.Extensibility;

/// <summary>
///  Provides general-purpose wrappers around <c>System.Windows.Forms.MessageBox</c>
///  for use by plugins and other projects that cannot reference GitUI.
///  Projects within GitUI should prefer the <c>GitUI.MessageBoxes</c> class
///  which provides additional domain-specific methods with translatable strings.
/// </summary>
public static class MessageBoxes
{
    /// <summary>
    ///  Shows a message box with the specified parameters.
    /// </summary>
    /// <returns>The <see cref="DialogResult"/> selected by the user.</returns>
#pragma warning disable RS0030 // Do not use banned APIs -- MessageBoxes is the approved wrapper
    public static DialogResult Show(IWin32Window? owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        => MessageBox.Show(owner ?? Form.ActiveForm, text, caption, buttons, icon, defaultButton);
#pragma warning restore RS0030

    /// <summary>
    ///  Shows a message box without specifying an icon.
    /// </summary>
    /// <returns>The <see cref="DialogResult"/> selected by the user.</returns>
    public static DialogResult Show(IWin32Window? owner, string text, string caption, MessageBoxButtons buttons)
        => Show(owner, text, caption, buttons, MessageBoxIcon.None);

    /// <summary>
    ///  Shows a message box without an explicit owner window.
    /// </summary>
    /// <returns>The <see cref="DialogResult"/> selected by the user.</returns>
    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        => Show(owner: null, text, caption, buttons, icon, defaultButton);

    /// <summary>
    ///  Shows a message box without an explicit owner window.
    /// </summary>
    /// <returns>The <see cref="DialogResult"/> selected by the user.</returns>
    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        => Show(owner: null, text, caption, buttons, icon);

    /// <summary>
    ///  Shows a message box without an explicit owner window or icon.
    /// </summary>
    /// <returns>The <see cref="DialogResult"/> selected by the user.</returns>
    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
        => Show(owner: null, text, caption, buttons, MessageBoxIcon.None);

    /// <summary>
    ///  Shows an error message box.
    /// </summary>
    public static void ShowError(IWin32Window? owner, string text, string? caption = null)
        => Show(owner, text, caption ?? "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

    /// <summary>
    ///  Shows a Yes/No confirmation dialog.
    /// </summary>
    /// <returns><see langword="true"/> if the user selected Yes.</returns>
    public static bool Confirm(IWin32Window? owner, string text, string caption, MessageBoxIcon icon = MessageBoxIcon.Question, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        => Show(owner, text, caption, MessageBoxButtons.YesNo, icon, defaultButton) == DialogResult.Yes;
}
