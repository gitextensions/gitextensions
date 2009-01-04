using System;
using System.Collections.Generic;

using System.Text;

namespace GitCommands
{
    public class DiffDto
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Result { get; set; }
        public string FileName { get; set; }

        public DiffDto(string from, string to, string fileName)
        {
            this.From = from;
            this.To= to;
            this.FileName = fileName;
        }
    }

    public class Diff
    {
        public DiffDto Dto { get; set; }
        public Diff(DiffDto dto)
        {
            this.Dto = dto;
        }

        public void Execute()
        {
            Dto.Result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", "diff \"" + Dto.From + "\"..\"" + Dto.To + "\" -- \"" + Dto.FileName + "\"");
        }
    }
}
