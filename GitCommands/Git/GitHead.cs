using System;
using System.Collections.Generic;

namespace GitCommands
{
    public class GitHead : IGitItem
    {
        private readonly string _mergeSettingName;
        private readonly string _remoteSettingName;
        private List<IGitItem> _subItems;

        public GitHead(string guid, string completeName) : this(guid, completeName, string.Empty) {}

        public GitHead(string guid, string completeName, string remote)
        {
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
            get { return GitCommandHelpers.GetSetting(_remoteSettingName); }
            set
            {
                if (String.IsNullOrEmpty(value))
                    GitCommandHelpers.UnsetSetting(_remoteSettingName);
                else
                {
                    GitCommandHelpers.SetSetting(_remoteSettingName, value);

                    if (MergeWith == "")
                        MergeWith = Name;
                }
            }
        }

        public string MergeWith
        {
            get
            {
                var merge = GitCommandHelpers.GetSetting(_mergeSettingName);
                return merge.StartsWith("refs/heads/") ? merge.Substring(11) : merge;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    GitCommandHelpers.UnsetSetting(_mergeSettingName);
                else
                    GitCommandHelpers.SetSetting(_mergeSettingName, "refs/heads/" + value);
            }
        }

        public static GitHead NoHead
        {
            get { return new GitHead(null, ""); }
        }

        public static GitHead AllHeads
        {
            get { return new GitHead(null, "*"); }
        }

        #region IGitItem Members

        public string Guid { get; private set; }
        public string Name { get; private set; }

        public List<IGitItem> SubItems
        {
            get { return _subItems ?? (_subItems = GitCommandHelpers.GetTree(Guid)); }
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

                Name = temp.Substring(CompleteName.LastIndexOf("/") + 1);
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