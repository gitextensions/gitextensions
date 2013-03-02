using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
        private string ProjectName { get; set; }

        private readonly HttpClient httpClient;

        private readonly IEnumerable<string> buildTypes;

        public TeamCityAdapter(IConfig config)
        {
            httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            var hostName = config.Get("BuildServerUrl");
            if (!string.IsNullOrEmpty(hostName))
            {
                var hostRootUri = hostName.Contains("://")
                                  ? new Uri(hostName, UriKind.Absolute)
                                  : new Uri(string.Format("{0}://{1}", Uri.UriSchemeHttp, hostName), UriKind.Absolute);
                httpClient.BaseAddress = hostRootUri;
            }

            ProjectName = config.Get("ProjectName");

            var project = GetProjectFromNameXmlResponse(ProjectName);
            buildTypes = project.XPathSelectElements("/project/buildTypes/buildType").Select(x => x.Attribute("id").Value);
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
            if (httpClient.BaseAddress == null || string.IsNullOrEmpty(ProjectName))
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
                                                       var buildIds = buildTypes.SelectMany(
                                                           buildTypeId =>
                                                               GetFilteredBuildsXmlResponse(buildTypeId, sinceDate, running)
                                                                   .XPathSelectElements("/builds/build")
                                                                   .Select(x => x.Attribute("id").Value))
                                                           .ToArray();
                                                       var tasks = new List<Task>(8);
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
            BuildInfo.BuildStatus status;
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


            switch (statusValue)
            {
                case "SUCCESS":
                    status = BuildInfo.BuildStatus.Success;
                    break;
                case "FAILURE":
                    status = BuildInfo.BuildStatus.Failure;
                    break;
                default:
                    status = BuildInfo.BuildStatus.Unknown;
                    break;
            }

            var buildInfo = new BuildInfo
                                {
                                    Id = idValue,
                                    StartDate = DecodeJsonDateTime(startDateText),
                                    Status = status,
                                    Description = statusText,
                                    CommitHashList = commitHashList,
                                    Url = webUrl
                                };
            return buildInfo;
        }

        private Task<Stream> GetStreamAsync(string relativePath)
        {
            return httpClient.GetStreamAsync(string.Format("/guestAuth/app/rest/{0}", relativePath));
        }

        private Task<XDocument> GetXmlResponseAsync(string relativePath, CancellationToken cancellationToken)
        {
            var getStreamTask = GetStreamAsync(relativePath);

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

        private XDocument GetXmlResponse(string relativePath)
        {
            return XDocument.Load(new Uri(httpClient.BaseAddress, string.Format("/guestAuth/app/rest/{0}", relativePath)).AbsoluteUri);
        }

        private Task<XDocument> GetBuildFromIdXmlResponseAsync(string buildId, CancellationToken cancellationToken)
        {
            return GetXmlResponseAsync(string.Format("builds/id:{0}", buildId), cancellationToken);
        }

        private XDocument GetProjectFromNameXmlResponse(string projectName)
        {
            return GetXmlResponse(string.Format("projects/name:{0}", projectName));
        }

        private XDocument GetFilteredBuildsXmlResponse(string buildTypeId, DateTime? sinceDate = null, bool? running = null)
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
            var filteredBuildsXmlResponse = GetXmlResponse(url);

            return filteredBuildsXmlResponse;
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