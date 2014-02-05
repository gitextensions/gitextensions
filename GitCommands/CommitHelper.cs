using System;
using System.IO;

namespace GitCommands
{
    public class CommitDto
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

    public class CommitHelper
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
            if (String.IsNullOrEmpty(commitMessageText))
            {
                File.Delete(GetCommitMessagePath(module));
                return;
            }

            using (var textWriter = new StreamWriter(GetCommitMessagePath(module), false, module.CommitEncoding))
            {
                textWriter.Write(commitMessageText);
            }
        }

        public static string GetCommitMessagePath(GitModule module)
        {
            return Path.Combine(module.GetGitDirectory(), "COMMITMESSAGE");
        }
    }
}