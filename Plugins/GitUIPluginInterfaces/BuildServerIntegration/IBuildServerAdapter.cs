using System.Reactive.Concurrency;

namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildServerAdapter : IDisposable
    {
        void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config, Action openSettings, Func<ObjectId, bool>? isCommitInRevisionGrid = null);

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        string UniqueKey { get; }

        IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null);

        IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler);

        /// <summary>
        ///  Provides an extension point for handling the switch of repositories.
        ///  For example, it could be used to clear build changes.
        /// </summary>
        void OnRepositoryChanged()
        {
            // Default implementation: we do nothing
        }
    }
}
