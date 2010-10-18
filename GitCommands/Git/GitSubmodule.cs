using System;
using System.Collections.Generic;
using System.Text;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public class GitSubmodule : IGitSubmodule
    {
        public string Name { get; set; }
        public string RemotePath
        {
            get
            {
                return GitCommandHelpers.GetSubmoduleRemotePath(Name);
            }
            set
            {
            }
        }
        public string LocalPath
        {
            get
            {
                return GitCommandHelpers.GetSubmoduleLocalPath(Name);
            }
            set
            {
            }
        }
        

        public string CurrentCommitGuid { get; set; }
        public string Branch { get; set; }

        public bool Initialized { get; set; }
        public bool UpToDate { get; set; }

        public string Status
        {
            get
            {
                if (!Initialized)
                    return "Not initialized";
                if (!UpToDate)
                    return "Modified";

                return "Up-to-date";
            }
        }
    }
}
