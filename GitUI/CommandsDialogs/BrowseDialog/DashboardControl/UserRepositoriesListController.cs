using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GitCommands;
using GitCommands.UserRepositoryHistory;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public interface IUserRepositoriesListController
    {
        Task AssignCategoryAsync(Repository repository, string category);
        string GetCurrentBranchName(string path);
        bool IsValidGitWorkingDir(string path);
        (IReadOnlyList<RecentRepoInfo> recentRepositories, IReadOnlyList<RecentRepoInfo> favouriteRepositories) PreRenderRepositories(Graphics g);
        void RemoveRepository(string path);
    }

    public sealed class UserRepositoriesListController : IUserRepositoriesListController
    {
        private readonly ILocalRepositoryManager _localRepositoryManager;

        public UserRepositoriesListController(ILocalRepositoryManager localRepositoryManager)
        {
            _localRepositoryManager = localRepositoryManager;
        }

        public async Task AssignCategoryAsync(Repository repository, string category)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            await _localRepositoryManager.AssignCategoryAsync(repository, category);
        }

        public string GetCurrentBranchName(string path)
        {
            if (!AppSettings.DashboardShowCurrentBranch || GitModule.IsBareRepository(path))
            {
                return string.Empty;
            }

            return GitModule.GetSelectedBranchFast(path);
        }

        public bool IsValidGitWorkingDir(string path)
        {
            return GitModule.IsValidGitWorkingDir(path);
        }

        public (IReadOnlyList<RecentRepoInfo> recentRepositories, IReadOnlyList<RecentRepoInfo> favouriteRepositories) PreRenderRepositories(Graphics g)
        {
            var mostRecentRepos = new List<RecentRepoInfo>();
            var lessRecentRepos = new List<RecentRepoInfo>();

            var splitter = new RecentRepoSplitter
            {
                Graphics = g,
                MeasureFont = AppSettings.Font,

                MaxRecentRepositories = AppSettings.MaxMostRecentRepositories,
                RecentReposComboMinWidth = AppSettings.RecentReposComboMinWidth,
                ShorteningStrategy = AppSettings.ShorteningRecentRepoPathStrategy,
                SortLessRecentRepos = AppSettings.SortLessRecentRepos,
                SortMostRecentRepos = AppSettings.SortMostRecentRepos
            };

            var repositories = ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.LoadRecentHistoryAsync());
            splitter.SplitRecentRepos(repositories, mostRecentRepos, lessRecentRepos);
            var recentRepositories = mostRecentRepos.Union(lessRecentRepos).ToList();

            repositories = ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync());
            mostRecentRepos.Clear();
            lessRecentRepos.Clear();
            splitter.SplitRecentRepos(repositories, mostRecentRepos, lessRecentRepos);
            var favouriteRepositories = mostRecentRepos.Union(lessRecentRepos).ToList();

            return (recentRepositories, favouriteRepositories);
        }

        public void RemoveRepository(string path)
        {
            ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveRecentAsync(path));
        }
    }
}