using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI
{
    public class GitUIEventArgs : GitUIBaseEventArgs
    {
        public GitUIEventArgs(IGitUICommands gitUICommands, IGitModule module)
            : this(null, gitUICommands, module)
        {
        }

        public GitUIEventArgs(object ownerForm, IGitUICommands gitUICommands, IGitModule module)
            : base(ownerForm, gitUICommands, module)
        {
        }

    }
}
