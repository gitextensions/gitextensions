using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GitCommands.Utils;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using JetBrains.Annotations;
using TfsInterop.Interface;

namespace TfsIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TfsIntegrationMetadataAttribute : BuildServerAdapterMetadataAttribute
    {
        public TfsIntegrationMetadataAttribute(string buildServerType)
            : base(buildServerType)
        {
        }

        public override string CanBeLoaded
        {
            get
            {
                if (EnvUtils.IsNet4FullOrHigher())
                {
                    return null;
                }

                return ".Net 4 full framework required";
            }
        }
    }

    [Export(typeof(IBuildServerAdapter))]
    [TfsIntegrationMetadata(PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class TfsAdapter : IBuildServerAdapter
    {
        public const string PluginName = "Team Foundation Server";
        private IBuildServerWatcher _buildServerWatcher;
        private ITfsHelper _tfsHelper;
        private string _tfsServer;
        private string _tfsTeamCollectionName;
        private string _projectName;
        private Regex _tfsBuildDefinitionNameFilter;

        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config, Func<ObjectId, bool> isCommitInRevisionGrid = null)
        {
            if (_buildServerWatcher != null)
            {
                throw new InvalidOperationException("Already initialized");
            }

            _buildServerWatcher = buildServerWatcher;

            _tfsServer = config.GetString("TfsServer", null);
            _tfsTeamCollectionName = config.GetString("TfsTeamCollectionName", null);
            _projectName = _buildServerWatcher.ReplaceVariables(config.GetString("ProjectName", null));
            var tfsBuildDefinitionNameFilterSetting = config.GetString("TfsBuildDefinitionName", "");
            if (!BuildServerSettingsHelper.IsRegexValid(tfsBuildDefinitionNameFilterSetting))
            {
                return;
            }

            _tfsBuildDefinitionNameFilter = new Regex(tfsBuildDefinitionNameFilterSetting, RegexOptions.Compiled);

            if (!string.IsNullOrEmpty(_tfsServer)
                && !string.IsNullOrEmpty(_tfsTeamCollectionName)
                && !string.IsNullOrEmpty(_projectName))
            {
                _tfsHelper = LoadAssemblyAndConnectToServer("TfsInterop.Vs2015")
                    ?? LoadAssemblyAndConnectToServer("TfsInterop.Vs2013")
                    ?? LoadAssemblyAndConnectToServer("TfsInterop.Vs2012");

                if (_tfsHelper == null)
                {
                    Trace.WriteLine("fail to load the good interop assembly :(");
                }
            }
        }

        [CanBeNull]
        private ITfsHelper LoadAssemblyAndConnectToServer(string assembly)
        {
            try
            {
                Trace.WriteLine("Try loading " + assembly + ".dll ...");
                var loadedAssembly = Assembly.Load(assembly);
                var tfsHelper = loadedAssembly.CreateInstance("TfsInterop.TfsHelper") as ITfsHelper;
                Trace.WriteLine("Create instance... OK");

                if (tfsHelper != null && tfsHelper.IsDependencyOk())
                {
                    tfsHelper.ConnectToTfsServer(_tfsServer, _tfsTeamCollectionName, _projectName, _tfsBuildDefinitionNameFilter);
                    Trace.WriteLine("Connection... OK");
                    return tfsHelper;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return null;
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
            if (_tfsHelper == null)
            {
                return Observable.Empty<BuildInfo>();
            }

            return Observable.Create<BuildInfo>((observer, cancellationToken) =>
                Task.Run(
                    () => scheduler.Schedule(() => ObserveBuilds(sinceDate, running, observer, cancellationToken))));
        }

        private void ObserveBuilds(DateTime? sinceDate, bool? running, IObserver<BuildInfo> observer, CancellationToken cancellationToken)
        {
            try
            {
                var builds = _tfsHelper.QueryBuilds(sinceDate, running);

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

        private static BuildInfo CreateBuildInfo(IBuild buildDetail)
        {
            var objectId = ObjectId.Parse(buildDetail.Revision.SubstringAfter(':'));

            return new BuildInfo
            {
                Id = buildDetail.Label,
                StartDate = buildDetail.StartDate,
                Status = (BuildInfo.BuildStatus)buildDetail.Status,
                Description = $"{buildDetail.Label} ({buildDetail.Description})",
                CommitHashList = new[] { objectId },
                Url = buildDetail.Url,
                ShowInBuildReportTab = false
            };
        }

        public void Dispose()
        {
            _tfsHelper?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
