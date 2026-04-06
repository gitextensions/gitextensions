using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using Newtonsoft.Json;

namespace GitExtensions.Plugins.GitHubActionsIntegration.ApiClient.Models;

/// <summary>
/// Represents a single GitHub Actions workflow run as returned by the REST API.
/// </summary>
public class GitHubActionsWorkflowRun
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("head_sha")]
    public string HeadSha { get; set; } = string.Empty;

    [JsonProperty("head_branch")]
    public string? HeadBranch { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("conclusion")]
    public string? Conclusion { get; set; }

    [JsonProperty("html_url")]
    public string? HtmlUrl { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonProperty("run_started_at")]
    public DateTime? RunStartedAt { get; set; }

    [JsonProperty("display_title")]
    public string? DisplayTitle { get; set; }

    [JsonProperty("event")]
    public string? Event { get; set; }

    [JsonProperty("run_number")]
    public int RunNumber { get; set; }

    [JsonProperty("run_attempt")]
    public int RunAttempt { get; set; }

    public BuildInfo ToBuildInfo()
    {
        BuildStatus status = MapStatus();

        BuildInfo result = new()
        {
            Id = Id.ToString(),
            StartDate = RunStartedAt ?? CreatedAt,
            CommitHashList = [ObjectId.Parse(HeadSha)],
            Url = HtmlUrl,
            ShowInBuildReportTab = false,
            Status = status,
            Duration = (long)(UpdatedAt - (RunStartedAt ?? CreatedAt)).TotalMilliseconds,
            Description = FormatDescription()
        };

        return result;

        BuildStatus MapStatus()
        {
            if (Status is "in_progress" or "queued" or "requested" or "waiting" or "pending")
            {
                return BuildStatus.InProgress;
            }

            return Conclusion switch
            {
                "success" => BuildStatus.Success,
                "failure" or "timed_out" => BuildStatus.Failure,
                "cancelled" => BuildStatus.Stopped,
                "action_required" or "stale" => BuildStatus.Unstable,
                "skipped" or "neutral" => BuildStatus.Success,
                _ => BuildStatus.Unknown,
            };
        }

        string FormatDescription()
        {
            string statusText = status switch
            {
                BuildStatus.InProgress => Status ?? "in progress",
                _ => Conclusion ?? Status ?? "unknown",
            };

            string name = Name ?? "workflow";
            return $"{name} #{RunNumber} ({statusText})";
        }
    }
}
