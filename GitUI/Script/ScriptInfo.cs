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
        AfterPush,
        ShowInUserMenuBar
    }

    public class ScriptInfo
    {
        private string _Icon;

        public ScriptInfo()
        {
            _Icon = "bug";
            Enabled = true;
        }

        public bool Enabled { get; set; }

        public string Name { get; set; }

        public string Command { get; set; }

        public string Arguments { get; set; }

        public bool AddToRevisionGridContextMenu { get; set; }

        public ScriptEvent OnEvent { get; set; }

        public bool AskConfirmation { get; set; }

        public int HotkeyCommandIdentifier { get; set; }

        public string Icon { set { _Icon = value; } }
            
        public System.Drawing.Bitmap GetIcon() {
            // Get all resources
                    System.Resources.ResourceManager rm 
                        = new System.Resources.ResourceManager("GitUI.Properties.Resources"
                            , System.Reflection.Assembly.GetExecutingAssembly());
            // return icon
                    return (System.Drawing.Bitmap)rm.GetObject(_Icon);
        }
    }
}
