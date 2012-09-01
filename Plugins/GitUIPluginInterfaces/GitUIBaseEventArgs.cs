using System.ComponentModel;
using System.Windows.Forms;


namespace GitUIPluginInterfaces
{
    public abstract class GitUIBaseEventArgs : CancelEventArgs
    {
        protected GitUIBaseEventArgs(IGitUICommands gitUICommands)
            : this(null, gitUICommands)
        {
        }

        protected GitUIBaseEventArgs(IWin32Window ownerForm, IGitUICommands gitUICommands)
            : base(false)
        {
            this.OwnerForm = ownerForm;
            this.GitUICommands = gitUICommands;
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

    }

}
