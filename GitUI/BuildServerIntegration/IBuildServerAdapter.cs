using System;
using System.Reactive.Concurrency;
using GitCommands;

namespace GitUI.BuildServerIntegration
{
    public interface IBuildServerAdapter
    {
        IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null);
        IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler);
    }
}