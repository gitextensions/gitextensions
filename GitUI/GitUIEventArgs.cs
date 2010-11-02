using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI
{
    public class GitUIEventArgs : GitUIBaseEventArgs
    {
        public GitUIEventArgs(IGitUICommands gitUICommands) : base(gitUICommands) { }

        public IGitUICommands GitUICommands { get; private set; }

        public override IGitCommands GitCommands
        {
            get
            {
                return new GitCommandsInstance();
            }
        }

        public override string GitWorkingDir
        {
            get
            {
                return Settings.WorkingDir;
            }
        }

        public override bool IsValidGitWorkingDir(string workingDir)
        {
            return Settings.ValidWorkingDir(workingDir);
        }

        public override string GitCommand
        {
            get
            {
                return Settings.GitCommand;
            }
        }

        public override string GitVersion
        {
            get
            {
                return Settings.GitExtensionsVersionInt.ToString();
            }
        }
    }
}
