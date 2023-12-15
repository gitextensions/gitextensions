using System.ComponentModel.Composition;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using GitCommands.Utils;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUI;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft;
using Microsoft.VisualStudio.Threading;

namespace TeamCityIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class TeamCityIntegrationMetadataAttribute : BuildServerAdapterMetadataAttribute
    {
        public TeamCityIntegrationMetadataAttribute(string buildServerType)
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
                else
                {
                    return ".NET Framework 4 or higher required";
                }
            }
        }
    }

    [Export(typeof(IBuildServerAdapter))]
    [TeamCityIntegrationMetadata(PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class TeamCityAdapter : IBuildServerAdapter
    {
        public const string PluginName = "TeamCity";
        private IBuildServerWatcher? _buildServerWatcher;

        private HttpClientHandler? _httpClientHandler;
        private HttpClient? _httpClient;

        private string? _httpClientHostSuffix;

        private readonly List<JoinableTask<IEnumerable<string>>> _getBuildTypesTask = [];

        private CookieContainer? _teamCityNtlmAuthCookie;

        private string? HostName { get; set; }

        private string[]? ProjectNames { get; set; }

        private Regex? BuildIdFilter { get; set; }

        private CookieContainer GetTeamCityNtlmAuthCookie(string serverUrl, IBuildServerCredentials? buildServerCredentials)
        {
            if (_teamCityNtlmAuthCookie is not null)
            {
                return _teamCityNtlmAuthCookie;
            }

            string url = serverUrl + "ntlmLogin.html";
            CookieContainer cookieContainer = new();
#pragma warning disable SYSLIB0014 // 'WebRequest.Create(string)' is obsolete
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
#pragma warning restore SYSLIB0014 // 'WebRequest.Create(string)' is obsolete
            request.CookieContainer = cookieContainer;

            if (buildServerCredentials is not null
                && !string.IsNullOrEmpty(buildServerCredentials.Username)
                && !string.IsNullOrEmpty(buildServerCredentials.Password))
            {
                request.Credentials = new NetworkCredential(buildServerCredentials.Username, buildServerCredentials.Password);
            }
            else
            {
                request.Credentials = CredentialCache.DefaultCredentials;
            }

            request.PreAuthenticate = true;
            request.GetResponse();

            _teamCityNtlmAuthCookie = cookieContainer;
            return _teamCityNtlmAuthCookie;
        }

        public string? LogAsGuestUrlParameter { get; set; }

        public void Initialize(IBuildServerWatcher buildServerWatcher, SettingsSource config, Action openSettings, Func<ObjectId, bool>? isCommitInRevisionGrid = null)
        {
            if (_buildServerWatcher is not null)
            {
                throw new InvalidOperationException("Already initialized");
            }

            _buildServerWatcher = buildServerWatcher;

            ProjectNames = buildServerWatcher.ReplaceVariables(config.GetString("ProjectName", ""))
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            string buildIdFilerSetting = config.GetString("BuildIdFilter", "");
            if (!BuildServerSettingsHelper.IsRegexValid(buildIdFilerSetting))
            {
                return;
            }

            BuildIdFilter = new Regex(buildIdFilerSetting);
            HostName = config.GetString("BuildServerUrl", null);
            LogAsGuestUrlParameter = config.GetBool("LogAsGuest", false) ? "&guest=1" : string.Empty;

            if (!string.IsNullOrEmpty(HostName))
            {
                InitializeHttpClient(HostName);
                if (ProjectNames.Length > 0)
                {
                    _getBuildTypesTask.Clear();
                    foreach (string name in ProjectNames)
                    {
                        _getBuildTypesTask.Add(ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                        {
                            XDocument response = await GetProjectFromNameXmlResponseAsync(name, CancellationToken.None).ConfigureAwait(false);
                            return from element in response.XPathSelectElements("/project/buildTypes/buildType")
                                   select element.Attribute("id").Value;
                        }));
                    }
                }
            }
        }

        public void InitializeHttpClient(string hostname)
        {
            CreateNewHttpClient(hostname);
            UpdateHttpClientOptionsGuestAuth();
        }

        private void CreateNewHttpClient(string hostName)
        {
            _httpClientHandler = new HttpClientHandler();
            _httpClient = new HttpClient(_httpClientHandler)
            {
                Timeout = TimeSpan.FromMinutes(2),
                BaseAddress = hostName.Contains("://")
                    ? new Uri(hostName, UriKind.Absolute)
                    : new Uri(string.Format("{0}://{1}", Uri.UriSchemeHttp, hostName), UriKind.Absolute)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        }

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        public string UniqueKey
        {
            get
            {
                Validates.NotNull(_httpClient);
                return _httpClient.BaseAddress.Host;
            }
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
            if (_httpClient?.BaseAddress is null || ProjectNames == null || ProjectNames.Length == 0)
            {
                return Observable.Empty<BuildInfo>(scheduler);
            }

            return Observable.Create<BuildInfo>((observer, cancellationToken) =>
                Task.Run(
                    () => scheduler.Schedule(() => ObserveBuilds(sinceDate, running, observer, cancellationToken))));
        }

        private void ObserveBuilds(DateTime? sinceDate, bool? running, IObserver<BuildInfo> observer, CancellationToken cancellationToken)
        {
            try
            {
                if (_getBuildTypesTask.Any(task => PropagateTaskAnomalyToObserver(task.Task, observer)))
                {
                    return;
                }

                Validates.NotNull(BuildIdFilter);

                IObserver<BuildInfo> localObserver = observer;
                IEnumerable<string> buildTypes = _getBuildTypesTask.SelectMany(t => t.Join()).Where(id => BuildIdFilter.IsMatch(id));
                Task<XDocument>[] buildIdTasks = buildTypes.Select(buildTypeId => GetFilteredBuildsXmlResponseAsync(buildTypeId, cancellationToken, sinceDate, running)).ToArray();

                Task.Factory
                    .ContinueWhenAll(
                        buildIdTasks,
                        completedTasks =>
                            {
                                string[] buildIds = completedTasks.Where(task => task.Status == TaskStatus.RanToCompletion)
                                                             .SelectMany(
                                                                 buildIdTask =>
                                                                 buildIdTask.CompletedResult()
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
            List<Task> tasks = new(8);
            int buildsLeft = buildIds.Length;

            foreach (string buildId in buildIds.OrderByDescending(int.Parse))
            {
                Task notifyObserverTask =
                    GetBuildFromIdXmlResponseAsync(buildId, cancellationToken)
                        .ContinueWith(
                            task =>
                                {
                                    if (task.Status == TaskStatus.RanToCompletion)
                                    {
                                        XDocument buildDetails = task.CompletedResult();
                                        BuildInfo buildInfo = CreateBuildInfo(buildDetails);
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
                    Task[] batchTasks = tasks.ToArray();
                    tasks.Clear();

                    try
                    {
#pragma warning disable VSTHRD002
                        Task.WaitAll(batchTasks, cancellationToken);
#pragma warning restore VSTHRD002
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
                DebugHelpers.Assert(task.Exception is not null, "task.Exception is not null");

                observer.OnError(task.Exception);
                return true;
            }

            return false;
        }

        private BuildInfo CreateBuildInfo(XDocument buildXmlDocument)
        {
            XElement buildXElement = buildXmlDocument.Element("build");
            string idValue = buildXElement.Attribute("id").Value;
            string statusValue = buildXElement.Attribute("status").Value;
            string startDateText = buildXElement.Element("startDate").Value;
            string statusText = buildXElement.Element("statusText").Value;
            string webUrl = buildXElement.Attribute("webUrl").Value + LogAsGuestUrlParameter;
            IEnumerable<XElement> revisionsElements = buildXElement.XPathSelectElements("revisions/revision");
            List<ObjectId> commitHashList = revisionsElements.Select(x => ObjectId.Parse(x.Attribute("version").Value)).ToList();
            XAttribute runningAttribute = buildXElement.Attribute("running");

            if (runningAttribute is not null && Convert.ToBoolean(runningAttribute.Value))
            {
                XElement runningInfoXElement = buildXElement.Element("running-info");
                string currentStageText = runningInfoXElement.Attribute("currentStageText").Value;

                statusText = currentStageText;
            }

            BuildInfo buildInfo = new()
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

        private static BuildStatus ParseBuildStatus(string statusValue)
        {
            return statusValue switch
            {
                "SUCCESS" => BuildStatus.Success,
                "FAILURE" => BuildStatus.Failure,
                _ => BuildStatus.Unknown
            };
        }

        private Task<Stream> GetStreamAsync(string restServicePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Validates.NotNull(_httpClient);

            return _httpClient.GetAsync(FormatRelativePath(restServicePath), HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                             .ContinueWith(
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
                                 task => GetStreamFromHttpResponseAsync(task, restServicePath, cancellationToken),
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
                                 cancellationToken,
                                 TaskContinuationOptions.AttachedToParent,
                                 TaskScheduler.Current)
                             .Unwrap();
        }

        private Task<Stream> GetStreamFromHttpResponseAsync(Task<HttpResponseMessage> task, string restServicePath, CancellationToken cancellationToken)
        {
            if (!task.IsCompleted)
            {
                throw new InvalidOperationException($"Task in state '{task.Status}' was expected to be completed.");
            }

            bool retry = task.IsCanceled && !cancellationToken.IsCancellationRequested;
            bool unauthorized = task.Status == TaskStatus.RanToCompletion &&
                                (task.CompletedResult().StatusCode == HttpStatusCode.Unauthorized || task.CompletedResult().StatusCode == HttpStatusCode.Forbidden);

            if (!retry)
            {
                if (task.CompletedResult().IsSuccessStatusCode)
                {
                    HttpContent httpContent = task.CompletedResult().Content;

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
                Validates.NotNull(_buildServerWatcher);

                IBuildServerCredentials? buildServerCredentials = _buildServerWatcher.GetBuildServerCredentials(this, true);
                bool useBuildServerCredentials = buildServerCredentials is not null
                    && buildServerCredentials.BuildServerCredentialsType == BuildServerCredentialsType.UsernameAndPassword
                    && !string.IsNullOrWhiteSpace(buildServerCredentials.Username) && !string.IsNullOrWhiteSpace(buildServerCredentials.Password);
                if (useBuildServerCredentials)
                {
                    UpdateHttpClientOptionsCredentialsAuth(buildServerCredentials!);
                }

                bool useBuildServerBearerToken = buildServerCredentials is not null
                    && buildServerCredentials.BuildServerCredentialsType == BuildServerCredentialsType.BearerToken
                    && !string.IsNullOrWhiteSpace(buildServerCredentials.BearerToken);
                if (useBuildServerBearerToken)
                {
                    UpdateHttpClientOptionsBearerTokenAuth(buildServerCredentials!);
                }

                if (buildServerCredentials is null
                    || buildServerCredentials.BuildServerCredentialsType == BuildServerCredentialsType.Guest)
                {
                    UpdateHttpClientOptionsNtlmAuth(buildServerCredentials);
                }

                return GetStreamAsync(restServicePath, cancellationToken);
            }

            throw new HttpRequestException(task.CompletedResult().ReasonPhrase);
        }

        public void UpdateHttpClientOptionsNtlmAuth(IBuildServerCredentials? buildServerCredentials)
        {
            try
            {
                Validates.NotNull(_httpClient);
                Validates.NotNull(_httpClientHandler);
                Validates.NotNull(HostName);

                _httpClient.Dispose();
                _httpClientHandler.Dispose();

                _httpClientHostSuffix = "httpAuth";
                CreateNewHttpClient(HostName);
                _httpClientHandler.CookieContainer = GetTeamCityNtlmAuthCookie(_httpClient.BaseAddress.AbsoluteUri, buildServerCredentials);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        public void UpdateHttpClientOptionsGuestAuth()
        {
            Validates.NotNull(_httpClient);

            _httpClientHostSuffix = "guestAuth";
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        private void UpdateHttpClientOptionsCredentialsAuth(IBuildServerCredentials buildServerCredentials)
        {
            Validates.NotNull(_httpClient);
            Validates.NotNull(buildServerCredentials.Username);
            Validates.NotNull(buildServerCredentials.Password);

            _httpClientHostSuffix = "httpAuth";
            _httpClient.DefaultRequestHeaders.Authorization = CreateBasicHeader(buildServerCredentials.Username, buildServerCredentials.Password);
        }

        private void UpdateHttpClientOptionsBearerTokenAuth(IBuildServerCredentials buildServerCredentials)
        {
            Validates.NotNull(_httpClient);
            Validates.NotNull(_httpClientHandler);
            Validates.NotNull(buildServerCredentials.BearerToken);

            _httpClientHostSuffix = "";
            _httpClient.DefaultRequestHeaders.Authorization = CreateBearerTokenHeader(buildServerCredentials.BearerToken);

            foreach (Cookie co in _httpClientHandler.CookieContainer.GetAllCookies())
            {
                co.Expired = true;
            }
        }

        private static AuthenticationHeaderValue CreateBasicHeader(string username, string password)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes($"{username}:{password}");
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        private static AuthenticationHeaderValue CreateBearerTokenHeader(string bearerToken)
        {
            return new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        private Task<XDocument> GetXmlResponseAsync(string relativePath, CancellationToken cancellationToken)
        {
            Task<Stream> getStreamTask = GetStreamAsync(relativePath, cancellationToken);

            return getStreamTask.ContinueWith(
                task =>
                    {
                        using Stream responseStream = task.Result;
                        return XDocument.Load(responseStream);
                    },
                cancellationToken,
                TaskContinuationOptions.AttachedToParent,
                TaskScheduler.Current);
        }

        private Uri FormatRelativePath(string restServicePath)
        {
            return new Uri(string.Format("{0}/app/rest/{1}", _httpClientHostSuffix, restServicePath), UriKind.Relative);
        }

        private Task<XDocument> GetBuildFromIdXmlResponseAsync(string buildId, CancellationToken cancellationToken)
        {
            return GetXmlResponseAsync(string.Format("builds/id:{0}", buildId), cancellationToken);
        }

        private Task<XDocument> GetBuildTypeFromIdXmlResponseAsync(string buildId, CancellationToken cancellationToken)
        {
            return GetXmlResponseAsync(string.Format("buildTypes/id:{0}", buildId), cancellationToken);
        }

        private Task<XDocument> GetProjectFromNameXmlResponseAsync(string projectName, CancellationToken cancellationToken)
        {
            return GetXmlResponseAsync(string.Format("projects/{0}", projectName), cancellationToken);
        }

        private Task<XDocument> GetProjectsResponseAsync(CancellationToken cancellationToken)
        {
            return GetXmlResponseAsync("projects", cancellationToken);
        }

        private Task<XDocument> GetFilteredBuildsXmlResponseAsync(string buildTypeId, CancellationToken cancellationToken, DateTime? sinceDate = null, bool? running = null)
        {
            List<string> values = [ "branch:(default:any)"];

            if (sinceDate.HasValue)
            {
                values.Add(string.Format("sinceDate:{0}", FormatJsonDate(sinceDate.Value)));
            }

            if (running.HasValue)
            {
                values.Add(string.Format("running:{0}", running.Value.ToString(CultureInfo.InvariantCulture)));
            }

            string buildLocator = string.Join(",", values);
            string url = string.Format("buildTypes/id:{0}/builds/?locator={1}", buildTypeId, buildLocator);
            Task<XDocument> filteredBuildsXmlResponseTask = GetXmlResponseAsync(url, cancellationToken);

            return filteredBuildsXmlResponseTask;
        }

        private static DateTime DecodeJsonDateTime(string dateTimeString)
        {
            DateTime dateTime = new DateTime(
                    int.Parse(dateTimeString.AsSpan(0, 4)),
                    int.Parse(dateTimeString.AsSpan(4, 2)),
                    int.Parse(dateTimeString.AsSpan(6, 2)),
                    int.Parse(dateTimeString.AsSpan(9, 2)),
                    int.Parse(dateTimeString.AsSpan(11, 2)),
                    int.Parse(dateTimeString.AsSpan(13, 2)),
                    DateTimeKind.Utc)
                .AddHours(int.Parse(dateTimeString.AsSpan(15, 3)))
                .AddMinutes(int.Parse(string.Concat(dateTimeString.AsSpan(15, 1), dateTimeString.AsSpan(18, 2))));

            return dateTime;
        }

        private static string FormatJsonDate(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyyMMdd'T'HHmmss-0000", CultureInfo.InvariantCulture).Replace(":", string.Empty);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _httpClient?.Dispose();
        }

        public Project? GetProjectsTree()
        {
            XDocument projectsRootElement = ThreadHelper.JoinableTaskFactory.Run(() => GetProjectsResponseAsync(CancellationToken.None));
            List<Project> projects = projectsRootElement.Root.Elements().Where(e => (string)e.Attribute("archived") != "true").Select(e => new Project
            {
                Id = (string)e.Attribute("id"),
                Name = (string)e.Attribute("name"),
                ParentProject = (string)e.Attribute("parentProjectId"),
                SubProjects = new List<Project>()
            }).ToList();

            Dictionary<string, Project> projectDictionary = projects.ToDictionary(p => p.Id, p => p);

            Project? rootProject = null;
            foreach (Project project in projects)
            {
                if (project.ParentProject is not null)
                {
                    Project parentProject = projectDictionary[project.ParentProject];
                    Validates.NotNull(parentProject.SubProjects);
                    parentProject.SubProjects.Add(project);
                }
                else
                {
                    rootProject = project;
                }
            }

            return rootProject;
        }

        public List<Build> GetProjectBuilds(string projectId)
        {
            XDocument projectsRootElement = ThreadHelper.JoinableTaskFactory.Run(() => GetProjectFromNameXmlResponseAsync(projectId, CancellationToken.None));
            return projectsRootElement.Root.Element("buildTypes").Elements().Select(e => new Build
            {
                Id = (string)e.Attribute("id"),
                Name = (string)e.Attribute("name"),
                ParentProject = (string)e.Attribute("projectId")
            }).ToList();
        }

        public Build GetBuildType(string buildId)
        {
            XDocument projectsRootElement = ThreadHelper.JoinableTaskFactory.Run(() => GetBuildTypeFromIdXmlResponseAsync(buildId, CancellationToken.None));
            XElement buildType = projectsRootElement.Root;
            return new Build
            {
                Id = buildId,
                Name = (string)buildType.Attribute("name"),
                ParentProject = (string)buildType.Attribute("projectId")
            };
        }
    }

    public class Project
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? ParentProject { get; set; }
        public IList<Project>? SubProjects { get; set; }
        public IList<Build>? Builds { get; set; }
    }

    public class Build
    {
        public string? ParentProject { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string DisplayName => Name + " (" + Id + ")";
    }
}
