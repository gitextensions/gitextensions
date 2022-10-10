﻿using GitCommands.Git;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public sealed class GitSubmoduleStatus
    {
        public string Name { get; }
        public string? OldName { get; }
        public bool IsDirty { get; }
        public ObjectId? Commit { get; }
        public ObjectId? OldCommit { get; }
        public int? AddedCommits { get; }
        public int? RemovedCommits { get; }

        public SubmoduleStatus Status { get; set; } = SubmoduleStatus.Unknown;

        public GitSubmoduleStatus(string name, string? oldName, bool isDirty, ObjectId? commit, ObjectId? oldCommit, int? addedCommits, int? removedCommits)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            OldName = oldName;
            IsDirty = isDirty;
            Commit = commit;
            OldCommit = oldCommit;
            AddedCommits = addedCommits;
            RemovedCommits = removedCommits;
        }

        public GitModule GetSubmodule(GitModule module)
        {
            return module.GetSubmodule(Name);
        }

        public void CheckSubmoduleStatus(GitModule? submodule)
        {
            if (submodule is null)
            {
                Status = SubmoduleStatus.NewSubmodule;
                return;
            }

            Status = submodule.CheckSubmoduleStatus(Commit, OldCommit);
        }

        public string AddedAndRemovedString()
        {
            if (RemovedCommits is null || AddedCommits is null ||
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
