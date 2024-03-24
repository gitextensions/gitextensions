using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public static class EditorHelper
{
    public static string FileEditorCommand
        => $"\"{AppSettings.GetGitExtensionsFullPath()}\" fileeditor";

    public static string[] GetEditors()
    {
        return new[]
        {
            FileEditorCommand,
            "vi",
            "notepad",
            GetNotepadPlusPlus(),
            GetSublimeText(),
            GetVsCode(),
        };
    }

    private static string GetNotepadPlusPlus()
        => GetEditorCommandLine("notepad++.exe", "-multiInst -nosession", "notepad++");

    private static string GetVsCode()
        => GetEditorCommandLine("code.exe", "--new-window --wait", "Microsoft VS Code");

    // http://stackoverflow.com/questions/8951275/git-config-core-editor-how-to-make-sublime-text-the-default-editor-for-git-on
    private static string GetSublimeText()
        => GetEditorCommandLine("sublime_text.exe", "--new-window --wait", "Sublime Text");

    private static string GetEditorCommandLine(string executableName, string commandLineParameter, params string[] installFolders)
    {
        string exec = executableName.FindInFolders(installFolders);

        if (string.IsNullOrEmpty(exec))
        {
            // hoping the tool is available in the PATH
            exec = Path.GetExtension(executableName) == ".exe" ? Path.GetFileNameWithoutExtension(executableName) : executableName;
        }
        else
        {
            exec = $"\"{exec}\"";
        }

        return $"{exec} {commandLineParameter}";
    }
}
