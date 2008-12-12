using System;
using System.Collections.Generic;
using System.Linq;
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
            //Dto.Result = GitCommands.RunCmd(Settings.GitDir + "git.exe", "clone " + Dto.Source + " " + Dto.Destination);
            GitCommands.RunRealCmd(Settings.GitDir + "cmd.exe", " /k git.exe clone " + Dto.Source + " " + Dto.Destination);
            Dto.Result = "Done";
        }
    }
}
