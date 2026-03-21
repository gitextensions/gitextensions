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

    private ToolStripMenuItem AddRecentRepositories(ToolStripDropDownItem menuItemContainer, Repository repo, string? caption, int number, bool anchored = false)
    {
        string numberString = number switch { < 10 => $"&{number}", 10 => "1&0", _ => $"{number}" };
        ToolStripMenuItem item = new($"{numberString}: {caption}")
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

        return item;
    }

    private void UpdateBranchNames(List<(ToolStripMenuItem Item, string Path)> items)
    {
        ThreadHelper.FileAndForget(async () =>
        {
            (ToolStripMenuItem Item, string BranchName)[] updates = [.. items
                .AsParallel()
                .Select(x => (x.Item, BranchName: _repositoryCurrentBranchNameProvider.GetCurrentBranchName(x.Path)))
                .Where(x => !string.IsNullOrWhiteSpace(x.BranchName))];

            if (updates.Length is 0)
            {
                return;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            foreach ((ToolStripMenuItem item, string branchName) in updates)
            {
                item.ShortcutKeyDisplayString = branchName;
            }
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

        List<(ToolStripMenuItem Item, string Path)> itemsForBranchUpdate = new(pinnedRepos.Count + allRecentRepos.Count);
        foreach (IGrouping<string, RecentRepoInfo> repo in pinnedRepos.Concat(allRecentRepos).GroupBy(k => k.Repo.Category).OrderBy(k => k.Key))
        {
            AddFavouriteRepositories(repo.Key, repo);
        }

        UpdateBranchNames(itemsForBranchUpdate);

        return;

        void AddFavouriteRepositories(string? category, IEnumerable<RecentRepoInfo> repos)
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
            int number = 0;
            foreach (RecentRepoInfo r in repos)
            {
                itemsForBranchUpdate.Add((AddRecentRepositories(menuItemCategory, r.Repo, r.Caption, ++number), r.Repo.Path));
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

        List<(ToolStripMenuItem Item, string Path)> itemsForBranchUpdate = new(pinnedRepos.Count + allRecentRepos.Count);
        int number = 0;
        foreach (RecentRepoInfo repo in pinnedRepos)
        {
            itemsForBranchUpdate.Add((AddRecentRepositories(container, repo.Repo, repo.Caption, ++number, repo.Anchored), repo.Repo.Path));
        }

        if (allRecentRepos.Count > 0)
        {
            if (pinnedRepos.Count > 0)
            {
                container.DropDownItems.Add(new ToolStripSeparator());
            }

            foreach (RecentRepoInfo repo in allRecentRepos)
            {
                itemsForBranchUpdate.Add((AddRecentRepositories(container, repo.Repo, repo.Caption, ++number, repo.Anchored), repo.Repo.Path));
            }
        }

        UpdateBranchNames(itemsForBranchUpdate);
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

        internal void AddRecentRepositories(ToolStripDropDownItem menuItemContainer, Repository repo, string? caption, int number)
            => _service.AddRecentRepositories(menuItemContainer, repo, caption, number);

        internal void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container, in IList<Repository> repositoryHistory)
            => _service.PopulateFavouriteRepositoriesMenu(container, repositoryHistory);
    }
}
