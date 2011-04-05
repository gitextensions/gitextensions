using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.Script
{
    public enum ScriptEvent
    {
        None,
        BeforeCommit,
        AfterCommit,
        BeforePull,
        AfterPull,
        BeforePush,
        AfterPush
    }

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

        public ScriptEvent OnEvent { get; set; }

        public bool AskConfirmation { get; set; }
    }
}
