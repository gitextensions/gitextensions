namespace GitUI;

public sealed class GitUICommandsSourceEventArgs : EventArgs
{
    public GitUICommandsSourceEventArgs(IGitUICommandsSource gitUiCommandsSource)
    {
        GitUICommandsSource = gitUiCommandsSource;
    }

    public IGitUICommandsSource GitUICommandsSource { get; }
}
