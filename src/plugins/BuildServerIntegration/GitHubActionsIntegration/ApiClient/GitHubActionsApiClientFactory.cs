namespace GitExtensions.Plugins.GitHubActionsIntegration.ApiClient;

public interface IGitHubActionsApiClientFactory
{
    IGitHubActionsApiClient CreateApiClient(string apiUrl, string owner, string repository, string? apiToken);
}

internal sealed class GitHubActionsApiClientFactory : IGitHubActionsApiClientFactory
{
    public IGitHubActionsApiClient CreateApiClient(string apiUrl, string owner, string repository, string? apiToken)
    {
        return new GitHubActionsApiClient(apiUrl, owner, repository, apiToken);
    }
}
