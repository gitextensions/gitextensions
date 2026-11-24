using GitCommands.UserRepositoryHistory;

namespace GitExtensionsTests;

// parity-scaffolding: value data for isolated menu and capture fixtures; production uses the original menu producer.
internal sealed record RepositoryHistoryEntry(
    Repository Repository,
    string Caption,
    string? BranchName,
    bool IsFavourite,
    bool IsAnchored);

// parity-scaffolding: preserves explicit fixture group boundaries without adding a product snapshot API.
internal sealed record RepositoryHistorySnapshot(
    IReadOnlyList<RepositoryHistoryEntry> Recent,
    IReadOnlyList<RepositoryHistoryEntry> Favourites)
{
    /// <summary>
    ///  Gets the number of recent entries supplied by the splitter's top group.
    /// </summary>
    public int TopCount { get; init; }
}
