using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Plugins.GitHubActionsIntegration.ApiClient;
using GitExtensions.Plugins.GitHubActionsIntegration.ApiClient.Models;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft;

namespace GitExtensions.Plugins.GitHubActionsIntegration;

[Export(typeof(IBuildServerAdapter))]
[GitHubActionsIntegrationMetadata(PluginName)]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class GitHubActionsAdapter : IBuildServerAdapter
{
    public const string PluginName = "GitHub Actions";

    private const string DefaultApiUrl = "https://api.github.com";
    private const int PageSize = 100;

    private readonly ConcurrentDictionary<string, DateTime> _loadedItems = new();
    private readonly IGitHubActionsApiClientFactory _apiClientFactory;

    private IGitHubActionsApiClient? _apiClient;

    public GitHubActionsAdapter()
        : this(new GitHubActionsApiClientFactory())
    {
    }

    public GitHubActionsAdapter(IGitHubActionsApiClientFactory apiClientFactory)
    {
        _apiClientFactory = apiClientFactory;
    }

    public void Initialize(IBuildServerWatcher buildServerWatcher, SettingsSource config, Action openSettings, Func<ObjectId, bool>? isCommitInRevisionGrid = null)
    {
        string apiUrl = config.GetString("GitHubActionsApiUrl", null) ?? DefaultApiUrl;
        string? apiToken = config.GetString("GitHubActionsApiToken", null);

        // Use explicit settings if available, otherwise auto-detect from the current git remote
        string? owner = config.GetString("GitHubActionsOwner", null);
        if (string.IsNullOrWhiteSpace(owner))
        {
            owner = buildServerWatcher.ReplaceVariables("{cRepoProject}");
        }

        string? repository = config.GetString("GitHubActionsRepository", null);
        if (string.IsNullOrWhiteSpace(repository))
        {
            repository = buildServerWatcher.ReplaceVariables("{cRepoShortName}");
        }

        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repository))
        {
            return;
        }

        _apiClient = _apiClientFactory.CreateApiClient(apiUrl, owner, repository, apiToken);
    }

    public string UniqueKey
    {
        get
        {
            Validates.NotNull(_apiClient);
            return _apiClient.BaseUrl;
        }
    }

    public IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null)
    {
        return GetBuilds(sinceDate, running: false);
    }

    public IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler)
    {
        return GetBuilds(sinceDate: null, running: true);
    }

    private IObservable<BuildInfo> GetBuilds(DateTime? sinceDate, bool running)
    {
        return Observable.Create<BuildInfo>((observer, cancellationToken) =>
            ObserveBuildsAsync(sinceDate, running, observer, cancellationToken));
    }

    private async Task ObserveBuildsAsync(DateTime? sinceDate, bool running, IObserver<BuildInfo> observer, CancellationToken cancellationToken)
    {
        if (_apiClient is null)
        {
            observer.OnCompleted();
            return;
        }

        try
        {
            int page = 1;

            while (!cancellationToken.IsCancellationRequested)
            {
                GitHubActionsWorkflowRunsResponse response = await _apiClient.GetWorkflowRunsAsync(
                    running,
                    sinceDate,
                    page,
                    PageSize,
                    cancellationToken);

                if (response.WorkflowRuns.Count == 0)
                {
                    break;
                }

                ProcessWorkflowRuns(response.WorkflowRuns, observer);

                if (response.WorkflowRuns.Count < PageSize)
                {
                    break;
                }

                page++;
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during cancellation
        }

        observer.OnCompleted();
    }

    private void ProcessWorkflowRuns(List<GitHubActionsWorkflowRun> runs, IObserver<BuildInfo> observer)
    {
        foreach (GitHubActionsWorkflowRun run in runs)
        {
            if (!_loadedItems.TryGetValue(run.HeadSha, out DateTime lastUpdated) || lastUpdated < run.UpdatedAt)
            {
                _loadedItems[run.HeadSha] = run.UpdatedAt;
                observer.OnNext(run.ToBuildInfo());
            }
        }
    }

    public void Dispose()
    {
        _apiClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
