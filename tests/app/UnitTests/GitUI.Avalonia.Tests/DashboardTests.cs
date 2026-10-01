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
using WinFormsControls = GitUI.Compat.WinFormsControls;

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
    [Category("P8.6i.126")]
    [TestCase("main")]
    [TestCase("")]
    public void Categorized_repository_should_use_the_source_tile_geometry_and_star_overlay(string branchName)
    {
        Repository favourite = new(@"C:\repos\favourite") { Category = "Team" };
        RepositoryHistorySnapshot snapshot = new(
            [],
            [new RepositoryHistoryEntry(favourite, "favourite", branchName, IsFavourite: true, IsAnchored: false)]);
        UserRepositoriesList list = new();
        list.Initialize(CreateController(snapshot), CreateHistory(snapshot), () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);
        Window window = new() { Content = list };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            ListBoxItem container = list.GetVisualDescendants()
                .OfType<ListBoxItem>()
                .Single(item => item.Content is UserRepositoriesList.RepositoryListItem);
            Grid row = container.GetVisualDescendants().OfType<Grid>().Single(grid => grid.MinHeight == 50);
            Image[] images = [.. row.Children.OfType<Image>()];

            images.Should().HaveCount(2);
            images.Should().OnlyContain(image => image.Width == 16 && image.Height == 16);
            images.Should().ContainSingle(image => ReferenceEquals(image.Source, GitUI.Properties.Images.Star));
            images[0].Source.Should().BeSameAs(GitUI.Properties.Images.Star);
            row.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == "favourite").FontWeight
                .Should().Be(Avalonia.Media.FontWeight.Normal);
            // The source binder adds only path and branch subitems, not a category text row.
            row.GetVisualDescendants().OfType<TextBlock>().Should().HaveCount(2);
            row.GetVisualDescendants().OfType<TextBlock>().Should().NotContain(text => text.Text == "Team");
            row.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == branchName)
                .IsVisible.Should().Be(!string.IsNullOrWhiteSpace(branchName));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Appearance_changes_should_repaint_existing_tiles_without_reloading_or_clearing_selection()
    {
        Repository repository = new(@"C:\repos\recent");
        RepositoryHistorySnapshot snapshot = new(
            [new RepositoryHistoryEntry(repository, "recent", "main", IsFavourite: false, IsAnchored: false)], []);
        IUserRepositoriesListController controller = CreateController(snapshot);
        UserRepositoriesList list = new();
        list.Initialize(controller, CreateHistory(snapshot), () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);
        Window window = new() { Content = list };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            UserRepositoriesList.TestAccessor accessor = list.GetTestAccessor();
            accessor.List.SelectedItem = accessor.List.Items.OfType<UserRepositoriesList.RepositoryListItem>().Single();
            object selected = accessor.List.SelectedItem;
            TextBlock caption = list.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == "recent");
            TextBlock branch = list.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == "main");
            controller.ClearReceivedCalls();

            list.ForeColor = Avalonia.Media.Colors.Red;
            list.BranchNameColor = Avalonia.Media.Colors.Green;
            list.FavouriteColor = Avalonia.Media.Colors.Gold;
            list.HoverColor = Avalonia.Media.Colors.Cyan;

            accessor.List.SelectedItem.Should().BeSameAs(selected);
            ((Avalonia.Media.SolidColorBrush)caption.Foreground!).Color.Should().Be(Avalonia.Media.Colors.Red);
            ((Avalonia.Media.SolidColorBrush)branch.Foreground!).Color.Should().Be(Avalonia.Media.Colors.Green);
            controller.DidNotReceive().PreRenderRepositories(Arg.Any<string>());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Initial_palette_assignments_should_paint_their_declared_defaults()
    {
        UserRepositoriesList list = new();
        list.MainBackColor = list.MainBackColor;
        list.SearchBackColor = list.SearchBackColor;
        list.HeaderColor = list.HeaderColor;
        TextBlock heading = list.FindControl<TextBlock>("lblRecentRepositories")!;

        ((Avalonia.Media.SolidColorBrush)list.Background!).Color.Should().Be(list.MainBackColor);
        ((Avalonia.Media.SolidColorBrush)list.GetTestAccessor().List.Background!).Color.Should().Be(list.MainBackColor);
        ((Avalonia.Media.SolidColorBrush)list.GetTestAccessor().Search.Background!).Color.Should().Be(list.SearchBackColor);
        ((Avalonia.Media.SolidColorBrush)heading.Foreground!).Color.Should().Be(list.HeaderColor);
        heading.FontFamily.Name.Should().Be(AppSettings.Font.Name);
        heading.FontSize.Should().Be(AvaloniaFontSettings.ToDeviceIndependentPixels(AppSettings.Font.Size + 5.5F));
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [NonParallelizable]
    public void Repository_tile_should_measure_its_source_width_when_no_minimum_is_configured()
    {
        int originalMinimum = AppSettings.RecentReposComboMinWidth;
        try
        {
            AppSettings.RecentReposComboMinWidth = 0;
            const string caption = "a deliberately long repository caption used for source-sized dashboard tiles";
            Repository repository = new(@"C:\repos\long");
            RepositoryHistorySnapshot snapshot = new(
                [new RepositoryHistoryEntry(repository, caption, "main", IsFavourite: false, IsAnchored: false)],
                []);
            UserRepositoriesList list = new();
            list.Initialize(CreateController(snapshot), CreateHistory(snapshot), () => Substitute.For<IGitUICommands>());
            list.ShowRecentRepositories(reloadData: false);
            Window window = new() { Width = 900, Height = 260, Content = list };

            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                Grid row = list.GetVisualDescendants()
                    .OfType<ListBoxItem>()
                    .Single(item => item.Content is UserRepositoriesList.RepositoryListItem)
                    .GetVisualDescendants()
                    .OfType<Grid>()
                    .Single(grid => grid.Children.OfType<StackPanel>().Any());
                TextBlock captionProbe = row.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == caption);
                double measuredCaption = WinFormsTextMeasurer.Measure(captionProbe, caption);

                row.MinWidth.Should().Be(Math.Ceiling(measuredCaption + 16 + 50));
                row.MinHeight.Should().BeGreaterThanOrEqualTo(50);
            }
            finally
            {
                window.Close();
            }
        }
        finally
        {
            AppSettings.RecentReposComboMinWidth = originalMinimum;
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
        repositoryTranslation.Received(1).AddTranslationItem(
            nameof(UserRepositoriesList), "clmhdrPath", "Text", "Path");
        repositoryTranslation.Received(1).AddTranslationItem(
            nameof(UserRepositoriesList), "clmhdrBranch", "Text", "Branch");
        repositoryTranslation.Received(1).AddTranslationItem(
            nameof(UserRepositoriesList), "clmhdrCategory", "Text", "Category");
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Dashboard_repository_settings_should_remain_a_source_owned_menu_item()
    {
        Dashboard dashboard = new();
        RepositoryHistorySnapshot snapshot = new([], []);
        dashboard.Initialize(CreateController(snapshot), CreateHistory(snapshot));

        MenuItem configure = dashboard.GetTestAccessor().Repositories.GetTestAccessor().Configure;

        configure.Header.Should().Be("Recent repositories _settings");
        dashboard.GetVisualDescendants().OfType<Button>()
            .Should().NotContain(button => button.Name == "mnuConfigure");
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

    [AvaloniaTest]
    [Category("P4.5")]
    public void Dashboard_layout_should_use_designer_authored_96_dpi_metrics()
    {
        Dashboard dashboard = new();
        WinFormsControls.TableLayoutPanel layout = dashboard.FindControl<WinFormsControls.TableLayoutPanel>("tableLayoutPanel1")!;
        WinFormsControls.Panel logo = dashboard.FindControl<WinFormsControls.Panel>("pnlLogo")!;

        layout.ColumnDefinitions.Should().HaveCount(4);
        layout.ColumnDefinitions[0].Width.Value.Should().BeApproximately(7.142857, 0.000001);
        layout.ColumnDefinitions[1].Width.Should().Be(new GridLength(213));
        layout.ColumnDefinitions[2].Width.Value.Should().BeApproximately(85.71428, 0.00001);
        layout.ColumnDefinitions[3].Width.Value.Should().BeApproximately(7.142857, 0.000001);
        logo.Padding.Should().Be(new Avalonia.Thickness(20, 0, 20, 14));
        dashboard.GetTestAccessor().Repositories.HeaderHeight.Should().Be(68);
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
