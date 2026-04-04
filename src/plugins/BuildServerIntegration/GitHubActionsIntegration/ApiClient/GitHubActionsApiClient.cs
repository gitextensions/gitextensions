using System.Net;
using System.Net.Http.Headers;
using GitExtensions.Plugins.GitHubActionsIntegration.ApiClient.Models;
using Microsoft;
using Newtonsoft.Json;

namespace GitExtensions.Plugins.GitHubActionsIntegration.ApiClient;

public sealed class GitHubActionsApiClient : IGitHubActionsApiClient
{
    private const int DefaultPerPage = 100;
    private readonly HttpClient _httpClient;
    private readonly string _repository;

    public GitHubActionsApiClient(string apiUrl, string owner, string repository, string? apiToken)
    {
        _repository = repository;

        string baseUrl = apiUrl.TrimEnd('/');

        BaseUrl = $"{baseUrl}/repos/{owner}/{_repository}";

        _httpClient = CreateHttpClient(baseUrl, apiToken);
    }

    public string BaseUrl { get; }

    public async Task<GitHubActionsWorkflowRunsResponse> GetWorkflowRunsAsync(
        bool running,
        DateTime? sinceDate,
        int page,
        int perPage,
        CancellationToken cancellationToken)
    {
        List<string> queryParams =
        [
            $"page={page}",
            $"per_page={Math.Min(perPage, DefaultPerPage)}"
        ];

        if (running)
        {
            queryParams.Add("status=in_progress");
        }
        else
        {
            queryParams.Add("status=completed");
        }

        if (sinceDate.HasValue)
        {
            queryParams.Add($"created=>={sinceDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
        }

        string query = string.Join("&", queryParams);
        string url = $"{BaseUrl}/actions/runs?{query}";

        return await GetAsync<GitHubActionsWorkflowRunsResponse>(url, cancellationToken);
    }

    private async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
        where T : class
    {
        using HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.Forbidden:
                throw new UnauthorizedAccessException($"GitHub API returned {response.StatusCode}. Check your API token.");
            case HttpStatusCode.NotFound:
                throw new InvalidOperationException($"Repository not found at {url}. Check owner/repository settings.");
        }

        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        T? result = JsonConvert.DeserializeObject<T>(json);
        Validates.NotNull(result);

        return result;
    }

    private static HttpClient CreateHttpClient(string baseUrl, string? apiToken)
    {
        HttpClient client = new()
        {
            Timeout = TimeSpan.FromMinutes(2),
        };

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("User-Agent", "GitExtensions");
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        if (!string.IsNullOrWhiteSpace(apiToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        }

        return client;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
