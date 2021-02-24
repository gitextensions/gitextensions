using System;
using System.Threading;

namespace GitCommands.Worktrees
{
    public class WorktreeStatusEventArgs : EventArgs
    {
        public WorktreeInfoResult Info { get; }

        public CancellationToken Token { get; }

        public WorktreeStatusEventArgs(WorktreeInfoResult info, CancellationToken token)
        {
            Info = info;
            Token = token;
        }
    }
}
