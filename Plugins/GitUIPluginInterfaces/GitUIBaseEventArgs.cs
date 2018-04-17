using System.ComponentModel;
using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public abstract class GitUIBaseEventArgs : CancelEventArgs
    {
        protected GitUIBaseEventArgs(IWin32Window ownerForm, IGitUICommands gitUICommands, string arguments = null)
            : base(false)
        {
            OwnerForm = ownerForm;
            GitUICommands = gitUICommands;
            Arguments = arguments;
        }

        public IGitUICommands GitUICommands { get; }

        public IWin32Window OwnerForm { get; }

        public IGitModule GitModule => GitUICommands.GitModule;

        public string Arguments { get; }
    }
}
