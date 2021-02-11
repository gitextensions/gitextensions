using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
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
                GetNotepadPP(),
                GetSublimeText3(),
                GetVsCode(),
                GetAtom()
            };
        }

        private static string GetNotepadPP()
        {
            return GetEditorCommandLine("Notepad++", "notepad++.exe", " -multiInst -nosession", "notepad++");
        }

        private static string GetVsCode()
        {
            return GetEditorCommandLine("Visual Studio Code", "code.exe", " --wait", "Microsoft VS Code");
        }

        private static string GetAtom()
        {
            return GetEditorCommandLine("Atom", "atom.exe", " --wait", "atom");
        }

        private static string GetSublimeText3()
        {
            // http://stackoverflow.com/questions/8951275/git-config-core-editor-how-to-make-sublime-text-the-default-editor-for-git-on
            return GetEditorCommandLine("SublimeText", "sublime_text.exe", " -w --multiinstance", "Sublime Text 3");
        }

        private static string GetEditorCommandLine(string editorName, string executableName, string commandLineParameter, params string[] installFolders)
        {
            string exec = executableName.FindInFolders(installFolders);

            if (string.IsNullOrEmpty(exec))
            {
                exec = editorName;
            }
            else
            {
                exec = $"\"{exec}\"";
            }

            return exec + commandLineParameter;
        }
    }
}
