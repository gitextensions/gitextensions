namespace GitCommands.Git.Tag
{
    public enum TagOperation
    {
        Lightweight = 0,
        Annotate,
        SignWithDefaultKey,
        SignWithSpecificKey
    }
}