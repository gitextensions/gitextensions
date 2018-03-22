using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TfsInterop.Interface
{
    public enum BuildStatus
    {
        Unknown,
        InProgress,
        Success,
        Failure,
        Unstable,
        Stopped
    }

    public interface IBuild
    {
        string Id { get; set; }
        string Label { get; set; }
        DateTime StartDate { get; set; }
        BuildStatus Status { get; set; }
        bool IsFinished { get; set; }
        string Description { get; set; }
        string Revision { get; set; }
        string Url { get; set; }
    }

    public interface ITfsHelper : IDisposable
    {
        bool IsDependencyOk();
        void ConnectToTfsServer(string hostname, string teamCollection, string projectName, Regex buildDefinitionFilter = null);
        IReadOnlyList<IBuild> QueryBuilds(DateTime? sinceDate, bool? running);
    }
}
