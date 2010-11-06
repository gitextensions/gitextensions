using System.ComponentModel;

namespace GitUIPluginInterfaces
{
    public abstract class GitUIBaseEventArgs : CancelEventArgs
    {
        public GitUIBaseEventArgs(IGitUICommands gitUICommands)
            : base(false)
        {
            this.GitUICommands = gitUICommands;
        }

        public IGitUICommands GitUICommands { get; private set; }

        public abstract IGitCommands GitCommands { get; }

        public abstract string GitWorkingDir { get; }

        public abstract bool IsValidGitWorkingDir(string workingDir);

        public abstract string GitCommand
        {
            get;
        }

        public abstract string GitVersion
        {
            get;
        }
    }

}
