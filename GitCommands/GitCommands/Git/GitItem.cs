using System;
using System.Collections.Generic;

using System.Text;

namespace GitCommands
{
    public class GitItem : IGitItem
    {
        public GitItem()
        {
        }

        public string Guid { get; set; }
        public string CommitGuid { get; set; }
        public string ItemType{ get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Date { get; set; }
        public string FileName { get; set; }
        public string Mode { get; set; }

        protected List<IGitItem> subItems;
        public List<IGitItem> SubItems
        {
            get
            {
                if (subItems == null)
                {
                    subItems = GitCommands.GetTree(Guid);

                    foreach (GitItem item in subItems)
                    {
                        item.FileName = FileName + "\\" + item.FileName;
                    }
                }

                return subItems;
            }
        }
    }
}
