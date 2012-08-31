using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI
{
    public class GitUIEventArgs : GitUIBaseEventArgs
    {
        public GitUIEventArgs(IWin32Window ownerForm, IGitUICommands gitUICommands)
            : base(ownerForm, gitUICommands) { }

        public override IGitModule Module
        {
            get
            {
                return GitModule.Current;
            }
        }
    }
}
