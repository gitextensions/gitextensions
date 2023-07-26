using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using GitExtensions.Plugins.GitlabIntegration.ApiClient;
using GitExtensions.Plugins.GitlabIntegration.ApiClient.Models;
using GitUIPluginInterfaces;
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
        private readonly Dictionary<string, DateTime> _loadedItems = new();

        private readonly IGitlabApiClientFactory _apiClientFactory;
        private IGitlabApiClient? _apiClient;

        public GitlabAdapter()
            : this(new GitlabApiClientFactory())
        {
        }

        public GitlabAdapter(IGitlabApiClientFactory apiClientFactory)
        {
            _apiClientFactory = apiClientFactory;
        }

        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config, Action openSettings, Func<ObjectId, bool>? isCommitInRevisionGrid = null)
        {
             _apiClient = _apiClientFactory.CreateGitlabApiClient(
                 config.GetString("InstanceUrl", string.Empty),
                 config.GetString("ApiToken", string.Empty),
                 config.GetInt("ProjectId", 0));
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

            PagedResponse<GitlabPipeline> firstPage = await _apiClient.GetPipelinesAsync(sinceDate, running, 1);
            firstPage.Items.ForEach(item => ProcessLoadedBuild(item, observer));

            if (firstPage.TotalPages is > 1)
            {
                Task[] pagesTasks = new Task[firstPage.TotalPages.Value - 1];
                for (int i = 2; i <= firstPage.TotalPages; i++)
                {
                    Task<PagedResponse<GitlabPipeline>> pageTask = _apiClient.GetPipelinesAsync(sinceDate, running, i);
                    pagesTasks[i - 2] = pageTask.ContinueWith(x =>
                        {
                            x.Result.Items.ForEach(item => ProcessLoadedBuild(item, observer));
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

                while (currentPage.NextPage.HasValue)
                {
                    currentPage = await _apiClient.GetPipelinesAsync(sinceDate, running, currentPage.NextPage.Value);
                    currentPage.Items.ForEach(item => ProcessLoadedBuild(item, observer));
                }

                observer.OnCompleted();
            }
        }

        private void ProcessLoadedBuild(GitlabPipeline item, IObserver<BuildInfo> observer)
        {
            if (_loadedItems.ContainsKey(item.sha) == false || _loadedItems[item.sha] < item.updated_at)
            {
                _loadedItems[item.sha] = item.updated_at;
                observer.OnNext(item.ToBuildInfo());
            }
        }

        public void Dispose()
        {
            _apiClient?.Dispose();
        }
    }
}
