using System;
using System.Collections.Generic;
using System.Text;

namespace GitUIPluginInterfaces.RepositoryHosts
{
    public interface IRepositoryHostPlugin : IGitPlugin
    {
        IList<IHostedRepository> SearchForRepository(string search);
        IList<IHostedRepository> GetRepositoriesOfUser(string user);
        IHostedRepository GetRepository(string user, string repositoryName);

        IList<IHostedRepository> GetMyRepos();

        bool ConfigurationOk { get; }

        bool CurrentWorkingDirRepoIsRelevantToMe { get; }
        List<IHostedRemote> GetHostedRemotesForCurrentWorkingDirRepo();
    }
}
