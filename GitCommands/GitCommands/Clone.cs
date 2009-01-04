using System;
using System.Collections.Generic;

using System.Text;

namespace GitCommands
{
    public class CloneDto
    {
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Result { get; set; }

        public CloneDto(string source, string destination)
        {
            this.Source = source;
            this.Destination = destination;
        }
    }

    public class Clone
    {
        public CloneDto Dto { get; set; }
        public Clone(CloneDto dto)
        {
            this.Dto = dto;
        }

        public void Execute()
        {
            GitCommands.RunRealCmd(Settings.GitDir + "cmd.exe", " /k git.cmd clone \"" + Dto.Source.Trim() + "\" \"" + Dto.Destination.Trim() + "\"");
            Dto.Result = "Done";
        }
    }
}
