namespace GitUI.CommandsDialogs.BrowseDialog
{
    public enum GitStatusMonitorState
    {
        // Not running
        Stopped = 0,

        // Normal operation
        Running,

        // Timer is running, not starting new commands
        Inactive,

        // Timer and file monitoring temporarily paused
        Paused
    }
}
