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
                return GitModule.Current;
            }
        }

        public override string GitWorkingDir
        {
            get
            {
                return GitModule.CurrentWorkingDir;
            }
        }

        public override string GetGitDirectory()
        {
             return GitModule.Current.GetGitDirectory();
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

        public override string GravatarCacheDir
        {
            get
            {
                return Settings.GravatarCachePath;
            }
        }
    }
}
