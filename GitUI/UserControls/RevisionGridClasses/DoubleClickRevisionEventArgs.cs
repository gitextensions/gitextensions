﻿using System;
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
