using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Plugins.GitlabIntegration.ApiClient;
using GitExtensions.Plugins.GitlabIntegration.ApiClient.Models;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft;

namespace GitExtensions.Plugins.GitlabIntegration
{
    [Export(typeof(IBuildServerAdapter))]
    [GitlabIntegrationMetadata(PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitlabAdapter : IBuildServerAdapter
    {
        public const string PluginName = "Gitlab";
        private readonly ConcurrentDictionary<string, DateTime> _loadedItems = new();

        private readonly IGitlabApiClientFactory _apiClientFactory;
        private IGitlabApiClient? _apiClient;
        private int? _pagesLimit;

        public GitlabAdapter()
            : this(new GitlabApiClientFactory())
        {
        }

        public GitlabAdapter(IGitlabApiClientFactory apiClientFactory)
        {
            _apiClientFactory = apiClientFactory;
        }

        public void Initialize(IBuildServerWatcher buildServerWatcher, SettingsSource config, Action openSettings, Func<ObjectId, bool>? isCommitInRevisionGrid = null)
        {
            _apiClient = _apiClientFactory.CreateGitlabApiClient(
                config.GetString("InstanceUrl", string.Empty),
                config.GetString("ApiToken", string.Empty),
                config.GetInt("ProjectId", 0));

            _pagesLimit = config.GetInt("PagesLimit");
            if (_pagesLimit is 0)
            {
                _pagesLimit = null;
            }
        }

        public string UniqueKey
        {
            get
            {
                Validates.NotNull(_apiClient);
                return _apiClient.InstanceUrl;
            }
        }

        public IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null)
        {
            return GetBuilds(scheduler, sinceDate, false);
        }

        public IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler)
        {
            return GetBuilds(scheduler, null, true);
        }

        private IObservable<BuildInfo> GetBuilds(IScheduler scheduler, DateTime? sinceDate = null, bool running = false)
        {
            return Observable.Create<BuildInfo>((observer, cancellationToken) => ObserveBuildsAsync(sinceDate, running, observer, cancellationToken));
        }

        private async Task ObserveBuildsAsync(DateTime? sinceDate, bool running, IObserver<BuildInfo> observer, CancellationToken cancellationToken)
        {
            Validates.NotNull(_apiClient);

            try
            {
                PagedResponse<GitlabPipeline> firstPage = await _apiClient.GetPipelinesAsync(sinceDate, running, 1, cancellationToken);
                ProcessLoadedBuilds(firstPage.Items, observer);

                if (firstPage.TotalPages is > 1)
                {
                    int totalPages = firstPage.TotalPages.Value;
                    if (_pagesLimit < totalPages)
                    {
                        totalPages = _pagesLimit.Value;
                    }

                    Task[] pagesTasks = new Task[totalPages - 1];
                    for (int i = 2; i <= totalPages; i++)
                    {
                        Task<PagedResponse<GitlabPipeline>> pageTask = _apiClient.GetPipelinesAsync(sinceDate, running, i, cancellationToken);
                        pagesTasks[i - 2] = pageTask.ContinueWith(x =>
                            {
                                ProcessLoadedBuilds(x.Result.Items, observer);
                            },
                            cancellationToken,
                            TaskContinuationOptions.None,
                            TaskScheduler.Current);
                    }

                    await Task.Factory.ContinueWhenAll(pagesTasks, t =>
                    {
                        observer.OnCompleted();
                    }, cancellationToken);
                }
                else
                {
                    PagedResponse<GitlabPipeline> currentPage = firstPage;

                    while (currentPage.NextPage.HasValue
                           && (_pagesLimit.HasValue == false || currentPage.NextPage.Value <= _pagesLimit.Value)
                           && cancellationToken.IsCancellationRequested == false)
                    {
                        currentPage = await _apiClient.GetPipelinesAsync(sinceDate, running, currentPage.NextPage.Value, cancellationToken);
                        ProcessLoadedBuilds(currentPage.Items, observer);
                    }

                    observer.OnCompleted();
                }
            }
            catch (OperationCanceledException)
            {
                observer.OnCompleted();
            }
        }

        private void ProcessLoadedBuilds(IEnumerable<GitlabPipeline> items, IObserver<BuildInfo> observer)
        {
            foreach (GitlabPipeline item in items)
            {
                if (!_loadedItems.TryGetValue(item.Sha, out DateTime dateTime) || dateTime < item.UpdatedAt)
                {
                    _loadedItems[item.Sha] = item.UpdatedAt;
                    observer.OnNext(item.ToBuildInfo());
                }
            }
        }

        public void Dispose()
        {
            _apiClient?.Dispose();
        }
    }
}
