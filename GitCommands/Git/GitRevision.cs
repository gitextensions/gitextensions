using System;
using System.Collections.Generic;

namespace GitCommands
{
    public class GitRevision : IGitItem
    {
        public String[] ParentGuids;
        private List<IGitItem> _subItems;

        public GitRevision()
        {
            Heads = new List<GitHead>();
        }

        public List<GitHead> Heads { get; set; }

        public string TreeGuid { get; set; }

        public string Author { get; set; }
        public DateTime AuthorDate { get; set; }
        public string Committer { get; set; }
        public DateTime CommitDate { get; set; }

        public string Message { get; set; }

        #region IGitItem Members

        public string Guid { get; set; }
        public string Name { get; set; }

        public List<IGitItem> SubItems
        {
            get { return _subItems ?? (_subItems = GitCommandHelpers.GetTree(TreeGuid)); }
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
            foreach (var gitHead in Heads)
            {
                if (gitHead.Name.ToLower().Contains(searchString))
                    return true;
            }

            if ((searchString.Length > 2) && Guid.StartsWith(searchString, StringComparison.CurrentCultureIgnoreCase))
                return true;


            return
                Author.StartsWith(searchString, StringComparison.CurrentCultureIgnoreCase) ||
                Message.ToLower().Contains(searchString);
        }
    }
}