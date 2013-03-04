using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCommands;

namespace GitUI.UserControls.RevisionGridClasses
{
    public class DoubleClickRevisionEventArgs : EventArgs
    {
        GitRevision _revision;

        public DoubleClickRevisionEventArgs(GitRevision revision)
        {
            _revision = revision;
        }

        public GitRevision Revision { get { return _revision; } }
    }
}
