using System;
using System.Threading;
using JetBrains.Annotations;

namespace GitCommands.Worktrees
{
    public class WorktreeStatusEventArgs : EventArgs
    {
        [NotNull]
        public WorktreeInfoResult Info { get; }

        [NotNull]
        public CancellationToken Token { get; }

        public WorktreeStatusEventArgs(WorktreeInfoResult info, CancellationToken token)
        {
            Info = info;
            Token = token;
        }
    }
}
