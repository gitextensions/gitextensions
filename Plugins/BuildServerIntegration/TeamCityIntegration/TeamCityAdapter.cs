using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
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
using System.Xml.Linq;
using System.Xml.XPath;
using GitCommands.Settings;
using GitCommands.Utils;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft.Win32;

namespace TeamCityIntegration
{

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TeamCityIntegrationMetadata : BuildServerAdapterMetadataAttribute
    {
        public TeamCityIntegrationMetadata(string buildServerType)
            : base(buildServerType)
        {
        }

        public override string CanBeLoaded
        {
            get
            {
                if (EnvUtils.IsNet4FullOrHigher())
                    return null;
                else
                    return ".Net 4 full freamwork required";
            }
        }
    }



    [Export(typeof(IBuildServerAdapter))]
    [TeamCityIntegrationMetadata("TeamCity")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class TeamCityAdapter : IBuildServerAdapter
    {
        private IBuildServerWatcher buildServerWatcher;

        private HttpClient httpClient;

        private string httpClientHostSuffix;

        private Task<IEnumerable<string>> getBuildTypesTask;

        private string ProjectName { get; set; }

        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config)
        {
            if (this.buildServerWatcher != null)
                throw new InvalidOperationException("Already initialized");

            this.buildServerWatcher = buildServerWatcher;

            ProjectName = config.GetString("ProjectName", null);
            var hostName = config.GetString("BuildServerUrl", null);
            if (!string.IsNullOrEmpty(hostName))
            {
                httpClient = new HttpClient
                    {
                        Timeout = TimeSpan.FromMinutes(2),
                        BaseAddress = hostName.Contains("://")
                                          ? new Uri(hostName, UriKind.Absolute)
                                          : new Uri(string.Format("{0}://{1}", Uri.UriSchemeHttp, hostName), UriKind.Absolute)
                    };
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                var buildServerCredentials = buildServerWatcher.GetBuildServerCredentials(this, true);

                UpdateHttpClientOptions(buildServerCredentials);

                if (!string.IsNullOrEmpty(ProjectName))
                {
                    getBuildTypesTask =
                        GetProjectFromNameXmlResponseAsync(ProjectName, CancellationToken.None)
                            .ContinueWith(
                                task => from element in task.Result.XPathSelectElements("/project/buildTypes/buildType")
                                        select element.Attribute("id").Value,
                                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.AttachedToParent);
                }
            }
        }

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        public string UniqueKey
        {
            get { return httpClient.BaseAddress.Host; }
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
            if (httpClient == null || httpClient.BaseAddress == null || string.IsNullOrEmpty(ProjectName))
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
                if (PropagateTaskAnomalyToObserver(getBuildTypesTask, observer))
                {
                    return;
                }

                var localObserver = observer;
                var buildTypes = getBuildTypesTask.Result;
                var buildIdTasks = buildTypes.Select(buildTypeId => GetFilteredBuildsXmlResponseAsync(buildTypeId, cancellationToken, sinceDate, running)).ToArray();

                Task.Factory
                    .ContinueWhenAll(
                        buildIdTasks,
                        completedTasks =>
                            {
                                var buildIds = completedTasks.Where(task => task.Status == TaskStatus.RanToCompletion)
                                                             .SelectMany(
                                                                 buildIdTask =>
                                                                 buildIdTask.Result
                                                                            .XPathSelectElements("/builds/build")
                                                                            .Select(x => x.Attribute("id").Value))
                                                             .ToArray();

                                NotifyObserverOfBuilds(buildIds, observer, cancellationToken);
                            },
                        cancellationToken,
                        TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously,
                        TaskScheduler.Current)
                    .ContinueWith(
                        task => localObserver.OnError(task.Exception),
                        CancellationToken.None,
                        TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
                        TaskScheduler.Current);
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

        private void NotifyObserverOfBuilds(string[] buildIds, IObserver<BuildInfo> observer, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>(8);
            var buildsLeft = buildIds.Length;

            foreach (var buildId in buildIds.OrderByDescending(int.Parse))
            {
                var notifyObserverTask =
                    GetBuildFromIdXmlResponseAsync(buildId, cancellationToken)
                        .ContinueWith(
                            task =>
                                {
                                    if (task.Status == TaskStatus.RanToCompletion)
                                    {
                                        var buildDetails = task.Result;
                                        var buildInfo = CreateBuildInfo(buildDetails);
                                        if (buildInfo.CommitHashList.Any())
                                        {
                                            observer.OnNext(buildInfo);
                                        }
                                    }
                                },
                            cancellationToken,
                            TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously,
                            TaskScheduler.Current);
                
                tasks.Add(notifyObserverTask);
                --buildsLeft;

                if (tasks.Count == tasks.Capacity || buildsLeft == 0)
                {
                    var batchTasks = tasks.ToArray();
                    tasks.Clear();

                    try
                    {
                        Task.WaitAll(batchTasks, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        observer.OnError(e);
                        return;
                    }
                }
            }

            observer.OnCompleted();
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

        private static BuildInfo CreateBuildInfo(XDocument buildXmlDocument)
        {
            var buildXElement = buildXmlDocument.Element("build");
            var idValue = buildXElement.Attribute("id").Value;
            var statusValue = buildXElement.Attribute("status").Value;
            var startDateText = buildXElement.Element("startDate").Value;
            var statusText = buildXElement.Element("statusText").Value;
            var webUrl = buildXElement.Attribute("webUrl").Value;
            var revisionsElements = buildXElement.XPathSelectElements("revisions/revision");
            var commitHashList = revisionsElements.Select(x => x.Attribute("version").Value).ToArray();
            var runningAttribute = buildXElement.Attribute("running");

            if (runningAttribute != null && Convert.ToBoolean(runningAttribute.Value))
            {
                var runningInfoXElement = buildXElement.Element("running-info");
                var currentStageText = runningInfoXElement.Attribute("currentStageText").Value;

                statusText = currentStageText;
            }

            var buildInfo = new BuildInfo
                {
                    Id = idValue,
                    StartDate = DecodeJsonDateTime(startDateText),
                    Status = ParseBuildStatus(statusValue),
                    Description = statusText,
                    CommitHashList = commitHashList,
                    Url = webUrl
                };
            return buildInfo;
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
                default:
                    return BuildInfo.BuildStatus.Unknown;
            }
        }

        private Task<Stream> GetStreamAsync(string restServicePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return httpClient.GetAsync(FormatRelativePath(restServicePath), HttpCompletionOption.ResponseHeadersRead, cancellationToken)
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
                        // TeamCity responds with an HTML login page when guest access is denied.
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
                var buildServerCredentials = buildServerWatcher.GetBuildServerCredentials(this, false);

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

            if (useGuestAccess)
            {
                httpClientHostSuffix = "guestAuth";
                httpClient.DefaultRequestHeaders.Authorization = null;
            }
            else
            {
                httpClientHostSuffix = "httpAuth";
                httpClient.DefaultRequestHeaders.Authorization = CreateBasicHeader(buildServerCredentials.Username, buildServerCredentials.Password);
            }
        }

        private Task<XDocument> GetXmlResponseAsync(string relativePath, CancellationToken cancellationToken)
        {
            var getStreamTask = GetStreamAsync(relativePath, cancellationToken);

            return getStreamTask.ContinueWith(
                task =>
                    {
                        using (var responseStream = task.Result)
                        {
                            return XDocument.Load(responseStream);
                        }
                    },
                cancellationToken, 
                TaskContinuationOptions.AttachedToParent, 
                TaskScheduler.Current);
        }

        private Uri FormatRelativePath(string restServicePath)
        {
            return new Uri(string.Format("{0}/app/rest/{1}", httpClientHostSuffix, restServicePath), UriKind.Relative);
        }

        private Task<XDocument> GetBuildFromIdXmlResponseAsync(string buildId, CancellationToken cancellationToken)
        {
            return GetXmlResponseAsync(string.Format("builds/id:{0}", buildId), cancellationToken);
        }

        private Task<XDocument> GetProjectFromNameXmlResponseAsync(string projectName, CancellationToken cancellationToken)
        {
            return GetXmlResponseAsync(string.Format("projects/{0}", projectName), cancellationToken);
        }

        private Task<XDocument> GetFilteredBuildsXmlResponseAsync(string buildTypeId, CancellationToken cancellationToken, DateTime? sinceDate = null, bool? running = null)
        {
            var values = new List<string> { "branch:(default:any)" };

            if (sinceDate.HasValue)
            {
                values.Add(string.Format("sinceDate:{0}", FormatJsonDate(sinceDate.Value)));
            }

            if (running.HasValue)
            {
                values.Add(string.Format("running:{0}", running.Value.ToString(CultureInfo.InvariantCulture)));
            }

            string buildLocator = string.Join(",", values);
            var url = string.Format("buildTypes/id:{0}/builds/?locator={1}", buildTypeId, buildLocator);
            var filteredBuildsXmlResponseTask = GetXmlResponseAsync(url, cancellationToken);

            return filteredBuildsXmlResponseTask;
        }

        private static DateTime DecodeJsonDateTime(string dateTimeString)
        {
            var dateTime = new DateTime(
                    int.Parse(dateTimeString.Substring(0, 4)),
                    int.Parse(dateTimeString.Substring(4, 2)),
                    int.Parse(dateTimeString.Substring(6, 2)),
                    int.Parse(dateTimeString.Substring(9, 2)),
                    int.Parse(dateTimeString.Substring(11, 2)),
                    int.Parse(dateTimeString.Substring(13, 2)),
                    DateTimeKind.Utc)
                .AddHours(int.Parse(dateTimeString.Substring(15, 3)))
                .AddMinutes(int.Parse(dateTimeString.Substring(15, 1) + dateTimeString.Substring(18, 2)));

            return dateTime;
        }

        private static string FormatJsonDate(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyyMMdd'T'HHmmss-0000", CultureInfo.InvariantCulture).Replace(":", string.Empty);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (httpClient != null)
            {
                httpClient.Dispose();
            }
        }
    }
}
