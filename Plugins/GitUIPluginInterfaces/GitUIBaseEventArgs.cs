using System.ComponentModel;
using System.Windows.Forms;


namespace GitUIPluginInterfaces
{
    public abstract class GitUIBaseEventArgs : CancelEventArgs
    {
        protected GitUIBaseEventArgs(IGitUICommands gitUICommands, string arguments = null)
            : this(null, gitUICommands, arguments)
        {
        }

        protected GitUIBaseEventArgs(IWin32Window ownerForm, IGitUICommands gitUICommands, string arguments = null)
            : base(false)
        {
            this.OwnerForm = ownerForm;
            this.GitUICommands = gitUICommands;
            this.Arguments = arguments;
        }

        public IGitUICommands GitUICommands { get; private set; }

        public IWin32Window OwnerForm { get; private set; }

        public IGitModule GitModule
        {
            get
            {
                return GitUICommands.GitModule;
            }
            
        }

        public string Arguments { get; private set; }

    }

}
