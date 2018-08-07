using System;
using GitCommands;
using JetBrains.Annotations;

namespace GitUI.UserControls.RevisionGrid
{
    public sealed class DoubleClickRevisionEventArgs : EventArgs
    {
        public DoubleClickRevisionEventArgs([CanBeNull] GitRevision revision)
        {
            Revision = revision;
        }

        [CanBeNull]
        public GitRevision Revision { get; }
    }
}
