using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog;

public static class EditorHelper
{
    public static string[] GetEditors()
    {
        if (!OperatingSystem.IsWindows())
        {
            return
            [
                "vi",
                "vim",
                "nano",
                "code --new-window --wait",
                "zed --wait",
            ];
        }

        return
        [
            "vi",
            "notepad",
            GetNotepadPlusPlus(),
            GetSublimeText(),
            GetVsCode(),
            GetZed(),
        ];
    }

    internal static string GetDefaultEditor() => "vi";

    private static string GetNotepadPlusPlus()
        => GetEditorCommandLine("notepad++.exe", "-multiInst -nosession", "notepad++");

    private static string GetVsCode()
        => GetEditorCommandLine("code.exe", "--new-window --wait", "Microsoft VS Code");

    private static string GetZed()
        => GetEditorCommandLine("zed.exe", "--wait", "Zed.dev");

    private static string GetSublimeText()
        => GetEditorCommandLine("sublime_text.exe", "--new-window --wait", "Sublime Text");

    private static string GetEditorCommandLine(string executableName, string commandLineParameter, params string[] installFolders)
    {
        string exec = executableName.FindInFolders(installFolders);
        if (string.IsNullOrEmpty(exec))
        {
            exec = Path.GetExtension(executableName) == ".exe"
                ? Path.GetFileNameWithoutExtension(executableName)
                : executableName;
        }
        else
        {
            exec = $"\"{exec}\"";
        }

        return $"{exec} {commandLineParameter}";
    }
}
