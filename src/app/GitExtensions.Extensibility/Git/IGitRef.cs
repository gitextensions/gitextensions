namespace GitExtensions.Extensibility.Git;

public interface IGitRef : INamedGitItem
{
    /// <summary>
    ///  The complete Git reference name, including prefix <c>refs/</c>.
    /// </summary>
    string CompleteName { get; }

    /// <summary>
    ///  Normally the same as <see cref="INamedGitItem.Name"/>.
    ///  <see cref="IsRemote"/>: <see cref="Remote"/> is dropped.
    /// </summary>
    string LocalName { get; }

    /// <summary>
    ///  <see cref="IsHead"/>: The local name of the branch tracked on the remote.
    ///  <see cref="IsRemote"/>: For `VirtualRef` (like for related remote labels
    ///  in the grid) only, the full local branch tracking this remote ref.
    /// </summary>
    string MergeWith { get; set; }

    /// <summary>
    ///  <see cref="IsRemote"/>: The name of the remote.
    /// </summary>
    string Remote { get; }

    /// <summary>
    ///  <see cref="IsHead"/>: The name of the remote this local branch tracks.
    /// </summary>
    string TrackingRemote { get; set; }

    /// <summary>
    ///  Indicates whether the ref is a local branch, with ref prefix <c>refs/heads/</c>.
    /// </summary>
    bool IsHead { get; }

    /// <summary>
    ///  Indicates whether the ref is a remote branch, with ref prefix <c>refs/remotes/</c>.
    /// </summary>
    bool IsRemote { get; }

    /// <summary>
    ///  Indicates whether the ref is a tag, with ref prefix <c>refs/tags/</c>.
    /// </summary>
    bool IsTag { get; }

    /// <summary>
    ///  Indicates whether the ref refers to a `tag` object (with tagger info),
    ///  that references a `commit` object where the tag is set.
    ///  Used for annotated tags.
    ///  Note that lightweight tags directly refers to `commit` objects.
    /// </summary>
    bool IsDereference { get; }

    bool IsStash { get; }

    /// <summary>
    ///  For local branches: Indicates that this ref is the currently checked out local branch.
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    ///  <see cref="IsRemote"/>: Indicates that this ref is the tracked remote reference
    ///  for the currently <see cref="IsSelected"/>.
    /// </summary>
    bool IsSelectedHeadMergeSource { get; set; }

    bool IsBisect { get; }
    bool IsBisectGood { get; }
    bool IsBisectBad { get; }

    IGitModule Module { get; }

    /// <summary>
    ///  Return if the current <c>GitRef</c> is tracking the remote <c>GitRef</c>.
    /// </summary>
    /// <param name="remote">the expected remote ref tracked</param>
    /// <returns>
    ///  true if the current ref is tracking the expected remote ref false otherwise.
    /// </returns>
    bool IsTrackingRemote(IGitRef? remote)
        => remote is not null && IsHead && remote.IsRemote
            && TrackingRemote == remote.Remote && MergeWith == remote.LocalName;
}
