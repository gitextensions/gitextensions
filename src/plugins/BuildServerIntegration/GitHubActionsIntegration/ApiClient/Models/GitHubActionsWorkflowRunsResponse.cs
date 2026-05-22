using System.Text.Json.Serialization;

namespace GitExtensions.Plugins.GitHubActionsIntegration.ApiClient.Models;

/// <summary>
/// Represents the paginated response from the GitHub Actions workflow runs endpoint.
/// </summary>
public class GitHubActionsWorkflowRunsResponse
{
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    [JsonPropertyName("workflow_runs")]
    public List<GitHubActionsWorkflowRun> WorkflowRuns { get; set; } = [];
}
