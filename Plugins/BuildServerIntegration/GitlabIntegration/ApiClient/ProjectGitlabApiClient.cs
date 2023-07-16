using GitExtensions.Plugins.GitlabIntegration.ApiClient.Models;

namespace GitExtensions.Plugins.GitlabIntegration.ApiClient
{
    internal class ProjectGitlabApiClient : GitlabApiClient
    {
        private readonly int _projectId;

        public ProjectGitlabApiClient(string instanceUrl, string apiToken, int projectId)
            : base(instanceUrl, apiToken)
        {
            _projectId = projectId;
        }

        public async Task<PagedResponse<GitlabPipeline>> GetPipelinesAsync(DateTime? sinceDate, bool running, int pageNumber, int pageSize)
        {
            Uri pipelinesUri = new($"api/v4/projects/{_projectId}/pipelines", UriKind.Relative);

            return await LoadListAsync<GitlabPipeline>(pipelinesUri);
        }
    }
}
