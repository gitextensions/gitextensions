using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.Script
{
    public class ScriptInfo
    {
        public ScriptInfo()
        {
            Enabled = true;
        }

        public bool Enabled { get; set; }

        public string Name { get; set; }

        public string Command { get; set; }

        public string Arguments { get; set; }

        public bool AddToRevisionGridContextMenu { get; set; }
    }
}
