using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using NSubstitute;

namespace GitExtensionsTests;

internal static class RepositoryHistoryTestHelper
{
    public static IRepositoryHistoryUIService CreateEmptyService()
        => new EmptyRepositoryHistoryService();

    // parity-scaffolding: isolated popup consumers supply value fixtures; real Browse tests use the public saved-history producer.
    public static void PopulateFavouriteFixture(RepositoryHistoryUIService service,
        GitUI.Compat.WinFormsControls.ToolStripDropDownItem container, IReadOnlyList<RepositoryHistoryEntry> entries)
    {
        foreach (IGrouping<string?, RepositoryHistoryEntry> group in entries.GroupBy(entry => entry.Repository.Category).OrderBy(group => group.Key))
        {
            GitUI.Compat.WinFormsControls.ToolStripMenuItem category = container.CreateMenuItem();
            category.Header = GitUI.Compat.AvaloniaTranslationUtils.ToAvaloniaMnemonics(group.Key ?? string.Empty);
            container.Items.Add(category);
            int number = 0;
            foreach (RepositoryHistoryEntry entry in group)
            {
                service.GetTestAccessor().AddRecentRepositories(category, entry.Repository, entry.Caption, ++number);
            }
        }
    }

    public static IUserRepositoriesListController CreateEmptyController()
    {
        IUserRepositoriesListController controller = Substitute.For<IUserRepositoriesListController>();
        controller.PreRenderRepositories(Arg.Any<string>()).Returns((
            Array.Empty<RecentRepoInfo>(), Array.Empty<RecentRepoInfo>()));
        return controller;
    }

    private sealed class EmptyRepositoryHistoryService : IRepositoryHistoryUIService
    {
        public event EventHandler<GitModuleEventArgs>? GitModuleChanged
        {
            add { }
            remove { }
        }

        public event EventHandler? HistoryChanged;

        public void PopulateFavouriteRepositoriesMenu(GitUI.Compat.WinFormsControls.ToolStripDropDownItem container)
            => container.Items.Clear();

        public void PopulateRecentRepositoriesMenu(GitUI.Compat.WinFormsControls.ToolStripDropDownItem container)
            => container.Items.Clear();

        public RepositoryHistorySnapshot LoadSnapshot() => new([], []);

        public void Invalidate() => HistoryChanged?.Invoke(this, EventArgs.Empty);

        public void TriggerBranchNameCacheUpdate(bool onlyIfEmpty = false)
        {
        }
    }
}
