using System;
using System.Collections.Generic;

using System.Text;

namespace GitCommands
{
    public class GitRevision : IGitItem
    {
        public GitRevision()
        {
            Heads = new List<GitHead>();
        }

        public string Guid { get; set; }
        public string Name { get; set; }
        public List<GitHead> Heads { get; set; }

        public string TreeGuid { get; set; }

        public String[] ParentGuids;

        public string Author { get; set; }
        public DateTime AuthorDate { get; set; }
        public string Committer { get; set; }
        public DateTime CommitDate { get; set; }

        public string Message { get; set; }

        protected List<IGitItem> subItems;
        public List<IGitItem> SubItems 
        {
            get
            {
                if (subItems == null)
                {
                    subItems = GitCommands.GetTree(TreeGuid);
                }

                return subItems;
            }
        }

        public override string ToString()
        {
            string sha = Guid;
            if (sha.Length > 8)
            {
                sha = sha.Substring(0, 4) + ".." + sha.Substring(sha.Length - 4, 4);
            }
            return string.Format("{0}:{1}", sha, Message);
        }
    }
}
