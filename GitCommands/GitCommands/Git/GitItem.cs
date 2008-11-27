using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCommands
{
    public class GitItem : IGitItem
    {
        public GitItem()
        {
        }

        public string Guid { get; set; }
        public string ItemType{ get; set; }
        public string Name { get; set; }
        public string Mode { get; set; }

        protected List<IGitItem> subItems;
        public List<IGitItem> SubItems
        {
            get
            {
                if (subItems == null)
                {
                    subItems = GitCommands.GetTree(Guid);
                }

                return subItems;
            }
        }
    }
}
