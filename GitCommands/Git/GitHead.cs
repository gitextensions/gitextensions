using System;
using System.Collections.Generic;
using GitCommands.Config;

namespace GitCommands
{
    public class GitHead : IGitItem
    {
        private readonly string _mergeSettingName;
        private readonly string _remoteSettingName;
        private IList<IGitItem> _subItems;
        public GitModule Module { get; private set; }

        public GitHead(GitModule module, string guid, string completeName) : this(module, guid, completeName, string.Empty) {}

        public GitHead(GitModule module, string guid, string completeName, string remote)
        {
            Module = module;
            Guid = guid;
            Selected = false;
            CompleteName = completeName;
            Remote = remote;
            IsTag = CompleteName.Contains("refs/tags/");
            IsHead = CompleteName.Contains("refs/heads/");
            IsRemote = CompleteName.Contains("refs/remotes/");
            IsBisect = CompleteName.Contains("refs/bisect/");

            ParseName();

            _remoteSettingName = String.Format("branch.{0}.remote", Name);
            _mergeSettingName = String.Format("branch.{0}.merge", Name);
        }

        public string CompleteName { get; private set; }
        public bool Selected { get; set; }
        public bool SelectedHeadMergeSource { get; set; }
        public bool IsTag { get; private set; }
        public bool IsHead { get; private set; }
        public bool IsRemote { get; private set; }
        public bool IsBisect { get; private set; }

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
                return GetTrackingRemote(Module.GetLocalConfig());    
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

        /// <summary>
        /// This method is a faster than the property above. The property reads the config file
        /// every time it is accessed. This method accepts a configfile what makes it faster when loading
        /// the revisiongraph.
        /// </summary>
        public string GetTrackingRemote(ConfigFile configFile)
        {
            return configFile.GetValue(_remoteSettingName);
        }

        public string MergeWith
        {
            get
            {
                return GetMergeWith(Module.GetLocalConfig());
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
        public string GetMergeWith(ConfigFile configFile)
        {
            string merge = configFile.GetValue(_mergeSettingName);
            return merge.StartsWith("refs/heads/") ? merge.Substring(11) : merge;
        }


        public static GitHead NoHead(GitModule module)
        {
            return new GitHead(module, null, "");
        }

        public static GitHead AllHeads(GitModule module)
        {
            return new GitHead(module, null, "*");
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
            if (CompleteName.Length == 0 || !CompleteName.Contains("/"))
            {
                Name = CompleteName;
                return;
            }
            if (IsRemote)
            {
                Name = CompleteName.Substring(CompleteName.LastIndexOf("remotes/") + 8);
                return;
            }
            if (IsTag)
            {
                // we need the one containing ^{}, because it contains the reference
                var temp =
                    CompleteName.Contains("^{}")
                        ? CompleteName.Substring(0, CompleteName.Length - 3)
                        : CompleteName;

                Name = temp.Substring(CompleteName.LastIndexOf("tags/") + 5);
                return;
            }
            if (IsHead)
            {
                Name = CompleteName.Substring(CompleteName.LastIndexOf("heads/") + 6);
                return;
            }
            Name = CompleteName.Substring(CompleteName.LastIndexOf("/") + 1);
        }
    }
}