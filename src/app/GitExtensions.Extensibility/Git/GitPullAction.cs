namespace GitExtensions.Extensibility.Git;

public enum GitPullAction
{
    None,
    Merge,
    Rebase,
    Fetch,
    FetchAll,
    FetchPruneAll,
    Default
}
