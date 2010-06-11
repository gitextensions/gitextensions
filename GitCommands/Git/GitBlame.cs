using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GitCommands
{
    public class GitBlame
    {
        public string CommitGuid { get; set; }
        public string LineId { get; set; }
        public string Text { get; set; }
        public string Author { get; set; }
        public string Message { get; set; }
        public Color color { get; set; }
    }
}
