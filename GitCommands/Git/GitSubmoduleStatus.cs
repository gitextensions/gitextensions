namespace GitCommands
{
    public sealed class GitSubmoduleStatus
    {
        public string Name { get; set; }
        public string OldName { get; set; }
        public bool IsDirty { get; set; }
        public string Commit { get; set; }
        public string OldCommit { get; set; }
        public SubmoduleStatus Status { get; set; } = SubmoduleStatus.Unknown;
        public int? AddedCommits { get; set; }
        public int? RemovedCommits { get; set; }

        public GitModule GetSubmodule(GitModule module)
        {
            return module.GetSubmodule(Name);
        }

        public void CheckSubmoduleStatus(GitModule submodule)
        {
            if (submodule == null)
            {
                Status = SubmoduleStatus.NewSubmodule;
                return;
            }

            Status = submodule.CheckSubmoduleStatus(Commit, OldCommit);
        }

        public string AddedAndRemovedString()
        {
            if (RemovedCommits == null || AddedCommits == null ||
                (RemovedCommits == 0 && AddedCommits == 0))
            {
                return "";
            }

            return " (" +
                (RemovedCommits == 0 ? "" : "-" + RemovedCommits) +
                (AddedCommits == 0 ? "" : "+" + AddedCommits) +
                ")";
        }
    }
}
