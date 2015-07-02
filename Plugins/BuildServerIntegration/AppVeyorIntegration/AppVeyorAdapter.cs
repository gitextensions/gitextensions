using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
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
        private const uint ProjectsToRetrieveCount = 50;
        private const string WebSiteUrl = "https://ci.appveyor.com";
        private const string ApiBaseUrl = WebSiteUrl + "/api/projects/";

        private IBuildServerWatcher _buildServerWatcher;

        private HttpClient _httpClient;

        private List<BuildDetails> _allBuilds = new List<BuildDetails>();
        private HashSet<string> _fetchBuilds;
        private string _accountToken;
        private static readonly Dictionary<string, Project> Projects = new Dictionary<string, Project>();
        private bool _shouldLoadTestResults;

        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config)
        {
            if (_buildServerWatcher != null)
                throw new InvalidOperationException("Already initialized");

            var accountName = config.GetString("AppVeyorAccountName", null);
            _accountToken = config.GetString("AppVeyorAccountToken", null);
            _shouldLoadTestResults = config.GetBool("AppVeyorLoadTestsResults", false);

            if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(_accountToken))
                return;

            _fetchBuilds = new HashSet<string>();
            _buildServerWatcher = buildServerWatcher;
            var projectNamesSetting = config.GetString("AppVeyorProjectName", null);

            _httpClient = new HttpClient(new HttpClientHandler { UseDefaultCredentials = true })
            {
                Timeout = TimeSpan.FromMinutes(2),
                BaseAddress = new Uri(WebSiteUrl, UriKind.Absolute)
            };

            UpdateHttpClientOptions();

            var useAllProjets = string.IsNullOrWhiteSpace(projectNamesSetting);
            string[] projectNames = null;
            if (!useAllProjets)
                projectNames = projectNamesSetting.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (Projects.Count == 0 ||
                (!useAllProjets && Projects.Keys.Intersect(projectNames).Count() != projectNames.Length))
            {
                GetResponseAsync(ApiBaseUrl, CancellationToken.None)
                    .ContinueWith(
                        task =>
                        {
                            var projects = JArray.Parse(task.Result);
                            foreach (var project in projects)
                            {
                                var projectId = project["slug"].ToString();
                                var projectName = project["name"].ToString();
                                if (useAllProjets)
                                {
                                    Projects.Add(projectName, new Project
                                    {
                                        Name = projectName,
                                        Id = projectId,
                                        QueryUrl = BuildQueryUrl(accountName, projectId)
                                    });
                                }
                                else
                                {
                                    if (projectNames.Contains(projectName))
                                        Projects.Add(projectName,
                                            new Project
                                            {
                                                Name = projectName,
                                                Id = projectId,
                                                QueryUrl = BuildQueryUrl(accountName, projectId)
                                            });
                                }
                            }
                        }).Wait();
            }
            var builds = Projects.Where(p => useAllProjets || projectNames.Contains(p.Value.Name)).Select(p => p.Value);
            _allBuilds =
                FilterBuilds(builds.SelectMany(project => QueryBuildsResults(accountName, project.Id, project.QueryUrl)));
        }

        private string BuildQueryUrl(string accountName, string projectId)
        {
            return ApiBaseUrl + accountName + "/" + projectId + "/history?recordsNumber=" + ProjectsToRetrieveCount;
        }

        private class Project
        {
            public string Name;
            public string Id;
            public string QueryUrl;
        }

        private List<BuildDetails> QueryBuildsResults(string accountName, string projectId, string projectUrl)
        {
            var buildTask = GetResponseAsync(projectUrl, CancellationToken.None)
                .ContinueWith(
                    task =>
                    {
                        var jobDescription = JObject.Parse(task.Result);
                        var builds = jobDescription["builds"];
                        var myBuilds = builds.Children();
                        var baseApiUrl = ApiBaseUrl + accountName + "/" + projectId;
                        var baseWebUrl = WebSiteUrl + "/project/" + accountName + "/" + projectId + "/build/";

                        return myBuilds.Select(b =>
                        {
                            var version = b["version"].ToObject<string>();
                            var status = ParseBuildStatus(b["status"].ToObject<string>());
                            string duration = string.Empty;
                            if (status == BuildInfo.BuildStatus.Success || status == BuildInfo.BuildStatus.Failure)
                            {
                                var startTime = b["started"].ToObject<DateTime>();
                                var updateTime = b["updated"].ToObject<DateTime>();
                                duration = " (" + (updateTime - startTime).ToString(@"mm\:ss") + ")";
                            }
                            return new BuildDetails
                            {
                                Id = version,
                                BuildId = b["buildId"].ToObject<string>(),
                                Branch = b["branch"].ToObject<string>(),
                                CommitId = b["commitId"].ToObject<string>(),
                                CommitHashList = new[] { b["commitId"].ToObject<string>() },
                                Status = status,
                                StartDate = b["started"] == null ? DateTime.MinValue : b["started"].ToObject<DateTime>(),
                                BaseWebUrl = baseWebUrl,
                                Url = WebSiteUrl + "/project/" + accountName + "/" + projectId + "/build/" + version,
                                BaseApiUrl = baseApiUrl,
                                AppVeyorBuildReportUrl = baseApiUrl + "/build/" + version,
                                Description = version + " " + status.ToString("G") + duration
                            };
                        }).ToList();
                    },
                    TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.AttachedToParent);
            return buildTask.Result;
        }

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        public string UniqueKey
        {
            get { return _httpClient.BaseAddress.Host; }
        }

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
                    observer.OnNext(build);
                }

                //Update finished build with tests results
                if (_shouldLoadTestResults)
                {
                    foreach (var build in _allBuilds.Where(b => b.Status == BuildInfo.BuildStatus.Success
                                                                || b.Status == BuildInfo.BuildStatus.Failure))
                    {
                        UpdateDescription(build, cancellationToken);
                        observer.OnNext(build);
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
                        observer.OnNext(build);
                    }
                    inProgressBuilds =
                        inProgressBuilds.Where(b => b.Status == BuildInfo.BuildStatus.InProgress).ToList();
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

        private int _buildProgressCount;

        private void UpdateDescription(BuildDetails buildDetails, CancellationToken cancellationToken)
        {
            var buildDetailsParsed = FetchBuildDetailsManagingVersionUpdate(buildDetails, cancellationToken);
            if (buildDetailsParsed == null)
                return;
            var buildData = buildDetailsParsed["build"];
            var buildDescription = buildData["jobs"].Last();

            var status = buildDescription["status"].ToObject<string>();
            buildDetails.Status = ParseBuildStatus(status);
            buildDetails.Description = buildDetails.Id + " " + buildDetails.Status.ToString("G");
            if (buildDetails.IsRunning)
            {
                _buildProgressCount = _buildProgressCount % 3 + 1;
                buildDetails.Description += new string('.', _buildProgressCount) + new string(' ', 3 - _buildProgressCount);
            }
            else
            {
                var startTime = buildData["started"].ToObject<DateTime>();
                var updateTime = buildData["updated"].ToObject<DateTime>();
                buildDetails.Description += " (" + (updateTime - startTime).ToString(@"mm\:ss") + ")";
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
            }

            buildDetails.Description = buildDetails.Description + " " + testResults;
        }

        private JObject FetchBuildDetailsManagingVersionUpdate(BuildDetails buildDetails, CancellationToken cancellationToken)
        {
            try
            {
                return JObject.Parse(GetResponseAsync(buildDetails.AppVeyorBuildReportUrl, cancellationToken).Result);
            }
            catch (Exception)
            {
                var buildHistoryUrl = buildDetails.BaseApiUrl + "/history?recordsNumber=1&startBuildId=" + (int.Parse(buildDetails.BuildId) + 1);
                var builds = JObject.Parse(GetResponseAsync(buildHistoryUrl, cancellationToken).Result);

                var version = builds["builds"][0]["version"].ToObject<string>();
                buildDetails.Id = version;
                buildDetails.AppVeyorBuildReportUrl = buildDetails.BaseApiUrl + "/build/" + version;
                buildDetails.Url = buildDetails.BaseWebUrl + version;

                return JObject.Parse(GetResponseAsync(buildDetails.AppVeyorBuildReportUrl, cancellationToken).Result);
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

        private Task<Stream> GetStreamAsync(string restServicePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return _httpClient.GetAsync(restServicePath, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                             .ContinueWith(
                                 task => GetStreamFromHttpResponseAsync(task, restServicePath, cancellationToken),
                                 cancellationToken,
                                 TaskContinuationOptions.AttachedToParent,
                                 TaskScheduler.Current)
                             .Unwrap();
        }

        private Task<Stream> GetStreamFromHttpResponseAsync(Task<HttpResponseMessage> task, string restServicePath, CancellationToken cancellationToken)
        {
#if !__MonoCS__
            var retry = task.IsCanceled && !cancellationToken.IsCancellationRequested;

            if (retry)
                return GetStreamAsync(restServicePath, cancellationToken);

            if (task.Result.IsSuccessStatusCode)
                return task.Result.Content.ReadAsStreamAsync();

            throw new HttpRequestException(task.Result.ReasonPhrase);
#else
            return null;
#endif
        }

        private void UpdateHttpClientOptions()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountToken);
        }

        private Task<string> GetResponseAsync(string relativePath, CancellationToken cancellationToken)
        {
            var getStreamTask = GetStreamAsync(relativePath, cancellationToken);

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
                TaskContinuationOptions.AttachedToParent,
                TaskScheduler.Current);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }
    }

    internal class BuildDetails : BuildInfo
    {
        //From build build list
        public string BuildId { get; set; }
        public string CommitId { get; set; }
        public string AppVeyorBuildReportUrl { get; set; }
        public bool IsRunning { get { return Status == BuildStatus.InProgress; } }
        public string Branch { get; set; }
        public string BaseApiUrl { get; set; }
        public string BaseWebUrl { get; set; }
    }
}
