namespace GitExtensions.Extensibility.Git;

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
        ArgumentNullException.ThrowIfNull(nameof(name));
        Name = name;
        OldName = oldName;
        IsDirty = isDirty;
        Commit = commit;
        OldCommit = oldCommit;
        AddedCommits = addedCommits;
        RemovedCommits = removedCommits;
    }

    public IGitModule GetSubmodule(IGitModule module)
    {
        return module.GetSubmodule(Name);
    }

    public void CheckSubmoduleStatus(IGitModule? submodule)
    {
        if (submodule is null)
        {
            Status = SubmoduleStatus.NewSubmodule;
            return;
        }

        Status = submodule.CheckSubmoduleStatus(Commit, OldCommit, data: null, oldData: null, loadData: true);
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
