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

        public bool IsValidGitWorkingDir(string workingDir)
        {
            return Settings.ValidWorkingDir(workingDir);
        }

        public string GitCommand
        {
            get
            {
                return Settings.GitCommand;
            }
        }

        public string GitVersion
        {
            get
            {
                return Settings.GitExtensionsVersionInt.ToString();
            }
        }


    }
}
