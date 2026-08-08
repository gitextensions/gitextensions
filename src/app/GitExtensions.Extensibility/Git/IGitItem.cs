namespace GitExtensions.Extensibility.Git;

public interface IGitItem
{
    /// <summary>
    /// Gets the object ID, or default/zero <see cref="ObjectId"/> if not known.
    /// </summary>
    ObjectId ObjectId { get; }

    string? Guid { get; }
}
