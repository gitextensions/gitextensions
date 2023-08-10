using GitCommands;
using GitCommands.UserRepositoryHistory;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public interface IUserRepositoriesListController
    {
        Task AssignCategoryAsync(Repository repository, string? category);
        string GetCurrentBranchName(string path);
        bool IsValidGitWorkingDir(string path);
        (IReadOnlyList<RecentRepoInfo> recentRepositories, IReadOnlyList<RecentRepoInfo> favouriteRepositories) PreRenderRepositories(Graphics g, string filter);
        bool RemoveInvalidRepository(string path);
        void ClearCache();
    }

    public sealed class UserRepositoriesListController : IUserRepositoriesListController
    {
        private readonly ILocalRepositoryManager _localRepositoryManager;
        private readonly IInvalidRepositoryRemover _invalidRepositoryRemover;

        // Holds the raw, unfiltered list of repositories.
        // This is done to allow fast filtering of all known repos.
        private IList<Repository>? _allRecentRepositories;
        private IList<Repository>? _allFavoriteRepositories;

        public UserRepositoriesListController(ILocalRepositoryManager localRepositoryManager, IInvalidRepositoryRemover invalidRepositoryRemover)
        {
            _localRepositoryManager = localRepositoryManager;
            _invalidRepositoryRemover = invalidRepositoryRemover;
        }

        public async Task AssignCategoryAsync(Repository repository, string? category)
        {
            if (repository is null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            await _localRepositoryManager.AssignCategoryAsync(repository, category);
        }

        /// <summary>
        /// Clears the repository cache. After this call the repository list will be loaded from disk.
        /// </summary>
        public void ClearCache()
        {
            _allRecentRepositories = null;
            _allFavoriteRepositories = null;
        }

        public string GetCurrentBranchName(string path)
        {
            if (!AppSettings.ShowRepoCurrentBranch || GitModule.IsBareRepository(path))
            {
                return string.Empty;
            }

            return GitModule.GetSelectedBranchFast(path);
        }

        public bool IsValidGitWorkingDir(string path)
        {
            return GitModule.IsValidGitWorkingDir(path);
        }

        public (IReadOnlyList<RecentRepoInfo> recentRepositories, IReadOnlyList<RecentRepoInfo> favouriteRepositories) PreRenderRepositories(Graphics g, string pattern)
        {
            List<RecentRepoInfo> pinnedRepos = new();
            List<RecentRepoInfo> allRecentRepos = new();

            RecentRepoSplitter splitter = new()
            {
                Graphics = g,
                MeasureFont = AppSettings.Font,

                MaxPinnedRepositories = AppSettings.MaxPinnedRepositories,
                RecentReposComboMinWidth = AppSettings.RecentReposComboMinWidth,
                ShorteningStrategy = AppSettings.ShorteningRecentRepoPathStrategy,
                SortAllRecentRepos = AppSettings.SortAllRecentRepos,
                SortPinnedRepos = AppSettings.SortPinnedRepos
            };

            _allRecentRepositories ??= ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Locals.LoadRecentHistoryAsync);
            var repositories = Filter(_allRecentRepositories, pattern);
            splitter.SplitRecentRepos(repositories, pinnedRepos, allRecentRepos);
            var recentRepositories = pinnedRepos.Union(allRecentRepos).ToList();

            _allFavoriteRepositories ??= ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync);
            repositories = Filter(_allFavoriteRepositories, pattern);
            pinnedRepos.Clear();
            allRecentRepos.Clear();
            splitter.SplitRecentRepos(repositories, pinnedRepos, allRecentRepos);
            var favouriteRepositories = pinnedRepos.Union(allRecentRepos).ToList();

            return (recentRepositories, favouriteRepositories);
        }

        public bool RemoveInvalidRepository(string path)
           => _invalidRepositoryRemover.ShowDeleteInvalidRepositoryDialog(path);

        private static IList<Repository> Filter(IList<Repository> repositories, string pattern)
        {
            if (pattern.Length == 0)
            {
                return repositories;
            }

            return repositories
                .Where(r => r.Path.Contains(pattern, StringComparison.CurrentCultureIgnoreCase))
                .ToList();
        }
    }
}
