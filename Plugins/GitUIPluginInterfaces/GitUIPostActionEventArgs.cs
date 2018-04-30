using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public class GitUIPostActionEventArgs : GitUIEventArgs
    {
        public bool ActionDone { get; }

        public GitUIPostActionEventArgs(IWin32Window ownerForm, IGitUICommands gitUICommands, bool actionDone)
            : base(ownerForm, gitUICommands)
        {
            ActionDone = actionDone;
        }
    }
}
