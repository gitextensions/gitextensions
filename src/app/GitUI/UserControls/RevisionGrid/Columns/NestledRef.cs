using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;

namespace GitUI.UserControls.RevisionGrid.Columns;

public sealed class NestledRef(IGitRef gitRef, string completeName, bool trackingBranchIsGone) : IGitRef
{
    public string Name { get; } = GitRef.ParseName(completeName);

    /// <summary>
    ///  <see cref="ObjectId"/> of a nestled ref is always default/zero.
    /// </summary>
    public ObjectId ObjectId => default;
    public string? Guid => null;
    public string CompleteName => completeName;
    public string LocalName => GitRef.ComputeLocalName(IsRemote, Remote, Name);

    /// <summary>
    ///  The tracked local branch name as on the remote for <see cref="IsHead"/>,
    ///  or the tracking local branch for <see cref="IsRemote"/>.
    /// </summary>
    public string MergeWith
    {
        get => gitRef.LocalName;
        set => throw new NotSupportedException();
    }

    public string Remote => gitRef.TrackingRemote;
    public string TrackingRemote
    {
        get => gitRef.Remote;
        set => throw new NotSupportedException();
    }

    public bool TrackingBranchIsGone => trackingBranchIsGone;

    public bool IsHead => gitRef.IsRemote;
    public bool IsRemote => gitRef.IsHead;
    public bool IsTag => false;
    public bool IsStash => false;
    public bool IsDereference => false;
    public bool IsSelected { get; set; }
    public bool IsSelectedHeadMergeSource { get; set; }
    public bool IsBisect => false;
    public bool IsBisectGood => false;
    public bool IsBisectBad => false;

    public IGitModule Module => gitRef.Module;
    public bool IsTrackingRemote(IGitRef? remote)
    => remote is not null && IsHead && remote.IsRemote
        && MergeWith == remote.LocalName && TrackingRemote == remote.Remote;

    public override bool Equals(object? obj) => obj is NestledRef other && CompleteName == other.CompleteName;

    public override int GetHashCode() => completeName.GetHashCode();

    public override string ToString() => $"NestledRef: {CompleteName}";
}
