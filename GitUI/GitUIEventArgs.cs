using System;
using System.Collections.Generic;
using System.Text;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI
{
    public class GitUIEventArgs : IGitUIEventArgs
    {
        public GitUIEventArgs(IGitUICommands gitUICommands)
        {
            Cancel = false;
            this.gitUICommands = gitUICommands;
        }

        public bool Cancel { get; set; }

        private IGitUICommands gitUICommands;
        public IGitUICommands GitUICommands 
        {
            get
            {
                return gitUICommands;
            }
        }

        public IGitCommands GitCommands
        {
            get
            {
                return new GitCommands.GitCommands();
            }
        }

        public string GitWorkingDir
        {
            get
            {
                return Settings.WorkingDir;
            }
        }

        public string GitDir
        {
            get
            {
                return Settings.GitDir;
            }
        }

        public string GitVersion
        {
            get
            {
                return "175";
            }
        }


    }
}
