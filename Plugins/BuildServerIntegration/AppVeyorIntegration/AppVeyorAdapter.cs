using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitCommands.Utils;
using GitExtUtils;
using GitUI;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft;
using Newtonsoft.Json.Linq;

namespace AppVeyorIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class AppVeyorIntegrationMetadata : BuildServerAdapterMetadataAttribute
    {
        public AppVeyorIntegrationMetadata(string buildServerType)
            : base(buildServerType)
        {
        }

        public override string? CanBeLoaded
        {
            get
            {
                if (EnvUtils.IsNet4FullOrHigher())
                {
                    return null;
                }

                return ".NET Framework 4 or higher required";
            }
        }
    }

    [Export(typeof(IBuildServerAdapter))]
    [AppVeyorIntegrationMetadata(PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class AppVeyorAdapter : IBuildServerAdapter
    {
        public const string PluginName = "AppVeyor";
        private const uint ProjectsToRetrieveCount = 25;
        private const string WebSiteUrl = "https://ci.appveyor.com";
        private const string ApiBaseUrl = WebSiteUrl + "/api/projects/";

        private IBuildServerWatcher? _buildServerWatcher;

        private HttpClient? _httpClientAppVeyor;

        private List<AppVeyorBuildInfo>? _allBuilds = new List<AppVeyorBuildInfo>();
        private HashSet<ObjectId>? _fetchBuilds;
        private Func<ObjectId, bool>? _isCommitInRevisionGrid;
        private bool _shouldLoadTestResults;

        public void Initialize(
            IBuildServerWatcher buildServerWatcher,
            ISettingsSource config,
            Action openSettings,
            Func<ObjectId, bool>? isCommitInRevisionGrid = null)
        {
            if (_buildServerWatcher is not null)
            {
                throw new InvalidOperationException("Already initialized");
            }

            _buildServerWatcher = buildServerWatcher;
            _isCommitInRevisionGrid = isCommitInRevisionGrid;
            string? accountName = config.GetString("AppVeyorAccountName", null);
            string? accountToken = config.GetString("AppVeyorAccountToken", null);
            _shouldLoadTestResults = config.GetBool("AppVeyorLoadTestsResults", false);

            _fetchBuilds = new HashSet<ObjectId>();

            _httpClientAppVeyor = GetHttpClient(WebSiteUrl, accountToken);

            // projectId has format accountName/repoName
            // accountName may be any accessible project (for instance upstream)
            // if AppVeyorAccountName is set, projectNamesSetting may exclude the accountName part
            string projectNamesSetting = config.GetString("AppVeyorProjectName", "");
            var projectNames = _buildServerWatcher.ReplaceVariables(projectNamesSetting)
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => p.Contains("/") || !string.IsNullOrEmpty(accountName))
                .Select(p => p.Contains("/") ? p : accountName.Combine("/", p)!)
                .ToList();

            if (projectNames.Count == 0)
            {
                if (string.IsNullOrWhiteSpace(accountName) || string.IsNullOrWhiteSpace(accountToken))
                {
                    // No projectIds in settings, cannot query
                    return;
                }

                // No settings, query projects for this account
                ThreadHelper.JoinableTaskFactory.Run(
                    async () =>
                    {
                        // v2 tokens requires a separate prefix
                        // (Documentation specifies that this is applicable for all requests, not the case though)
                        string apiBaseUrl = !string.IsNullOrWhiteSpace(accountName) && !string.IsNullOrWhiteSpace(accountToken) && accountToken.StartsWith("v2.")
                            ? $"{WebSiteUrl}/api/account/{accountName}/projects/"
                            : ApiBaseUrl;

                        // get the project ids for this account - no possibility to check if they are for the current repo
                        var result = await GetResponseAsync(_httpClientAppVeyor, apiBaseUrl, CancellationToken.None).ConfigureAwait(false);

                        if (string.IsNullOrWhiteSpace(result))
                        {
                            return;
                        }

                        foreach (var project in JArray.Parse(result))
                        {
                            // "slug" and "name" are normally the same
                            var repoName = project["slug"].ToString();
                            var projectId = accountName.Combine("/", repoName)!;
                            projectNames.Add(projectId);
                        }
                    });
            }

            _allBuilds = FilterBuilds(projectNames.SelectMany(project => QueryBuildsResults(project)));

            return;

            static HttpClient GetHttpClient(string baseUrl, string? accountToken)
            {
                var httpClient = new HttpClient(new HttpClientHandler { UseDefaultCredentials = true })
                {
                    Timeout = TimeSpan.FromMinutes(2),
                    BaseAddress = new Uri(baseUrl, UriKind.Absolute),
                };
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrWhiteSpace(accountToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accountToken);
                }

                return httpClient;
            }

            List<AppVeyorBuildInfo> FilterBuilds(IEnumerable<AppVeyorBuildInfo> allBuilds)
            {
                var filteredBuilds = new List<AppVeyorBuildInfo>();
                foreach (var build in allBuilds.OrderByDescending(b => b.StartDate))
                {
                    Validates.NotNull(build.CommitId);
                    if (!_fetchBuilds.Contains(build.CommitId))
                    {
                        filteredBuilds.Add(build);
                        _fetchBuilds.Add(build.CommitId);
                    }
                }

                return filteredBuilds;
            }
        }

        private IEnumerable<AppVeyorBuildInfo> QueryBuildsResults(string projectId)
        {
            try
            {
                Validates.NotNull(_httpClientAppVeyor);
                return ThreadHelper.JoinableTaskFactory.Run(
                    async () =>
                    {
                        string queryUrl = $"{ApiBaseUrl}{projectId}/history?recordsNumber={ProjectsToRetrieveCount}";
                        var result = await GetResponseAsync(_httpClientAppVeyor, queryUrl, CancellationToken.None).ConfigureAwait(false);

                        return ExtractBuildInfo(projectId, result);
                    });
            }
            catch
            {
                return Enumerable.Empty<AppVeyorBuildInfo>();
            }
        }

        internal IEnumerable<AppVeyorBuildInfo> ExtractBuildInfo(string projectId, string? result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                return Enumerable.Empty<AppVeyorBuildInfo>();
            }

            Validates.NotNull(_isCommitInRevisionGrid);

            var content = JObject.Parse(result);

            var projectData = content["project"];
            var repositoryName = projectData["repositoryName"];
            var repositoryType = projectData["repositoryType"];

            var builds = content["builds"].Children();
            var baseWebUrl = $"{WebSiteUrl}/project/{projectId}/build/";
            var baseApiUrl = $"{ApiBaseUrl}{projectId}/";

            var buildDetails = new List<AppVeyorBuildInfo>();
            foreach (var b in builds)
            {
                try
                {
                    if (!ObjectId.TryParse((b["pullRequestHeadCommitId"] ?? b["commitId"]).ToObject<string>(),
                            out var objectId) || !_isCommitInRevisionGrid(objectId))
                    {
                        continue;
                    }

                    var pullRequestId = b["pullRequestId"];
                    var version = b["version"].ToObject<string>();
                    var status = ParseBuildStatus(b["status"].ToObject<string>());
                    long? duration = null;
                    if (status is (BuildInfo.BuildStatus.Success or BuildInfo.BuildStatus.Failure))
                    {
                        duration = GetBuildDuration(b);
                    }

                    var pullRequestTitle = b["pullRequestName"];

                    buildDetails.Add(new AppVeyorBuildInfo
                    {
                        Id = version,
                        BuildId = b["buildId"].ToObject<string>(),
                        Branch = b["branch"].ToObject<string>(),
                        CommitId = objectId,
                        CommitHashList = new[] { objectId },
                        Status = status,
                        StartDate = b["started"]?.ToObject<DateTime>() ?? DateTime.MinValue,
                        BaseWebUrl = baseWebUrl,
                        Url = baseWebUrl + version,
                        PullRequestUrl = repositoryType is not null && repositoryName is not null && pullRequestId is not null
                            ? BuildPullRequetUrl(repositoryType.Value<string>(), repositoryName.Value<string>(),
                                pullRequestId.Value<string>())
                            : null,
                        BaseApiUrl = baseApiUrl,
                        AppVeyorBuildReportUrl = baseApiUrl + "build/" + version,
                        PullRequestText = pullRequestId is not null ? "PR#" + pullRequestId.Value<string>() : string.Empty,
                        PullRequestTitle = pullRequestTitle is not null ? pullRequestTitle.Value<string>() : string.Empty,
                        Duration = duration,
                        TestsResultText = string.Empty
                    });
                }
                catch (Exception)
                {
                    // Failure on reading data on a build detail should not prevent to display the others build results
                }
            }

            return buildDetails;

            static string? BuildPullRequetUrl(string repositoryType, string repositoryName, string pullRequestId)
            {
                return repositoryType.ToLowerInvariant() switch
                {
                    "bitbucket" => $"https://bitbucket.org/{repositoryName}/pull-requests/{pullRequestId}",
                    "github" => $"https://github.com/{repositoryName}/pull/{pullRequestId}",
                    "gitlab" => $"https://gitlab.com/{repositoryName}/merge_requests/{pullRequestId}",
                    "vso" => null,
                    "git" => null,
                    _ => null
                };
            }
        }

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        public string UniqueKey
        {
            get
            {
                Validates.NotNull(_httpClientAppVeyor);
                return _httpClientAppVeyor.BaseAddress.Host;
            }
        }

        public IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null)
        {
            // AppVeyor api is different than TeamCity one and all build results are fetch in one call without
            // filter parameters possible (so this call is useless!)
            return Observable.Empty<BuildInfo>();
        }

        public IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler)
        {
            return GetBuilds(scheduler);
        }

        private IObservable<BuildInfo> GetBuilds(IScheduler scheduler)
        {
            return Observable.Create<BuildInfo>((observer, cancellationToken) =>
                Task.Run(
                    () => scheduler.Schedule(() => ObserveBuilds(observer, cancellationToken))));
        }

        private void ObserveBuilds(IObserver<BuildInfo> observer, CancellationToken cancellationToken)
        {
            try
            {
                if (_allBuilds is null)
                {
                    // builds are updated, no requery for new builds
                    return;
                }

                // Display all builds found
                foreach (var build in _allBuilds)
                {
                    // Update finished build with tests results
                    if (_shouldLoadTestResults
                        && (build.Status == BuildInfo.BuildStatus.Success
                            || build.Status == BuildInfo.BuildStatus.Failure))
                    {
                        UpdateDescription(build, cancellationToken);
                    }

                    UpdateDisplay(observer, build);
                }

                // Manage in progress builds...
                var inProgressBuilds = _allBuilds.Where(b => b.Status == BuildInfo.BuildStatus.InProgress).ToList();

                // Reset current build list - refresh required to see new builds
                _allBuilds = null;
                while (inProgressBuilds.Any() && !cancellationToken.IsCancellationRequested)
                {
                    const int inProgressRefresh = 10000;
                    Thread.Sleep(inProgressRefresh);
                    foreach (var build in inProgressBuilds)
                    {
                        UpdateDescription(build, cancellationToken);
                        UpdateDisplay(observer, build);
                    }

                    inProgressBuilds = inProgressBuilds.Where(b => b.Status == BuildInfo.BuildStatus.InProgress).ToList();
                }

                observer.OnCompleted();
            }
            catch (OperationCanceledException)
            {
                // Do nothing, the observer is already stopped
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }
        }

        private void UpdateDisplay(IObserver<BuildInfo> observer, AppVeyorBuildInfo build)
        {
            build.UpdateDescription();
            observer.OnNext(build);
        }

        private void UpdateDescription(AppVeyorBuildInfo buildDetails, CancellationToken cancellationToken)
        {
            var buildDetailsParsed = ThreadHelper.JoinableTaskFactory.Run(() => FetchBuildDetailsManagingVersionUpdateAsync(buildDetails, cancellationToken));
            if (buildDetailsParsed is null)
            {
                return;
            }

            var buildData = buildDetailsParsed["build"];
            var buildDescription = buildData["jobs"].Last();

            var status = buildDescription["status"].ToObject<string>();
            buildDetails.Status = ParseBuildStatus(status);

            buildDetails.ChangeProgressCounter();
            if (!buildDetails.IsRunning)
            {
                buildDetails.Duration = GetBuildDuration(buildData);
            }

            int testCount = buildDescription["testsCount"].ToObject<int>();
            if (testCount != 0)
            {
                int failedTestCount = buildDescription["failedTestsCount"].ToObject<int>();
                int skippedTestCount = testCount - buildDescription["passedTestsCount"].ToObject<int>();
                var testResults = testCount + " tests";
                if (failedTestCount != 0 || skippedTestCount != 0)
                {
                    testResults += $" ( {failedTestCount} failed, {skippedTestCount} skipped )";
                }

                buildDetails.TestsResultText = testResults;
            }
        }

        private long GetBuildDuration(JToken buildData)
        {
            var startTime = (buildData["started"] ?? buildData["created"])?.ToObject<DateTime>();
            var updateTime = buildData["updated"]?.ToObject<DateTime>();
            if (!startTime.HasValue || !updateTime.HasValue)
            {
                return 0;
            }

            return (long)(updateTime.Value - startTime.Value).TotalMilliseconds;
        }

        private async Task<JObject> FetchBuildDetailsManagingVersionUpdateAsync(AppVeyorBuildInfo buildDetails, CancellationToken cancellationToken)
        {
            Validates.NotNull(_httpClientAppVeyor);

            try
            {
                Validates.NotNull(buildDetails.AppVeyorBuildReportUrl);
                return JObject.Parse(await GetResponseAsync(_httpClientAppVeyor, buildDetails.AppVeyorBuildReportUrl, cancellationToken).ConfigureAwait(false));
            }
            catch
            {
                var buildHistoryUrl = buildDetails.BaseApiUrl + "/history?recordsNumber=1&startBuildId=" + (int.Parse(buildDetails.BuildId) + 1);
                var builds = JObject.Parse(await GetResponseAsync(_httpClientAppVeyor, buildHistoryUrl, cancellationToken).ConfigureAwait(false));

                var version = builds["builds"][0]["version"].ToObject<string>();
                buildDetails.Id = version;
                buildDetails.AppVeyorBuildReportUrl = buildDetails.BaseApiUrl + "/build/" + version;
                buildDetails.Url = buildDetails.BaseWebUrl + version;

                return JObject.Parse(await GetResponseAsync(_httpClientAppVeyor, buildDetails.AppVeyorBuildReportUrl, cancellationToken).ConfigureAwait(false));
            }
        }

        private static BuildInfo.BuildStatus ParseBuildStatus(string statusValue)
        {
            return statusValue switch
            {
                "success" => BuildInfo.BuildStatus.Success,
                "failed" => BuildInfo.BuildStatus.Failure,
                "cancelled" => BuildInfo.BuildStatus.Stopped,
                "queued" or "running" => BuildInfo.BuildStatus.InProgress,
                _ => BuildInfo.BuildStatus.Unknown
            };
        }

        private Task<Stream?> GetStreamAsync(HttpClient httpClient, string restServicePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return httpClient.GetAsync(restServicePath, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                             .ContinueWith(
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
                                 task => GetStreamFromHttpResponseAsync(httpClient, task, restServicePath, cancellationToken),
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
                                 cancellationToken,
                                 restServicePath.Contains("github") ? TaskContinuationOptions.None : TaskContinuationOptions.AttachedToParent,
                                 TaskScheduler.Current)
                             .Unwrap();
        }

        private Task<Stream?> GetStreamFromHttpResponseAsync(HttpClient httpClient, Task<HttpResponseMessage> task, string restServicePath, CancellationToken cancellationToken)
        {
            var retry = task.IsCanceled && !cancellationToken.IsCancellationRequested;

            if (retry)
            {
                return GetStreamAsync(httpClient, restServicePath, cancellationToken);
            }

            if (task.Status == TaskStatus.RanToCompletion && task.CompletedResult().IsSuccessStatusCode)
            {
                return task.CompletedResult().Content.ReadAsStreamAsync();
            }

            return Task.FromResult<Stream?>(null);
        }

        private Task<string> GetResponseAsync(HttpClient httpClient, string relativePath, CancellationToken cancellationToken)
        {
            var getStreamTask = GetStreamAsync(httpClient, relativePath, cancellationToken);

            var taskContinuationOptions = relativePath.Contains("github") ? TaskContinuationOptions.None : TaskContinuationOptions.AttachedToParent;
            return getStreamTask.ContinueWith(
                task =>
                {
                    if (task.Status != TaskStatus.RanToCompletion)
                    {
                        return string.Empty;
                    }

                    using var responseStream = task.Result;

                    if (responseStream is null)
                    {
                        return "";
                    }

                    return new StreamReader(responseStream).ReadToEnd();
                },
                cancellationToken,
                taskContinuationOptions,
                TaskScheduler.Current);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _httpClientAppVeyor?.Dispose();
        }
    }
}
