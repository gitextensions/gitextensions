using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace AzureDevOpsIntegration
{
    /// <summary>
    /// Provides access to the REST API of a Azure DevOps (or TFS>=2015) instance
    /// </summary>
    public class ApiClient : IDisposable
    {
        private const string BuildDefinitionsUrl = "build/definitions?api-version=2.0";
        private const string Properties = "properties=sourceVersion,status,buildNumber,result,definition,_links,startTime,finishTime";
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates a new API client instance for the given Azure DevOps / TFS project, that uses the given authentication token.
        /// </summary>
        /// <param name="projectUrl">
        /// The home page url of the project the API client should provide access to.
        /// </param>
        /// <param name="apiToken">
        /// The authentication token that is required and used to access the REST API of the Azure DevOps / TFS instance.
        /// </param>
        public ApiClient(string projectUrl, string apiToken)
        {
            _httpClient = new HttpClient();
            InitializeHttpClient(projectUrl, apiToken);
        }

        /// <summary>
        /// Configures the <see cref="HttpClient"/> of the API client instance, so that API requests can be made with it.
        /// </summary>
        /// <param name="projectUrl">
        /// The home page url of the Azure DevOps / TFS project the API client should provide access to.
        /// </param>
        /// <param name="apiToken">
        /// The authentication token that is required and used to access the REST API of the Azure DevOps / TFS instance.
        /// </param>
        private void InitializeHttpClient(string projectUrl, string apiToken)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var apiTokenHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{apiToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", apiTokenHeaderValue);

            _httpClient.BaseAddress = new Uri(projectUrl.EndsWith("/") ? projectUrl + "_apis/" : projectUrl + "/_apis/");
        }

        private async Task<T> HttpGetAsync<T>(string url)
        {
            using (var response = await _httpClient.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        [ItemCanBeNull]
        public async Task<string> GetBuildDefinitionsAsync(string buildDefinitionNameFilter)
        {
            var isNotFiltered = string.IsNullOrWhiteSpace(buildDefinitionNameFilter);
            var buildDefinitionUriFilter = isNotFiltered ? string.Empty : "&name=" + buildDefinitionNameFilter;
            var buildDefinitions = await HttpGetAsync<ListWrapper<BuildDefinition>>(BuildDefinitionsUrl + buildDefinitionUriFilter);
            if (buildDefinitions.Count != 0)
            {
                return GetBuildDefinitionsIds(buildDefinitions.Value);
            }

            if (isNotFiltered)
            {
                return null;
            }

            buildDefinitions = await HttpGetAsync<ListWrapper<BuildDefinition>>(BuildDefinitionsUrl);

            var tfsBuildDefinitionNameFilter = new Regex(buildDefinitionNameFilter, RegexOptions.Compiled);
            return GetBuildDefinitionsIds(buildDefinitions.Value.Where(b => tfsBuildDefinitionNameFilter.IsMatch(b.Name)));
        }

        [CanBeNull]
        private static string GetBuildDefinitionsIds(IEnumerable<BuildDefinition> buildDefinitions)
        {
            if (buildDefinitions != null && buildDefinitions.Any())
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
        public async Task<string> GetBuildDefinitionNameFromIdAsync(int buildId)
        {
            var build = await HttpGetAsync<Build>($"build/builds/{buildId}?api-version=2.0");
            return build.Definition.Name;
        }

        public async Task<IList<Build>> QueryFinishedBuildsAsync(string buildDefinitionsToQuery, DateTime? sinceDate)
        {
            string queryUrl = QueryForBuildStatus(buildDefinitionsToQuery, "completed");
            queryUrl += sinceDate.HasValue
                ? $"&minTime={sinceDate.Value.ToUniversalTime():s}&api-version=4.1"
                : "&api-version=2.0";

            var finishedBuilds = (await HttpGetAsync<ListWrapper<Build>>(queryUrl)).Value;
            return finishedBuilds;
        }

        public async Task<IList<Build>> QueryRunningBuildsAsync(string buildDefinitionsToQuery)
        {
            string queryUrl = QueryForBuildStatus(buildDefinitionsToQuery, "cancelling,inProgress,none,notStarted,postponed") + "&api-version=2.0";

            var runningBuilds = (await HttpGetAsync<ListWrapper<Build>>(queryUrl)).Value;

            return runningBuilds;
        }

        // Api doc: https://docs.microsoft.com/en-us/rest/api/azure/devops/build/builds/list?view=azure-devops-rest-4.1
        private string QueryForBuildStatus(string buildDefinitionsToQuery, string statusFilter)
            => $"build/builds?{Properties}&definitions={buildDefinitionsToQuery}&statusFilter={statusFilter}";

        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    internal class ListWrapper<T>
    {
        public int Count { get; set; }
        public IList<T> Value { get; set; }
    }

    internal class Project
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class BuildDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
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

        public string SourceVersion { get; set; }
        public string Status { get; set; }
        public string BuildNumber { get; set; }
        public string Result { get; set; }
        public BuildDefinition Definition { get; set; }
        public BuildLinks _links { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }

        public bool IsInProgress => Status != StatusCompleted;
    }

    public class BuildLinks
    {
        public Link Web { get; set; }
    }

    public class Link
    {
        public string Href { get; set; }
    }
}
