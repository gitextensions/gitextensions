namespace GitUI.UserControls.RevisionGrid;

/// <summary>
///  Specifies the icon drawn inside a ref label in the revision grid.
/// </summary>
internal enum RefLabelIcon
{
    None,

    /// <summary>
    ///  Icon for the checked out branch.
    /// </summary>
    Head,

    /// <summary>
    ///  Icon for the remote-tracking branch of the checked out branch.
    /// </summary>
    HeadMergeSource,

    // The following icons are mapped to None for now.
    LocalBranch,
    Remote,
    Tag,
    Stash,
    WorkingDirectory,
    CommitIndex,
}
