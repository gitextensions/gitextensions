using System;
using System.Reactive.Concurrency;

namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildServerAdapter : IDisposable
    {
        void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config, Func<ObjectId, bool> isCommitInRevisionGrid = null);

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        string UniqueKey { get; }

        IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null);

        IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler);
    }
}