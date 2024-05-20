namespace GitExtensions.Extensibility.Git;

public interface IGitItem
{
    ObjectId? ObjectId { get; }

    string? Guid { get; }
}
