namespace GitExtensions.Extensibility.Git;

/// <summary>
/// Represents the head type of a git worktree.
/// </summary>
public enum GitWorktreeHeadType
{
    Branch,
    Detached,
    Bare
}

/// <summary>
/// Represents a git worktree as reported by <c>git worktree list --porcelain</c>.
/// </summary>
/// <param name="Path">The absolute path to the worktree directory.</param>
/// <param name="HeadType">Whether the worktree HEAD is on a branch, detached, or bare.</param>
/// <param name="Sha1">The HEAD commit SHA, or <see langword="null"/> for bare worktrees.</param>
/// <param name="Branch">The branch name (without <c>refs/heads/</c> prefix), or <see langword="null"/> if detached or bare.</param>
/// <param name="IsDeleted"><see langword="true"/> if the worktree directory no longer exists on disk.</param>
public sealed record GitWorktree(
    string Path,
    GitWorktreeHeadType HeadType,
    string? Sha1,
    string? Branch,
    bool IsDeleted)
{
    /// <summary>
    /// <see langword="true"/> for the main worktree, which <c>git worktree list</c> always reports
    /// first. Git refuses to remove it even with <c>--force</c>, because its directory holds the
    /// repository itself.
    /// </summary>
    public bool IsMain { get; init; }

    /// <summary>
    /// <see langword="true"/> if the worktree is locked by <c>git worktree lock</c>, which keeps
    /// <c>git worktree prune</c> and <c>git worktree remove</c> from removing it.
    /// </summary>
    public bool IsLocked { get; init; }

    /// <summary>
    /// The reason given to <c>git worktree lock --reason</c>, or <see langword="null"/> if none was given.
    /// </summary>
    public string? LockReason { get; init; }

    /// <summary>
    /// <see langword="true"/> if <c>git worktree prune</c> would remove the worktree, e.g. because its directory is missing.
    /// </summary>
    public bool IsPrunable { get; init; }

    /// <summary>
    /// Why git considers the worktree prunable, or <see langword="null"/> if it is not.
    /// </summary>
    public string? PrunableReason { get; init; }

    /// <summary>
    /// Formats a display name by combining <paramref name="name"/> with the branch, detached HEAD, or bare status.
    /// </summary>
    /// <param name="name">The name portion to display (e.g. directory name or relative path).</param>
    /// <returns>A string like <c>"my-worktree (main)"</c> or <c>"my-worktree (detached at abc1234)"</c>.</returns>
    public string GetDisplayName(string name)
    {
        if (HeadType is GitWorktreeHeadType.Bare)
        {
            return $"{name} (bare)";
        }

        if (HeadType is GitWorktreeHeadType.Detached)
        {
            string shortSha = Sha1?.Length >= 7 ? Sha1[..7] : Sha1 ?? "???";
            return $"{name} (detached at {shortSha})";
        }

        return Branch is not null
            ? $"{name} ({Branch})"
            : name;
    }
}
