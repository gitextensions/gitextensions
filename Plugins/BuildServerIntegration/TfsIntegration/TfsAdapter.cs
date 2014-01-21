using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GitCommands.Settings;
using GitCommands.Utils;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using TfsInterop.Interface;

namespace TfsIntegration
{

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TfsIntegrationMetadata : BuildServerAdapterMetadataAttribute
    {
        public TfsIntegrationMetadata(string buildServerType)
            : base(buildServerType)
        {
        }

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
    [TfsIntegrationMetadata("Team Foundation Server")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class TfsAdapter : IBuildServerAdapter
    {
        private IBuildServerWatcher _buildServerWatcher;
        private ITfsHelper _tfsHelper;
        string _tfsServer;
        string _tfsTeamCollectionName;
        string _projectName;
        string _tfsBuildDefinitionName;

        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config)
        {
            if (_buildServerWatcher != null)
                throw new InvalidOperationException("Already initialized");

            _buildServerWatcher = buildServerWatcher;

            _tfsServer = config.GetString("TfsServer", null);
            _tfsTeamCollectionName = config.GetString("TfsTeamCollectionName", null);
            _projectName = config.GetString("ProjectName", null);
            _tfsBuildDefinitionName = config.GetString("TfsBuildDefinitionName", null);

            if (!string.IsNullOrEmpty(_tfsServer)
                && !string.IsNullOrEmpty(_tfsTeamCollectionName)
                && !string.IsNullOrEmpty(_projectName))
            {
                _tfsHelper = LoadAssemblyAndConnectToServer("TfsInterop.Vs2013")
                    ?? LoadAssemblyAndConnectToServer("TfsInterop.Vs2012");

                if (_tfsHelper == null)
                {
                    Trace.WriteLine("fail to load the good interop assembly :(");
                }
            }
        }

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
                    tfsHelper.ConnectToTfsServer(_tfsServer, _tfsTeamCollectionName, _projectName, _tfsBuildDefinitionName);
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
        public string UniqueKey
        {
            get { return _tfsServer + "/" + _tfsTeamCollectionName + "/" + _projectName; }
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
            if (_tfsHelper == null)
                return Observable.Empty<BuildInfo>();

            return Observable.Create<BuildInfo>((observer, cancellationToken) =>
                Task<IDisposable>.Factory.StartNew(
                    () => scheduler.Schedule(() => ObserveBuilds(sinceDate, running, observer, cancellationToken))));
        }

        private void ObserveBuilds(DateTime? sinceDate, bool? running, IObserver<BuildInfo> observer, CancellationToken cancellationToken)
        {
            try
            {
                var builds = _tfsHelper.QueryBuilds().AsQueryable();
                if (sinceDate.HasValue)
                    builds = builds.Where(b => b.StartDate > sinceDate.Value);
                if (running.HasValue)
                    builds = builds.Where(b => b.IsFinished != running.Value);

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
            string sha = buildDetail.Revision.Substring(buildDetail.Revision.LastIndexOf(":") + 1);

            var buildInfo = new BuildInfo
            {
                Id = buildDetail.Label,
                StartDate = buildDetail.StartDate,
                Status = (BuildInfo.BuildStatus)buildDetail.Status,
                Description = buildDetail.Label + " (" + buildDetail.Description + ")",
                CommitHashList = new[] { sha },
                Url = buildDetail.Url
            };

            return buildInfo;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
