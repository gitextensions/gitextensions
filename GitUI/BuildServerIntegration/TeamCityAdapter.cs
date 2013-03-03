using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using GitCommands;
using Nini.Config;

namespace GitUI.BuildServerIntegration
{
    internal class TeamCityAdapter : IBuildServerAdapter
    {
        private readonly IBuildServerWatcher buildServerWatcher;

        private string ProjectName { get; set; }

        private readonly HttpClient httpClient;

        private readonly Task<IEnumerable<string>> getBuildTypesTask;

        public TeamCityAdapter(IBuildServerWatcher buildServerWatcher, IConfig config)
        {
            this.buildServerWatcher = buildServerWatcher;

            httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            var hostName = config.Get("BuildServerUrl");
            if (!string.IsNullOrEmpty(hostName))
            {
                var hostRootUri = Uri.CheckSchemeName(hostName)
                                  ? new Uri(string.Format("{0}://{1}", Uri.UriSchemeHttp, hostName), UriKind.Absolute)
                                  : new Uri(hostName, UriKind.Absolute);
                httpClient.BaseAddress = hostRootUri;
            }

            string username, password;
            if (buildServerWatcher.GetBuildServerCredentials(this, true, out username, out password))
            {
                // Assign the authentication headers
                httpClient.DefaultRequestHeaders.Authorization = CreateBasicHeader(username, password);
            }

            ProjectName = config.Get("ProjectName");

            getBuildTypesTask =
                GetProjectFromNameXmlResponseAsync(ProjectName, CancellationToken.None)
                    .ContinueWith(task => task.Result
                                              .XPathSelectElements("/project/buildTypes/buildType")
                                              .Select(x => x.Attribute("id").Value));
        }

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        public string UniqueKey { get { return httpClient.BaseAddress.Host; } }

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
            if (httpClient.BaseAddress == null || string.IsNullOrEmpty(ProjectName))
            {
                return Observable.Empty<BuildInfo>(scheduler);
            }

            IEnumerable<string> buildTypes;
            try
            {
                getBuildTypesTask.Wait();

                buildTypes = getBuildTypesTask.Result;
            }
            catch (AggregateException)
            {
                return Observable.Empty<BuildInfo>(scheduler);
            }

            return Observable.Create<BuildInfo>((observer, cancellationToken) =>
                Task<IDisposable>.Factory.StartNew(() =>
                    {
                        scheduler.Schedule(() =>
                                               {
                                                   try
                                                   {
                                                       var buildIdTasks = buildTypes.Select(buildTypeId => GetFilteredBuildsXmlResponseAsync(buildTypeId, CancellationToken.None, sinceDate, running)).ToArray();
                                                       var getBuildIdsTask = Task.Factory
                                                                                 .ContinueWhenAll(buildIdTasks, completedTasks => completedTasks.SelectMany(buildIdTask => buildIdTask.Result.XPathSelectElements("/builds/build").Select(x => x.Attribute("id").Value)).ToArray())
                                                                                 .ContinueWith(completedTask => buildIdTasks.SelectMany(buildIdTask => buildIdTask.Result.XPathSelectElements("/builds/build").Select(x => x.Attribute("id").Value)).ToArray());

                                                       getBuildIdsTask.ContinueWith(
                                                           buildIdTask =>
                                                                {
                                                                    var tasks = new List<Task>(8);
                                                                    var buildIds = buildIdTask.Result;
                                                                    var buildsLeft = buildIds.Length;

                                                                    foreach (var buildId in buildIds.OrderByDescending(int.Parse))
                                                                    {
                                                                        var notifyObserverTask =
                                                                            GetBuildFromIdXmlResponseAsync(buildId, cancellationToken)
                                                                                .ContinueWith(
                                                                                    task =>
                                                                                        {
                                                                                            var buildDetails = task.Result;
                                                                                            var buildInfo = CreateBuildInfo(buildDetails);
                                                                                            if (buildInfo.CommitHashList.Any())
                                                                                            {
                                                                                                observer.OnNext(buildInfo);
                                                                                            }
                                                                                        },
                                                                                    cancellationToken);

                                                                        tasks.Add(notifyObserverTask);
                                                                        --buildsLeft;

                                                                        if (tasks.Count == tasks.Capacity || buildsLeft == 0)
                                                                        {
                                                                            var batchTasks = tasks.ToArray();
                                                                            tasks.Clear();

                                                                            Task.WaitAll(batchTasks, cancellationToken);
                                                                        }
                                                                    }

                                                                    observer.OnCompleted();
                                                                });
                                                   }
                                                   catch (OperationCanceledException)
                                                   {
                                                       // Do nothing, the observer is already stopped
                                                   }
                                                   catch (Exception ex)
                                                   {
                                                       observer.OnError(ex);
                                                   }
                                               });

                        return Disposable.Empty;
                    }))
                .OnErrorResumeNext(Observable.Empty<BuildInfo>());
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
            return httpClient.GetAsync(FormatRelativePath(restServicePath), HttpCompletionOption.ResponseHeadersRead)
                             .ContinueWith(task =>
                                               {
                                                   // task.Result.Content.Headers.ContentType.MediaType = "application/xml";
                                                   // task.Result.Content.Headers.Allow.Add()
                                                   if (task.Result.IsSuccessStatusCode)
                                                       return task.Result.Content.ReadAsStreamAsync();

                                                   if (task.Result.StatusCode == HttpStatusCode.Unauthorized)
                                                   {
                                                       string username;
                                                       string password;

                                                       if (buildServerWatcher.GetBuildServerCredentials(this, false, out username, out password))
                                                       {
                                                           // Assign the authentication headers
                                                           httpClient.DefaultRequestHeaders.Authorization = CreateBasicHeader(username, password);

                                                           return GetStreamAsync(restServicePath, cancellationToken);
                                                       }

                                                       throw new OperationCanceledException(task.Result.ReasonPhrase);
                                                   }

                                                   throw new HttpRequestException(task.Result.ReasonPhrase);
                                               },
                                           cancellationToken)
                             .Unwrap();
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
                cancellationToken);
        }

        private Uri FormatRelativePath(string restServicePath)
        {
            return new Uri(string.Format("/httpAuth/app/rest/{0}", restServicePath), UriKind.Relative);
        }

        private Task<XDocument> GetBuildFromIdXmlResponseAsync(string buildId, CancellationToken cancellationToken)
        {
            return GetXmlResponseAsync(string.Format("builds/id:{0}", buildId), cancellationToken);
        }

        private Task<XDocument> GetProjectFromNameXmlResponseAsync(string projectName, CancellationToken cancellationToken)
        {
            return GetXmlResponseAsync(string.Format("projects/name:{0}", projectName), cancellationToken);
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
            return dateTime.ToString("yyyyMMdd'T'HHmmsszzzz", CultureInfo.InvariantCulture).Replace(":", string.Empty).Replace("+", "-");
        }
    }
}
