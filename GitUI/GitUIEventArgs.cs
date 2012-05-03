using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI
{
    public class GitUIEventArgs : GitUIBaseEventArgs
    {
        public GitUIEventArgs(IWin32Window ownerForm, IGitUICommands gitUICommands)
            : base(ownerForm, gitUICommands) { }

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

        public override string GetGitDirectory()
        {
             return Settings.Module.GetGitDirectory();
        }

        public override bool IsValidGitWorkingDir(string workingDir)
        {
            return GitModule.ValidWorkingDir(workingDir);
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
