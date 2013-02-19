using System;
using GitCommands;

namespace GitUI.RevisionGridClasses
{
    public interface IBuildServerWatcher
    {
        IObservable<BuildInfo> CreateObservable(DateTime? sinceDate = null);
    }
}