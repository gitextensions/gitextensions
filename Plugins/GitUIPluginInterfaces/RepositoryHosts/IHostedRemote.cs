namespace GitUIPluginInterfaces.RepositoryHosts
{
    public interface IHostedRemote
    {
        IHostedRepository GetHostedRepository();

        string Name { get; } // This is the name of the remote in the local git repository. This might be null
        string Data { get; }
        string DisplayData { get; }

        bool IsOwnedByMe { get; }
    }
}
