using System.IO;

namespace GitCommands
{
    public static class CommitHelper
    {
        public static void SetCommitMessage(GitModule module, string commitMessageText, bool amendCommit)
        {
            if (string.IsNullOrEmpty(commitMessageText))
            {
                File.Delete(GetCommitMessagePath(module));
                File.Delete(GetAmendPath(module));
                return;
            }

            using (var textWriter = new StreamWriter(GetCommitMessagePath(module), false, module.CommitEncoding))
            {
                textWriter.Write(commitMessageText);
            }

            if (AppSettings.RememberAmendCommitState && amendCommit)
            {
                File.WriteAllText(GetAmendPath(module), true.ToString());
            }
            else if (File.Exists(GetAmendPath(module)))
            {
                File.Delete(GetAmendPath(module));
            }
        }

        public static string GetCommitMessage(GitModule module)
        {
            if (File.Exists(GetCommitMessagePath(module)))
            {
                return File.ReadAllText(GetCommitMessagePath(module), module.CommitEncoding);
            }

            return string.Empty;
        }

        public static string GetCommitMessagePath(GitModule module)
        {
            return GetFilePath(module, "COMMITMESSAGE");
        }

        private static string GetAmendPath(GitModule module)
        {
            return GetFilePath(module, "GitExtensions.amend");
        }

        private static string GetFilePath(GitModule module, string action)
        {
            return Path.Combine(module.WorkingDirGitDir, action);
        }

        public static bool GetAmendState(GitModule module)
        {
            bool amendState = false;

            if (AppSettings.RememberAmendCommitState && File.Exists(GetAmendPath(module)))
            {
                var amendSaveStateFilePath = GetAmendPath(module);
                bool.TryParse(File.ReadAllText(amendSaveStateFilePath), out amendState);
                try
                {
                    File.Delete(amendSaveStateFilePath);
                }
                catch
                {
                    // ignore
                }
            }

            return amendState;
        }
    }
}