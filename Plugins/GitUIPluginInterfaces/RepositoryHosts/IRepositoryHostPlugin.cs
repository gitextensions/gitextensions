using System.Collections.Generic;

namespace GitUIPluginInterfaces.RepositoryHosts
{
    public interface IRepositoryHostPlugin : IGitPlugin
    {
        IList<IHostedRepository> SearchForRepository(string search);
        IList<IHostedRepository> GetRepositoriesOfUser(string user);
        IHostedRepository GetRepository(string user, string repositoryName);

        IList<IHostedRepository> GetMyRepos();

        bool ConfigurationOk { get; }

        bool GitModuleIsRelevantToMe(IGitModule aModule);
        List<IHostedRemote> GetHostedRemotesForModule(IGitModule aModule);
    }
}
