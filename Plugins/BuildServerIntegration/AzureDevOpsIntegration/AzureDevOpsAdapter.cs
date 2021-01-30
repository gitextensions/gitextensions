using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AzureDevOpsIntegration.Settings;
using GitUI;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft.VisualStudio.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using ResourceManager;

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

        private bool _firstCallForFinishedBuildsWasIgnored = false;
        private IBuildServerWatcher _buildServerWatcher;
        private IntegrationSettings _settings;
        private ApiClient _apiClient;
        private static readonly IBuildDurationFormatter _buildDurationFormatter = new BuildDurationFormatter();
        private JoinableTask<string> _buildDefinitionsTask;
        private string _projectUrl;
        private string _buildDefinitions;

        // Static variable used to convey the data between the different instances of the class but that doesn't necessarily require synchronisation because:
        // * there is only one instance at every given time (link to the revision grid and recreated on revision grid refresh)
        // * data is created by the first instance and used in readonly by the later instances
        private static CacheAzureDevOps CacheAzureDevOps = null;
        private static string ProjectOnErrorKey = null;

        private readonly TranslationString _buildIntegrationErrorCaption = new("Azure DevOps error");
        private readonly TranslationString _badTokenErrorMessage = new(@"The personal access token is invalid or has expired. Update it in the 'Build server integration' settings.

The build server integration has been disabled for this session.");
        private readonly TranslationString _genericErrorMessage = new(@"An error occured when requesting build server results.

As a consequence, the build server integration has been disabled for this session.

Detail of the error:");

        private Action _openSettings;
        private string CacheKey => _projectUrl + "|" + _settings.BuildDefinitionFilter;

        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config, Action openSettings, Func<ObjectId, bool> isCommitInRevisionGrid = null)
        {
            if (_buildServerWatcher is not null)
            {
                throw new InvalidOperationException("Already initialized");
            }

            _buildServerWatcher = buildServerWatcher;
            _openSettings = openSettings;
            _settings = IntegrationSettings.ReadFrom(config);

            if (!_settings.IsValid())
            {
                return;
            }

            _projectUrl = _buildServerWatcher.ReplaceVariables(_settings.ProjectUrl);

            if (!Uri.IsWellFormedUriString(_projectUrl, UriKind.Absolute) || string.IsNullOrWhiteSpace(_settings.ApiToken))
            {
                return;
            }

            _apiClient = new ApiClient(_projectUrl, _settings.ApiToken);
            if (CacheAzureDevOps is null || CacheAzureDevOps.Id != CacheKey)
            {
                CacheAzureDevOps = null;
                _buildDefinitionsTask = ThreadHelper.JoinableTaskFactory.RunAsync(() => _apiClient.GetBuildDefinitionsAsync(_settings.BuildDefinitionFilter));
            }
            else
            {
                _buildDefinitions = CacheAzureDevOps.BuildDefinitions;
            }
        }

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        public string UniqueKey => _settings?.ProjectUrl ?? throw new InvalidOperationException($"{nameof(AzureDevOpsAdapter)} is not yet initialized");

        public IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null)
        {
            if (!_firstCallForFinishedBuildsWasIgnored)
            {
                _firstCallForFinishedBuildsWasIgnored = true;
                return Observable.Empty<BuildInfo>();
            }

            return GetBuilds(scheduler, sinceDate, false);
        }

        public IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler)
        {
            return GetBuilds(scheduler, null, true);
        }

        private IObservable<BuildInfo> GetBuilds(IScheduler scheduler, DateTime? sinceDate = null, bool running = false)
        {
            return Observable.Create<BuildInfo>((observer, cancellationToken) => ObserveBuildsAsync(sinceDate, running, observer, cancellationToken));
        }

        private async Task ObserveBuildsAsync(DateTime? sinceDate, bool running, IObserver<BuildInfo> observer, CancellationToken cancellationToken)
        {
            if (_apiClient is null)
            {
                observer.OnCompleted();
                return;
            }

            try
            {
                if (_buildDefinitions is null)
                {
                    try
                    {
                        _buildDefinitions = await _buildDefinitionsTask.JoinAsync();

                        if (_buildDefinitions is null)
                        {
                            observer.OnCompleted();
                            return;
                        }

                        CacheAzureDevOps = new CacheAzureDevOps { Id = CacheKey, BuildDefinitions = _buildDefinitions };
                    }
                    catch (UnauthorizedAccessException)
                    {
                        string errorMessage = _badTokenErrorMessage.Text;
                        ManageError(errorMessage);

                        if (ProjectOnErrorKey is null || ProjectOnErrorKey != CacheKey)
                        {
                            ProjectOnErrorKey = CacheKey;

                            var btnOpenSettings = new TaskDialogButton("btnOpenSettings", "Open settings");
                            var btnIgnore = new TaskDialogButton("btnIgnoreError", "Ignore");
                            using var errorDialog = new TaskDialog
                            {
                                InstructionText = errorMessage,
                                Icon = TaskDialogStandardIcon.Error,
                                Cancelable = true,
                                Caption = _buildIntegrationErrorCaption.Text,
                                Controls = { btnOpenSettings, btnIgnore }
                            };

                            btnOpenSettings.Click += (sender, e) => errorDialog.Close(TaskDialogResult.Yes);
                            btnIgnore.Click += (sender, e) => errorDialog.Close(TaskDialogResult.No);

                            var result = errorDialog.Show();
                            if (result == TaskDialogResult.Yes)
                            {
                                ProjectOnErrorKey = null;
                                _openSettings.Invoke();
                            }
                        }

                        return;
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = _genericErrorMessage.Text + ex.Message;
                        ManageError(errorMessage);

                        if (ProjectOnErrorKey is null || ProjectOnErrorKey != CacheKey)
                        {
                            ProjectOnErrorKey = CacheKey;
                            MessageBox.Show(errorMessage, _buildIntegrationErrorCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        return;
                    }
                }

                void ManageError(string errorMessage)
                {
                    _apiClient = null;
                    observer.OnNext(new BuildInfo { CommitHashList = new[] { ObjectId.WorkTreeId }, Description = errorMessage, Status = BuildInfo.BuildStatus.Failure });
                    observer.OnCompleted();
                }

                if (_buildDefinitions is null)
                {
                    observer.OnCompleted();
                    return;
                }

                var builds = running ?
                    FilterRunningBuilds(await _apiClient.QueryRunningBuildsAsync(_buildDefinitions)) :
                    await _apiClient.QueryFinishedBuildsAsync(_buildDefinitions, sinceDate);

                foreach (var build in builds)
                {
                    observer.OnNext(CreateBuildInfo(build));
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

        private IEnumerable<Build> FilterRunningBuilds(IList<Build> runningBuilds)
        {
            if (runningBuilds.Count < 2)
            {
                return runningBuilds;
            }

            var byCommitBuilds = runningBuilds.GroupBy(b => b.SourceVersion);
            runningBuilds = new List<Build>();

            // Filter running builds to display the best build as we can only display one build for a commit
            // by selecting the first started or if none, one that is waiting to start
            foreach (var commitBuilds in byCommitBuilds)
            {
                var buildSelected = commitBuilds.Where(b => b.StartTime.HasValue).OrderBy(b => b.StartTime).FirstOrDefault()
                                    ?? commitBuilds.First();
                runningBuilds.Add(buildSelected);
            }

            return runningBuilds;
        }

        private static BuildInfo CreateBuildInfo(Build buildDetail)
        {
            string duration = string.Empty;

            if (buildDetail.Status is not null
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
                StartDate = buildDetail.StartTime ?? DateTime.MinValue,
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

        #region TestAccessor
        internal TestAccessor GetTestAccessor() => new TestAccessor(this);

        internal readonly struct TestAccessor
        {
            public AzureDevOpsAdapter AzureDevOpsAdapter { get; }

            public TestAccessor(AzureDevOpsAdapter azureDevOpsAdapter)
            {
                AzureDevOpsAdapter = azureDevOpsAdapter;
            }

            public IEnumerable<Build> FilterRunningBuilds(IList<Build> runningBuilds) => AzureDevOpsAdapter.FilterRunningBuilds(runningBuilds);
        }
        #endregion

    }
}
