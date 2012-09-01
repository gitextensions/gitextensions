using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI
{
    public class GitUIEventArgs : GitUIBaseEventArgs
    {
        public GitUIEventArgs()
            : this(null)
        {
        }

        public GitUIEventArgs(IWin32Window ownerForm, IGitUICommands gitUICommands)
            : base(ownerForm, gitUICommands)
        {
        }

        public GitUIEventArgs(IWin32Window ownerForm)
            : this(ownerForm, GitUI.GitUICommands.Instance)
        {
        }


    }
}
