namespace GitCommands
{
    //Unsure if these should be in a sealed or static class.  I'll leave that up to you to decide.
    public enum PullAction
    {
        None,
        Merge,
        Rebase,
        Fetch,
        FetchAll,
        Default
    }
    public enum LocalChangesAction
    {
        DontChange,
        Merge,
        Reset,
        Stash
    }
}