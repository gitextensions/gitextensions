using System.ComponentModel;

namespace GitUIPluginInterfaces
{
    public abstract class GitUIBaseEventArgs : CancelEventArgs
    {
        protected GitUIBaseEventArgs(IGitUICommands gitUICommands)
            : this(null, gitUICommands)
        {
        }

        protected GitUIBaseEventArgs(object ownerForm, IGitUICommands gitUICommands)
            : base(false)
        {
            this.OwnerForm = ownerForm;
            this.GitUICommands = gitUICommands;
        }

        public IGitUICommands GitUICommands { get; private set; }

        public object OwnerForm { get; private set; }

        public abstract IGitCommands GitCommands { get; }

        public abstract string GitWorkingDir { get; }

        public abstract string GetGitDirectory();

        public abstract bool IsValidGitWorkingDir(string workingDir);

        public abstract string GitCommand { get; }

        public abstract string GitVersion { get; }
    }

}
