using System;
using System.Collections.Generic;

using System.Text;

namespace GitCommands
{
    public class PullDto
    {
        public string Result { get; set; }

        public PullDto()
        {
        }
    }

    public class Pull
    {
        public PullDto Dto { get; set; }
        public Pull(PullDto dto)
        {
            this.Dto = dto;
        }

        public void Execute()
        {
            Dto.Result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", "pull");
        }
    }
}
