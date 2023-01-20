using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitUI.CommandsDialogs;
using GitUI.Properties;

namespace GitUI;

/// <summary>
///  Represents a service for managing the git repository history.
/// </summary>
public interface IRepositoryHistoryUIService
{
    /// <summary>
    ///  Occurs whenever the git module changes.
    /// </summary>
    event EventHandler<GitModuleEventArgs> GitModuleChanged;

    /// <summary>
    ///  Populates the "Favourite repositories" menu.
    /// </summary>
    /// <param name="container">The container to populate with menu items.</param>
    void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container);

    /// <summary>
    ///  Populates the "Recent repositories" menu.
    /// </summary>
    /// <param name="container">The container to populate with menu items.</param>
    void PopulateRecentRepositoriesMenu(ToolStripDropDownItem container);
}

internal class RepositoryHistoryUIService : IRepositoryHistoryUIService
{
    private readonly IRepositoryCurrentBranchNameProvider _repositoryCurrentBranchNameProvider;
    private readonly IInvalidRepositoryRemover _invalidRepositoryRemover;

    public event EventHandler<GitModuleEventArgs> GitModuleChanged;

    internal RepositoryHistoryUIService(IRepositoryCurrentBranchNameProvider repositoryCurrentBranchNameProvider, IInvalidRepositoryRemover invalidRepositoryRemover)
    {
        _repositoryCurrentBranchNameProvider = repositoryCurrentBranchNameProvider;
        _invalidRepositoryRemover = invalidRepositoryRemover;
    }

    private void AddRecentRepositories(ToolStripDropDownItem menuItemContainer, Repository repo, string? caption, bool anchored = false)
    {
        ToolStripMenuItem item = new(caption)
        {
            DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
        };

        if (anchored)
        {
            item.Image = Images.Pin;
        }

        menuItemContainer.DropDownItems.Add(item);

        item.Click += (obj, args) =>
        {
            OpenRepo(repo.Path);
        };

        if (repo.Path != caption)
        {
            item.ToolTipText = repo.Path;
        }

        ThreadHelper.FileAndForget(async () =>
        {
            string branchName = _repositoryCurrentBranchNameProvider.GetCurrentBranchName(repo.Path);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            item.ShortcutKeyDisplayString = branchName;
        });
    }

    private void ChangeWorkingDir(string path)
    {
        GitModule module = new(path);
        if (module.IsValidGitWorkingDir())
        {
            GitModuleChanged?.Invoke(this, new GitModuleEventArgs(module));
            return;
        }

        _invalidRepositoryRemover.ShowDeleteInvalidRepositoryDialog(path);
    }

    private void OpenRepo(string repoPath)
    {
        if (Control.ModifierKeys != Keys.Control)
        {
            ChangeWorkingDir(repoPath);
            return;
        }

        GitUICommands.LaunchBrowse(repoPath);
    }

    public void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container)
    {
        IList<Repository> repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync);
        if (repositoryHistory.Count < 1)
        {
            return;
        }

        PopulateFavouriteRepositoriesMenu(container, repositoryHistory);
    }

    private void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container, in IList<Repository> repositoryHistory)
    {
        List<RecentRepoInfo> pinnedRepos = [];
        List<RecentRepoInfo> allRecentRepos = [];

        RecentRepoSplitter splitter = new()
        {
            MeasureFont = container.Font,
        };

        splitter.SplitRecentRepos(repositoryHistory, pinnedRepos, allRecentRepos);

        foreach (IGrouping<string, RecentRepoInfo> repo in pinnedRepos.Union(allRecentRepos).GroupBy(k => k.Repo.Category).OrderBy(k => k.Key))
        {
            AddFavouriteRepositories(repo.Key, repo.ToList());
        }

        void AddFavouriteRepositories(string? category, IList<RecentRepoInfo> repos)
        {
            ToolStripMenuItem menuItemCategory;
            if (!container.DropDownItems.ContainsKey(category))
            {
                menuItemCategory = new ToolStripMenuItem(category);
                container.DropDownItems.Add(menuItemCategory);
            }
            else
            {
                menuItemCategory = (ToolStripMenuItem)container.DropDownItems[category];
            }

            menuItemCategory.DropDown.SuspendLayout();
            foreach (RecentRepoInfo r in repos)
            {
                AddRecentRepositories(menuItemCategory, r.Repo, r.Caption);
            }

            menuItemCategory.DropDown.ResumeLayout();
        }
    }

    public void PopulateRecentRepositoriesMenu(ToolStripDropDownItem container)
    {
        List<RecentRepoInfo> pinnedRepos = [];
        List<RecentRepoInfo> allRecentRepos = [];

        IList<Repository> repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Locals.LoadRecentHistoryAsync);
        if (repositoryHistory.Count < 1)
        {
            return;
        }

        RecentRepoSplitter splitter = new()
        {
            MeasureFont = container.Font,
        };

        splitter.SplitRecentRepos(repositoryHistory, pinnedRepos, allRecentRepos);

        foreach (RecentRepoInfo repo in pinnedRepos)
        {
            AddRecentRepositories(container, repo.Repo, repo.Caption, repo.Anchored);
        }

        if (allRecentRepos.Count > 0)
        {
            if (pinnedRepos.Count > 0)
            {
                container.DropDownItems.Add(new ToolStripSeparator());
            }

            foreach (RecentRepoInfo repo in allRecentRepos)
            {
                AddRecentRepositories(container, repo.Repo, repo.Caption, repo.Anchored);
            }
        }
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor
    {
        private readonly RepositoryHistoryUIService _service;

        public TestAccessor(RepositoryHistoryUIService service)
        {
            _service = service;
        }

        internal void AddRecentRepositories(ToolStripDropDownItem menuItemContainer, Repository repo, string? caption)
            => _service.AddRecentRepositories(menuItemContainer, repo, caption);

        internal void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container, in IList<Repository> repositoryHistory)
            => _service.PopulateFavouriteRepositoriesMenu(container, repositoryHistory);
    }
}
