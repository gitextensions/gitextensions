namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.FolderBrowserDialog</c>: routes to the
///  <see cref="IFolderPicker"/> service installed in <see cref="ShimHost"/>.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitCommands/Git/OsShellUtil.cs</c> (<c>PickFolder</c>).
/// </remarks>
public sealed class FolderBrowserDialog : IDisposable
{
    /// <summary>
    ///  Gets or sets the selected path; set before showing to define the initial selection.
    /// </summary>
    public string? SelectedPath { get; set; }

    /// <summary>
    ///  Shows the dialog and blocks until it is dismissed.
    /// </summary>
    /// <returns><see cref="DialogResult.OK"/> with <see cref="SelectedPath"/> set, or <see cref="DialogResult.Cancel"/>.</returns>
    public DialogResult ShowDialog(IWin32Window? owner)
    {
        string? result = ShimHost.FolderPicker.PickFolder(owner, SelectedPath);
        if (result is null)
        {
            return DialogResult.Cancel;
        }

        SelectedPath = result;
        return DialogResult.OK;
    }

    public void Dispose()
    {
        // Stateless; nothing to release. Present because callers dispose the dialog.
    }
}
