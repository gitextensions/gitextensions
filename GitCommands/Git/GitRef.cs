using System;
using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public class GitRef : IGitRef
    {
        private readonly string _mergeSettingName;
        private readonly string _remoteSettingName;
        private IList<IGitItem> _subItems;
       
        /// <summary>"refs/tags/"</summary>
        public static readonly string RefsTagsPrefix = "refs/tags/";
        /// <summary>"refs/heads/"</summary>
        public static readonly string RefsHeadsPrefix = "refs/heads/";
        /// <summary>"refs/remotes/"</summary>
        public static readonly string RefsRemotesPrefix = "refs/remotes/";
        /// <summary>"refs/bisect/"</summary>
        public static readonly string RefsBisectPrefix = "refs/bisect/";
        /// <summary>"^{}"</summary>
        public static readonly string TagDereferenceSuffix = "^{}";
       
        public IGitModule Module { get; private set; }

        public GitRef(IGitModule module, string guid, string completeName)
            : this(module, guid, completeName, string.Empty) { }

        public GitRef(IGitModule module, string guid, string completeName, string remote)
        {
            Module = module;
            Guid = guid;
            Selected = false;
            CompleteName = completeName;
            Remote = remote;
            IsTag = CompleteName.StartsWith(RefsTagsPrefix);
            IsDereference = CompleteName.EndsWith(TagDereferenceSuffix);
            IsHead = CompleteName.StartsWith(RefsHeadsPrefix);
            IsRemote = CompleteName.StartsWith(RefsRemotesPrefix);
            IsBisect = CompleteName.StartsWith(RefsBisectPrefix);

            ParseName();

            _remoteSettingName = RemoteSettingName(Name);
            _mergeSettingName = String.Format("branch.{0}.merge", Name);
        }

        public static GitRef CreateBranchRef(GitModule module, string guid, string name)
        {
            return new GitRef(module, guid, RefsHeadsPrefix + name);
        }

        public string CompleteName { get; private set; }
        public bool Selected { get; set; }
        public bool SelectedHeadMergeSource { get; set; }
        public bool IsTag { get; private set; }
        public bool IsHead { get; private set; }
        public bool IsRemote { get; private set; }
        public bool IsBisect { get; private set; }

        /// <summary>
        /// True when Guid is a checksum of an object (e.g. commit) to which another object 
        /// with Name (e.g. annotated tag) is applied. 
        /// <para>False when Name and Guid are denoting the same object.</para>
        /// </summary>
        public bool IsDereference { get; private set; }

        public bool IsOther
        {
            get { return !IsHead && !IsRemote && !IsTag; }
        }

        public string LocalName
        {
            get { return IsRemote ? Name.Substring(Remote.Length + 1) : Name; }
        }

        public string Remote { get; private set; }

        public string TrackingRemote
        {
            get
            {
                return GetTrackingRemote(Module.LocalConfigFile);
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    Module.UnsetSetting(_remoteSettingName);
                else
                {
                    Module.SetSetting(_remoteSettingName, value);

                    if (MergeWith == "")
                        MergeWith = Name;
                }
            }
        }

        /// <summary>Gets the setting name for a branch's remote.</summary>
        public static string RemoteSettingName(string branch)
        {
            return String.Format("branch.{0}.remote", branch);
        }

        /// <summary>
        /// This method is a faster than the property above. The property reads the config file
        /// every time it is accessed. This method accepts a config file what makes it faster when loading
        /// the revision graph.
        /// </summary>
        public string GetTrackingRemote(ISettingsValueGetter configFile)
        {
            return configFile.GetValue(_remoteSettingName);
        }

        public string MergeWith
        {
            get
            {
                return GetMergeWith(Module.LocalConfigFile);
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    Module.UnsetSetting(_mergeSettingName);
                else
                    Module.SetSetting(_mergeSettingName, GitCommandHelpers.GetFullBranchName(value));
            }
        }

        /// <summary>
        /// This method is a faster than the property above. The property reads the config file
        /// every time it is accessed. This method accepts a configfile what makes it faster when loading
        /// the revisiongraph.
        /// </summary>
        public string GetMergeWith(ISettingsValueGetter configFile)
        {
            string merge = configFile.GetValue(_mergeSettingName);
            return merge.StartsWith(RefsHeadsPrefix) ? merge.Substring(11) : merge;
        }


        public static GitRef NoHead(GitModule module)
        {
            return new GitRef(module, null, "");
        }

        #region IGitItem Members

        public string Guid { get; private set; }
        public string Name { get; private set; }

        public IEnumerable<IGitItem> SubItems
        {
            get { return _subItems ?? (_subItems = Module.GetTree(Guid, false)); }
        }

        #endregion

        public override string ToString()
        {
            return CompleteName;
        }

        private void ParseName()
        {
            if (IsRemote)
            {
                Name = CompleteName.Substring(CompleteName.LastIndexOf("remotes/") + 8);
            } 
            else if (IsTag)
            {
                // we need the one containing ^{}, because it contains the reference
                var temp =
                    CompleteName.Contains(TagDereferenceSuffix)
                        ? CompleteName.Substring(0, CompleteName.Length - TagDereferenceSuffix.Length)
                        : CompleteName;

                Name = temp.Substring(CompleteName.LastIndexOf("tags/") + 5);
            }
            else if (IsHead)
            {
                Name = CompleteName.Substring(CompleteName.LastIndexOf("heads/") + 6);
            }
            else
                //if we don't know ref type then we don't know if '/' is a valid ref character
                Name = CompleteName.SkipStr("refs/");
        }

        public static ISet<string> GetAmbiguousRefNames(IEnumerable<IGitRef> refs)
        {
            return refs.
                GroupBy(r => r.Name).
                Where(group => group.Count() > 1).
                ToHashSet(e => e.Key);
        }
    }
}
