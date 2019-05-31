using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GitCommands.Utils;
using GitUI;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GitlabIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class GitlabIntegrationMetadata : BuildServerAdapterMetadataAttribute
    {
        public GitlabIntegrationMetadata(string buildServerType)
            : base(buildServerType)
        {
        }

        public override string CanBeLoaded
        {
            get
            {
                if (EnvUtils.IsNet4FullOrHigher())
                {
                    return null;
                }

                return ".Net 4 full framework required";
            }
        }
    }

    [Export(typeof(IBuildServerAdapter))]
    [GitlabIntegrationMetadata(PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class GitlabAdapter : IBuildServerAdapter
    {
        public const string PluginName = "Gitlab";
        private static readonly IBuildDurationFormatter _buildDurationFormatter = new BuildDurationFormatter();
        private IBuildServerWatcher _buildServerWatcher;

        private HttpClient _httpClient;

        // private string _projectID;

        // key is the project's id
        private readonly Dictionary<string, GitlabCacheInfo> _lastBuildCache = new Dictionary<string, GitlabCacheInfo>();

        private readonly List<string> _projectsIDs = new List<string>();

        // private readonly string _projectsUrl;

        // private Regex _ignoreBuilds;

        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config, Func<ObjectId, bool> isCommitInRevisionGrid = null)
        {
            if (_buildServerWatcher != null)
            {
                throw new InvalidOperationException("Already initialized");
            }

            _buildServerWatcher = buildServerWatcher;

            var projectName = config.GetString("ProjectName", null);
            var hostName = config.GetString("BuildServerUrl", null);
            var userToken = config.GetString("GitlabApiToken", null);

            if (!string.IsNullOrEmpty(hostName) && !string.IsNullOrEmpty(projectName))
            {
                var baseAddress = hostName.Contains("://")
                    ? new Uri(hostName + "api/v4/", UriKind.Absolute)
                    : new Uri($"{Uri.UriSchemeHttp}://{hostName}/api/v4/", UriKind.Absolute);

                _httpClient = new HttpClient(new HttpClientHandler { UseDefaultCredentials = true })
                {
                    Timeout = TimeSpan.FromMinutes(2),
                    BaseAddress = baseAddress
                };

                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // _httpClient.DefaultRequestHeaders.Add("Private-Token", userToken);

                var buildServerCredentials = buildServerWatcher.GetBuildServerCredentials(this, true);
                UpdateHttpClientOptions(buildServerCredentials);

                string[] projectUrls = projectName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var projectUrl in projectUrls.Select(s => s.Trim()))
                {
                    AddGetBuildUrl(projectUrl);
                }

                // _projectID = ThreadHelper.JoinableTaskFactory.RunAsync(() => GetProjectIdAsync(projectName)).Join();

                /*
                UpdateHttpClientOptions(buildServerCredentials);

                string[] projectUrls = _buildServerWatcher.ReplaceVariables(projectName)
                    .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var projectUrl in projectUrls.Select(s => baseAddress + "job/" + s.Trim() + "/"))
                {
                    AddGetBuildUrl(projectUrl);
                }*/
            }
        }

        // <summary>
        // Gets a unique key which identifies this build server.
        // </summary>
        public string UniqueKey => _httpClient.BaseAddress.Host;

        private void AddGetBuildUrl(string projectName)
        {
            var prjID = ThreadHelper.JoinableTaskFactory.RunAsync(() => GetProjectIdAsync(projectName)).Join();

            if (!prjID.IsNullOrEmpty() && !_projectsIDs.Contains(prjID))
            {
                _projectsIDs.Add(prjID);
                _lastBuildCache[prjID] = new GitlabCacheInfo();
            }
        }

        public class ResponseInfo
        {
            public string Url { get; set; }
            public long Timestamp { get; set; }
            public IEnumerable<JToken> JobDescription { get; set; }
        }

        public class GitlabCacheInfo
        {
            public long Timestamp = -1;
        }

        public class ProjectSearchInfo
        {
            public string id { get; set; }
        }

        private async Task<string> GetProjectIdAsync(string prjName)
        {
            string retVal = "";

            // Get the Project ID from the name
            HttpResponseMessage response = await _httpClient.GetAsync(string.Format("search?scope=projects&search={0}", prjName));
            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<List<ProjectSearchInfo>>(await response.Content.ReadAsStringAsync());
                retVal = result[0].id;
            }

            return retVal;
        }

        private async Task<ResponseInfo> GetBuildInfoTaskAsync(string projectUrl, bool fullInfo, CancellationToken cancellationToken)
        {
            string t = null;
            long timestamp = 0;

            IEnumerable<JToken> s = Enumerable.Empty<JToken>();

            try
            {
                t = await GetResponseAsync(FormatToGetJson(projectUrl, fullInfo), cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Could be cancelled or failed. Explicitly assign 't' to reveal the intended behavior of following code
                // for this case.
                t = null;
            }

            if (t.IsNotNullOrWhitespace() && !cancellationToken.IsCancellationRequested)
            {
                JObject jobDescription = JObject.Parse(t);
                if (jobDescription["builds"] != null)
                {
                    // Freestyle jobs
                    s = jobDescription["builds"];
                }
                else if (jobDescription["jobs"] != null)
                {
                    // Multi-branch pipeline
                    s = jobDescription["jobs"]
                        .SelectMany(j => j["builds"]);
                    foreach (var j in jobDescription["jobs"])
                    {
                        var ts = j["lastBuild"]["timestamp"];
                        if (ts != null)
                        {
                            timestamp = Math.Max(timestamp, ts.ToObject<long>());
                        }
                    }
                }

                // else: The server had no response (overloaded?) or a multi-branch pipeline is not configured
                if (timestamp == 0 && jobDescription["lastBuild"]?["timestamp"] != null)
                {
                    timestamp = jobDescription["lastBuild"]["timestamp"].ToObject<long>();
                }
            }

            return new ResponseInfo
            {
                Url = projectUrl,
                Timestamp = timestamp,
                JobDescription = s
            };
        }

        public IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null)
        {
            // GetBuilds() will return the same builds as for GetRunningBuilds().
            // Multiple calls will fetch same info multiple times and make debugging very confusing
            // Similar as for AppVeyor
            return Observable.Empty<BuildInfo>();
        }

        public IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler)
        {
            return GetBuilds(scheduler, null, true);
        }

        private IObservable<BuildInfo> GetBuilds(IScheduler scheduler, DateTime? sinceDate = null, bool? running = null)
        {
            return Observable.Create<BuildInfo>((observer, cancellationToken) =>
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    await TaskScheduler.Default;
                    return scheduler.Schedule(() => ObserveBuilds(sinceDate, running, observer, cancellationToken));
                }).Task);
        }

        private void ObserveBuilds(DateTime? sinceDate, bool? running, IObserver<BuildInfo> observer, CancellationToken cancellationToken)
        {
            // Note that 'running' is ignored (attempt to fetch data when updated)
            // Similar for 'sinceDate', not supported in Gitlab API
            try
            {
                var allBuildInfos = new List<JoinableTask<ResponseInfo>>();
                var latestBuildInfos = new List<JoinableTask<ResponseInfo>>();

                /* foreach (var projectUrl in _projectsIDs)
                {
                    if (_lastBuildCache[projectUrl].Timestamp <= 0)
                    {
                        // This job must be updated, no need to to check the latest builds
                        allBuildInfos.Add(ThreadHelper.JoinableTaskFactory.RunAsync(() => GetBuildInfoTaskAsync(projectUrl, true, cancellationToken)));
                    }
                    else
                    {
                        latestBuildInfos.Add(ThreadHelper.JoinableTaskFactory.RunAsync(() => GetBuildInfoTaskAsync(projectUrl, false, cancellationToken)));
                    }
                }*/

                // Check the latest build on the server to the existing build cache
                // The simple request will limit the load on the Gitlab server
                // To fetch just new builds is possible too, but it will make the solution more complicated
                // Similar, the build results could be cached so they are available when switching repos
                foreach (var info in latestBuildInfos)
                {
                    if (!info.Task.IsFaulted)
                    {
                        if (info.Join().Timestamp > _lastBuildCache[info.Join().Url].Timestamp)
                        {
                            // The cache has at least one newer job, query the status
                            allBuildInfos.Add(ThreadHelper.JoinableTaskFactory.RunAsync(() => GetBuildInfoTaskAsync(info.Task.CompletedResult().Url, true, cancellationToken)));
                        }
                    }
                }

                if (allBuildInfos.All(t => t.Task.IsCanceled))
                {
                    observer.OnCompleted();
                    return;
                }

                foreach (var build in allBuildInfos)
                {
                    if (build.Task.IsFaulted)
                    {
                        Debug.Assert(build.Task.Exception != null, "build.Task.Exception != null");

                        observer.OnError(build.Task.Exception);
                        continue;
                    }

                    if (build.Task.IsCanceled || build.Join().Timestamp <= 0)
                    {
                        // No valid information received for the build
                        continue;
                    }

                    _lastBuildCache[build.Join().Url].Timestamp = build.Join().Timestamp;

                    // Present information in reverse, so the latest job is displayed (i.e. new inprogress on one commit)
                    // (for multi-branch pipeline, ignore the corner case with multiple branches with inprogress builds on one commit)
                    foreach (var buildDetails in build.Join().JobDescription.Reverse())
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        try
                        {
                            var buildInfo = CreateBuildInfo((JObject)buildDetails);

                            if (buildInfo != null)
                            {
                                observer.OnNext(buildInfo);

                                if (buildInfo.Status == BuildInfo.BuildStatus.InProgress)
                                {
                                    // Need to make a full request next time
                                    _lastBuildCache[build.Join().Url].Timestamp = 0;
                                }
                            }
                        }
                        catch
                        {
                            // Ignore unexpected responses
                        }
                    }
                }

                // Complete the job, it will be run again with Observe.Retry() (every 10th sec)
                observer.OnCompleted();
            }
            catch (OperationCanceledException)
            {
                // Do nothing, the observer is already stopped
            }
            catch (Exception ex)
            {
                // Cancelling a sub-task is similar to cancelling this task
                if (!(ex.InnerException is OperationCanceledException))
                {
                    observer.OnError(ex);
                }
            }
        }

        private const string _jenkinsTreeBuildInfo = "number,result,timestamp,url,actions[lastBuiltRevision[SHA1,branch[name]],totalCount,failCount,skipCount],building,duration";

        [CanBeNull]
        private BuildInfo CreateBuildInfo(JObject buildDescription)
        {
            var idValue = buildDescription["number"].ToObject<string>();
            var statusValue = buildDescription["result"].ToObject<string>();
            var startDateTicks = buildDescription["timestamp"].ToObject<long>();
            var webUrl = buildDescription["url"].ToObject<string>();

            var action = buildDescription["actions"];
            var commitHashList = new List<ObjectId>();
            string testResults = string.Empty;
            foreach (var element in action)
            {
                if (element["lastBuiltRevision"] != null)
                {
                    commitHashList.Add(ObjectId.Parse(element["lastBuiltRevision"]["SHA1"].ToObject<string>()));
                    var branches = element["lastBuiltRevision"]["branch"];
                    /*if (_ignoreBuilds != null && branches != null)
                    {
                        // Ignore build events for specified branches
                        foreach (var branch in branches)
                        {
                            var name = branch["name"];
                            if (name != null)
                            {
                                var name2 = name.ToObject<string>();
                                if (name2.IsNotNullOrWhitespace() && _ignoreBuilds.IsMatch(name2))
                                {
                                    return null;
                                }
                            }
                        }
                    }*/
                }

                if (element["totalCount"] != null)
                {
                    int testCount = element["totalCount"].ToObject<int>();
                    if (testCount != 0)
                    {
                        int failedTestCount = element["failCount"].ToObject<int>();
                        int skippedTestCount = element["skipCount"].ToObject<int>();
                        testResults = $"{testCount} tests ({failedTestCount} failed, {skippedTestCount} skipped)";
                    }
                }
            }

            var isRunning = buildDescription["building"].ToObject<bool>();
            long? buildDuration;
            if (isRunning)
            {
                buildDuration = null;
            }
            else
            {
                buildDuration = buildDescription["duration"].ToObject<long>();
            }

            var status = isRunning ? BuildInfo.BuildStatus.InProgress : ParseBuildStatus(statusValue);
            var statusText = status.ToString("G");
            var buildInfo = new BuildInfo
            {
                Id = idValue,
                StartDate = TimestampToDateTime(startDateTicks),
                Duration = buildDuration,
                Status = status,
                CommitHashList = commitHashList.ToArray(),
                Url = webUrl
            };
            var durationText = _buildDurationFormatter.Format(buildInfo.Duration);
            buildInfo.Description = $"#{idValue} {durationText} {testResults} {statusText}";
            return buildInfo;
        }

        public static DateTime TimestampToDateTime(long timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTime.Now.Kind).AddMilliseconds(timestamp);
        }

        private static AuthenticationHeaderValue CreateBasicHeader(string username, string password)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, password));
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        private static BuildInfo.BuildStatus ParseBuildStatus(string statusValue)
        {
            switch (statusValue)
            {
                case "SUCCESS":
                    return BuildInfo.BuildStatus.Success;
                case "FAILURE":
                    return BuildInfo.BuildStatus.Failure;
                case "UNSTABLE":
                    return BuildInfo.BuildStatus.Unstable;
                case "ABORTED":
                    return BuildInfo.BuildStatus.Stopped;
                default:
                    return BuildInfo.BuildStatus.Unknown;
            }
        }

        private async Task<Stream> GetStreamAsync(string restServicePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = await _httpClient.GetAsync(restServicePath, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            return await GetStreamFromHttpResponseAsync(response);

            async Task<Stream> GetStreamFromHttpResponseAsync(HttpResponseMessage resp)
            {
                bool unauthorized = resp.StatusCode == HttpStatusCode.Unauthorized;

                if (resp.IsSuccessStatusCode)
                {
                    var httpContent = resp.Content;

                    if (httpContent.Headers.ContentType.MediaType == "text/html")
                    {
                        // Gitlab responds with an HTML login page when guest access is denied.
                        unauthorized = true;
                    }
                    else
                    {
                        return await httpContent.ReadAsStreamAsync();
                    }
                }
                else if (resp.StatusCode == HttpStatusCode.NotFound)
                {
                    // The url does not exist, no jobs to retrieve
                    return null;
                }
                else if (resp.StatusCode == HttpStatusCode.Forbidden)
                {
                    unauthorized = true;
                }

                if (unauthorized)
                {
                    var buildServerCredentials = _buildServerWatcher.GetBuildServerCredentials(this, false);

                    /*if (buildServerCredentials != null)
                    {
                        UpdateHttpClientOptions(buildServerCredentials);

                        return await GetStreamAsync(restServicePath, cancellationToken);
                    }*/

                    throw new OperationCanceledException(resp.ReasonPhrase);
                }

                throw new HttpRequestException(resp.ReasonPhrase);
            }
        }

        private void UpdateHttpClientOptions(IBuildServerCredentials buildServerCredentials)
        {
            var useGuestAccess = buildServerCredentials == null || buildServerCredentials.UseGuestAccess;

            /*_httpClient.DefaultRequestHeaders.Authorization = useGuestAccess
                ? null : CreateBasicHeader(buildServerCredentials.Username, buildServerCredentials.Password);*/

            if (useGuestAccess == true)
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Add("Private-Token", buildServerCredentials.Password);
            }
        }

        private async Task<string> GetResponseAsync(string relativePath, CancellationToken cancellationToken)
        {
            using (var responseStream = await GetStreamAsync(relativePath, cancellationToken).ConfigureAwait(false))
            {
                using (var reader = new StreamReader(responseStream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        private static string FormatToGetJson(string restServicePath, bool buildsInfo = false)
        {
            string buildTree = "lastBuild[timestamp]";
            int depth = 1;
            int postIndex = restServicePath.IndexOf('?');
            if (postIndex >= 0)
            {
                int endLen = restServicePath.Length - postIndex;
                if (restServicePath.EndsWith("/"))
                {
                    endLen--;
                }

                string post = restServicePath.Substring(postIndex, endLen);
                if (post == "?m")
                {
                    // Multi pipeline project
                    buildTree = "jobs[" + buildTree;
                    if (buildsInfo)
                    {
                        depth = 2;
                        buildTree += ",builds[" + _jenkinsTreeBuildInfo + "]";
                    }

                    buildTree += "]";
                }
                else
                {
                    // user defined format (will likely require changes in the code)
                    buildTree = post;
                }

                restServicePath = restServicePath.Substring(0, postIndex);
            }
            else
            {
                // Freestyle project
                if (buildsInfo)
                {
                    buildTree += ",builds[" + _jenkinsTreeBuildInfo + "]";
                }
            }

            if (!restServicePath.EndsWith("/"))
            {
                restServicePath += "/";
            }

            restServicePath += "api/json?depth=" + depth + "&tree=" + buildTree;
            return restServicePath;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _httpClient?.Dispose();
        }
    }
}
