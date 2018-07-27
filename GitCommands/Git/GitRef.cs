using System;
using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands
{
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
        private readonly string _mergeSettingName;
        private readonly string _remoteSettingName;
        private readonly GitRefType _type;

        public IGitModule Module { get; }

        public GitRef(IGitModule module, [CanBeNull] ObjectId objectId, string completeName, string remote = "")
        {
            Module = module;
            ObjectId = objectId;
            Guid = objectId?.ToString();
            CompleteName = completeName;
            Remote = remote;

            IsDereference = CompleteName.EndsWith(GitRefName.TagDereferenceSuffix);

            _type = GetType();

            var name = ParseName();

            Name = name.IsNullOrWhiteSpace() ? CompleteName : name;

            _remoteSettingName = $"branch.{Name}.remote";
            _mergeSettingName = $"branch.{Name}.merge";

            return;

            GitRefType GetType()
            {
                if (CompleteName.StartsWith(GitRefName.RefsTagsPrefix))
                {
                    return GitRefType.Tag;
                }

                if (CompleteName.StartsWith(GitRefName.RefsHeadsPrefix))
                {
                    return GitRefType.Head;
                }

                if (CompleteName.StartsWith(GitRefName.RefsRemotesPrefix))
                {
                    return GitRefType.Remote;
                }

                if (CompleteName.StartsWith(GitRefName.RefsBisectPrefix))
                {
                    if (CompleteName.StartsWith(GitRefName.RefsBisectGoodPrefix))
                    {
                        return GitRefType.BisectGood;
                    }

                    if (CompleteName.StartsWith(GitRefName.RefsBisectBadPrefix))
                    {
                        return GitRefType.BisectBad;
                    }

                    return GitRefType.Bisect;
                }

                if (CompleteName.StartsWith(GitRefName.RefsStashPrefix))
                {
                    return GitRefType.Stash;
                }

                return GitRefType.Other;
            }

            string ParseName()
            {
                if (IsRemote)
                {
                    return CompleteName.SubstringAfterLast("remotes/");
                }

                if (IsTag)
                {
                    // we need the one containing ^{}, because it contains the reference
                    return CompleteName.RemoveSuffix(GitRefName.TagDereferenceSuffix).SubstringAfterLast("tags/");
                }

                if (IsHead)
                {
                    return CompleteName.SubstringAfterLast("heads/");
                }

                // if we don't know ref type then we don't know if '/' is a valid ref character
                return CompleteName.SubstringAfter("refs/");
            }
        }

        public string CompleteName { get; }
        public bool IsSelected { get; set; } = false;
        public bool IsSelectedHeadMergeSource { get; set; }

        public bool IsTag => _type == GitRefType.Tag;
        public bool IsHead => _type == GitRefType.Head;
        public bool IsRemote => _type == GitRefType.Remote;
        public bool IsBisect => _type == GitRefType.Bisect;
        public bool IsBisectGood => _type == GitRefType.BisectGood;
        public bool IsBisectBad => _type == GitRefType.BisectBad;
        public bool IsStash => _type == GitRefType.Stash;

        public bool IsDereference { get; }

        public string LocalName => IsRemote ? Name.Substring(Remote.Length + 1) : Name;

        public string Remote { get; }

        public string TrackingRemote
        {
            get => GetTrackingRemote(Module.LocalConfigFile);
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Module.UnsetSetting(_remoteSettingName);
                }
                else
                {
                    Module.SetSetting(_remoteSettingName, value);

                    if (MergeWith == "")
                    {
                        MergeWith = Name;
                    }
                }
            }
        }

        /// <inheritdoc />
        public string GetTrackingRemote(ISettingsValueGetter configFile)
        {
            return configFile.GetValue(_remoteSettingName);
        }

        /// <inheritdoc />
        public string MergeWith
        {
            get => GetMergeWith(Module.LocalConfigFile);
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Module.UnsetSetting(_mergeSettingName);
                }
                else
                {
                    Module.SetSetting(_mergeSettingName, GitRefName.GetFullBranchName(value));
                }
            }
        }

        /// <inheritdoc />
        public string GetMergeWith(ISettingsValueGetter configFile)
        {
            return configFile.GetValue(_mergeSettingName).RemovePrefix(GitRefName.RefsHeadsPrefix);
        }

        public static GitRef NoHead(GitModule module)
        {
            return new GitRef(module, null, "");
        }

        #region IGitItem Members

        [CanBeNull]
        public ObjectId ObjectId { get; }
        [CanBeNull]
        public string Guid { get; }
        public string Name { get; }

        #endregion

        public override string ToString() => CompleteName;

        public static IReadOnlyCollection<string> GetAmbiguousRefNames(IEnumerable<IGitRef> refs)
        {
            return refs
                .GroupBy(r => r.Name)
                .Where(group => group.Count() > 1)
                .ToHashSet(e => e.Key);
        }
    }
}
