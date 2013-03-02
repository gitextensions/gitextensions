using System;
using System.Reactive.Concurrency;
using GitCommands;

namespace GitUI.BuildServerIntegration
{
    public interface IBuildServerAdapter
    {
        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        string UniqueKey { get; }

        IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null);

        IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler);
    }
}