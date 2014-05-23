using System;
using GitCommands;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public static class EditorHelper
    {
        [NotNull]
        public static Object[] GetEditors()
        {
            return new Object[]
            {
                "\"" + AppSettings.GetGitExtensionsFullPath() + "\" fileeditor",
                "vi",
                "notepad",
                GetNotepadPP(),
                GetSublimeText3(),
            };
        }

        [NotNull]
        private static string GetNotepadPP()
        {
            string npp = MergeToolsHelper.FindFileInFolders("notepad++.exe", "Notepad++");
            if (String.IsNullOrEmpty(npp))
                npp = "notepad++";
            else
                npp = "\"" + npp + "\"";
            npp = npp + " -multiInst -nosession";
            return npp;
        }

        [NotNull]
        private static string GetSublimeText3()
        {
            string exec = MergeToolsHelper.FindFileInFolders("sublime_text.exe", "Sublime Text 3");
            if (String.IsNullOrEmpty(exec))
                exec = "SublimeText";
            else
                exec = "\"" + exec + "\"";
            //http://stackoverflow.com/questions/8951275/git-config-core-editor-how-to-make-sublime-text-the-default-editor-for-git-on
            exec = exec + " -w --multiinstance";
            return exec;
        }
    }
}