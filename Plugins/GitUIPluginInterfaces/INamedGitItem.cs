namespace GitUIPluginInterfaces
{
    public interface INamedGitItem : IGitItem
    {
        string Name { get; }
    }
}
