using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class DashboardTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Repository_list_should_show_shared_recent_favourite_and_branch_data()
    {
        Repository recent = new(@"C:\repos\recent");
        Repository favourite = new(@"C:\repos\favourite") { Category = "Team" };
        RepositoryHistorySnapshot snapshot =
            new RepositoryHistorySnapshot(
                [new RepositoryHistoryEntry(recent, "recent", "main", IsFavourite: false, IsAnchored: false)],
                [new RepositoryHistoryEntry(favourite, "favourite", "feature", IsFavourite: true, IsAnchored: false)]);
        IUserRepositoriesListController controller = CreateController(snapshot);
        IRepositoryHistoryUIService history = CreateHistory(snapshot);
        UserRepositoriesList list = new();

        list.Initialize(controller, history, () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);

        UserRepositoriesList.TestAccessor accessor = list.GetTestAccessor();
        accessor.List.Items.OfType<UserRepositoriesList.RepositoryGroupItem>().Select(row => row.Name)
            .Should().Contain("Recent repositories", "Team");
        accessor.List.Items.OfType<UserRepositoriesList.RepositoryListItem>().Select(row => row.BranchName)
            .Should().Contain("main", "feature");

        accessor.Search.Text = "feature";

        accessor.List.Items.OfType<UserRepositoriesList.RepositoryListItem>()
            .Should().ContainSingle(row => row.Repository.Repo.Path == favourite.Path);
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Category_header_action_should_remain_enabled_and_hover_color_should_be_applied()
    {
        Repository favourite = new(@"C:\repos\favourite") { Category = "Team" };
        RepositoryHistorySnapshot snapshot = new(
            [],
            [new RepositoryHistoryEntry(favourite, "favourite", "main", IsFavourite: true, IsAnchored: false)]);
        UserRepositoriesList list = new();
        list.Initialize(CreateController(snapshot), CreateHistory(snapshot), () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);
        Window window = new() { Content = list };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Button categoryAction = list.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => button.Classes.Contains("dashboard-group-action"));
            categoryAction.IsEffectivelyEnabled.Should().BeTrue();

            Avalonia.Media.Color hoverColor = Avalonia.Media.Color.FromRgb(1, 2, 3);
            list.HoverColor = hoverColor;
            ((Avalonia.Media.SolidColorBrush)list.Resources["DashboardRepositoryHoverBrush"]!).Color
                .Should().Be(hoverColor);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Selecting_a_valid_dashboard_repository_should_raise_the_module_transition()
    {
        string repositoryPath = FindRepositoryRoot();
        Repository repository = new(repositoryPath);
        RepositoryHistorySnapshot snapshot =
            new RepositoryHistorySnapshot(
                [new RepositoryHistoryEntry(repository, "selected", "main", IsFavourite: false, IsAnchored: false)],
                []);
        IUserRepositoriesListController controller = CreateController(snapshot);
        IRepositoryHistoryUIService history = CreateHistory(snapshot);
        IGitExecutorProvider executorProvider = Substitute.For<IGitExecutorProvider>();
        IGitExecutor executor = Substitute.For<IGitExecutor>();
        executor.WorkingDir.Returns(repositoryPath);
        executor.GetGitDirectory().Returns(Path.Join(repositoryPath, ".git"));
        executorProvider.GetExecutor(repositoryPath).Returns(executor);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.GetService(typeof(IGitExecutorProvider)).Returns(executorProvider);
        UserRepositoriesList list = new();
        GitModuleEventArgs? transition = null;
        list.GitModuleChanged += (_, e) => transition = e;
        list.Initialize(controller, history, () => commands);
        list.ShowRecentRepositories(reloadData: false);
        UserRepositoriesList.TestAccessor accessor = list.GetTestAccessor();
        accessor.List.SelectedItem = accessor.List.Items
            .OfType<UserRepositoriesList.RepositoryListItem>()
            .Single();

        accessor.OpenSelected();

        transition.Should().NotBeNull();
        Path.TrimEndingDirectorySeparator(transition!.GitModule.WorkingDir)
            .Should().Be(Path.TrimEndingDirectorySeparator(repositoryPath));
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Single_clicking_a_valid_dashboard_repository_should_raise_the_module_transition()
    {
        string repositoryPath = FindRepositoryRoot();
        Repository repository = new(repositoryPath);
        RepositoryHistorySnapshot snapshot =
            new RepositoryHistorySnapshot(
                [new RepositoryHistoryEntry(repository, "selected", "main", IsFavourite: false, IsAnchored: false)],
                []);
        IUserRepositoriesListController controller = CreateController(snapshot);
        IRepositoryHistoryUIService history = CreateHistory(snapshot);
        IGitExecutorProvider executorProvider = Substitute.For<IGitExecutorProvider>();
        IGitExecutor executor = Substitute.For<IGitExecutor>();
        executor.WorkingDir.Returns(repositoryPath);
        executor.GetGitDirectory().Returns(Path.Join(repositoryPath, ".git"));
        executorProvider.GetExecutor(repositoryPath).Returns(executor);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.GetService(typeof(IGitExecutorProvider)).Returns(executorProvider);
        UserRepositoriesList list = new();
        GitModuleEventArgs? transition = null;
        list.GitModuleChanged += (_, e) => transition = e;
        list.Initialize(controller, history, () => commands);
        list.ShowRecentRepositories(reloadData: false);
        Window window = new()
        {
            Width = 560,
            Height = 260,
            Content = list,
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            ListBoxItem repositoryRow = list.GetVisualDescendants()
                .OfType<ListBoxItem>()
                .Single(item => item.Content is UserRepositoriesList.RepositoryListItem);
            Avalonia.Point clickPoint = Avalonia.VisualExtensions.TranslatePoint(
                repositoryRow,
                new Avalonia.Point(repositoryRow.Bounds.Width / 2, repositoryRow.Bounds.Height / 2),
                window) ?? throw new InvalidOperationException("The repository row position was not available.");

            window.MouseDown(clickPoint, MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(clickPoint, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            transition.Should().NotBeNull("WinForms uses ItemActivation.OneClick for this list");
            Path.TrimEndingDirectorySeparator(transition!.GitModule.WorkingDir)
                .Should().Be(Path.TrimEndingDirectorySeparator(repositoryPath));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Attached_repository_list_should_replace_rows_after_branch_cache_refresh()
    {
        Repository repository = new(@"C:\repos\recent");
        RepositoryHistorySnapshot initial =
            new RepositoryHistorySnapshot(
                [new RepositoryHistoryEntry(repository, "recent", BranchName: null, IsFavourite: false, IsAnchored: false)],
                []);
        RepositoryHistorySnapshot refreshed = new(
            [new RepositoryHistoryEntry(repository, "recent", "main", IsFavourite: false, IsAnchored: false)],
            []);
        IUserRepositoriesListController controller = CreateController(initial);
        IRepositoryHistoryUIService history = CreateHistory(initial);
        UserRepositoriesList list = new();
        list.Initialize(controller, history, () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);
        Window window = new() { Content = list };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            ConfigureController(controller, refreshed);

            history.HistoryChanged += Raise.Event<EventHandler>();
            Dispatcher.UIThread.RunJobs();

            list.GetTestAccessor().List.Items
                .OfType<UserRepositoriesList.RepositoryListItem>()
                .Select(item => item.BranchName)
                .Should().Contain("main");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Dashboard_should_preserve_the_original_translation_strings()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        Dashboard dashboard = new();

        dashboard.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(Dashboard), "_createRepository", "Text", "Create new repository");
        translation.Received(1).AddTranslationItem(
            nameof(Dashboard), "_openRepository", "Text", "Open repository");
        translation.Received(1).AddTranslationItem(
            nameof(Dashboard), "_donate", "Text", "Donate");
        translation.Received(1).AddTranslationItem(
            nameof(Dashboard), "_issues", "Text", "Issues");

        ITranslation repositoryTranslation = Substitute.For<ITranslation>();
        dashboard.GetTestAccessor().Repositories.AddTranslationItems(repositoryTranslation);
        repositoryTranslation.Received(1).AddTranslationItem(
            nameof(UserRepositoriesList),
            "mnuConfigure",
            "Text",
            "Recent repositories &settings");
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Dashboard_repository_settings_button_should_raise_the_host_request()
    {
        Dashboard dashboard = new();
        RepositoryHistorySnapshot snapshot = new([], []);
        dashboard.Initialize(CreateController(snapshot), CreateHistory(snapshot));
        bool requested = false;
        dashboard.ConfigureRepositoriesRequested += (_, _) => requested = true;

        dashboard.GetTestAccessor().Repositories.GetTestAccessor().Configure.RaiseEvent(
            new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));

        requested.Should().BeTrue();
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Repository_category_menu_should_assign_an_existing_category_through_the_original_controller()
    {
        Repository selected = new(@"C:\repos\selected");
        Repository categorized = new(@"C:\repos\categorized") { Category = "Team" };
        RepositoryHistorySnapshot snapshot = new(
            [new RepositoryHistoryEntry(selected, "selected", "main", IsFavourite: false, IsAnchored: false)],
            [new RepositoryHistoryEntry(categorized, "categorized", "feature", IsFavourite: true, IsAnchored: false)]);
        IUserRepositoriesListController controller = CreateController(snapshot);
        UserRepositoriesList list = new();
        list.Initialize(controller, CreateHistory(snapshot), () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);
        UserRepositoriesList.TestAccessor accessor = list.GetTestAccessor();
        accessor.List.SelectedItem = accessor.List.Items.OfType<UserRepositoriesList.RepositoryListItem>().First();

        accessor.UpdateContextMenu().Should().BeTrue();
        accessor.OpenCategories();
        accessor.CategoryAdd.IsEnabled.Should().BeTrue();
        MenuItem team = accessor.Categories.Items.OfType<MenuItem>().Single(item => Equals(item.Tag, "Team"));
        team.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));

        controller.Received(1).AssignCategoryAsync(selected, "Team");
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Category_title_should_disable_unchanged_names_and_accept_a_new_name()
    {
        FormDashboardCategoryTitle form = new(["Team", "Personal"], "Team");
        FormDashboardCategoryTitle.TestAccessor accessor = form.GetTestAccessor();

        accessor.Ok.IsEnabled.Should().BeFalse();
        accessor.CategoryName.Text = "Release";
        Dispatcher.UIThread.RunJobs();
        accessor.Ok.IsEnabled.Should().BeTrue();
        accessor.Ok.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));

        form.Category.Should().Be("Release");
        form.DialogResult.Should().Be(GitExtensions.Shims.WinForms.DialogResult.OK);
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Repository_drop_should_accept_exactly_one_existing_directory()
    {
        string directory = TestContext.CurrentContext.WorkDirectory;

        UserRepositoriesList.CanDropRepositoryDirectory([directory]).Should().BeTrue();
        UserRepositoriesList.CanDropRepositoryDirectory([]).Should().BeFalse();
        UserRepositoriesList.CanDropRepositoryDirectory([directory, directory]).Should().BeFalse();
        UserRepositoriesList.CanDropRepositoryDirectory([Path.Join(directory, "missing")]).Should().BeFalse();
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Dashboard_theme_should_preserve_the_original_light_palette()
    {
        DashboardTheme.Light.LogoBackColor.Should().Be(Avalonia.Media.Color.FromRgb(19, 122, 212));
        DashboardTheme.Light.StartBackColor.Should().Be(Avalonia.Media.Color.FromRgb(219, 235, 248));
        DashboardTheme.Light.ContributeBackColor.Should().Be(Avalonia.Media.Color.FromRgb(230, 241, 250));
        DashboardTheme.Light.SearchBackColor.Should().Be(Avalonia.Media.Color.FromRgb(248, 248, 255));
    }

    [Test]
    [Category("P4.5")]
    public async Task User_repositories_controller_should_assign_remove_and_clear()
    {
        Repository beta = new(@"C:\repos\beta") { Category = "Team" };
        ILocalRepositoryManager manager = Substitute.For<ILocalRepositoryManager>();
        manager.AssignCategoryAsync(beta, "Release").Returns(Task.FromResult<IList<Repository>>([beta]));
        IInvalidRepositoryRemover remover = Substitute.For<IInvalidRepositoryRemover>();
        remover.ShowDeleteInvalidRepositoryDialog(beta.Path).Returns(true);
        IRepositoryCurrentBranchNameCache branchCache = Substitute.For<IRepositoryCurrentBranchNameCache>();
        UserRepositoriesListController controller = new(manager, remover, branchCache);

        await controller.AssignCategoryAsync(beta, "Release");
        controller.RemoveInvalidRepository(beta.Path).Should().BeTrue();
        controller.ClearCache();

        await manager.Received(1).AssignCategoryAsync(beta, "Release");
        remover.Received(1).ShowDeleteInvalidRepositoryDialog(beta.Path);
        branchCache.Received(1).InvalidateAll();
    }

    private static IRepositoryHistoryUIService CreateHistory(RepositoryHistorySnapshot snapshot)
    {
        IRepositoryHistoryUIService history = Substitute.For<IRepositoryHistoryUIService>();
        history.LoadSnapshot().Returns(snapshot);
        return history;
    }

    private static IUserRepositoriesListController CreateController(RepositoryHistorySnapshot snapshot)
    {
        IUserRepositoriesListController controller = Substitute.For<IUserRepositoriesListController>();
        ConfigureController(controller, snapshot);
        controller.IsValidGitWorkingDir(Arg.Any<string>()).Returns(true);
        return controller;
    }

    private static void ConfigureController(IUserRepositoriesListController controller, RepositoryHistorySnapshot snapshot)
    {
        controller.PreRenderRepositories(Arg.Any<string>()).Returns(call =>
        {
            string filter = call.ArgAt<string>(0);
            IReadOnlyList<RepositoryHistoryEntry> recent = Filter(snapshot.Recent, filter);
            IReadOnlyList<RepositoryHistoryEntry> favourites = Filter(snapshot.Favourites, filter);
            foreach (RepositoryHistoryEntry entry in recent.Concat(favourites))
            {
                controller.GetCurrentBranchName(entry.Repository.Path).Returns(entry.BranchName ?? string.Empty);
            }

            return (CreateRecent(recent), CreateRecent(favourites));
        });

        static IReadOnlyList<RepositoryHistoryEntry> Filter(IReadOnlyList<RepositoryHistoryEntry> entries, string filter)
            => [.. entries.Where(entry => string.IsNullOrWhiteSpace(filter)
                || entry.Caption.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
                || entry.Repository.Path.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
                || (entry.BranchName?.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ?? false))];

        static IReadOnlyList<RecentRepoInfo> CreateRecent(IReadOnlyList<RepositoryHistoryEntry> entries)
            => [.. entries.Select(entry => new RecentRepoInfo(entry.Repository, topRepo: false, entry.IsAnchored)
            {
                Caption = entry.Caption,
            })];
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !Directory.Exists(Path.Join(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("The test checkout root was not found.");
    }
}
