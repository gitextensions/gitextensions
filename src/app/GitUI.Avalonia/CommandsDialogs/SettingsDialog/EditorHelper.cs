using GitCommands;
using GitUI.Compat;

namespace GitUI.CommandsDialogs.SettingsDialog;

public static class EditorHelper
{
    public static string[] GetEditors()
    {
        if (!OperatingSystem.IsWindows())
        {
            return
            [
                GetBuiltInEditorCommand(),
                "vi",
                "vim",
                "nano",
                GetVsCode(),
                GetZed(),
                GetSublimeText(),
            ];
        }

        return
        [
            GetBuiltInEditorCommand(),
            "vi",
            "notepad",
            GetNotepadPlusPlus(),
            GetSublimeText(),
            GetVsCode(),
            GetZed(),
        ];
    }

    internal static string GetDefaultEditor() => GetBuiltInEditorCommand();

    internal static string GetBuiltInEditorCommand()
    {
        if (FlatpakEnvironment.IsFlatpak())
        {
            // Cross-platform constraint: the saved editor must also run from the host's Git, including host-private temporary files.
            return FlatpakToolEnvironment.GetBuiltInEditorCommand();
        }

        string executableName = OperatingSystem.IsWindows()
            ? "GitExtensions.Avalonia.exe"
            : "GitExtensions.Avalonia";
        return $"\"{Path.Combine(AppContext.BaseDirectory, executableName)}\" fileeditor";
    }

    private static string GetNotepadPlusPlus()
        => GetEditorCommandLine("notepad++.exe", "-multiInst -nosession", "notepad++");

    private static string GetVsCode()
        => GetEditorCommandLine("code.exe", "--new-window --wait", "Microsoft VS Code");

    private static string GetZed()
        => GetEditorCommandLine("zed.exe", "--wait", "Zed.dev");

    // http://stackoverflow.com/questions/8951275/git-config-core-editor-how-to-make-sublime-text-the-default-editor-for-git-on
    private static string GetSublimeText()
        => GetEditorCommandLine("sublime_text.exe", "--new-window --wait", "Sublime Text");

    private static string GetEditorCommandLine(string executableName, string commandLineParameter, params string[] installFolders)
    {
        string exec = executableName.FindInFolders(installFolders);

        // Cross-platform constraint: supplement the shared search with native installation locations.
        if (string.IsNullOrEmpty(exec) && PlatformToolLocator.Current.FindForConfiguration(executableName) is LocatedTool tool)
        {
            return PlatformToolLocator.GetEditorCommand(tool, commandLineParameter);
        }

        if (string.IsNullOrEmpty(exec))
        {
            // hoping the tool is available in the PATH
            exec = PlatformToolLocator.Current.GetFallbackExecutableName(executableName);
        }
        else
        {
            exec = $"\"{exec}\"";
        }

        return $"{exec} {commandLineParameter}";
    }
}
