using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCommands
{

    public class GitSubmoduleStatus
    {
        public GitSubmoduleStatus()
        {
            IsCommitNewer = true; 
        }

        public string Name { get; set; }
        public string OldName { get; set; }
        public bool   IsDirty { get; set; }
        public string Commit { get; set; }
        public string OldCommit { get; set; }
        public bool   IsCommitNewer { get; set; }
        
        public GitModule GetSubmodule(GitModule module)
        {
            return module.GetSubmodule(Name);
        }

        public void CheckIsCommitNewer(GitModule submodule)
        {
            if (submodule == null || !submodule.ValidWorkingDir())
                return;

            string baseCommit = submodule.GetMergeBase(Commit, OldCommit);
            IsCommitNewer = baseCommit == OldCommit;
        }

        public CommitData GetCommitData(GitModule submodule)
        {
            if (submodule == null || !submodule.ValidWorkingDir())
                return null;

            string error = "";
            return CommitData.GetCommitData(submodule, Commit, ref error);
        }

        public CommitData GetOldCommitData(GitModule submodule)
        {
            if (submodule == null || !submodule.ValidWorkingDir())
                return null;

            string error = "";
            return CommitData.GetCommitData(submodule, OldCommit, ref error);
        }            
    }

}
