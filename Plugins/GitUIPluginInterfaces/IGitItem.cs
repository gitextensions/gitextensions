namespace GitUIPluginInterfaces
{
    public interface IGitItem
    {
        ObjectId ObjectId { get; }
        string Guid { get; }
        string Name { get; }
    }
}