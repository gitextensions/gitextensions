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
    bool IsDeleted);
