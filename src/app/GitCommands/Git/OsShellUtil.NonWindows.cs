using GitExtensions.Extensibility;

namespace GitCommands;

/// <summary>
///  Provides helper methods for interacting with the OS shell (opening files, URLs, and the
///  platform file manager).
/// </summary>
public static class OsShellUtil
{
    private const string ErrorCaption = "Git Extensions";

    /// <summary>
    ///  Let the user chose an application to open a file.
    /// </summary>
    /// <param name="filePath">Pathname of the file to open.</param>
    public static void Open(string filePath)
        => Launch(filePath, OsShellLaunchKind.Open);

    /// <summary>
    ///  Open a file with its associated default application.
    /// </summary>
    /// <param name="filePath">Pathname of the file to open.</param>
    public static void OpenAs(string filePath)
        => Launch(filePath, OsShellLaunchKind.OpenAs);

    /// <summary>
    ///  Opens the directory containing the specified file.
    /// </summary>
    /// <param name="filePath">The full path of the file to reveal.</param>
    public static void SelectPathInFileExplorer(string filePath)
        => Launch(filePath, OsShellLaunchKind.ShowInDirectory);

    /// <summary>
    ///  Opens the platform file manager at the specified directory.
    /// </summary>
    /// <param name="arguments">The directory to open.</param>
    /// <param name="quote">Retained for source compatibility; portable launchers receive paths without command-line quoting.</param>
    public static void OpenWithFileExplorer(string arguments, bool quote = true)
        => Launch(arguments, OsShellLaunchKind.OpenDirectory);

    /// <summary>
    ///  Opens the specified URL in the user's default web browser.
    /// </summary>
    /// <param name="url">The URL to open, or <see langword="null"/> to do nothing.</param>
    public static void OpenUrlInDefaultBrowser(string? url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            Launch(url, OsShellLaunchKind.OpenUri);
        }
    }

    /// <summary>
    ///  Prompts the user to select a directory.
    /// </summary>
    /// <param name="ownerWindow">The owner window.</param>
    /// <param name="selectedPath">The initially selected path.</param>
    /// <returns>The path selected by the user, or <see langword="null"/> if the user cancels the dialog.</returns>
    public static string? PickFolder(IWin32Window ownerWindow, string? selectedPath = null)
    {
        using FolderBrowserDialog dialog = new();
        if (selectedPath is not null)
        {
            dialog.SelectedPath = selectedPath;
        }

        DialogResult result = dialog.ShowDialog(ownerWindow);
        if (result == DialogResult.OK)
        {
            return dialog.SelectedPath;
        }

        // return null if the user cancelled
        return null;
    }

    private static void Launch(string target, OsShellLaunchKind kind)
    {
        if (TestAccessor.MockExecutable is { } executable)
        {
            _ = executable.Start(useShellExecute: true, throwOnErrorExit: false);
            return;
        }

        if (ShimHost.OsShell.TryLaunch(target, kind))
        {
            return;
        }

        _ = ShimHost.MessageBoxHost.Show(
            owner: null,
            $"The desktop could not open '{target}'. On Linux, verify that an XDG desktop portal backend is installed and running.",
            ErrorCaption,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1);
    }

    // parity-scaffolding: preserves the original shell test interception boundary for portable tests.
    internal struct TestAccessor
    {
        public static IExecutable? MockExecutable { get; set; }
    }
}
