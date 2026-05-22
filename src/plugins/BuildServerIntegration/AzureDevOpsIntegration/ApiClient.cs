using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft;

namespace AzureDevOpsIntegration;

/// <summary>
/// Provides access to the REST API of a Azure DevOps (or TFS>=2015) instance
/// </summary>
public sealed class ApiClient : IDisposable
{
    private const string Properties = "properties=sourceVersion,status,buildNumber,result,definition,_links,startTime,finishTime";
    private readonly HttpClient _httpClient;

    /// <summary>
    ///  Creates a new API client for the given Azure DevOps / TFS project.
    /// </summary>
    /// <param name="projectUrl">
    ///  The home page url of the project the API client should provide access to.
    /// </param>
    /// <param name="apiToken">
    ///  A Personal Access Token for Basic auth. When <see langword="null"/> or empty,
    ///  the client attempts to obtain a Bearer token from Git Credential Manager, and
    ///  falls back to default Windows credentials (NTLM/Negotiate) for on-premises TFS.
    /// </param>
    /// <param name="gitExecutable">
    ///  Path to the git executable, used to invoke <c>git credential fill</c> when
    ///  no PAT is provided. When <see langword="null"/>, defaults to <c>"git"</c>.
    /// </param>
    public ApiClient(string projectUrl, string? apiToken, string? gitExecutable = null)
    {
        if (!string.IsNullOrWhiteSpace(apiToken))
        {
            _httpClient = new HttpClient();
            SetBasicAuth(apiToken);
        }
        else
        {
            string? bearerToken = TryGetBearerTokenFromGcm(projectUrl, gitExecutable);
            if (bearerToken is not null)
            {
                _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }
            else
            {
                _httpClient = new HttpClient(new HttpClientHandler { UseDefaultCredentials = true });
            }
        }

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.BaseAddress = new Uri(projectUrl.EndsWith('/') ? projectUrl + "_apis/" : projectUrl + "/_apis/");
    }

