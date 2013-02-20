using System;
using GitCommands;

namespace GitUI.UserControls.RevisionGridClasses
{
    public interface IBuildServerWatcher
    {
        IObservable<BuildInfo> CreateObservable(DateTime? sinceDate = null);
    }
}