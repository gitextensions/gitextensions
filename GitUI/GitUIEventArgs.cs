using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitUI
{
    public class GitUIEventArgs : GitUIBaseEventArgs
    {
        public GitUIEventArgs(IWin32Window ownerForm, IGitUICommands gitUICommands, string arguments = null)
            : base(ownerForm, gitUICommands, arguments)
        {
        }
    }
}
