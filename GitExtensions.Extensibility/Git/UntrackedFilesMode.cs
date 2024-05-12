namespace GitExtensions.Extensibility.Git;

/// <summary>Specifies whether to check untracked files/directories (e.g. via 'git status')</summary>
public enum UntrackedFilesMode
{
    /// <summary>
    ///  Default is <see cref="All"/>; when <see cref="UntrackedFilesMode"/> is NOT used,
    ///  'git status' uses <see cref="Normal"/>.
    /// </summary>
    Default = 0,

    /// <summary>
    ///  Show no untracked files.
    /// </summary>
    No,

    /// <summary>
    ///  Shows untracked files and directories.
    /// </summary>
    Normal,

    /// <summary>
    ///  Shows untracked files and directories, and individual files in untracked directories.
    /// </summary>
    All
}
