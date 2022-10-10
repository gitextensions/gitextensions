namespace GitUIPluginInterfaces.RepositoryHosts
{
    public interface IRepositoryHostPlugin : IGitPlugin
    {
        IReadOnlyList<IHostedRepository> SearchForRepository(string search);
        IReadOnlyList<IHostedRepository> GetRepositoriesOfUser(string user);
        IHostedRepository GetRepository(string user, string repositoryName);

        IReadOnlyList<IHostedRepository> GetMyRepos();

        void ConfigureContextMenu(ContextMenuStrip contextMenu);

        bool ConfigurationOk { get; }

        bool GitModuleIsRelevantToMe();
        IReadOnlyList<IHostedRemote> GetHostedRemotesForModule();
        string? OwnerLogin { get; }

        Task<string?> AddUpstreamRemoteAsync();
    }
}
