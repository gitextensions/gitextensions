using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using GitExtensions.Plugins.GitlabIntegration.ApiClient;
using GitExtensions.Plugins.GitlabIntegration.ApiClient.Models;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitExtensions.Plugins.GitlabIntegration
{
    [Export(typeof(IBuildServerAdapter))]
    [GitlabIntegrationMetadata(PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitlabAdapter : IBuildServerAdapter
    {
        private const int _pageSize = 100;
        public const string PluginName = "Gitlab";

        private ProjectGitlabApiClient _apiClient;

        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config, Action openSettings, Func<ObjectId, bool>? isCommitInRevisionGrid = null)
        {
            _apiClient = new ProjectGitlabApiClient("", "", 0);
        }

        public string UniqueKey { get; }

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
            PagedResponse<GitlabPipeline> firstPage = await _apiClient.GetPipelinesAsync(sinceDate, running, 0, _pageSize);

            Task[] pagesTasks = new Task[firstPage.Total / firstPage.PageSize];
            for (int i = 1; i < firstPage.Total / firstPage.PageSize; i++)
            {
                Task<PagedResponse<GitlabPipeline>> pageTask = _apiClient.GetPipelinesAsync(sinceDate, running, i, _pageSize);
                pagesTasks[i - 1] = pageTask.ContinueWith(x =>
                    {
                        x.Result.Items.ForEach(item => observer.OnNext(item.ToBuildInfo()));
                    },
                    cancellationToken,
                    TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
                    TaskScheduler.Current);
            }

            await Task.Factory.ContinueWhenAll(pagesTasks, t =>
            {
                observer.OnCompleted();
            }, cancellationToken);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
