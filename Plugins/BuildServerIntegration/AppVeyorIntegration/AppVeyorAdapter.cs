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
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Newtonsoft.Json.Linq;

namespace AppVeyorIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AppVeyorIntegrationMetadata : BuildServerAdapterMetadataAttribute
    {
        public AppVeyorIntegrationMetadata(string buildServerType)
            : base(buildServerType)
        {
        }

        public override string CanBeLoaded
        {
            get
            {
                if (EnvUtils.IsNet4FullOrHigher())
                    return null;
                return ".Net 4 full framework required";
            }
        }
    }

    [Export(typeof(IBuildServerAdapter))]
    [AppVeyorIntegrationMetadata("AppVeyor")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class AppVeyorAdapter : IBuildServerAdapter
    {
        private const uint ProjectsToRetrieveCount = 25;
        private const string WebSiteUrl = "https://ci.appveyor.com";
        private const string ApiBaseUrl = WebSiteUrl + "/api/projects/";
        private const string GitHubUrl = "https://api.github.com/repos/{0}/commits/{1}";

        private IBuildServerWatcher _buildServerWatcher;

        private HttpClient _httpClientAppVeyor;
        private HttpClient _httpClientGitHub;

        private List<BuildDetails> _allBuilds = new List<BuildDetails>();
        private HashSet<string> _fetchBuilds;
        private string _accountToken;
        private static readonly Dictionary<string, Project> Projects = new Dictionary<string, Project>();
        private Func<string, bool> IsCommitInRevisionGrid;
        private bool _shouldLoadTestResults;
        private bool _shouldDisplayGitHubPullRequestBuilds;
        private string _gitHubToken;

        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config,
            Func<string, bool> isCommitInRevisionGrid)
        {
            if (_buildServerWatcher != null)
                throw new InvalidOperationException("Already initialized");

            IsCommitInRevisionGrid = isCommitInRevisionGrid;
            var accountName = config.GetString("AppVeyorAccountName", null);
            _accountToken = config.GetString("AppVeyorAccountToken", null);
            var projectNamesSetting = config.GetString("AppVeyorProjectName", null);
            if (accountName.IsNullOrWhiteSpace() && projectNamesSetting.IsNullOrWhiteSpace())
                return;

            _shouldLoadTestResults = config.GetBool("AppVeyorLoadTestsResults", false);
            _gitHubToken = config.GetString("AppVeyorGitHubToken", null);
            _shouldDisplayGitHubPullRequestBuilds = config.GetBool("AppVeyorDisplayGitHubPullRequests", false)
                    && !string.IsNullOrWhiteSpace(_gitHubToken);

            _fetchBuilds = new HashSet<string>();
            _buildServerWatcher = buildServerWatcher;

            _httpClientAppVeyor = GetHttpClient(WebSiteUrl, _accountToken);

            _httpClientGitHub = GetHttpClient("https://api.github.com/", _gitHubToken);
            _httpClientGitHub.DefaultRequestHeaders.Add("User-Agent", "Anything");

            var useAllProjets = string.IsNullOrWhiteSpace(projectNamesSetting);
            string[] projectNames = null;
            if (!useAllProjets)
                projectNames = projectNamesSetting.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
            if (Projects.Count == 0 ||
                (!useAllProjets && Projects.Keys.Intersect(projectNames).Count() != projectNames.Length))
            {
                Projects.Clear();
                if (_accountToken.IsNullOrWhiteSpace())
                {
                    FillProjectsFromSettings(accountName, projectNames);
                }
                else
                {
                    if (accountName.IsNullOrWhiteSpace())
                    {
                        return;
                    }
                    GetResponseAsync(_httpClientAppVeyor, ApiBaseUrl, CancellationToken.None)
                        .ContinueWith(
                            task =>
                            {
                                if (task.Result.IsNullOrWhiteSpace())
                                    return;

                                var projects = JArray.Parse(task.Result);
                                foreach (var project in projects)
                                {
                                    var projectId = project["slug"].ToString();
                                    projectId = accountName.Combine("/", projectId);
                                    var projectName = project["name"].ToString();
                                    var projectObj = new Project
                                    {
                                        Name = projectName,
                                        Id = projectId,
                                        QueryUrl = BuildQueryUrl(projectId)
                                    };

                                    if (useAllProjets || projectNames.Contains(projectObj.Name))
                                    {
                                        Projects.Add(projectObj.Name, projectObj);
                                    }
                                }
                            }).Wait();
                }
            }
            var builds = Projects.Where(p => useAllProjets || projectNames.Contains(p.Value.Name)).Select(p => p.Value);
            _allBuilds =
                FilterBuilds(builds.SelectMany(QueryBuildsResults));
        }

        private static void FillProjectsFromSettings(string accountName, string[] projectNames)
        {
            foreach (var projectName in projectNames)
            {
                var projectId = accountName.Combine("/", projectName);
                Projects.Add(projectName, new Project
                {
                    Name = projectName,
                    Id = projectId,
                    QueryUrl = BuildQueryUrl(projectId)
                });
            }
        }

        private static HttpClient GetHttpClient(string baseUrl, string accountToken)
        {
            var httpClient = new HttpClient(new HttpClientHandler { UseDefaultCredentials = true })
            {
                Timeout = TimeSpan.FromMinutes(2),
                BaseAddress = new Uri(baseUrl, UriKind.Absolute),
            };
            if (accountToken.IsNotNullOrWhitespace())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accountToken);
            }
            return httpClient;
        }

        private static string BuildQueryUrl(string projectId)
        {
            return ApiBaseUrl + projectId + "/history?recordsNumber=" + ProjectsToRetrieveCount;
        }

        private class Project
        {
            public string Name;
            public string Id;
            public string QueryUrl;
        }

        private IEnumerable<BuildDetails> QueryBuildsResults(Project project)
        {
            try
            {
                var buildTask = GetResponseAsync(_httpClientAppVeyor, project.QueryUrl, CancellationToken.None)
                    .ContinueWith(
                        task =>
                        {
                            if (string.IsNullOrWhiteSpace(task.Result))
                            {
                                return Enumerable.Empty<BuildDetails>();
                            }

                            var jobDescription = JObject.Parse(task.Result);
                            var builds = jobDescription["builds"];
                            var myBuilds = builds.Children();
                            var baseApiUrl = ApiBaseUrl + project.Id;
                            var baseWebUrl = WebSiteUrl + "/project/" + project.Id + "/build/";
                            var isGitHubRepository = jobDescription["project"]["repositoryType"].ToObject<string>() ==
                                                     "gitHub";
                            var repoName = jobDescription["project"]["repositoryName"].ToObject<string>();

                            return myBuilds.Select(b =>
                            {
                                string commitSha1 = null;
                                var pullRequestId = b["pullRequestId"];
                                if (isGitHubRepository && pullRequestId != null)
                                {
                                    if (!_shouldDisplayGitHubPullRequestBuilds)
                                        return null;
                                    try
                                    {
                                        var githubCommitUrl = string.Format(GitHubUrl, repoName, b["commitId"]);
                                        var gitHubTask = GetResponseAsync(_httpClientGitHub, githubCommitUrl, CancellationToken.None).ContinueWith(
                                            task2 =>
                                            {
                                                var content = task2.Result;
                                                if (string.IsNullOrWhiteSpace(content))
                                                    return;
                                                var commitResult = JObject.Parse(content);
                                                commitSha1 = commitResult["parents"][1]["sha"].ToObject<string>();
                                            });
                                        gitHubTask.Wait();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                                else
                                {
                                    commitSha1 = b["commitId"].ToObject<string>();
                                }
                                if (commitSha1 == null || !IsCommitInRevisionGrid(commitSha1))
                                {
                                    return null;
                                }

                                var version = b["version"].ToObject<string>();
                                var status = ParseBuildStatus(b["status"].ToObject<string>());
                                long? duration = null;
                                if (status == BuildInfo.BuildStatus.Success || status == BuildInfo.BuildStatus.Failure)
                                    duration = GetBuildDuration(b);

                                return new BuildDetails
                                {
                                    Id = version,
                                    BuildId = b["buildId"].ToObject<string>(),
                                    Branch = b["branch"].ToObject<string>(),
                                    CommitId = commitSha1,
                                    CommitHashList = new[] { commitSha1 },
                                    Status = status,
                                    StartDate = b["started"]?.ToObject<DateTime>() ?? DateTime.MinValue,
                                    BaseWebUrl = baseWebUrl,
                                    Url = WebSiteUrl + "/project/" + project.Id + "/build/" + version,
                                    BaseApiUrl = baseApiUrl,
                                    AppVeyorBuildReportUrl = baseApiUrl + "/build/" + version,
                                    PullRequestText = (pullRequestId != null ? " PR#" + pullRequestId.Value<string>() : string.Empty),
                                    Duration = duration,
                                    TestsResultText = string.Empty
                                };
                            })
                            .Where(x => x != null)
                            .ToList();
                        },
                        TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.AttachedToParent);
                return buildTask.Result.Where(b => b != null).ToList();
            }
            catch
            {
                return Enumerable.Empty<BuildDetails>();
            }
        }

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        public string UniqueKey => _httpClientAppVeyor.BaseAddress.Host;

        public IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null)
        {
            //AppVeyor api is different than TeamCity one and all build results are fetch in one call without
            //filter parameters possible (so this call is useless!)
            return Observable.Empty<BuildInfo>();
        }

        public IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler)
        {
            return GetBuilds(scheduler);
        }

        private IObservable<BuildInfo> GetBuilds(IScheduler scheduler)
        {
            return Observable.Create<BuildInfo>((observer, cancellationToken) =>
                Task<IDisposable>.Factory.StartNew(
                    () => scheduler.Schedule(() => ObserveBuilds(observer, cancellationToken))));
        }

        private void ObserveBuilds(IObserver<BuildInfo> observer, CancellationToken cancellationToken)
        {
            try
            {
                if (_allBuilds == null)
                    return;

                //Display all builds found
                foreach (var build in _allBuilds)
                {
                    UpdateDisplay(observer, build);
                }

                //Update finished build with tests results
                if (_shouldLoadTestResults)
                {
                    foreach (var build in _allBuilds.Where(b => b.Status == BuildInfo.BuildStatus.Success
                                                                || b.Status == BuildInfo.BuildStatus.Failure))
                    {
                        UpdateDescription(build, cancellationToken);
                        UpdateDisplay(observer, build);
                    }
                }

                //Manage in progress builds...
                var inProgressBuilds = _allBuilds.Where(b => b.Status == BuildInfo.BuildStatus.InProgress).ToList();
                _allBuilds = null;
                do
                {
                    Thread.Sleep(5000);
                    foreach (var build in inProgressBuilds)
                    {
                        UpdateDescription(build, cancellationToken);
                        UpdateDisplay(observer, build);
                    }
                    inProgressBuilds = inProgressBuilds.Where(b => b.Status == BuildInfo.BuildStatus.InProgress).ToList();
                } while (inProgressBuilds.Any());

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

        private static void UpdateDisplay(IObserver<BuildInfo> observer, BuildDetails build)
        {
            build.UpdateDescription();
            observer.OnNext(build);
        }

        private List<BuildDetails> FilterBuilds(IEnumerable<BuildDetails> allBuilds)
        {
            var filteredBuilds = new List<BuildDetails>();
            foreach (var build in allBuilds.OrderByDescending(b => b.StartDate))
            {
                if (!_fetchBuilds.Contains(build.CommitId))
                {
                    filteredBuilds.Add(build);
                    _fetchBuilds.Add(build.CommitId);
                }
            }
            return filteredBuilds;
        }

        private void UpdateDescription(BuildDetails buildDetails, CancellationToken cancellationToken)
        {
            var buildDetailsParsed = FetchBuildDetailsManagingVersionUpdate(buildDetails, cancellationToken);
            if (buildDetailsParsed == null)
                return;
            var buildData = buildDetailsParsed["build"];
            var buildDescription = buildData["jobs"].Last();

            var status = buildDescription["status"].ToObject<string>();
            buildDetails.Status = ParseBuildStatus(status);

            buildDetails.ChangeProgressCounter();
            if (!buildDetails.IsRunning)
            {
                buildDetails.Duration = GetBuildDuration(buildData);
            }

            string testResults = string.Empty;
            int nbTests = buildDescription["testsCount"].ToObject<int>();
            if (nbTests != 0)
            {
                int nbFailedTests = buildDescription["failedTestsCount"].ToObject<int>();
                int nbSkippedTests = nbTests - buildDescription["passedTestsCount"].ToObject<int>();
                testResults = " : " + nbTests + " tests";
                if (nbFailedTests != 0 || nbSkippedTests != 0)
                    testResults += string.Format(" ( {0} failed, {1} skipped )", nbFailedTests, nbSkippedTests);
                buildDetails.TestsResultText = " " + testResults;
            }
        }

        private static long GetBuildDuration(JToken buildData)
        {
            var startTime = buildData["started"].ToObject<DateTime>();
            var updateTime = buildData["updated"].ToObject<DateTime>();
            return (long)(updateTime - startTime).TotalMilliseconds;
        }

        private JObject FetchBuildDetailsManagingVersionUpdate(BuildDetails buildDetails, CancellationToken cancellationToken)
        {
            try
            {
                return JObject.Parse(GetResponseAsync(_httpClientAppVeyor, buildDetails.AppVeyorBuildReportUrl, cancellationToken).Result);
            }
            catch (Exception)
            {
                var buildHistoryUrl = buildDetails.BaseApiUrl + "/history?recordsNumber=1&startBuildId=" + (int.Parse(buildDetails.BuildId) + 1);
                var builds = JObject.Parse(GetResponseAsync(_httpClientAppVeyor, buildHistoryUrl, cancellationToken).Result);

                var version = builds["builds"][0]["version"].ToObject<string>();
                buildDetails.Id = version;
                buildDetails.AppVeyorBuildReportUrl = buildDetails.BaseApiUrl + "/build/" + version;
                buildDetails.Url = buildDetails.BaseWebUrl + version;

                return JObject.Parse(GetResponseAsync(_httpClientAppVeyor, buildDetails.AppVeyorBuildReportUrl, cancellationToken).Result);
            }
        }

        private static BuildInfo.BuildStatus ParseBuildStatus(string statusValue)
        {
            switch (statusValue)
            {
                case "success":
                    return BuildInfo.BuildStatus.Success;
                case "failed":
                    return BuildInfo.BuildStatus.Failure;
                case "cancelled":
                    return BuildInfo.BuildStatus.Stopped;
                case "queued":
                case "running":
                    return BuildInfo.BuildStatus.InProgress;
                default:
                    return BuildInfo.BuildStatus.Unknown;
            }
        }

        private Task<Stream> GetStreamAsync(HttpClient httpClient, string restServicePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return httpClient.GetAsync(restServicePath, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                             .ContinueWith(
                                 task => GetStreamFromHttpResponseAsync(httpClient, task, restServicePath, cancellationToken),
                                 cancellationToken,
                                 restServicePath.Contains("github") ? TaskContinuationOptions.None : TaskContinuationOptions.AttachedToParent,
                                 TaskScheduler.Current)
                             .Unwrap();
        }

        private Task<Stream> GetStreamFromHttpResponseAsync(HttpClient httpClient, Task<HttpResponseMessage> task, string restServicePath, CancellationToken cancellationToken)
        {
            var retry = task.IsCanceled && !cancellationToken.IsCancellationRequested;

            if (retry)
                return GetStreamAsync(httpClient, restServicePath, cancellationToken);

            if (task.Status == TaskStatus.RanToCompletion && task.Result.IsSuccessStatusCode)
                return task.Result.Content.ReadAsStreamAsync();

            return null;
        }

        private Task<string> GetResponseAsync(HttpClient httpClient, string relativePath, CancellationToken cancellationToken)
        {
            var getStreamTask = GetStreamAsync(httpClient, relativePath, cancellationToken);

            var taskContinuationOptions = relativePath.Contains("github") ? TaskContinuationOptions.None : TaskContinuationOptions.AttachedToParent;
            return getStreamTask.ContinueWith(
                task =>
                {
                    if (task.Status != TaskStatus.RanToCompletion)
                        return string.Empty;
                    using (var responseStream = task.Result)
                    {
                        return new StreamReader(responseStream).ReadToEnd();
                    }
                },
                cancellationToken,
                taskContinuationOptions,
                TaskScheduler.Current);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _httpClientAppVeyor?.Dispose();
            _httpClientGitHub?.Dispose();
        }
    }

    internal class BuildDetails : BuildInfo
    {
        private static readonly IBuildDurationFormatter _buildDurationFormatter = new BuildDurationFormatter();
        private int _buildProgressCount;
        //From build build list
        public string BuildId { get; set; }
        public string CommitId { get; set; }
        public string AppVeyorBuildReportUrl { get; set; }
        public bool IsRunning => Status == BuildStatus.InProgress;
        public string Branch { get; set; }
        public string BaseApiUrl { get; set; }
        public string BaseWebUrl { get; set; }
        public string PullRequestText { get; set; }
        public string TestsResultText { get; set; }

        public void ChangeProgressCounter()
        {
            _buildProgressCount = _buildProgressCount % 3 + 1;
        }

        public void UpdateDescription()
        {
            Description = Id + PullRequestText + " " + DisplayStatus + " " + _buildDurationFormatter.Format(Duration) + TestsResultText;
        }

        private string DisplayStatus
        {
            get
            {

                if (Status != BuildStatus.InProgress)
                {
                    return Status.ToString("G");
                }
                return Status.ToString("G") + new string('.', _buildProgressCount) + new string(' ', 3 - _buildProgressCount);
            }
        }
    }
}
