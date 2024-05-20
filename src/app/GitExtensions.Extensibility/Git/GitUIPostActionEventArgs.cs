namespace GitExtensions.Extensibility.Git;

public class GitUIPostActionEventArgs : GitUIEventArgs
{
    public bool ActionDone { get; }

    public GitUIPostActionEventArgs(IWin32Window? ownerForm, IGitUICommands gitUICommands, bool actionDone)
        : base(ownerForm, gitUICommands)
    {
        ActionDone = actionDone;
    }
}
