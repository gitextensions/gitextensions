namespace GitExtensions.Core.Module
{
    public interface IGitItem
    {
        ObjectId ObjectId { get; }
        string Guid { get; }
        string Name { get; }
    }
}