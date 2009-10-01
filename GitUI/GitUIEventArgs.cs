using System;
using System.Collections.Generic;
using System.Text;
using GitCommands;

namespace GitUI
{
    public class GitUIEventArgs
    {
        public GitUIEventArgs()
        {
            Cancel = false;
        }

        public bool Cancel { get; set; }
        public GitUICommands GitUICommands 
        {
            get
            {
                return GitUICommands.Instance;
            }
        }

        public string GitWorkingDir
        {
            get
            {
                return Settings.WorkingDir;
            }
        }


    }
}
