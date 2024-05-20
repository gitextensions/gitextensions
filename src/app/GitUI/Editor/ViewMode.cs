namespace GitUI.Editor;

/// <summary>
/// The type of information currently shown in the file viewer.
/// </summary>
public enum ViewMode
{
    // Plain text
    Text,

    // Diff or patch
    Diff,

    // Diffs that will not be affected by diff arguments like white space etc (limited options)
    FixedDiff,

    // special difftool output
    Difftastic,

    // range-diff output
    RangeDiff,

    // Get if the given diff is a combined diff, with changes from all parents.
    // <see href="https://git-scm.com/docs/git-diff#_combined_diff_format"/> for more information.
    CombinedDiff,

    // output from git-grep
    Grep,

    // Image viewer
    Image
}