    private void SetBasicAuth(string apiToken)
    {
        string apiTokenHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{apiToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", apiTokenHeaderValue);
    }

    /// <summary>
    ///  Attempts to obtain a Bearer token from Git Credential Manager via <c>git credential fill</c>.
    /// </summary>
    private static string? TryGetBearerTokenFromGcm(string projectUrl, string? gitExecutable)
    {
        try
        {
            if (!Uri.TryCreate(projectUrl, UriKind.Absolute, out Uri? uri))
            {
                return null;
            }

            string input = $"protocol={uri.Scheme}\nhost={uri.Host}\npath={uri.AbsolutePath.TrimStart('/')}\n\n";

            using System.Diagnostics.Process process = new()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = gitExecutable ?? "git",
                    Arguments = "credential fill",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            process.StandardInput.Write(input);
            process.StandardInput.Close();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(TimeSpan.FromSeconds(30));

            if (process.ExitCode != 0)
            {
                return null;
            }

            foreach (string line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                // Split on the first '=' only to handle tokens containing '='
                int separatorIndex = line.IndexOf('=');
                if (separatorIndex > 0)
                {
                    string key = line[..separatorIndex].Trim();
                    string value = line[(separatorIndex + 1)..].Trim();

                    if (key == "password" && !string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }
        }
        catch
        {
            // GCM not available or failed — fall back to default credentials
        }

        return null;
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private async Task<T> HttpGetAsync<T>(string url)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync(url);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException();
        }

        response.EnsureSuccessStatusCode();
        string json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(json, _jsonOptions)!;
    }

    public async Task<string?> GetBuildDefinitionsAsync(string buildDefinitionNameFilter, string? repositoryName = null)
    {
        bool isNotFiltered = string.IsNullOrWhiteSpace(buildDefinitionNameFilter);
        string buildDefinitionUriFilter = isNotFiltered ? string.Empty : "&name=" + buildDefinitionNameFilter;

        string repositoryFilter = "";
        if (!string.IsNullOrWhiteSpace(repositoryName))
        {
            string? repositoryId = await GetRepositoryIdAsync(repositoryName);
            if (repositoryId is not null)
            {
                repositoryFilter = $"&repositoryId={repositoryId}&repositoryType=TfsGit";
            }
        }

        string url = $"build/definitions?api-version=6.0{buildDefinitionUriFilter}{repositoryFilter}";
        ListWrapper<BuildDefinition> buildDefinitions = await HttpGetAsync<ListWrapper<BuildDefinition>>(url);
        if (buildDefinitions.Count != 0)
        {
            return GetBuildDefinitionsIds(buildDefinitions.Value);
        }

        if (isNotFiltered)
        {
            return null;
        }

        buildDefinitions = await HttpGetAsync<ListWrapper<BuildDefinition>>($"build/definitions?api-version=6.0{repositoryFilter}");

        return GetBuildDefinitionsIds(buildDefinitions.Value?.Where(b => b.Name is not null && Regex.IsMatch(b.Name, buildDefinitionNameFilter)));
    }

    /// <summary>
    ///  Resolves a repository name to its GUID via the Azure DevOps Git API.
    /// </summary>
    private async Task<string?> GetRepositoryIdAsync(string repositoryName)
    {
        try
        {
            GitRepository repo = await HttpGetAsync<GitRepository>($"git/repositories/{Uri.EscapeDataString(repositoryName)}?api-version=6.0");
            return repo.Id;
        }
        catch
        {
            return null;
        }
    }

    private static string? GetBuildDefinitionsIds(IEnumerable<BuildDefinition>? buildDefinitions)
    {
        if (buildDefinitions?.Any() is true)
        {
            return string.Join(",", buildDefinitions.Select(b => b.Id));
        }

        return null;
    }

    /// <summary>
    /// Gets the name of the build definition a given build has been built with.
    /// </summary>
    /// <param name="buildId">
    /// The id of the build to get the build definition name for.
    /// </param>
    public async Task<string?> GetBuildDefinitionNameFromIdAsync(int buildId)
    {
        Build build = await HttpGetAsync<Build>($"build/builds/{buildId}?api-version=2.0");
        return build.Definition?.Name;
    }

    public async Task<IList<Build>> QueryFinishedBuildsAsync(string buildDefinitionsToQuery, DateTime? sinceDate)
    {
        string queryUrl = QueryForBuildStatus(buildDefinitionsToQuery, "completed");
        queryUrl += sinceDate.HasValue
            ? $"&minTime={sinceDate.Value.ToUniversalTime():s}&api-version=4.1"
            : "&api-version=2.0";

        return await QueryBuildsAsync(queryUrl);
    }

    private async Task<IList<Build>> QueryBuildsAsync(string queryUrl)
    {
        IList<Build>? builds = (await HttpGetAsync<ListWrapper<Build>>(queryUrl)).Value;
        Validates.NotNull(builds);
        return builds;
    }

    public async Task<IList<Build>> QueryRunningBuildsAsync(string buildDefinitionsToQuery)
    {
        string queryUrl = QueryForBuildStatus(buildDefinitionsToQuery, "cancelling,inProgress,none,notStarted,postponed") + "&api-version=2.0";

        return await QueryBuildsAsync(queryUrl);
    }

    // Api doc: https://docs.microsoft.com/en-us/rest/api/azure/devops/build/builds/list?view=azure-devops-rest-4.1
    private static string QueryForBuildStatus(string buildDefinitionsToQuery, string statusFilter)
        => $"build/builds?{Properties}&definitions={buildDefinitionsToQuery}&statusFilter={statusFilter}";

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

internal class ListWrapper<T>
{
    public int Count { get; set; }
    public IList<T>? Value { get; set; }
}

internal class Project
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Url { get; set; }
}

internal class GitRepository
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}

public class BuildDefinition
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Url { get; set; }
}

/// <summary>
/// Data received from https://docs.microsoft.com/en-us/rest/api/vsts/build/builds/get?view=vsts-rest-4.1
/// </summary>
public class Build
{
    public const string StatusAll = "all"; // All status.
    public const string StatusCancelling = "cancelling"; // The build is cancelling
    public const string StatusCompleted = "completed"; // The build has completed.
    public const string StatusInProgress = "inProgress"; // The build is currently in progress.
    public const string StatusNone = "none"; // No status.
    public const string StatusNotStarted = "notStarted"; // The build has not yet started.
    public const string StatusPostponed = "postponed"; // The build is inactive in the queue.

    public const string ReasonPullRequest = "validateShelveset"; // The build is triggered from a Pull Request.

    public string? SourceVersion { get; set; }
    public string? Status { get; set; }
    public string? BuildNumber { get; set; }
    public string? Result { get; set; }
    public string? Reason { get; set; }
    public Repository? Repository { get; set; }
    public string? Parameters { get; set; }
    public BuildDefinition? Definition { get; set; }
    public BuildLinks? _links { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? FinishTime { get; set; }

    public bool IsInProgress => Status != StatusCompleted;
    public bool IsPullRequest => Reason == ReasonPullRequest;
}

public class Repository
{
    public string? Url { get; set; }
}

public class BuildLinks
{
    public Link? Web { get; set; }
}

public class Link
{
    public string? Href { get; set; }
}
