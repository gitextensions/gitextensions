using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitExtensions.Plugins.GitHubActionsIntegration.ApiClient.Models;

namespace GitHubActionsIntegrationTests;

[TestFixture]
internal class GitHubActionsWorkflowRunTests
{
    private const string TestSha = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    [Test]
    public void ToBuildInfo_should_map_success_correctly()
    {
        GitHubActionsWorkflowRun run = CreateRun(status: "completed", conclusion: "success");

        BuildInfo result = run.ToBuildInfo();

        Assert.That(result.Status, Is.EqualTo(BuildStatus.Success));
        Assert.That(result.Id, Is.EqualTo("42"));
        Assert.That(result.CommitHashList, Has.Count.EqualTo(1));
        Assert.That(result.CommitHashList[0], Is.EqualTo(ObjectId.Parse(TestSha)));
        Assert.That(result.Url, Is.EqualTo("https://github.com/owner/repo/actions/runs/42"));
        Assert.That(result.ShowInBuildReportTab, Is.False);
    }

    [Test]
    public void ToBuildInfo_should_map_failure_correctly()
    {
        GitHubActionsWorkflowRun run = CreateRun(status: "completed", conclusion: "failure");

        BuildInfo result = run.ToBuildInfo();

        Assert.That(result.Status, Is.EqualTo(BuildStatus.Failure));
    }

    [Test]
    public void ToBuildInfo_should_map_cancelled_to_stopped()
    {
        GitHubActionsWorkflowRun run = CreateRun(status: "completed", conclusion: "cancelled");

        BuildInfo result = run.ToBuildInfo();

        Assert.That(result.Status, Is.EqualTo(BuildStatus.Stopped));
    }

    [Test]
    public void ToBuildInfo_should_map_in_progress_correctly()
    {
        GitHubActionsWorkflowRun run = CreateRun(status: "in_progress", conclusion: null);

        BuildInfo result = run.ToBuildInfo();

        Assert.That(result.Status, Is.EqualTo(BuildStatus.InProgress));
    }

    [Test]
    public void ToBuildInfo_should_map_queued_to_in_progress()
    {
        GitHubActionsWorkflowRun run = CreateRun(status: "queued", conclusion: null);

        BuildInfo result = run.ToBuildInfo();

        Assert.That(result.Status, Is.EqualTo(BuildStatus.InProgress));
    }

    [Test]
    public void ToBuildInfo_should_map_timed_out_to_failure()
    {
        GitHubActionsWorkflowRun run = CreateRun(status: "completed", conclusion: "timed_out");

        BuildInfo result = run.ToBuildInfo();

        Assert.That(result.Status, Is.EqualTo(BuildStatus.Failure));
    }

    [Test]
    public void ToBuildInfo_should_map_action_required_to_unstable()
    {
        GitHubActionsWorkflowRun run = CreateRun(status: "completed", conclusion: "action_required");

        BuildInfo result = run.ToBuildInfo();

        Assert.That(result.Status, Is.EqualTo(BuildStatus.Unstable));
    }

    [Test]
    public void ToBuildInfo_should_map_skipped_to_success()
    {
        GitHubActionsWorkflowRun run = CreateRun(status: "completed", conclusion: "skipped");

        BuildInfo result = run.ToBuildInfo();

        Assert.That(result.Status, Is.EqualTo(BuildStatus.Success));
    }

    [Test]
    public void ToBuildInfo_should_map_neutral_to_success()
    {
        GitHubActionsWorkflowRun run = CreateRun(status: "completed", conclusion: "neutral");

        BuildInfo result = run.ToBuildInfo();

        Assert.That(result.Status, Is.EqualTo(BuildStatus.Success));
    }

    [Test]
    public void ToBuildInfo_should_map_stale_to_unstable()
    {
        GitHubActionsWorkflowRun run = CreateRun(status: "completed", conclusion: "stale");

        BuildInfo result = run.ToBuildInfo();

        Assert.That(result.Status, Is.EqualTo(BuildStatus.Unstable));
    }

    [TestCase("waiting")]
    [TestCase("pending")]
    [TestCase("requested")]
    public void ToBuildInfo_should_map_pending_statuses_to_in_progress(string status)
    {
        GitHubActionsWorkflowRun run = CreateRun(status: status, conclusion: null);

        BuildInfo result = run.ToBuildInfo();

        Assert.That(result.Status, Is.EqualTo(BuildStatus.InProgress));
    }

    [Test]
    public void ToBuildInfo_should_format_description()
    {
        GitHubActionsWorkflowRun run = CreateRun(status: "completed", conclusion: "success");
        run.Name = "CI Build";
        run.RunNumber = 123;

        BuildInfo result = run.ToBuildInfo();

        Assert.That(result.Description, Is.EqualTo("CI Build #123 (success)"));
    }

    private static GitHubActionsWorkflowRun CreateRun(string status, string? conclusion)
    {
        DateTime now = DateTime.UtcNow;
        return new GitHubActionsWorkflowRun
        {
            Id = 42,
            Name = "Build",
            HeadSha = TestSha,
            HeadBranch = "main",
            Status = status,
            Conclusion = conclusion,
            HtmlUrl = "https://github.com/owner/repo/actions/runs/42",
            CreatedAt = now.AddMinutes(-5),
            UpdatedAt = now,
            RunStartedAt = now.AddMinutes(-5),
            RunNumber = 1,
            RunAttempt = 1,
            Event = "push",
            DisplayTitle = "Test commit",
        };
    }
}
