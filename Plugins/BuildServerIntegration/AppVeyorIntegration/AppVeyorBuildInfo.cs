using System;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace AppVeyorIntegration
{
    public sealed class AppVeyorBuildInfo : BuildInfo
    {
        private static readonly IBuildDurationFormatter _buildDurationFormatter = new BuildDurationFormatter();

        private int _buildProgressCount;

        public string BuildId { get; set; }
        public ObjectId CommitId { get; set; }
        public string AppVeyorBuildReportUrl { get; set; }
        public string Branch { get; set; }
        public string BaseApiUrl { get; set; }
        public string BaseWebUrl { get; set; }
        public string PullRequestText { get; set; }
        public string PullRequestTitle { get; set; }
        public string TestsResultText { get; set; }

        public bool IsRunning => Status == BuildStatus.InProgress;

        public void ChangeProgressCounter()
        {
            _buildProgressCount = (_buildProgressCount % 3) + 1;
        }

        public void UpdateDescription()
        {
            Description = _buildDurationFormatter.Format(Duration) + " " + TestsResultText + (!string.IsNullOrWhiteSpace(PullRequestText) ? " " + PullRequestText : string.Empty) + " " + Id;
            Tooltip = DisplayStatus + Environment.NewLine
                                    + (Duration.HasValue ? _buildDurationFormatter.Format(Duration) + Environment.NewLine : string.Empty)
                                    + (!string.IsNullOrWhiteSpace(TestsResultText) ? TestsResultText + Environment.NewLine : string.Empty)
                                    + (!string.IsNullOrWhiteSpace(PullRequestText) ? PullRequestText + ": " + PullRequestTitle + Environment.NewLine : string.Empty)
                                    + Id;
        }

        private string DisplayStatus
        {
            get
            {
                if (Status != BuildStatus.InProgress)
                {
                    return Status.ToString("G");
                }

                return "In progress" + new string('.', _buildProgressCount) + new string(' ', 3 - _buildProgressCount);
            }
        }
    }
}