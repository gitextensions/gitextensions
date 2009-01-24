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
        public bool Bare { get; set; }

        public CloneDto(string source, string destination, bool bare)
        {
            this.Bare = bare;
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
            if (Dto.Bare)
                GitCommands.RunRealCmd("cmd.exe", " /k \"\"" + Settings.GitDir + "git.cmd\" clone --bare --shared=all \"" + Dto.Source.Trim() + "\" \"" + Dto.Destination.Trim() + "\"\"");
            else
                GitCommands.RunRealCmd("cmd.exe", " /k \"\"" + Settings.GitDir + "git.cmd\" clone \"" + Dto.Source.Trim() + "\" \"" + Dto.Destination.Trim() + "\"\"");
            //GitCommands.RunRealCmd(Settings.GitDir + "git.cmd", "clone \"" + Dto.Source.Trim() + "\" \"" + Dto.Destination.Trim() + "\"");
            Dto.Result = "Done";
        }
    }
}
