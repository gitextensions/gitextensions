using System.ComponentModel;

namespace GitUIPluginInterfaces
{
    public abstract class GitUIBaseEventArgs : CancelEventArgs
    {
        protected GitUIBaseEventArgs(IGitUICommands gitUICommands, IGitModule module)
            : this(null, gitUICommands, module)
        {
        }

        protected GitUIBaseEventArgs(object ownerForm, IGitUICommands gitUICommands, IGitModule module)
            : base(false)
        {
            this.OwnerForm = ownerForm;
            this.GitUICommands = gitUICommands;
            Module = module;
        }

        public IGitUICommands GitUICommands { get; private set; }

        public object OwnerForm { get; private set; }

        public IGitModule Module { get; private set; }

    }

}
