
namespace GitCommands
{
    public class GitSubmoduleStatus
    {
        public GitSubmoduleStatus()
        {
            Status = SubmoduleStatus.Unknown; 
        }

        public string Name { get; set; }
        public string OldName { get; set; }
        public bool   IsDirty { get; set; }
        public string Commit { get; set; }
        public string OldCommit { get; set; }
        public SubmoduleStatus Status { get; set; }
        public int? AddedCommits { get; set; }
        public int? RemovedCommits { get; set; }
        
        public GitModule GetSubmodule(GitModule module)
        {
            return module.GetSubmodule(Name);
        }

        public void CheckSubmoduleStatus(GitModule submodule)
        {
            Status = SubmoduleStatus.NewSubmodule;
            if (submodule == null)
                return;

            Status = submodule.CheckSubmoduleStatus(Commit, OldCommit);
        }

        public CommitData GetCommitData(GitModule submodule)
        {
            if (submodule == null || !submodule.IsValidGitWorkingDir())
                return null;

            string error = "";
            return CommitData.GetCommitData(submodule, Commit, ref error);
        }

        public CommitData GetOldCommitData(GitModule submodule)
        {
            if (submodule == null || !submodule.IsValidGitWorkingDir())
                return null;

            string error = "";
            return CommitData.GetCommitData(submodule, OldCommit, ref error);
        }

        public string AddedAndRemovedString()
        {
            return "(" +
                ((RemovedCommits == null || RemovedCommits == 0) ? "" : ("-" + RemovedCommits)) +
                ((AddedCommits == null || AddedCommits == 0) ? "" : ("+" + AddedCommits)) +
                ")"; 
               
        }
    }

}
