using System;
using System.Collections.Generic;

using System.Text;

namespace GitCommands
{
    public class GitRevision : IGitItem
    {
        public GitRevision()
        {
            GraphLines = new List<string>();
            Heads = new List<GitHead>();
        }

        public string Guid { get; set; }
        public string Name { get; set; }
        public List<GitHead> Heads { get; set; }

        public string TreeGuid { get; set; }
        //public string parentGuid { get; set; }

        public List<String> ParentGuids = new List<string>();

        public string Date { get; set; }
        public string Author { get; set; }
        public string Committer { get; set; }

        public string Message { get; set; }
        public List<string> GraphLines { get; set; }

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
    }
}
