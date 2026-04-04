using Newtonsoft.Json;

namespace GitExtensions.Plugins.GitHubActionsIntegration.ApiClient.Models;

/// <summary>
/// Represents the paginated response from the GitHub Actions workflow runs endpoint.
/// </summary>
public class GitHubActionsWorkflowRunsResponse
{
    [JsonProperty("total_count")]
    public int TotalCount { get; set; }

    [JsonProperty("workflow_runs")]
    public List<GitHubActionsWorkflowRun> WorkflowRuns { get; set; } = [];
}
