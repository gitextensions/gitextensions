using System.Diagnostics.CodeAnalysis;
using GitCommands.Config;
using GitExtensions.Extensibility.Git;

namespace GitCommands;

internal enum GitRefType
{
    Other,
    Head,
    Remote,
    Tag,
    Bisect,
    BisectGood,
    BisectBad,
    Stash
}

public sealed class GitRef : IGitRef
{
    private readonly GitRefType _type;
    private string? _localName;
    private string? _mergeWith;
    private string? _trackingRemote;

    public IGitModule Module { get; }

    public GitRef(IGitModule module, ObjectId objectId, string completeName, string remote = "")
    {
        Module = module;
        ObjectId = objectId;
        CompleteName = completeName;
        Remote = remote;
        IsDereference = CompleteName.EndsWith(GitRefName.TagDereferenceSuffix);
        _type = DetermineType(completeName);
        Name = ParseName(completeName, _type, IsDereference);
    }

    public string CompleteName { get; }

    public string Name { get; }

    public string LocalName => _localName ??= ComputeLocalName(IsRemote, Remote, Name);

    [AllowNull]
    public string MergeWith
    {
        get => _mergeWith ??= IsHead ? Module.GetEffectiveSetting(string.Format(SettingKeyString.BranchMerge, LocalName)).RemovePrefix(GitRefName.RefsHeadsPrefix) : "";
        set
        {
            if (!IsHead)
            {
                throw new InvalidOperationException("MergeWith can only be set for local branches.");
            }

            string settingName = string.Format(SettingKeyString.BranchMerge, LocalName);
            if (string.IsNullOrEmpty(value))
            {
                Module.UnsetSetting(settingName);
                _mergeWith = "";
            }
            else
            {
                Module.SetSetting(settingName, GitRefName.GetFullBranchName(value));
                _mergeWith = value;
            }
        }
    }

    public string Remote { get; }

    [AllowNull]
    public string TrackingRemote
    {
        get => _trackingRemote ??= IsHead ? Module.GetEffectiveSetting(string.Format(SettingKeyString.BranchRemote, LocalName)) : "";
        set
        {
            if (!IsHead)
            {
                throw new InvalidOperationException("Tracking remote can only be set for local branches.");
            }

            string settingName = string.Format(SettingKeyString.BranchRemote, LocalName);
            if (string.IsNullOrEmpty(value))
            {
                Module.UnsetSetting(settingName);
                _trackingRemote = "";
            }
            else
            {
                Module.SetSetting(settingName, value);
                _trackingRemote = value;

                if (MergeWith == "")
                {
                    MergeWith = LocalName;
                }
            }
        }
    }

    public bool IsHead => _type == GitRefType.Head;
    public bool IsRemote => _type == GitRefType.Remote;
    public bool IsTag => _type == GitRefType.Tag;
    public bool IsStash => _type == GitRefType.Stash;
    public bool IsDereference { get; }
    public bool IsSelected { get; set; }
    public bool IsSelectedHeadMergeSource { get; set; }

    public bool IsBisect => _type == GitRefType.Bisect;
    public bool IsBisectGood => _type == GitRefType.BisectGood;
    public bool IsBisectBad => _type == GitRefType.BisectBad;

    public static GitRef NoHead(IGitModule module)
    {
        return new GitRef(module, default, "");
    }

    #region IGitItem Members

    public ObjectId ObjectId { get; }

    public string? Guid => ObjectId.IsZero ? null : ObjectId.ToString();
    public bool IsTrackingRemote(IGitRef? remote)
    => remote is not null && IsHead && remote.IsRemote
        && MergeWith == remote.LocalName && TrackingRemote == remote.Remote;

    #endregion

    public override string ToString() => CompleteName;

    public static IReadOnlyCollection<string> GetAmbiguousRefNames(IEnumerable<IGitRef> refs)
    {
        HashSet<string> seen = [];
        HashSet<string> ambiguous = [];

        foreach (IGitRef r in refs)
        {
            if (!seen.Add(r.Name))
            {
                ambiguous.Add(r.Name);
            }
        }

        return ambiguous;
    }

    internal static GitRefType DetermineType(string completeName)
    {
        ReadOnlySpan<char> span = completeName.AsSpan();

        if (span.StartsWith(GitRefName.RefsTagsPrefix))
        {
            return GitRefType.Tag;
        }

        if (span.StartsWith(GitRefName.RefsHeadsPrefix))
        {
            return GitRefType.Head;
        }

        if (span.StartsWith(GitRefName.RefsRemotesPrefix))
        {
            return GitRefType.Remote;
        }

        if (span.StartsWith(GitRefName.RefsBisectPrefix))
        {
            if (span.StartsWith(GitRefName.RefsBisectGoodPrefix))
            {
                return GitRefType.BisectGood;
            }

            if (span.StartsWith(GitRefName.RefsBisectBadPrefix))
            {
                return GitRefType.BisectBad;
            }

            return GitRefType.Bisect;
        }

        if (span.StartsWith(GitRefName.RefsStashPrefix))
        {
            return GitRefType.Stash;
        }

        return GitRefType.Other;
    }

    /// <summary>
    ///  Computes <see cref="IGitRef.LocalName"/> for a ref,
    ///  given <see cref="IGitRef.Remote"/> and <see cref="INamedGitItem.Name"/>.
    /// </summary>
    public static string ComputeLocalName(bool isRemote, string remote, string name)
    {
        if (!isRemote || remote.Length == 0 || name.Length <= remote.Length || name[remote.Length] != '/'
            || !name.AsSpan().StartsWith(remote, StringComparison.Ordinal))
        {
            return name;
        }

        return name[(remote.Length + 1)..];
    }

    public static string ParseName(string completeName)
    {
        GitRefType type = DetermineType(completeName);
        bool isDereference = completeName.EndsWith(GitRefName.TagDereferenceSuffix);
        return ParseName(completeName, type, isDereference);
    }

    internal static string ParseName(string completeName, GitRefType type, bool isDereference)
    {
        // DetermineType already verified each prefix, so slice directly at the known
        // offset rather than repeating an IndexOf search via SubstringAfter.
        string name;

        if (type == GitRefType.Remote)
        {
            name = completeName[GitRefName.RefsRemotesPrefix.Length..];
        }
        else if (type == GitRefType.Tag)
        {
            // Strip the dereference suffix (if present) without an intermediate string
            // allocation by computing the end offset directly before slicing.
            int end = completeName.Length - (isDereference ? GitRefName.TagDereferenceSuffix.Length : 0);
            name = completeName[GitRefName.RefsTagsPrefix.Length..end];
        }
        else if (type == GitRefType.Head)
        {
            name = completeName[GitRefName.RefsHeadsPrefix.Length..];
        }
        else
        {
            // if we don't know ref type then we don't know if '/' is a valid ref character
            name = completeName.SubstringAfter("refs/");
        }

        return name.Length == 0 ? completeName : name;
    }
}
