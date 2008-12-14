using System;
using System.Collections.Generic;

using System.Text;

namespace GitCommands
{
    public class GitHead : IGitItem
    {
        public string Guid { get; set; }
        public string Name { get; set; }
        public string HeadType { get; set; }
        public bool Selected { get; set; }

        public GitHead()
        {
            Selected = false;
        }


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
