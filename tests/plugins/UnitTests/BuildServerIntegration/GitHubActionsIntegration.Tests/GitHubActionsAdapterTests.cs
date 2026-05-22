using System.Reactive.Concurrency;
using CommonTestUtils;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Plugins.GitHubActionsIntegration;
using GitExtensions.Plugins.GitHubActionsIntegration.ApiClient;
using GitExtensions.Plugins.GitHubActionsIntegration.ApiClient.Models;
using GitUIPluginInterfaces.BuildServerIntegration;
using NSubstitute;

namespace GitHubActionsIntegrationTests;
internal class GitHubActionsAdapterTests
{
    private const string TestSha1 = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string TestSha2 = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string TestSha3 = "cccccccccccccccccccccccccccccccccccccccc";

    private GitHubActionsAdapter _target = null!;
    private readonly IScheduler _scheduler = ImmediateScheduler.Instance;
    private IGitHubActionsApiClient _apiClient = null!;

    private IGitHubActionsApiClientFactory _apiClientFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _apiClient = Substitute.For<IGitHubActionsApiClient>();
        _apiClient.BaseUrl.Returns("https://api.github.com/repos/owner/repo");

        _apiClientFactory = Substitute.For<IGitHubActionsApiClientFactory>();
        _apiClientFactory.CreateApiClient(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>())
            .Returns(_apiClient);

        _target = new GitHubActionsAdapter(_apiClientFactory);

        MemorySettings settings = new();
        settings.SetString("GitHubActionsOwner", "owner");
        settings.SetString("GitHubActionsRepository", "repo");

