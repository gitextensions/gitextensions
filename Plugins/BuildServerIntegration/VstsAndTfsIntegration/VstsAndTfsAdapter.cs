using System;
using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using VstsAndTfsIntegration.Settings;

namespace VstsAndTfsIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class VstsAndTfsIntegrationMetadata : BuildServerAdapterMetadataAttribute
    {
        public VstsAndTfsIntegrationMetadata(string buildServerType)
            : base(buildServerType)
        {
        }
    }

    [Export(typeof(IBuildServerAdapter))]
    [VstsAndTfsIntegrationMetadata(PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class VstsAndTfsAdapter : IBuildServerAdapter
    {
        public const string PluginName = "Azure DevOps / VSTS and Team Foundation Server (since TFS2015)";

        private IBuildServerWatcher _buildServerWatcher;
        private VstsIntegrationSettings _settings;
        private TfsApiHelper _tfsHelper;

        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config, Func<ObjectId, bool> isCommitInRevisionGrid = null)
        {
            if (_buildServerWatcher != null)
            {
                throw new InvalidOperationException("Already initialized");
            }

            _buildServerWatcher = buildServerWatcher;
            _settings = VstsIntegrationSettings.ReadFrom(config);

            if (!_settings.IsValid())
            {
                return;
            }

            var projectUrl = _buildServerWatcher.ReplaceVariables(_settings.ProjectUrl);
            _tfsHelper = new TfsApiHelper(projectUrl, _settings.ApiToken);
        }

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        public string UniqueKey => _settings?.ProjectUrl ?? throw new InvalidOperationException($"{nameof(VstsAndTfsAdapter)} is not yet initialized");

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

        private async Task ObserveBuildsAsync(DateTime? sinceDate, bool? running, IObserver<GitUIPluginInterfaces.BuildServerIntegration.BuildInfo> observer, CancellationToken cancellationToken)
        {
            try
            {
                var builds = await _tfsHelper.QueryBuildsAsync(_settings.BuildDefinitionFilter, sinceDate, running);

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
                    duration = " / " + GetDuration(DateTime.UtcNow - buildDetail.StartTime.Value);
                }
                else
                {
                    if (buildDetail.FinishTime.HasValue)
                    {
                        duration = " / " + GetDuration(buildDetail.FinishTime.Value - buildDetail.StartTime.Value);
                    }
                    else
                    {
                        duration = " / ???";
                    }
                }
            }

            var buildInfo = new BuildInfo
            {
                Id = buildDetail.BuildNumber,
                StartDate = buildDetail.StartTime ?? DateTime.Now.AddHours(1),
                Status = buildDetail.IsInProgress ? BuildInfo.BuildStatus.InProgress : MapResult(buildDetail.Result),
                Description = buildDetail.BuildNumber + " (" + (buildDetail.IsInProgress ? buildDetail.Status : buildDetail.Result) + " " + duration + ")",
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

        private static string GetDuration(TimeSpan duration)
        {
            var s = new StringBuilder();
            if (duration.Hours != 0)
            {
                s.Append($"{duration.Hours}h");
            }

            if (duration.Minutes != 0)
            {
                s.Append($"{duration.Minutes:00}m");
            }

            s.Append($"{duration.Seconds:00}s");
            return s.ToString();
        }

        public void Dispose()
        {
            _tfsHelper?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
