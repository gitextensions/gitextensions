using System.ComponentModel;

namespace GitUIPluginInterfaces
{
    public abstract class GitUIBaseEventArgs : CancelEventArgs
    {
        protected GitUIBaseEventArgs(IGitUICommands gitUICommands)
            : this(null, gitUICommands)
        {
        }

        protected GitUIBaseEventArgs(object ownerForm, IGitUICommands gitUICommands)
            : base(false)
        {
            this.OwnerForm = ownerForm;
            this.GitUICommands = gitUICommands;
        }

        public IGitUICommands GitUICommands { get; private set; }

        public object OwnerForm { get; private set; }

        public abstract IGitModule Module { get; }

    }

}