        _target.Initialize(Substitute.For<IBuildServerWatcher>(), settings, () => { });
    }

    [TearDown]
    public void TearDown()
    {
        _target.Dispose();
        _apiClient.Dispose();
    }

    [Test]
    public void UniqueKey_should_return_base_url()
    {
        _target.UniqueKey.Should().Be("https://api.github.com/repos/owner/repo");
    }

    [Test]
    public void Initialize_should_use_owner_and_repo_from_settings()
    {
        GitHubActionsAdapter adapter = new(_apiClientFactory);

        MemorySettings settings = new();
        settings.SetString("GitHubActionsOwner", "my-owner");
        settings.SetString("GitHubActionsRepository", "my-repo");

        adapter.Initialize(Substitute.For<IBuildServerWatcher>(), settings, () => { });

        _apiClientFactory.Received(1).CreateApiClient(
            "https://api.github.com",
            "my-owner",
            "my-repo",
            Arg.Any<string?>());
    }

    [Test]
    public void Initialize_should_not_create_client_when_owner_is_missing()
    {
        IGitHubActionsApiClientFactory freshFactory = Substitute.For<IGitHubActionsApiClientFactory>();
        GitHubActionsAdapter adapter = new(freshFactory);

        MemorySettings settings = new();
        settings.SetString("GitHubActionsRepository", "my-repo");

        adapter.Initialize(Substitute.For<IBuildServerWatcher>(), settings, () => { });

        freshFactory.DidNotReceive().CreateApiClient(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>());
    }

    [Test]
    public void GetFinishedBuildsSince_should_return_empty_when_no_runs()
    {
        GitHubActionsWorkflowRunsResponse emptyResponse = new()
        {
            TotalCount = 0,
            WorkflowRuns = []
        };

        _apiClient.GetWorkflowRunsAsync(false, Arg.Any<DateTime?>(), 1, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(emptyResponse);

        List<BuildInfo> result = ProcessGetFinishedBuildsRequest();

        result.Should().BeEmpty();
        _apiClient.Received(1).GetWorkflowRunsAsync(false, Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public void GetFinishedBuildsSince_should_return_single_build()
    {
        DateTime now = DateTime.UtcNow;
        GitHubActionsWorkflowRunsResponse response = new()
        {
            TotalCount = 1,
            WorkflowRuns =
            [
                new GitHubActionsWorkflowRun
                {
                    Id = 1,
                    Name = "CI",
                    HeadSha = TestSha1,
                    Status = "completed",
                    Conclusion = "success",
                    HtmlUrl = "https://github.com/owner/repo/actions/runs/1",
                    CreatedAt = now.AddMinutes(-5),
                    UpdatedAt = now,
                    RunStartedAt = now.AddMinutes(-5),
                    RunNumber = 1,
                    RunAttempt = 1,
                }
            ]
        };

        _apiClient.GetWorkflowRunsAsync(false, Arg.Any<DateTime?>(), 1, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(response);

        List<BuildInfo> result = ProcessGetFinishedBuildsRequest();

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(BuildStatus.Success);
        result[0].Id.Should().Be("1");
    }

    [Test]
    public void GetRunningBuilds_should_return_in_progress_builds()
    {
        DateTime now = DateTime.UtcNow;
        GitHubActionsWorkflowRunsResponse response = new()
        {
            TotalCount = 1,
            WorkflowRuns =
            [
                new GitHubActionsWorkflowRun
                {
                    Id = 2,
                    Name = "CI",
                    HeadSha = TestSha2,
                    Status = "in_progress",
                    Conclusion = null,
                    HtmlUrl = "https://github.com/owner/repo/actions/runs/2",
                    CreatedAt = now.AddMinutes(-2),
                    UpdatedAt = now,
                    RunStartedAt = now.AddMinutes(-2),
                    RunNumber = 2,
                    RunAttempt = 1,
                }
            ]
        };

        _apiClient.GetWorkflowRunsAsync(true, Arg.Any<DateTime?>(), 1, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(response);

        List<BuildInfo> result = ProcessGetRunningBuildsRequest();

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(BuildStatus.InProgress);
    }

    [Test]
    public void GetFinishedBuildsSince_should_not_emit_duplicates_for_same_sha()
    {
        DateTime now = DateTime.UtcNow;
        GitHubActionsWorkflowRun run = new()
        {
            Id = 3,
            Name = "CI",
            HeadSha = TestSha3,
            Status = "completed",
            Conclusion = "success",
            HtmlUrl = "https://github.com/owner/repo/actions/runs/3",
            CreatedAt = now.AddMinutes(-5),
            UpdatedAt = now,
            RunStartedAt = now.AddMinutes(-5),
            RunNumber = 3,
            RunAttempt = 1,
        };

        GitHubActionsWorkflowRunsResponse response = new()
        {
            TotalCount = 1,
            WorkflowRuns = [run]
        };

        _apiClient.GetWorkflowRunsAsync(false, Arg.Any<DateTime?>(), 1, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(response);

        List<BuildInfo> firstResult = ProcessGetFinishedBuildsRequest();
        List<BuildInfo> secondResult = ProcessGetFinishedBuildsRequest();

        firstResult.Should().HaveCount(1);
        secondResult.Should().BeEmpty("duplicate SHA should be filtered out");
    }

    [Test]
    public void GetFinishedBuildsSince_should_update_when_sha_has_newer_timestamp()
    {
        DateTime now = DateTime.UtcNow;
        GitHubActionsWorkflowRun firstRun = new()
        {
            Id = 4,
            Name = "CI",
            HeadSha = TestSha1,
            Status = "completed",
            Conclusion = "failure",
            HtmlUrl = "https://github.com/owner/repo/actions/runs/4",
            CreatedAt = now.AddMinutes(-10),
            UpdatedAt = now.AddMinutes(-5),
            RunStartedAt = now.AddMinutes(-10),
            RunNumber = 4,
            RunAttempt = 1,
        };

        GitHubActionsWorkflowRunsResponse firstResponse = new()
        {
            TotalCount = 1,
            WorkflowRuns = [firstRun]
        };

        _apiClient.GetWorkflowRunsAsync(false, Arg.Any<DateTime?>(), 1, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(firstResponse);

        List<BuildInfo> firstResult = ProcessGetFinishedBuildsRequest();
        firstResult.Should().HaveCount(1);
        firstResult[0].Status.Should().Be(BuildStatus.Failure);

        GitHubActionsWorkflowRun updatedRun = new()
        {
            Id = 5,
            Name = "CI",
            HeadSha = TestSha1,
            Status = "completed",
            Conclusion = "success",
            HtmlUrl = "https://github.com/owner/repo/actions/runs/5",
            CreatedAt = now.AddMinutes(-3),
            UpdatedAt = now,
            RunStartedAt = now.AddMinutes(-3),
            RunNumber = 5,
            RunAttempt = 1,
        };

        GitHubActionsWorkflowRunsResponse updatedResponse = new()
        {
            TotalCount = 1,
            WorkflowRuns = [updatedRun]
        };

        _apiClient.GetWorkflowRunsAsync(false, Arg.Any<DateTime?>(), 1, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(updatedResponse);

        List<BuildInfo> secondResult = ProcessGetFinishedBuildsRequest();
        secondResult.Should().HaveCount(1, "updated timestamp should allow re-emission");
        secondResult[0].Status.Should().Be(BuildStatus.Success);
    }

    private List<BuildInfo> ProcessGetFinishedBuildsRequest()
    {
        using ManualResetEvent observableEvent = new(false);
        List<BuildInfo> result = [];
        Exception? observableException = null;

        IObservable<BuildInfo> observable = _target.GetFinishedBuildsSince(_scheduler);
        using IDisposable subscription = observable.Subscribe(
            e => result.Add(e),
            ex =>
            {
                observableException = ex;
                observableEvent.Set();
            },
            () => observableEvent.Set());

        observableEvent.WaitOne(TimeSpan.FromSeconds(10)).Should().BeTrue();
        observableException.Should().BeNull();

        return result;
    }

    private List<BuildInfo> ProcessGetRunningBuildsRequest()
    {
        using ManualResetEvent observableEvent = new(false);
        List<BuildInfo> result = [];
        Exception? observableException = null;

        IObservable<BuildInfo> observable = _target.GetRunningBuilds(_scheduler);
        using IDisposable subscription = observable.Subscribe(
            e => result.Add(e),
            ex =>
            {
                observableException = ex;
                observableEvent.Set();
            },
            () => observableEvent.Set());

        observableEvent.WaitOne(TimeSpan.FromSeconds(10)).Should().BeTrue();
        observableException.Should().BeNull();

        return result;
    }
}
