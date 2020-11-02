using System.ComponentModel;
using System.Windows.Forms;
using GitExtensions.Core.Module;

namespace GitExtensions.Core.Commands.Events
{
    public class GitUIEventArgs : CancelEventArgs
    {
        public GitUIEventArgs(IWin32Window ownerForm, IGitUICommands gitUICommands)
            : base(cancel: false)
        {
            OwnerForm = ownerForm;
            GitUICommands = gitUICommands;
        }

        public IGitUICommands GitUICommands { get; }

        public IWin32Window OwnerForm { get; }

        public IGitModule GitModule => GitUICommands.GitModule;
    }
}
