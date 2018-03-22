using System.Collections.Generic;

namespace GitUIPluginInterfaces.RepositoryHosts
{
    public interface IRepositoryHostPlugin : IGitPlugin
    {
        IReadOnlyList<IHostedRepository> SearchForRepository(string search);
        IReadOnlyList<IHostedRepository> GetRepositoriesOfUser(string user);
        IHostedRepository GetRepository(string user, string repositoryName);

        IReadOnlyList<IHostedRepository> GetMyRepos();

        bool ConfigurationOk { get; }

        bool GitModuleIsRelevantToMe(IGitModule module);
        IReadOnlyList<IHostedRemote> GetHostedRemotesForModule(IGitModule module);
    }
}
