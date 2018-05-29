using System;
using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GitCommands.Utils;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace VstsAndTfsIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
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
        public const string PluginName = "VSTS and Team Foundation Server (since TFS2015)";

        public const string VstsTfsServerUrlSettingKey = "VstsTfsServerUrl";
        public const string VstsTfsCollectionNameSettingKey = "VstsTfsCollectionName";
        public const string VstsTfsProjectNameSettingKey = "VstsTfsProjectName";
        public const string VstsTfsBuildDefinitionNameFilterSettingKey = "VstsTfsBuildDefinitionNameFilter";
        public const string VstsTfsRestApiTokenSettingKey = "VstsTfsRestApiToken";

        private IBuildServerWatcher _buildServerWatcher;
        private TfsApiHelper _tfsHelper;
        private string _tfsServer;
        private string _tfsTeamCollectionName;
        private string _projectName;
        private Regex _tfsBuildDefinitionNameFilter;
        private string _restApiToken;

        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config, Func<string, bool> isCommitInRevisionGrid)
        {
            if (_buildServerWatcher != null)
            {
                throw new InvalidOperationException("Already initialized");
            }

            _buildServerWatcher = buildServerWatcher;

            _tfsServer = config.GetString(VstsTfsServerUrlSettingKey, null);
            _tfsTeamCollectionName = config.GetString(VstsTfsCollectionNameSettingKey, null);
            _projectName = _buildServerWatcher.ReplaceVariables(config.GetString(VstsTfsProjectNameSettingKey, null));
            _restApiToken = config.GetString(VstsTfsRestApiTokenSettingKey, null);
            var tfsBuildDefinitionNameFilterSetting = config.GetString(VstsTfsBuildDefinitionNameFilterSettingKey, "");
            if (string.IsNullOrWhiteSpace(_tfsServer)
                || string.IsNullOrWhiteSpace(_tfsTeamCollectionName)
                || string.IsNullOrWhiteSpace(_projectName)
                || string.IsNullOrWhiteSpace(_restApiToken)
                || !BuildServerSettingsHelper.IsRegexValid(tfsBuildDefinitionNameFilterSetting))
            {
                return;
            }

            _tfsBuildDefinitionNameFilter = new Regex(tfsBuildDefinitionNameFilterSetting, RegexOptions.Compiled);

            _tfsHelper = new TfsApiHelper();
            _tfsHelper.ConnectToTfsServer(_tfsServer, _tfsTeamCollectionName, _projectName, _restApiToken, _tfsBuildDefinitionNameFilter);
        }

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        public string UniqueKey => _tfsServer + "/" + _tfsTeamCollectionName + "/" + _projectName;

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
                var builds = await _tfsHelper.QueryBuildsAsync(sinceDate, running);

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
                StartDate = buildDetail.StartTime.HasValue ? buildDetail.StartTime.Value : DateTime.Now.AddHours(1),
                Status = buildDetail.IsInProgress ? BuildInfo.BuildStatus.InProgress : MapResult(buildDetail.Result),
                Description = buildDetail.BuildNumber + " (" + (buildDetail.IsInProgress ? buildDetail.Status : buildDetail.Result) + " " + duration + ")",
                CommitHashList = new[] { buildDetail.SourceVersion },
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
                s.Append($"{duration.Minutes.ToString("00")}m");
            }

            s.Append($"{duration.Seconds.ToString("00")}s");
            return s.ToString();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
