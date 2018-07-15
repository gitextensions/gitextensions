using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands.Config;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands
{
    public sealed class GitRef : IGitRef
    {
        private readonly string _mergeSettingName;
        private readonly string _remoteSettingName;

        public IGitModule Module { get; }

        public GitRef(IGitModule module, [CanBeNull] ObjectId objectId, string completeName)
            : this(module, objectId, completeName, string.Empty)
        {
        }

        public GitRef(IGitModule module, [CanBeNull] ObjectId objectId, string completeName, string remote)
        {
            Module = module;
            ObjectId = objectId;
            Guid = objectId?.ToString();
            CompleteName = completeName;
            Remote = remote;

            IsTag = CompleteName.StartsWith(GitRefName.RefsTagsPrefix);
            IsDereference = CompleteName.EndsWith(GitRefName.TagDereferenceSuffix);
            IsHead = CompleteName.StartsWith(GitRefName.RefsHeadsPrefix);
            IsRemote = CompleteName.StartsWith(GitRefName.RefsRemotesPrefix);
            IsBisect = CompleteName.StartsWith(GitRefName.RefsBisectPrefix);
            IsBisectGood = CompleteName.StartsWith(GitRefName.RefsBisectGoodPrefix);
            IsBisectBad = CompleteName.StartsWith(GitRefName.RefsBisectBadPrefix);

            var name = ParseName();
            Name = name.IsNullOrWhiteSpace() ? CompleteName : name;

            _remoteSettingName = RemoteSettingName(Name);
            _mergeSettingName = $"branch.{Name}.merge";
        }

        public string CompleteName { get; }
        public bool IsSelected { get; set; } = false;
        public bool IsSelectedHeadMergeSource { get; set; }
        public bool IsTag { get; }
        public bool IsHead { get; }
        public bool IsRemote { get; }
        public bool IsBisect { get; }
        public bool IsBisectGood { get; }
        public bool IsBisectBad { get; }

        /// <summary>
        /// True when Guid is a checksum of an object (e.g. commit) to which another object
        /// with Name (e.g. annotated tag) is applied.
        /// <para>False when Name and Guid are denoting the same object.</para>
        /// </summary>
        public bool IsDereference { get; }

        public bool IsOther => !IsHead && !IsRemote && !IsTag;

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

        /// <summary>Gets the setting name for a branch's remote.</summary>
        public static string RemoteSettingName(string branch)
        {
            return string.Format(SettingKeyString.BranchRemote, branch);
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
            string merge = configFile.GetValue(_mergeSettingName);
            return merge.StartsWith(GitRefName.RefsHeadsPrefix) ? merge.Substring(11) : merge;
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

        [CanBeNull]
        private string ParseName()
        {
            if (IsRemote)
            {
                return CompleteName.Substring(CompleteName.LastIndexOf("remotes/") + 8);
            }

            if (IsTag)
            {
                // we need the one containing ^{}, because it contains the reference
                var temp =
                    CompleteName.Contains(GitRefName.TagDereferenceSuffix)
                        ? CompleteName.Substring(0, CompleteName.Length - GitRefName.TagDereferenceSuffix.Length)
                        : CompleteName;

                return temp.Substring(CompleteName.LastIndexOf("tags/") + 5);
            }

            if (IsHead)
            {
                return CompleteName.Substring(CompleteName.LastIndexOf("heads/") + 6);
            }

            // if we don't know ref type then we don't know if '/' is a valid ref character
            return CompleteName.SkipStr("refs/");
        }

        public static IReadOnlyCollection<string> GetAmbiguousRefNames(IEnumerable<IGitRef> refs)
        {
            return refs
                .GroupBy(r => r.Name)
                .Where(group => group.Count() > 1)
                .ToHashSet(e => e.Key);
        }
    }
}
