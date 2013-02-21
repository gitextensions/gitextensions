using System;
using GitCommands;

namespace GitUI.UserControls.RevisionGridClasses
{
    public interface IBuildServerAdapter
    {
        IObservable<BuildInfo> CreateObservable(DateTime? sinceDate = null);
    }
}