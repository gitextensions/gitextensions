using System;
using System.Collections.Generic;

using System.Text;

namespace GitCommands
{
    public class CommitDto
    {
        public string Message { get; set; }
        public string Result { get; set; }
        public bool Amend { get; set; }

        public CommitDto(string message, bool amend)
        {
            this.Message = message;
            this.Amend = amend;
        }
    }

    public class Commit
    {
        public CommitDto Dto { get; set; }
        public Commit(CommitDto dto)
        {
            this.Dto = dto;
        }

        public void Execute()
        {
            if (Dto.Amend)
                Dto.Result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", "commit --amend -m \"" + Dto.Message + "\"");
            else
                Dto.Result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", "commit -m \"" + Dto.Message + "\"");
        }
    }
}
