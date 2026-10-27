using GitCommands.Settings;

namespace GitCommands;

public static partial class AppSettings
{
    private static readonly SettingsPath AppearanceSettingsPath = new AppSettingsPath("Appearance");

    /// <summary>
    ///  Additional refs filters to be applied when listing refs for the revision grid in dependency on <see cref="ShowSessionRefs"/>.
    /// </summary>
    public static TextSetting SessionRefsFilters { get; } = new(AppearanceSettingsPath, nameof(SessionRefsFilters),
        defaultValue: "--exclude=refs/agents/** --exclude=refs/sessions/** --exclude=refs/*/checkpoints/** --exclude=refs/copilot/** --exclude=refs/jj/keep/**");

    /// <summary>
    ///  Gets or sets whether to remember the preference for diff appearance.
    /// </summary>
    public static BooleanSetting RememberDiffDisplayAppearance { get; } = new(AppearanceSettingsPath, nameof(RememberDiffDisplayAppearance), defaultValue: false);

    /// <summary>
    ///  Color the background at changes (invert colors).
    /// </summary>
    public static BooleanSetting ReverseGitColoring { get; } = new(AppearanceSettingsPath, nameof(ReverseGitColoring), defaultValue: true);

    /// <summary>
    ///  Controls whether the Find in Commit Files (git grep) box is shown.
    /// </summary>
    public static BooleanSetting ShowFindInCommitFilesGitGrep { get; } = new(AppearanceSettingsPath, nameof(ShowFindInCommitFilesGitGrep), defaultValue: false);

    /// <summary>
    ///  Controls whether the Git Notes column is shown.
    /// </summary>
    public static BooleanSetting ShowGitNotesColumn { get; } = new(AppearanceSettingsPath, nameof(ShowGitNotesColumn), defaultValue: false);

    /// <summary>
    ///  Controls whether tooltips are shown in the revision grid.
    /// </summary>
    public static BooleanSetting ShowRevisionGridTooltips { get; } = new(AppearanceSettingsPath, nameof(ShowRevisionGridTooltips), defaultValue: true);

    /// <summary>
    ///  Use Git coloring for selected commands.
    /// </summary>
    public static BooleanSetting UseGitColoring { get; } = new(AppearanceSettingsPath, nameof(UseGitColoring), defaultValue: true);
}
