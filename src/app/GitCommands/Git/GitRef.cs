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
        => completeName switch
        {
            _ when completeName.StartsWith(GitRefName.RefsHeadsPrefix, StringComparison.Ordinal) => GitRefType.Head,
            _ when completeName.StartsWith(GitRefName.RefsTagsPrefix, StringComparison.Ordinal) => GitRefType.Tag,
            _ when completeName.StartsWith(GitRefName.RefsRemotesPrefix, StringComparison.Ordinal) => GitRefType.Remote,
            _ when completeName.StartsWith(GitRefName.RefsStashPrefix, StringComparison.Ordinal) => GitRefType.Stash,
            _ when completeName.StartsWith(GitRefName.RefsBisectGoodPrefix, StringComparison.Ordinal) => GitRefType.BisectGood,
            _ when completeName.StartsWith(GitRefName.RefsBisectBadPrefix, StringComparison.Ordinal) => GitRefType.BisectBad,
            _ when completeName.StartsWith(GitRefName.RefsBisectPrefix, StringComparison.Ordinal) => GitRefType.Bisect,
            _ => GitRefType.Other
        };

    /// <summary>
    ///  Computes <see cref="IGitRef.LocalName"/> for a ref,
    ///  given <see cref="IGitRef.Remote"/> and <see cref="INamedGitItem.Name"/>.
    /// </summary>
    public static string ComputeLocalName(bool isRemote, string remote, string name)
    {
        if (!isRemote || remote.Length == 0 || name.Length <= remote.Length || name[remote.Length] != '/'
            || !name.StartsWith(remote, StringComparison.Ordinal))
        {
            return name;
        }

        return name[(remote.Length + 1)..];
    }

    public static string ParseName(string completeName)
    {
        GitRefType type = DetermineType(completeName);
        bool isDereference = type == GitRefType.Tag && completeName.EndsWith(GitRefName.TagDereferenceSuffix);
        return ParseName(completeName, type, isDereference);
    }

    internal static string ParseName(string completeName, GitRefType type, bool isDereference)
    {
        // DetermineType already verified each prefix, so slice directly at the known
        // offset rather than repeating an IndexOf search via SubstringAfter.
        string name = type switch
        {
            GitRefType.Head => completeName[GitRefName.RefsHeadsPrefix.Length..],
            GitRefType.Remote => completeName[GitRefName.RefsRemotesPrefix.Length..],
            GitRefType.Tag => completeName[GitRefName.RefsTagsPrefix.Length..(completeName.Length - (isDereference ? GitRefName.TagDereferenceSuffix.Length : 0))],
            _ => completeName.SubstringAfter("refs/")
        };

        return name.Length == 0 ? completeName : name;
    }
}
