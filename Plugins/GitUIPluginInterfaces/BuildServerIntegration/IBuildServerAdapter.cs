using System;
using System.Reactive.Concurrency;
using Nini.Config;

namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildServerAdapter
    {
        void Initialize(IBuildServerWatcher buildServerWatcher, IConfig config);

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        string UniqueKey { get; }

        IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null);

        IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler);
    }
}