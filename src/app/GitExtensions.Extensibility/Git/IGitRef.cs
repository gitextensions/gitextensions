using GitExtensions.Extensibility.Settings;

namespace GitExtensions.Extensibility.Git;

public interface IGitRef : INamedGitItem
{
    string CompleteName { get; }
    bool IsBisect { get; }
    bool IsBisectGood { get; }
    bool IsBisectBad { get; }
    bool IsStash { get; }

    /// <summary>
    ///  Indicates whether it is a checksum of an object (e.g., commit) to which another object
    ///  with <c>name</c> (e.g. annotated tag) is applied.
    /// </summary>
    /// <value>
    ///  <see langword="true"/> if it is a checksum of an object to which another object with <c>name</c> is applied;
    ///  <see langword="false"/> when <c>name</c> and <c>guid</c> are denoting the same object.
    /// </value>
    bool IsDereference { get; }

    /// <summary>
    ///  Indicates whether the ref is a local, i.e., it is a <c>refs/heads/xyz</c>.
    /// </summary>
    bool IsHead { get; }

    /// <summary>
    ///  Indicates whether the ref is a remote, i.e., it is a <c>refs/remotes/origin/xyz</c>.
    /// </summary>
    bool IsRemote { get; }

    /// <summary>
    ///  Indicates whether the ref is a tag, i.e., it is a <c>refs/tags/xyz</c>.
    /// </summary>
    bool IsTag { get; }

    string LocalName { get; }
    string MergeWith { get; set; }
    IGitModule Module { get; }
    string Remote { get; }
    string TrackingRemote { get; set; }
    bool IsSelected { get; set; }
    bool IsSelectedHeadMergeSource { get; set; }

    /// <summary>
    /// This method is a faster than the property above. The property reads the config file
    /// every time it is accessed. This method accepts a config file what makes it faster when loading
    /// the revision graph.
    /// </summary>
    string GetTrackingRemote(ISettingsValueGetter configFile);

    /// <summary>
    /// This method is a faster than the property above. The property reads the config file
    /// every time it is accessed. This method accepts a config file which makes it faster when loading
    /// the revision graph.
    /// </summary>
    string GetMergeWith(ISettingsValueGetter configFile);

    /// <summary>
    /// Return if the current `GitRef` is tracking another `GitRef` as a remote.
    /// </summary>
    /// <param name="remote">the expected remote ref tracked</param>
    /// <returns>true if the current ref is tracking the expected remote ref
    /// false otherwise</returns>
    bool IsTrackingRemote(IGitRef? remote);
}
