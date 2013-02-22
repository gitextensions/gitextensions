using System;
using System.Reactive.Concurrency;
using GitCommands;

namespace GitUI.BuildServerIntegration
{
    public interface IBuildServerAdapter
    {
        IObservable<BuildInfo> CreateObservable(IScheduler scheduler, DateTime? sinceDate = null);
    }
}