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
        private readonly HttpClient _httpClient;

        private string _buildDefinitionsToQuery;
        private string _lastBuildDefinitionFilter;

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
        private async Task<string> GetBuildDefinitionsAsync(string buildDefinitionNameFilter)
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

        public async Task<IEnumerable<Build>> QueryBuildsAsync(string buildDefinitionFilter, DateTime? sinceDate, bool? running)
        {
            if (_buildDefinitionsToQuery == null || !string.Equals(_lastBuildDefinitionFilter, buildDefinitionFilter, StringComparison.OrdinalIgnoreCase))
            {
                _buildDefinitionsToQuery = await GetBuildDefinitionsAsync(buildDefinitionFilter);
                _lastBuildDefinitionFilter = buildDefinitionFilter;
            }

            if (_buildDefinitionsToQuery == null)
            {
                return Enumerable.Empty<Build>();
            }

            var builds = (await HttpGetAsync<ListWrapper<Build>>($"build/builds?api-version=2.0&definitions={_buildDefinitionsToQuery}")).Value;

            return builds
                .Where(b => !running.HasValue || running.Value == b.IsInProgress)
                .Where(b => !sinceDate.HasValue || b.StartTime >= sinceDate.Value)
                .ToList();
        }

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
        public string SourceVersion { get; set; }
        public string Status { get; set; }
        public string BuildNumber { get; set; }
        public string Result { get; set; }
        public BuildDefinition Definition { get; set; }
        public BuildLinks _links { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }

        public bool IsInProgress => Status != "completed";
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
