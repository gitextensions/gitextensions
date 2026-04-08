using GitExtensions.Plugins.GitHubActionsIntegration.ApiClient.Models;

namespace GitExtensions.Plugins.GitHubActionsIntegration.ApiClient;

public interface IGitHubActionsApiClient : IDisposable
{
    /// <summary>
    /// Gets the unique base URL used for this client instance.
    /// </summary>
    string BaseUrl { get; }

    /// <summary>
    /// Fetches workflow runs for the repository, optionally filtering by status and date.
    /// </summary>
    Task<GitHubActionsWorkflowRunsResponse> GetWorkflowRunsAsync(
        bool running,
        DateTime? sinceDate,
        int page,
        int perPage,
        CancellationToken cancellationToken);
}
