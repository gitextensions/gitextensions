using GitExtensions.Extensibility.Git;

namespace GitExtensions.Extensibility.BuildServerIntegration;

public class BuildInfo
{
    public string? Id { get; set; }
    public DateTime StartDate { get; set; }
    public long? Duration { get; set; }
    public BuildStatus Status { get; set; }
    public string? Description { get; set; }
    public IReadOnlyList<ObjectId> CommitHashList { get; set; } = Array.Empty<ObjectId>();
    public string? Url { get; set; }
    public bool ShowInBuildReportTab { get; set; } = true;
    public string? Tooltip { get; set; }
    public string? PullRequestUrl { get; set; }

    public string StatusSymbol => Status switch
    {
        BuildStatus.Success => "✔",
        BuildStatus.Failure => "❌",
        BuildStatus.InProgress => "▶️",
        BuildStatus.Stopped => "⏹️",
        BuildStatus.Unstable => "❗",
        _ => "❓",
    };
}
