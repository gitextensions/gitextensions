using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitUI;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;

namespace VstsAndTfsIntegration
{
    /// <summary>
    /// For VSTS and TFS >=2015
    /// </summary>
    public class TfsApiHelper : IDisposable
    {
        private HttpClient _httpClient;
        private IEnumerable<BuildDefinition> _buildDefinitions;

        private HttpClient InitializeHttpClient(string serverUrl, string projectName, string personalAccessToken)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}")));

            var hostUri = new Uri(serverUrl);

            // Uri constructor takes care of ensuring there's a slash after serverUrl, as well as path encoding projectName.
            client.BaseAddress = new Uri(hostUri, projectName + "/");
            return client;
        }

        private async Task<IEnumerable<BuildDefinition>> GetBuildDefinitionsAsync(string teamCollection, string projectName, Regex buildDefinitionNameFilter)
        {
            var buildDefs = new List<BuildDefinition>();

            using (var response = await _httpClient.GetAsync($"_apis/build/definitions?api-version=2.0"))
            {
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();

                var buildDefinitions = JsonConvert.DeserializeObject<ListWrapper<BuildDefinition>>(json);

                if (buildDefinitionNameFilter == null)
                {
                    buildDefs.AddRange(buildDefinitions.Value);
                }
                else
                {
                    foreach (var buildDefinition in buildDefinitions.Value)
                    {
                        if (buildDefinitionNameFilter.IsMatch(buildDefinition.Name))
                        {
                            buildDefs.Add(buildDefinition);
                        }
                    }
                }
            }

            return buildDefs;
        }

        public void ConnectToTfsServer(string serverUrl, string teamCollection, string projectName, string restApiToken, Regex buildDefinitionNameFilter = null)
        {
            try
            {
                _httpClient = InitializeHttpClient(serverUrl, projectName, restApiToken);

                var taskServerInit = ThreadHelper.JoinableTaskFactory.RunAsync(async () => await GetBuildDefinitionsAsync(teamCollection, projectName, buildDefinitionNameFilter));
                _buildDefinitions = taskServerInit.Join();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public async Task<IReadOnlyList<Build>> QueryBuildsAsync(DateTime? sinceDate, bool? running)
        {
            if (_buildDefinitions == null)
            {
                return new List<Build>();
            }

            var result = new List<Build>();
            string buildDefIds = string.Join(",", _buildDefinitions.Select(b => b.Id));
            using (HttpResponseMessage response = await _httpClient.GetAsync(
                $"_apis/build/builds?api-version=2.0&definitions={buildDefIds}"))
            {
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();

                var builds = JsonConvert.DeserializeObject<ListWrapper<Build>>(json).Value;

                return builds
                    .Where(b => !running.HasValue || (running.Value == b.IsInProgress))
                    .Where(b => !sinceDate.HasValue || (b.StartTime >= sinceDate.Value))
                    .ToList();
            }
        }

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
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
        public string url { get; set; }
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
