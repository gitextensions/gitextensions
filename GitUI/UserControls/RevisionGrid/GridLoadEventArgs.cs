using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid
{
    public class GridLoadEventArgs : GitUIEventArgs
    {
        public GridLoadEventArgs(IWin32Window? ownerForm, IGitUICommands gitUICommands, Lazy<IReadOnlyList<IGitRef>> getRefs, bool forceRefresh)
            : base(ownerForm, gitUICommands, getRefs)
        {
            ForceRefresh = forceRefresh;
        }

        public bool ForceRefresh { get; init; }
    }
}
