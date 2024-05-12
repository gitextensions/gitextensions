using GitExtensions.Extensibility.Extensions;

namespace GitExtensions.Extensibility.Git;

public sealed class CommitData
{
    public CommitData(
        ObjectId objectId,
        IReadOnlyList<ObjectId>? parentIds,
        string author,
        DateTime authorDate,
        string committer,
        DateTime commitDate,
        string body)
    {
        ObjectId = objectId;
        ParentIds = parentIds;
        Author = author;
        AuthorDate = authorDate.ToDateTimeOffset();
        Committer = committer;
        CommitDate = commitDate.ToDateTimeOffset();
        Body = body;
    }

    public ObjectId ObjectId { get; }
    public IReadOnlyList<ObjectId>? ParentIds { get; }
    public string Author { get; }
    public DateTimeOffset AuthorDate { get; }
    public string Committer { get; }
    public DateTimeOffset CommitDate { get; }

    // TODO mutable properties need review

    public IReadOnlyList<ObjectId>? ChildIds { get; set; }

    /// <summary>
    /// Gets and sets the commit message.
    /// </summary>
    public string Body { get; set; }
}
