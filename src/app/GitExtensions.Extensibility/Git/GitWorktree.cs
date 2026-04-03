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
