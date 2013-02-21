using System;
using GitCommands;

namespace GitUI.BuildServerIntegration
{
    public interface IBuildServerAdapter
    {
        IObservable<BuildInfo> CreateObservable(DateTime? sinceDate = null);
    }
}