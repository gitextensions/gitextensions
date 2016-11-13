using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using JetBrains.Annotations;

namespace GitCommands
{
    public sealed class GitRevision : IGitItem, INotifyPropertyChanged
    {
        /// <summary>40 characters of 0's</summary>
        public const string UnstagedGuid = "0000000000000000000000000000000000000000";
        /// <summary>40 characters of 1's</summary>
        public const string IndexGuid = "1111111111111111111111111111111111111111";
        /// <summary>40 characters of a-f or any digit.</summary>
        public const string Sha1HashPattern = @"[a-f\d]{40}";
        public static readonly Regex Sha1HashRegex = new Regex("^" + Sha1HashPattern + "$", RegexOptions.Compiled);

        public string[] ParentGuids;
        private IList<IGitItem> _subItems;
        private readonly List<IGitRef> _refs = new List<IGitRef>();
        private readonly GitModule _module;
        private BuildInfo _buildStatus;

        public GitRevision(GitModule aModule, string guid)
        {
            Guid = guid;
            Subject = "";
            _module = aModule;
        }

        public List<IGitRef> Refs { get { return _refs; } }

        public string TreeGuid { get; set; }

        public string Author { get; set; }
        public string AuthorEmail { get; set; }
        public DateTime AuthorDate { get; set; }
        public string Committer { get; set; }
        public string CommitterEmail { get; set; }
        public DateTime CommitDate { get; set; }

        public BuildInfo BuildStatus
        {
            get { return _buildStatus; }
            set
            {
                if (Equals(value, _buildStatus)) return;
                _buildStatus = value;
                OnPropertyChanged("BuildStatus");
            }
        }

        public string Subject { get; set; }
        public string Body { get; set; }
        //UTF-8 when is null or empty
        public string MessageEncoding { get; set; }

        #region IGitItem Members

        public string Guid { get; set; }
        public string Name { get; set; }

        public IEnumerable<IGitItem> SubItems
        {
            get { return _subItems ?? (_subItems = _module.GetTree(TreeGuid, false)); }
        }

        #endregion

        public override string ToString()
        {
            var sha = Guid;
            if (sha.Length > 8)
            {
                sha = sha.Substring(0, 4) + ".." + sha.Substring(sha.Length - 4, 4);
            }
            return String.Format("{0}:{1}", sha, Subject);
        }

        public bool MatchesSearchString(string searchString)
        {
            if (Refs.Any(gitHead => gitHead.Name.ToLower().Contains(searchString)))
                return true;

            if ((searchString.Length > 2) && Guid.StartsWith(searchString, StringComparison.CurrentCultureIgnoreCase))
                return true;

            return (Author != null && Author.StartsWith(searchString, StringComparison.CurrentCultureIgnoreCase)) ||
                    Subject.ToLower().Contains(searchString);
        }

        public bool IsArtificial()
        {
            return IsArtificial(Guid);
        }

        public static bool IsArtificial(string guid)
        {
            return guid == UnstagedGuid ||
                    guid == IndexGuid;
        }

        public bool HasParent()
        {
            return ParentGuids != null && ParentGuids.Length > 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
