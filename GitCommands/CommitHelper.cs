using System.IO;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public sealed class CommitDto
    {
        public CommitDto(string message, bool amend)
        {
            Message = message;
            Amend = amend;
        }

        public string Message { get; set; }
        public string Result { get; set; }
        public bool Amend { get; set; }
    }

    public sealed class CommitHelper
    {
        public CommitHelper(CommitDto dto)
        {
            Dto = dto;
        }

        public CommitDto Dto { get; set; }

        public void Execute(GitModule module)
        {
            if (Dto.Amend)
                Dto.Result = module.RunGitCmd("commit --amend -m \"" + Dto.Message + "\"");
            else
                Dto.Result = module.RunGitCmd("commit -m \"" + Dto.Message + "\"");
        }

        public static void SetCommitMessage(GitModule module, string commitMessageText)
        {
            if (string.IsNullOrEmpty(commitMessageText))
            {
                File.Delete(GetCommitMessagePath(module));
                return;
            }

            using (var textWriter = new StreamWriter(GetCommitMessagePath(module), false, module.CommitEncoding))
            {
                textWriter.Write(commitMessageText);
            }
        }

        public static void SetAmendCommit(GitModule module, bool amendCommit)
        {
            var path = GetAmendCommitPath(module);
            using (var textWriter = new StreamWriter(path, false, module.CommitEncoding))
            {
                textWriter.Write(amendCommit);
            }
        }

        public static string GetCommitMessage(GitModule module)
        {
            var path = GetCommitMessagePath(module);

            var commitMessage = File.ReadAllText(path, module.CommitEncoding);

            return commitMessage;
        }

        public static bool GetAmendCommit(GitModule module)
        {
            var path = GetAmendCommitPath(module);

            var amendCommitAsString = File.ReadAllText(path, module.FilesEncoding);
            bool amendCommit;
            bool.TryParse(amendCommitAsString, out amendCommit);

            return amendCommit;
        }

        public static bool IsCommitMessageSaved(GitModule module)
        {
            var path = GetCommitMessagePath(module);

            var isCommitMessageExists = File.Exists(path);

            return isCommitMessageExists;
        }

        public static bool IsAmendCommitSaved(GitModule module)
        {
            var path = GetAmendCommitPath(module);

            var isAmendCommitExists = File.Exists(path);

            return isAmendCommitExists;
        }

        private static string GetCommitMessagePath(IGitModule module)
        {
            return Path.Combine(module.GetGitDirectory(), "COMMITMESSAGE");
        }

        private static string GetAmendCommitPath(IGitModule module)
        {
            return Path.Combine(module.GetGitDirectory(), "AMENDCOMMIT");
        }
    }
}