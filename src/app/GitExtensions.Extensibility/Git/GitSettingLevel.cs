namespace GitExtensions.Extensibility.Git;

/// <summary>
///  The scope of git config settings.
/// </summary>
public enum GitSettingLevel
{
    SystemWide,
    Global,
    Local,
    ////WorkTree omitted because "git config --worktree" does not work without additional configuration
    Effective
}
