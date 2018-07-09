using System;
using GitCommands;

namespace GitUI.UserControls.RevisionGrid
{
    public class DoubleClickRevisionEventArgs : EventArgs
    {
        public DoubleClickRevisionEventArgs(GitRevision revision)
        {
            Revision = revision;
        }

        public GitRevision Revision { get; }
    }
}
