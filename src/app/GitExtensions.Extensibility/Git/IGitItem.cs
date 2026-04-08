namespace GitExtensions.Extensibility.Git;

public interface IGitItem
{
    /// <summary>
    /// Gets the object ID, or the zero <see cref="ObjectId"/> if not known.
    /// </summary>
    ObjectId ObjectId { get; }

    string? Guid { get; }
}
