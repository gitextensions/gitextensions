using System;
using GitCommands;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public static class EditorHelper
    {
        [NotNull]
        public static object[] GetEditors()
        {
            return new object[]
            {
                "\"" + AppSettings.GetGitExtensionsFullPath() + "\" fileeditor",
                "vi",
                "notepad",
                GetNotepadPP(),
                GetSublimeText3(),
                GetVsCode()
            };
        }

        [NotNull]
        private static string GetNotepadPP()
        {
            return GetEditorCommandLine("Notepad++", "notepad++.exe", " -multiInst -nosession", "notepad++");
        }

        [NotNull]
        private static string GetVsCode()
        {
            return GetEditorCommandLine("Visual Studio Code", "code.exe", " --wait", "Microsoft VS Code");
        }

        [NotNull]
        private static string GetSublimeText3()
        {
            //http://stackoverflow.com/questions/8951275/git-config-core-editor-how-to-make-sublime-text-the-default-editor-for-git-on
            return GetEditorCommandLine("SublimeText", "sublime_text.exe", " -w --multiinstance", "Sublime Text 3");
        }

        private static string GetEditorCommandLine(string editorName, string executableName, string commandLineParameter, params string[] installFolders)
        {
            string exec = MergeToolsHelper.FindFileInFolders(executableName, installFolders);
            if (string.IsNullOrEmpty(exec))
                exec = editorName;
            else
                exec = "\"" + exec + "\"";
            return exec + commandLineParameter;
        }

    }
}