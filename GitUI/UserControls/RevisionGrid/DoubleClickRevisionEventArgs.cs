using System;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid
{
    public sealed class DoubleClickRevisionEventArgs : EventArgs
    {
        public DoubleClickRevisionEventArgs(GitRevision? revision)
        {
            Revision = revision;
        }

        public GitRevision? Revision { get; }
    }
}
