using System;
using System.Collections.Generic;

namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public class BuildInfo
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

        public string Id { get; set; }
        public DateTime StartDate { get; set; }
        public long? Duration { get; set; }
        public BuildStatus Status { get; set; }
        public string Description { get; set; }
        public IReadOnlyList<ObjectId> CommitHashList { get; set; }
        public string Url { get; set; }
        public bool ShowInBuildReportTab { get; set; } = true;
        public string Tooltip { get; set; }
    }
}