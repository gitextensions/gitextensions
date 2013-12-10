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
using GitCommands.Settings;
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
                return ".Net 4 full freamwork required";
            }
        }
    }



    [Export(typeof(IBuildServerAdapter))]
    [JenkinsIntegrationMetadata("Jenkins")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class JenkinsAdapter : IBuildServerAdapter
    {
        private IBuildServerWatcher _buildServerWatcher;

        private HttpClient _httpClient;

        private Task<IEnumerable<string>> _getBuildTypesTask;

        private string ProjectName { get; set; }
        private string ProjectUrl { get; set; }

        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config)
        {
            if (_buildServerWatcher != null)
                throw new InvalidOperationException("Already initialized");

            _buildServerWatcher = buildServerWatcher;

            ProjectName = config.GetString("ProjectName", null);
            var hostName = config.GetString("BuildServerUrl", null);
            if (!string.IsNullOrEmpty(hostName))
            {
                var baseAdress = hostName.Contains("://")
                                     ? new Uri(hostName, UriKind.Absolute)
                                     : new Uri(string.Format("{0}://{1}:8080", Uri.UriSchemeHttp, hostName), UriKind.Absolute);
                
                _httpClient = new HttpClient
                    {
                        Timeout = TimeSpan.FromMinutes(2),
                        BaseAddress = baseAdress
                    };

                var buildServerCredentials = buildServerWatcher.GetBuildServerCredentials(this, true);

                UpdateHttpClientOptions(buildServerCredentials);

                ProjectUrl = baseAdress + "job/" + ProjectName + "/";

                if (!string.IsNullOrEmpty(ProjectName))
                {
                    _getBuildTypesTask =
                        GetProjectFromNameJsonResponseAsync(CancellationToken.None)
                            .ContinueWith(
                                task =>
                                    {
                                        JObject jobDescription = JObject.Parse(task.Result);
                                        return jobDescription["builds"].Select(b => b["number"].ToObject<string>());
                                    },
                                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.AttachedToParent);
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

        public IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null)
        {
            return GetBuilds(scheduler, sinceDate, false);
        }

        public IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler)
        {
            return GetBuilds(scheduler, null, true);
        }

        public IObservable<BuildInfo> GetBuilds(IScheduler scheduler, DateTime? sinceDate = null, bool? running = null)
        {
            if (_httpClient == null || _httpClient.BaseAddress == null || string.IsNullOrEmpty(ProjectName))
            {
                return Observable.Empty<BuildInfo>(scheduler);
            }

            return Observable.Create<BuildInfo>((observer, cancellationToken) =>
                Task<IDisposable>.Factory.StartNew(
                    () => scheduler.Schedule(() => ObserveBuilds(sinceDate, running, observer, cancellationToken))));
        }

        private void ObserveBuilds(DateTime? sinceDate, bool? running, IObserver<BuildInfo> observer, CancellationToken cancellationToken)
        {
            try
            {
                if (PropagateTaskAnomalyToObserver(_getBuildTypesTask, observer))
                {
                    return;
                }

                var buildIds = _getBuildTypesTask.Result;
                var buildTasks = buildIds.Select(buildId => GetResponseAsync(FormatToGetJson(ProjectUrl + buildId), cancellationToken)).ToArray();

                foreach (var buildDetails in buildTasks.Select(b=>b.Result))
                {
                    JObject buildDescription = JObject.Parse(buildDetails);
                    var startDate = TimestampToDateTime(buildDescription["timestamp"].ToObject<long>());
                    var isRunning = buildDescription["building"].ToObject<bool>();

                    if (sinceDate.HasValue && sinceDate.Value > startDate)
                        continue;

                    if (running.HasValue && running.Value != isRunning)
                        continue;

                    var buildInfo = CreateBuildInfo(buildDescription);
                    if (buildInfo.CommitHashList.Any())
                    {
                        observer.OnNext(buildInfo);
                    }
                }
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

        private static bool PropagateTaskAnomalyToObserver(Task task, IObserver<BuildInfo> observer)
        {
            if (task.IsCanceled)
            {
                observer.OnCompleted();
                return true;
            }

            if (task.IsFaulted)
            {
                Debug.Assert(task.Exception != null);

                observer.OnError(task.Exception);
                return true;
            }

            return false;
        }

        private static BuildInfo CreateBuildInfo(JObject buildDescription)
        {
            var idValue = buildDescription["number"].ToObject<string>();
            var statusValue = buildDescription["result"].ToObject<string>();
            var startDateTicks = buildDescription["timestamp"].ToObject<long>();
            var displayName = buildDescription["fullDisplayName"].ToObject<string>();
            var webUrl = buildDescription["url"].ToObject<string>();
            var action = buildDescription["actions"];
            var commitHashList = new List<string>();
            int nbTests = 0;
            int nbFailedTests = 0;
            int nbSkippedTests = 0;
            foreach (var element in action)
            {
                if (element["lastBuiltRevision"] != null)
                    commitHashList.Add(element["lastBuiltRevision"]["SHA1"].ToObject<string>());
                if (element["totalCount"] != null)
                {
                    nbTests = element["totalCount"].ToObject<int>();
                    nbFailedTests = element["failCount"].ToObject<int>();
                    nbSkippedTests = element["skipCount"].ToObject<int>();
                }
            }

            string testResults = string.Empty;
            if (nbTests != 0)
            {
                testResults = String.Format(" : {0} tests ( {1} failed, {2} skipped )", nbTests, nbFailedTests, nbSkippedTests);
            }

            var isRunning = buildDescription["building"].ToObject<bool>();

            var status = ParseBuildStatus(statusValue);
            var statusText = isRunning ? string.Empty : status.ToString("G");
            var buildInfo = new BuildInfo
                {
                    Id = idValue,
                    StartDate = TimestampToDateTime(startDateTicks),
                    Status = isRunning ? BuildInfo.BuildStatus.InProgress : status,
                    Description = displayName + " " + statusText + testResults,
                    CommitHashList = commitHashList.ToArray(),
                    Url = webUrl
                };
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
            bool retry = task.IsCanceled && !cancellationToken.IsCancellationRequested;
            bool unauthorized = task.Status == TaskStatus.RanToCompletion &&
                                task.Result.StatusCode == HttpStatusCode.Unauthorized;

            if (!retry)
            {
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
            }

            if (retry)
            {
                return GetStreamAsync(restServicePath, cancellationToken);
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
                        using (var responseStream = task.Result)
                        {
                            return new StreamReader(responseStream).ReadToEnd();
                        }
                    },
                cancellationToken, 
                TaskContinuationOptions.AttachedToParent, 
                TaskScheduler.Current);
        }

        private string FormatToGetJson(string restServicePath)
        {
            if (!restServicePath.EndsWith("/"))
                restServicePath += "/";
            return restServicePath + "api/json";
        }

        private Task<string> GetProjectFromNameJsonResponseAsync(CancellationToken cancellationToken)
        {
            return GetResponseAsync(FormatToGetJson(ProjectUrl), cancellationToken);
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
