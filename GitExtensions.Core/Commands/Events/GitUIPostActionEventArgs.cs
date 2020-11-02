using System.Windows.Forms;

namespace GitExtensions.Core.Commands.Events
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
