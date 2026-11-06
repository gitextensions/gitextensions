namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Shows a folder-selection dialog for the <see cref="FolderBrowserDialog"/> stand-in.
///  Implemented by the Avalonia application over its storage-provider API.
/// </summary>
public interface IFolderPicker
{
    /// <summary>
    ///  Prompts the user to select a directory and blocks until the dialog is dismissed.
    /// </summary>
    /// <param name="owner">The owner window, if any.</param>
    /// <param name="selectedPath">The initially selected path, if any.</param>
    /// <returns>The selected path, or <see langword="null"/> if the user cancelled.</returns>
    string? PickFolder(IWin32Window? owner, string? selectedPath);
}
