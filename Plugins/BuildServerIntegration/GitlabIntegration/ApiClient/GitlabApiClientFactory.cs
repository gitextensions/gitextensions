using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitExtensions.Plugins.GitlabIntegration.ApiClient
{
    public interface IGitlabApiClientFactory
    {
        IGitlabApiClient CreateGitlabApiClient(string instanceUrl, string apiToken, int projectId);
    }

    internal class GitlabApiClientFactory : IGitlabApiClientFactory
    {
        public IGitlabApiClient CreateGitlabApiClient(string instanceUrl, string apiToken, int projectId)
        {
            return new GitlabApiClient(instanceUrl, apiToken, projectId);
        }
    }
}
