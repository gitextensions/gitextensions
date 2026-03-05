using GitExtensions.Extensibility;

namespace GitCommands;

public static class OsShellUtil
{
    private static IExecutable? _mockExecutable;

    private static IExecutable CreateExecutable(string command)
    {
        if (_mockExecutable is not null)
        {
            _mockExecutable.Command = command;
            return _mockExecutable;
        }

        return new Executable(command);
    }

    /// <summary>
    /// Open a file with its associated default application.
    /// </summary>
    /// <param name="filePath">Pathname of the file to open.</param>
    public static void Open(string filePath)
    {
        try
        {
            CreateExecutable(filePath).Start(useShellExecute: true, throwOnErrorExit: false);
        }
        catch (Exception)
        {
            OpenAs(filePath);
        }
    }

    /// <summary>
    /// Let the user chose an application to open a file.
    /// </summary>
    /// <param name="filePath">Pathname of the file to open.</param>
    public static void OpenAs(string filePath)
    {
        // filePath must not be quoted
        CreateExecutable("rundll32.exe").Start("shell32.dll,OpenAs_RunDLL " + filePath, redirectOutput: true, outputEncoding: System.Text.Encoding.UTF8);
    }

    public static void SelectPathInFileExplorer(string filePath) => OpenWithFileExplorer($"/select, {filePath.Quote()}", quote: false);

    public static void OpenWithFileExplorer(string arguments, bool quote = true) => CreateExecutable("explorer.exe").Start(quote ? arguments.Quote() : arguments);

    public static void OpenUrlInDefaultBrowser(string? url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            CreateExecutable(url).Start(useShellExecute: true, throwOnErrorExit: false);
        }
    }

    /// <summary>
    /// Prompts the user to select a directory.
    /// </summary>
    /// <param name="ownerWindow">The owner window.</param>
    /// <param name="selectedPath">The initially selected path.</param>
    /// <returns>The path selected by the user, or null if the user cancels the dialog.</returns>
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

    internal static TestAccessor GetTestAccessor() => new();

    internal struct TestAccessor
    {
        public readonly IExecutable? MockExecutable
        {
            get => _mockExecutable;
            set => _mockExecutable = value;
        }
    }
}
