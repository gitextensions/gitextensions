using System;

namespace GitCommands
{
    public class BuildInfo
    {
        public enum BuildStatus
        {
            Unknown,
            Success,
            Failure
        }

        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public BuildStatus Status { get; set; }
        public string Description { get; set; }
        public string CommitHash { get; set; }
    }
}