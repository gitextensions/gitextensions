namespace GitExtensions.Extensibility.Git;

public interface IGitRef : INamedGitItem
{
    /// <summary>
    ///  Display name of the ref.
    ///  Deviates from <see cref="INamedGitItem.Name"/>: the prefix <c>refs/*/</c>,
    ///  is stripped.
    ///  See <see cref="CompleteName"/> for the full ref path.
    /// </summary>
    new string Name { get; }

    /// <summary>
    ///  The complete Git reference name, including prefix <c>refs/</c>.
    /// </summary>
    string CompleteName { get; }

    /// <summary>
    ///  The name of the reference in a local repo.
    ///  The same as <see cref="Name"/>, except for <see cref="IsRemote"/>:
    ///  <see cref="Remote"/> is dropped.
    /// </summary>
    string LocalName { get; }

    /// <summary>
    ///  The tracked local branch name as on the remote for <see cref="IsHead"/>,
    ///  expanded use for <see cref="IsRemote"/> in <c>NestledRef</c>.
    /// </summary>
    string MergeWith { get; set; }

    /// <summary>
    ///  <see cref="IsRemote"/>: The name of the remote.
    /// </summary>
    string Remote { get; }

    /// <summary>
    ///  <see cref="IsHead"/>: The name of the remote this local branch tracks.
    ///  If this is set, then <see cref="MergeWith"/> is set too.
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
    ///  Indicates whether the ref refers to a <c>tag</c> object (with tagger info),
    ///  that references a <c>commit</c> object where the tag is set.
    ///  Used for annotated/signed tags.
    ///  Note that lightweight tags directly refer to <c>commit</c> objects.
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
