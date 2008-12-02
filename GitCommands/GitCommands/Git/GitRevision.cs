using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCommands
{
    public class GitRevision : IGitItem
    {
        public string Guid { get; set; }
        public string Name { get; set; }

        public string TreeGuid { get; set; }
        //public string parentGuid { get; set; }

        public List<String> ParentGuids = new List<string>();

        public string Author { get; set; }
        public string Committer { get; set; }

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
    }
}
