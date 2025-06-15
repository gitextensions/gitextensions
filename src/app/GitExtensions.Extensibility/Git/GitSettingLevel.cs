namespace GitExtensions.Extensibility.Git;

/// <summary>
///  The scope of git config settings.
/// </summary>
public enum GitSettingLevel
{
    SystemWide,
    Global,
    Local,
    ////WorkTree is shown with Effective. Separate get/set is not imlemented as it is dynamic and requires explicit "extensions.worktreeConfig" setting.
    Effective
}
