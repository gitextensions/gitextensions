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

    public class Commit
    {
        public Commit(CommitDto dto)
        {
            Dto = dto;
        }

        public CommitDto Dto { get; set; }

        public void Execute()
        {
            if (Dto.Amend)
                Dto.Result = Settings.Module.RunGitCmd("commit --amend -m \"" + Dto.Message + "\"");
            else
                Dto.Result = Settings.Module.RunGitCmd("commit -m \"" + Dto.Message + "\"");
        }

        public static void SetCommitMessage(string commitMessageText)
        {
            if (String.IsNullOrEmpty(commitMessageText))
            {
                File.Delete(GetCommitMessagePath());
                return;
            }

            using (var textWriter = new StreamWriter(GetCommitMessagePath(), false, Settings.CommitEncoding))
            {
                textWriter.Write(commitMessageText);
            }
        }

        public static string GetCommitMessagePath()
        {
            return Settings.Module.WorkingDirGitDir() + Settings.PathSeparator.ToString() + "COMMITMESSAGE";
        }
    }
}