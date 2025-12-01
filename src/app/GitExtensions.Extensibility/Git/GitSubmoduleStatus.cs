namespace GitExtensions.Extensibility.Git;

public sealed class GitSubmoduleStatus
{
    private Func<string, CommitData?> GetCommitData { get; }
    private Func<GitSubmoduleStatus, SubmoduleStatus> GetSubmoduleStatus { get; }
    private SubmoduleStatus? _cachedStatus = null;

    public string Name { get; }
    public string? OldName { get; }
    public bool IsDirty { get; }
    public ObjectId? Commit { get; }
    public ObjectId? OldCommit { get; }
    public int? AddedCommits { get; }
    public int? RemovedCommits { get; }

    public SubmoduleStatus Status
    {
        get
        {
            if (_cachedStatus is not null)
            {
                return _cachedStatus.Value;
            }

            _cachedStatus = GetSubmoduleStatus is null ? SubmoduleStatus.Unknown : GetSubmoduleStatus(this);
            return _cachedStatus.Value;
        }
    }

    public void ResetSubmoduleStatus() => _cachedStatus = null;

    // Get CommitData without Notes (will cache contents)
    public CommitData? CommitData
        => GetCommitData is not null && Commit is not null ? GetCommitData(Commit.ToString()) : null;
    public CommitData? OldCommitData
        => GetCommitData is not null && OldCommit is not null ? GetCommitData(OldCommit.ToString()) : null;

    public GitSubmoduleStatus(string name, string? oldName, bool isDirty, ObjectId? commit, ObjectId? oldCommit, int? addedCommits, int? removedCommits, Func<string, CommitData?> getCommitData, Func<GitSubmoduleStatus, SubmoduleStatus> getSubmoduleStatus)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;
        OldName = oldName;
        IsDirty = isDirty;
        Commit = commit;
        OldCommit = oldCommit;
        AddedCommits = addedCommits;
        RemovedCommits = removedCommits;
        GetCommitData = getCommitData;
        GetSubmoduleStatus = getSubmoduleStatus;
    }

    public string AddedAndRemovedString()
    {
        if (RemovedCommits is null || AddedCommits is null ||
            (RemovedCommits == 0 && AddedCommits == 0))
        {
            return "";
        }

        // similar to GetCommitCountString
        return $" (+{AddedCommits}-{RemovedCommits})";
    }
}
