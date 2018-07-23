using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitUI;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace VstsAndTfsIntegration
{
    /// <summary>
    /// For VSTS and TFS >=2015
    /// </summary>
    public class TfsApiHelper
    {
        private const string BuildDefinitionsUrl = "_apis/build/definitions?api-version=2.0";
        private readonly HttpClient _httpClient;
        private string _buildDefinitionsToQuery;

        public TfsApiHelper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private void InitializeHttpClient(string serverUrl, string projectName, string personalAccessToken)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}")));

            var hostUri = new Uri(serverUrl);

            // Uri constructor takes care of ensuring there's a slash after serverUrl, as well as path encoding projectName.
            _httpClient.BaseAddress = new Uri(hostUri, projectName + "/");
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

        public void ConnectToTfsServer(string serverUrl, string teamCollection, string projectName, string restApiToken, string buildDefinitionNameFilter)
        {
            try
            {
                InitializeHttpClient(serverUrl, projectName, restApiToken);

                var taskServerInit = ThreadHelper.JoinableTaskFactory.RunAsync(async () => await GetBuildDefinitionsAsync(buildDefinitionNameFilter));
                _buildDefinitionsToQuery = taskServerInit.Join();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public async Task<IEnumerable<Build>> QueryBuildsAsync(DateTime? sinceDate, bool? running)
        {
            if (_buildDefinitionsToQuery == null)
            {
                return Enumerable.Empty<Build>();
            }

            var builds = (await HttpGetAsync<ListWrapper<Build>>($"_apis/build/builds?api-version=2.0&definitions={_buildDefinitionsToQuery}")).Value;

            return builds
                .Where(b => !running.HasValue || running.Value == b.IsInProgress)
                .Where(b => !sinceDate.HasValue || b.StartTime >= sinceDate.Value)
                .ToList();
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

    internal class BuildDefinition
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
