using System;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public class GitStatusMonitorStateEventArgs : EventArgs
    {
        public GitStatusMonitorStateEventArgs(GitStatusMonitorState state)
        {
            State = state;
        }

        public GitStatusMonitorState State { get; }
    }
}