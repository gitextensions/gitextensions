using System;
using GitCommands;

namespace GitUI.UserControls.RevisionGridClasses
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
