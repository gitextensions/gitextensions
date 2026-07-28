using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
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
    public void Repository_list_should_show_shared_recent_favourite_and_branch_data()
    {
        Repository recent = new(@"C:\repos\recent");
        Repository favourite = new(@"C:\repos\favourite") { Category = "Team" };
        IRepositoryHistoryUIService history = CreateHistory(
            new RepositoryHistorySnapshot(
                [new RepositoryHistoryEntry(recent, "recent", "main", IsFavourite: false, IsAnchored: false)],
                [new RepositoryHistoryEntry(favourite, "favourite", "feature", IsFavourite: true, IsAnchored: false)]));
        UserRepositoriesList list = new();

        list.Initialize(history, () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);

        UserRepositoriesList.TestAccessor accessor = list.GetTestAccessor();
        UserRepositoriesList.RepositoryListItem[] rows =
        [
            .. accessor.List.Items.OfType<UserRepositoriesList.RepositoryListItem>(),
        ];
        rows.Where(row => row.Repository is null).Select(row => row.Text)
            .Should().Contain("Recent repositories", "Team");
        rows.Where(row => row.Repository is not null).Select(row => row.Repository!.BranchName)
            .Should().Contain("main", "feature");

        accessor.Search.Text = "feature";

        accessor.List.Items.OfType<UserRepositoriesList.RepositoryListItem>()
            .Where(row => row.Repository is not null)
            .Should().ContainSingle(row => row.Repository!.Repository.Path == favourite.Path);
    }

    [AvaloniaTest]
    public void Selecting_a_valid_dashboard_repository_should_raise_the_module_transition()
    {
        string repositoryPath = FindRepositoryRoot();
        Repository repository = new(repositoryPath);
        IRepositoryHistoryUIService history = CreateHistory(
            new RepositoryHistorySnapshot(
                [new RepositoryHistoryEntry(repository, "selected", "main", IsFavourite: false, IsAnchored: false)],
                []));
        history.CanOpenRepository(repositoryPath).Returns(true);
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
        list.Initialize(history, () => commands);
        list.ShowRecentRepositories(reloadData: false);
        UserRepositoriesList.TestAccessor accessor = list.GetTestAccessor();
        accessor.List.SelectedItem = accessor.List.Items
            .OfType<UserRepositoriesList.RepositoryListItem>()
            .Single(item => item.Repository is not null);

        accessor.OpenSelected();

        transition.Should().NotBeNull();
        Path.TrimEndingDirectorySeparator(transition!.GitModule.WorkingDir)
            .Should().Be(Path.TrimEndingDirectorySeparator(repositoryPath));
    }

    [AvaloniaTest]
    public void Single_clicking_a_valid_dashboard_repository_should_raise_the_module_transition()
    {
        string repositoryPath = FindRepositoryRoot();
        Repository repository = new(repositoryPath);
        IRepositoryHistoryUIService history = CreateHistory(
            new RepositoryHistorySnapshot(
                [new RepositoryHistoryEntry(repository, "selected", "main", IsFavourite: false, IsAnchored: false)],
                []));
        history.CanOpenRepository(repositoryPath).Returns(true);
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
        list.Initialize(history, () => commands);
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
                .Single(item => item.Content is UserRepositoriesList.RepositoryListItem { Repository: not null });
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
    public void Attached_repository_list_should_replace_rows_after_branch_cache_refresh()
    {
        Repository repository = new(@"C:\repos\recent");
        IRepositoryHistoryUIService history = CreateHistory(
            new RepositoryHistorySnapshot(
                [new RepositoryHistoryEntry(repository, "recent", BranchName: null, IsFavourite: false, IsAnchored: false)],
                []));
        UserRepositoriesList list = new();
        list.Initialize(history, () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);
        Window window = new() { Content = list };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            history.LoadSnapshot().Returns(new RepositoryHistorySnapshot(
                [new RepositoryHistoryEntry(repository, "recent", "main", IsFavourite: false, IsAnchored: false)],
                []));

            history.HistoryChanged += Raise.Event<EventHandler>();
            Dispatcher.UIThread.RunJobs();

            list.GetTestAccessor().List.Items
                .OfType<UserRepositoriesList.RepositoryListItem>()
                .Where(item => item.Repository is not null)
                .Select(item => item.Repository!.BranchName)
                .Should().Contain("main");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
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
    public void Dashboard_repository_settings_button_should_raise_the_host_request()
    {
        Dashboard dashboard = new();
        dashboard.Initialize(CreateHistory(new RepositoryHistorySnapshot([], [])));
        bool requested = false;
        dashboard.ConfigureRepositoriesRequested += (_, _) => requested = true;

        dashboard.GetTestAccessor().Repositories.GetTestAccessor().Configure.RaiseEvent(
            new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));

        requested.Should().BeTrue();
    }

    private static IRepositoryHistoryUIService CreateHistory(RepositoryHistorySnapshot snapshot)
    {
        IRepositoryHistoryUIService history = Substitute.For<IRepositoryHistoryUIService>();
        history.LoadSnapshot().Returns(snapshot);
        return history;
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
