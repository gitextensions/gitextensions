﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitCommands
{
    public sealed class GitRevision : IGitItem
    {
        public const string UnstagedGuid = "0000000000000000000000000000000000000000";
        public const string IndexGuid = "1111111111111111111111111111111111111111";
        public const string Sha1HashPattern = @"[a-f\d]{40}";
        public static readonly Regex Sha1HashRegex = new Regex("^" + GitRevision.Sha1HashPattern + "$", RegexOptions.Compiled);


        public String[] ParentGuids;
        private IList<IGitItem> _subItems;
        private readonly List<GitHead> heads = new List<GitHead>();
        private readonly GitModule Module;

        public GitRevision(GitModule aModule, string guid)
        {
            Guid = guid;
            Message = "";
            Module = aModule;
        }

        public List<GitHead> Heads { get { return heads; } }

        public string TreeGuid { get; set; }

        public string Author { get; set; }
        public string AuthorEmail { get; set; }
        public DateTime AuthorDate { get; set; }
        public string Committer { get; set; }
        public string CommitterEmail { get; set; }
        public DateTime CommitDate { get; set; }

        public string Message { get; set; }
        //UTF-8 when is null or empty
        public string MessageEncoding { get; set; }

        #region IGitItem Members

        public string Guid { get; set; }
        public string Name { get; set; }

        public IEnumerable<IGitItem> SubItems
        {
            get { return _subItems ?? (_subItems = Module.GetTree(TreeGuid, false)); }
        }

        #endregion

        public override string ToString()
        {
            var sha = Guid;
            if (sha.Length > 8)
            {
                sha = sha.Substring(0, 4) + ".." + sha.Substring(sha.Length - 4, 4);
            }
            return String.Format("{0}:{1}", sha, Message);
        }

        public bool MatchesSearchString(string searchString)
        {
            if (Heads.Any(gitHead => gitHead.Name.ToLower().Contains(searchString)))
                return true;

            if ((searchString.Length > 2) && Guid.StartsWith(searchString, StringComparison.CurrentCultureIgnoreCase))
                return true;

            return
                (Author != null && Author.StartsWith(searchString, StringComparison.CurrentCultureIgnoreCase)) ||
                Message.ToLower().Contains(searchString);
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

    }
}