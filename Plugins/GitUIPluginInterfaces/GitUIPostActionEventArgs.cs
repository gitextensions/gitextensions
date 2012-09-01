using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public class GitUIPostActionEventArgs : GitUIBaseEventArgs
    {

        public bool ActionDone { get; private set; }

        public GitUIPostActionEventArgs(IWin32Window ownerForm, IGitUICommands gitUICommands, bool actionDone)
            : base(ownerForm, gitUICommands)
        {
            ActionDone = actionDone;
        }

    }
}
