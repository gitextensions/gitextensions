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

        public bool IsHead { get; set; }
        public bool IsTag { get; set; }
        public bool IsRemote { get; set; }
        public bool IsOther { get; set; }

        public string Remote 
        {
            get
            {
                return GitCommands.GetSetting("branch." + Name + ".remote");
            }
            set
            {
                GitCommands.SetSetting("branch." + Name + ".remote", value);
            }
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
