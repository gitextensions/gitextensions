namespace GitExtensions.Extensibility.Git;

public enum GitSettingLevel
{
    SystemWide,
    Global,
    Local,
    ////WorkTree omitted because "git config --worktree" does not work without additional configuration
    Effective
}
