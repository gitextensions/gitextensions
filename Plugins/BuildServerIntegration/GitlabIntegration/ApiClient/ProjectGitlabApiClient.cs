using System.Collections.Specialized;
using System.Web;
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
            UriBuilder pipelinesUriBuilder = new($"{InstanceUrl}/api/v4/projects/{_projectId}/pipelines");
            NameValueCollection? query = HttpUtility.ParseQueryString(pipelinesUriBuilder.Query);

            if (sinceDate != null)
            {
                query["updated_after"] = sinceDate.Value.ToString("u");
            }

            if (running)
            {
                query["scope"] = "running";
            }
            else
            {
                query["scope"] = "finished";
            }

            query["page"] = pageNumber.ToString();
            query["per_page"] = pageSize.ToString();

            pipelinesUriBuilder.Query = query.ToString() ?? string.Empty;

            return await LoadListAsync<GitlabPipeline>(pipelinesUriBuilder.Uri);
        }
    }
}
