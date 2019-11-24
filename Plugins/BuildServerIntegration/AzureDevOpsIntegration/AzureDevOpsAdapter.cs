using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureDevOpsIntegration.Settings;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace AzureDevOpsIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class AzureDevOpsIntegrationMetadata : BuildServerAdapterMetadataAttribute
    {
        public AzureDevOpsIntegrationMetadata(string buildServerType)
            : base(buildServerType)
        {
        }
    }

    /// <summary>
    /// Provides build server integration for Azure DevOps (or TFS>=2015) into GitExtensions
    /// </summary>
    [Export(typeof(IBuildServerAdapter))]
    [AzureDevOpsIntegrationMetadata(PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class AzureDevOpsAdapter : IBuildServerAdapter
    {
        public const string PluginName = "Azure DevOps and Team Foundation Server (since TFS2015)";

        private IBuildServerWatcher _buildServerWatcher;
        private IntegrationSettings _settings;
        private ApiClient _apiClient;
        private static readonly IBuildDurationFormatter _buildDurationFormatter = new BuildDurationFormatter();

        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config, Func<ObjectId, bool> isCommitInRevisionGrid = null)
        {
            if (_buildServerWatcher != null)
            {
                throw new InvalidOperationException("Already initialized");
            }

            _buildServerWatcher = buildServerWatcher;
            _settings = IntegrationSettings.ReadFrom(config);

            if (!_settings.IsValid())
            {
                return;
            }

            var projectUrl = _buildServerWatcher.ReplaceVariables(_settings.ProjectUrl);

            if (!Uri.IsWellFormedUriString(projectUrl, UriKind.Absolute) || string.IsNullOrWhiteSpace(_settings.ApiToken))
            {
                return;
            }

            _apiClient = new ApiClient(projectUrl, _settings.ApiToken);
        }

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        public string UniqueKey => _settings?.ProjectUrl ?? throw new InvalidOperationException($"{nameof(AzureDevOpsAdapter)} is not yet initialized");

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
            return Observable.Create<BuildInfo>((observer, cancellationToken) => ObserveBuildsAsync(sinceDate, running, observer, cancellationToken));
        }

        private async Task ObserveBuildsAsync(DateTime? sinceDate, bool? running, IObserver<BuildInfo> observer, CancellationToken cancellationToken)
        {
            if (_apiClient == null)
            {
                observer.OnCompleted();
                return;
            }

            try
            {
                var builds = await _apiClient.QueryBuildsAsync(_settings.BuildDefinitionFilter, sinceDate, running);

                Parallel.ForEach(builds, detail => { observer.OnNext(CreateBuildInfo(detail)); });
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

        private static BuildInfo CreateBuildInfo(Build buildDetail)
        {
            string duration = string.Empty;

            if (buildDetail.Status != null
                && buildDetail.Status != "none"
                && buildDetail.Status != "notStarted"
                && buildDetail.Status != "postponed"
                && buildDetail.StartTime.HasValue)
            {
                if (buildDetail.Status == "inProgress")
                {
                    duration = _buildDurationFormatter.Format((long)(DateTime.UtcNow - buildDetail.StartTime.Value).TotalMilliseconds);
                }
                else
                {
                    duration = buildDetail.FinishTime.HasValue ? _buildDurationFormatter.Format((long)(buildDetail.FinishTime.Value - buildDetail.StartTime.Value).TotalMilliseconds) : "???";
                }
            }

            var buildInfo = new BuildInfo
            {
                Id = buildDetail.BuildNumber,
                StartDate = buildDetail.StartTime ?? DateTime.Now.AddHours(1),
                Status = buildDetail.IsInProgress ? BuildInfo.BuildStatus.InProgress : MapResult(buildDetail.Result),
                Description = duration + " " + buildDetail.BuildNumber,
                Tooltip = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(buildDetail.IsInProgress ? buildDetail.Status : buildDetail.Result) + Environment.NewLine + duration + Environment.NewLine + buildDetail.BuildNumber,
                CommitHashList = new[] { ObjectId.Parse(buildDetail.SourceVersion) },
                Url = buildDetail._links.Web.Href,
                ShowInBuildReportTab = false
            };

            return buildInfo;
        }

        private static BuildInfo.BuildStatus MapResult(string status)
        {
            switch (status)
            {
                case "failed":
                    return BuildInfo.BuildStatus.Failure;
                case "canceled":
                    return BuildInfo.BuildStatus.Stopped;
                case "succeeded":
                    return BuildInfo.BuildStatus.Success;
                case "partiallySucceeded":
                    return BuildInfo.BuildStatus.Unstable;
                default:
                    return BuildInfo.BuildStatus.Unknown;
            }
        }

        public void Dispose()
        {
            _apiClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
