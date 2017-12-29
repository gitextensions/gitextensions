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
using System.Threading;
using System.Threading.Tasks;
using GitCommands.Utils;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Newtonsoft.Json.Linq;

namespace JenkinsIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class JenkinsIntegrationMetadata : BuildServerAdapterMetadataAttribute
    {
        public JenkinsIntegrationMetadata(string buildServerType)
            : base(buildServerType) { }

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
    [JenkinsIntegrationMetadata("Jenkins")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class JenkinsAdapter : IBuildServerAdapter
    {
        private static readonly IBuildDurationFormatter _buildDurationFormatter = new BuildDurationFormatter();
        private IBuildServerWatcher _buildServerWatcher;

        private HttpClient _httpClient;

        //Build info cache, surviving repo switch
        private static readonly Dictionary<string, JenkinsCacheInfo> BuildCache = new Dictionary<string, JenkinsCacheInfo>();
        private readonly IList<string> _projectsUrls = new List<string>();
        
        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config, Func<string, bool> isCommitInRevisionGrid)
        {
            if (_buildServerWatcher != null)
                throw new InvalidOperationException("Already initialized");

            _buildServerWatcher = buildServerWatcher;

            var projectName = config.GetString("ProjectName", null);
            var hostName = config.GetString("BuildServerUrl", null);

            if (!string.IsNullOrEmpty(hostName) && !string.IsNullOrEmpty(projectName))
            {
                var baseAdress = hostName.Contains("://")
                                     ? new Uri(hostName, UriKind.Absolute)
                                     : new Uri(string.Format("{0}://{1}:8080", Uri.UriSchemeHttp, hostName), UriKind.Absolute);

                _httpClient = new HttpClient(new HttpClientHandler(){ UseDefaultCredentials = true});
                _httpClient.Timeout = TimeSpan.FromMinutes(2);
                _httpClient.BaseAddress = baseAdress;

                var buildServerCredentials = buildServerWatcher.GetBuildServerCredentials(this, true);

                UpdateHttpClientOptions(buildServerCredentials);

                string[] projectUrls = projectName.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var projectUrl in projectUrls.Select(s => baseAdress + "job/" + s.Trim() + "/"))
                {
                    AddGetBuildUrl(projectUrl);
                }
            }
        }

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        public string UniqueKey
        {
            get { return _httpClient.BaseAddress.Host; }
        }

        private void AddGetBuildUrl(string projectUrl)
        {
            if (!_projectsUrls.Contains(projectUrl))
            {
                _projectsUrls.Add(projectUrl);
                if (BuildCache.ContainsKey(projectUrl))
                {
                    //Make sure the cache is recalculated
                    BuildCache[projectUrl].Timestamp = 0;
                }
                else
                {
                    BuildCache[projectUrl] = new JenkinsCacheInfo();
                }

            }
        }

        public class ResponseInfo
        {
            public string Url { get; set; }
            public long Timestamp { get; set; }
            public IEnumerable<JToken> JobDescription { get; set; }
        }

        public class JenkinsCacheInfo
        {
            public long Timestamp = -1;
            public readonly IList<BuildInfo> BuildInfo = new List<BuildInfo>();
        }

        private Task<ResponseInfo> GetBuildInfoTask(string projectUrl, bool fullInfo, CancellationToken cancellationToken)
        {
            return GetResponseAsync(FormatToGetJson(projectUrl, fullInfo), cancellationToken)
                .ContinueWith(
                    task =>
                    {
                        long timestamp = 0;
                        IEnumerable<JToken> s = Enumerable.Empty<JToken>();
                        string t = task.Result;
                        if (t.IsNotNullOrWhitespace())
                        {
                            JObject jobDescription = JObject.Parse(t);
                            if (jobDescription["builds"] != null)
                            {
                                //Freestyle jobs
                                s = jobDescription["builds"];
                            }
                            else if (jobDescription["jobs"] != null)
                            {
                                //Multibranch pipeline
                                s = jobDescription["jobs"]
                                    .SelectMany(j => j["builds"]);
                                foreach (var j in jobDescription["jobs"])
                                {
                                    long ts = j["lastBuild"]["timestamp"].ToObject<long>();
                                    timestamp = Math.Max(timestamp, ts);
                                }
                            }
                            //else: The server had no response (overloaded?) or a multibranch pipeline is not configured

                            if (jobDescription["lastBuild"] != null)
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
                    },
                    TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.AttachedToParent);
        }

        public IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null)
        {
            //GetBuilds() will return the same builds as for GetRunningBuilds().
            //Multiple calls will fetch same info multiple times and make debugging very confusing
            //Similar as for AppVeyor
            return Observable.Empty<BuildInfo>();
        }

        public IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler)
        {
            return GetBuilds(scheduler, null, true);
        }

        private IObservable<BuildInfo> GetBuilds(IScheduler scheduler, DateTime? sinceDate = null, bool? running = null)
        {
            return Observable.Create<BuildInfo>((observer, cancellationToken) =>
                Task<IDisposable>.Factory.StartNew(
                    () => scheduler.Schedule(() => ObserveBuilds(sinceDate, running, observer, cancellationToken))));
        }

        //Jenkins builds are retrieved with latest build first
        //Prefer the inprogress builds to the finished
        //If there are many builds on the same commit, it is normally the latest that is interesting, ignore the following
        private bool DisplayBuild(BuildInfo curr, HashSet<string> inProgressDisplayed, HashSet<string> finishedDisplayed)
        {
            bool res = false;
            if (curr != null
                && curr.CommitHashList.Length >= 1
                && (curr.Status == BuildInfo.BuildStatus.InProgress
                    && inProgressDisplayed.Add(curr.CommitHashList[0])
                  || !inProgressDisplayed.Contains(curr.CommitHashList[0])
                    && finishedDisplayed.Add(curr.CommitHashList[0])))
            {
                res = true;
            }

            return res;
        }

        private void ObserveBuilds(DateTime? sinceDate, bool? running, IObserver<BuildInfo> observer, CancellationToken cancellationToken)
        {
            //Note that 'running' is ignored (attempt to fetch data when updated) 
            //Similar for 'sinceDate', not supported in Jenkins API
            try
            {
                IList<Task<ResponseInfo>> allBuildInfos = new List<Task<ResponseInfo>>();
                IList<Task<ResponseInfo>> latestBuildInfos = new List<Task<ResponseInfo>>();
                HashSet<string> inProgressDisplayed = new HashSet<string>();
                HashSet<string> finishedDisplayed = new HashSet<string>();
                foreach (var projectUrl in _projectsUrls)
                {
                    //Update the build info from the cache and find if (inprogress) jobs may need to be requeried
                    bool hasInProgress = false;
                    foreach (var buildInfo in BuildCache[projectUrl].BuildInfo)
                    {
                        if (DisplayBuild(buildInfo, inProgressDisplayed, finishedDisplayed))
                        {
                            observer.OnNext(buildInfo);
                        }

                        //If any displayed build is InProgress, it has to be requeried
                        //This could be done per build too, but probably decreasing load only in special situations
                        //(Updating all builds individually increases the total load)
                        if (buildInfo.Status == BuildInfo.BuildStatus.InProgress)
                        {
                            hasInProgress = true;
                        }
                    }

                    if (hasInProgress || BuildCache[projectUrl].Timestamp <= 0)
                    {
                        //This job must be updated, no need to to check the latest builds
                        allBuildInfos.Add(GetBuildInfoTask(projectUrl, true, cancellationToken));
                    }
                    else
                    {
                        //Check the latest build on the server to the existing build cache
                        latestBuildInfos.Add(GetBuildInfoTask(projectUrl, false, cancellationToken));
                    }
                }

                //Query the latest builds, to find if the cache has all builds
                foreach (var url in latestBuildInfos)
                {
                    if (!url.IsFaulted)
                    {
                        if (url.Result.Timestamp > BuildCache[url.Result.Url].Timestamp)
                        {
                            //The cache has at least one newer job, query the status
                            allBuildInfos.Add(GetBuildInfoTask(url.Result.Url, true, cancellationToken));
                        }
                    }
                }

                if (allBuildInfos.All(t => t.IsCanceled))
                {
                    observer.OnCompleted();
                    return;
                }


                foreach (var currentGetBuildUrls in allBuildInfos)
                {
                    if (currentGetBuildUrls.IsFaulted)
                    {
                        Debug.Assert(currentGetBuildUrls.Exception != null);

                        observer.OnError(currentGetBuildUrls.Exception);
                        continue;
                    }

                    if (currentGetBuildUrls.Result.Timestamp <= 0)
                    {
                        //No valid information received for the build, keep the cache
                        continue;
                    }

                    //InProgress may have finished. Clear the status about display so it can be overridden
                    //If "display order" should have been changed, this is updated at next update
                    inProgressDisplayed.Clear();

                    //Information for all builds for the job
                    BuildCache[currentGetBuildUrls.Result.Url].Timestamp = currentGetBuildUrls.Result.Timestamp;
                    BuildCache[currentGetBuildUrls.Result.Url].BuildInfo.Clear();
                    foreach (var buildDetails in currentGetBuildUrls.Result.JobDescription)
                    {
                        var buildInfo = CreateBuildInfo((JObject) buildDetails);
                        BuildCache[currentGetBuildUrls.Result.Url].BuildInfo.Add(buildInfo);
                        if (DisplayBuild(buildInfo, inProgressDisplayed, finishedDisplayed))
                        {
                            observer.OnNext(buildInfo);
                        }
                    }
                }

                //Complete the job, it will be run again with Observe.Retry() (every 10th sec)
                //(this will find new builds and update InProgress jobs)
                observer.OnCompleted();
            }
            catch (OperationCanceledException)
            {
                // Do nothing, the observer is already stopped
            }
            catch (Exception ex)
            {
                //Cancelling a subtask is similar to cancelling this task
                if (ex.InnerException == null || !(ex.InnerException is OperationCanceledException))
                {
                    observer.OnError(ex);
                }
            }
        }

        private readonly string JenkinsTreeBuildInfo = "number,result,timestamp,url,actions[lastBuiltRevision[SHA1],totalCount,failCount,skipCount],building,duration";
        private static BuildInfo CreateBuildInfo(JObject buildDescription)
        {
            var idValue = buildDescription["number"].ToObject<string>();
            var statusValue = buildDescription["result"].ToObject<string>();
            var startDateTicks = buildDescription["timestamp"].ToObject<long>();
            //var displayName = buildDescription["fullDisplayName"].ToObject<string>();
            var webUrl = buildDescription["url"].ToObject<string>();

            var action = buildDescription["actions"];
            var commitHashList = new List<string>();
            string testResults = string.Empty;
            foreach (var element in action)
            {
                if (element["lastBuiltRevision"] != null)
                    commitHashList.Add(element["lastBuiltRevision"]["SHA1"].ToObject<string>());
                if (element["totalCount"] != null)
                {
                    int nbTests = element["totalCount"].ToObject<int>();
                    if (nbTests != 0)
                    {
                        int nbFailedTests = element["failCount"].ToObject<int>();
                        int nbSkippedTests = element["skipCount"].ToObject<int>();
                        testResults = $"{nbTests} tests ({nbFailedTests} failed, {nbSkippedTests} skipped)";
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
            bool unauthorized = task.Status == TaskStatus.RanToCompletion &&
                                task.Result.StatusCode == HttpStatusCode.Unauthorized;

            if (task.IsFaulted || task.IsCanceled)
            {
                //No results for this task
                return null;
            }

            if (task.Result.IsSuccessStatusCode)
            {
                var httpContent = task.Result.Content;

                if (httpContent.Headers.ContentType.MediaType == "text/html")
                {
                    // Jenkins responds with an HTML login page when guest access is denied.
                    unauthorized = true;
                }
                else
                {
                    return httpContent.ReadAsStreamAsync();
                }
            }
            else if (task.Result.StatusCode == HttpStatusCode.NotFound)
            {
                //The url does not exist, no jobs to retrieve
                return null;
            }
            else if (task.Result.StatusCode == HttpStatusCode.Forbidden)
            {
                unauthorized = true;
            }

            if (unauthorized)
            {
                var buildServerCredentials = _buildServerWatcher.GetBuildServerCredentials(this, false);

                if (buildServerCredentials != null)
                {
                    UpdateHttpClientOptions(buildServerCredentials);

                    return GetStreamAsync(restServicePath, cancellationToken);
                }

                throw new OperationCanceledException(task.Result.ReasonPhrase);
            }

            throw new HttpRequestException(task.Result.ReasonPhrase);
#else
            return null;
#endif
        }

        private void UpdateHttpClientOptions(IBuildServerCredentials buildServerCredentials)
        {
            var useGuestAccess = buildServerCredentials == null || buildServerCredentials.UseGuestAccess;

            _httpClient.DefaultRequestHeaders.Authorization = useGuestAccess
                ? null : CreateBasicHeader(buildServerCredentials.Username, buildServerCredentials.Password);
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

        private string FormatToGetJson(string restServicePath, bool buildsInfo = false)
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
                    //Multi pipeline project
                    buildTree = "jobs[" + buildTree;
                    if (buildsInfo)
                    {
                        depth = 2;
                        buildTree += ",builds[" + JenkinsTreeBuildInfo + "]";
                    }
                    buildTree += "]";
                }
                else
                {
                    //user defined format (will likely require changes in the code)
                    buildTree = post;
                }

                restServicePath = restServicePath.Substring(0, postIndex);
            }
            else
            {
                //Freestyle project
                if (buildsInfo)
                {
                    buildTree += ",builds[" + JenkinsTreeBuildInfo + "]";
                }
            }

            if (!restServicePath.EndsWith("/"))
                restServicePath += "/";
            restServicePath += "api/json?depth=" + depth + "&tree=" + buildTree;
            return restServicePath;
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
}
