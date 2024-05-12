namespace GitExtensions.Extensibility.Git;

public class GitRemoteCommandCompletedEventArgs : EventArgs
{
    public IGitRemoteCommand Command { get; }

    public bool IsError { get; }

    public bool Handled { get; }

    public GitRemoteCommandCompletedEventArgs(IGitRemoteCommand command, bool isError, bool handled)
    {
        Command = command;
        IsError = isError;
        Handled = handled;
    }
}
