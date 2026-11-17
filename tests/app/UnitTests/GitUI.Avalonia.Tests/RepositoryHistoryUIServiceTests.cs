using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.Menus;
using GitUI.Compat;
using NSubstitute;
using ToolStripDropDownItem = GitUI.Compat.WinFormsControls.ToolStripDropDownItem;
using ToolStripMenuItem = GitUI.Compat.WinFormsControls.ToolStripMenuItem;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i")]
[Category("P8.6i.126")]
public sealed class RepositoryHistoryUIServiceTests
{
    private IGitExecutorProvider _executorProvider = null!;
    private IRepositoryCurrentBranchNameCache _branchNameCache = null!;
    private IInvalidRepositoryRemover _invalidRepositoryRemover = null!;
    private RepositoryHistoryUIService _service = null!;

    [SetUp]
    public void Setup()
    {
        _executorProvider = Substitute.For<IGitExecutorProvider>();
        _branchNameCache = Substitute.For<IRepositoryCurrentBranchNameCache>();
        _invalidRepositoryRemover = Substitute.For<IInvalidRepositoryRemover>();
        _service = new RepositoryHistoryUIService(_executorProvider, _branchNameCache, _invalidRepositoryRemover);
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task PopulateRecentRepositoriesMenu_should_append_without_removing_existing_items(bool hasHistory)
    {
        using AppSettingsTestScope settings = new();
        using SplitSettingsScope splitSettings = new();
        string path = Path.Join(Path.GetTempPath(), "repository-menu-append");
        await RepositoryHistoryManager.Locals.SaveRecentHistoryAsync(hasHistory ? [new Repository(path)] : []);
        ToolStripMenuItem existing = new() { Header = "Existing" };
        ToolStripDropDownItem container = new();
        container.Items.Add(existing);

        _service.PopulateRecentRepositoriesMenu(container);

        container.Items.Should().HaveCount(hasHistory ? 2 : 1);
        container.Items[0].Should().BeSameAs(existing);
        if (hasHistory)
        {
            container.Items.OfType<ToolStripMenuItem>().Last().Tag
                .Should().BeOfType<Repository>().Which.Path.Should().Be(path);
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task Start_menu_should_rebuild_recent_items_before_each_population(bool hasHistory)
    {
        using AppSettingsTestScope settings = new();
        using SplitSettingsScope splitSettings = new();
        string path = Path.Join(Path.GetTempPath(), "repository-menu-repeat");
        await RepositoryHistoryManager.Locals.SaveRecentHistoryAsync(hasHistory ? [new Repository(path)] : []);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.GetService(typeof(IRepositoryHistoryUIService)).Returns(_service);
        using StartToolStripMenuItem menu = new();
        menu.Initialize(() => commands);
        MenuItem recent = menu.GetTestAccessor().RecentRepositoriesMenuItem;
        MenuItem clear = menu.FindControl<MenuItem>("tsmiRecentRepositoriesClear")!;

        for (int opening = 0; opening < 2; opening++)
        {
            recent.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));

            recent.Items.Should().HaveCount(hasHistory ? 3 : 0);
            if (hasHistory)
            {
                recent.Items.OfType<ToolStripMenuItem>().Should().ContainSingle()
                    .Which.Tag.Should().BeOfType<Repository>().Which.Path.Should().Be(path);
                recent.Items.OfType<Separator>().Should().ContainSingle();
                recent.Items.Last().Should().BeSameAs(clear);
            }
        }
    }

    [AvaloniaTest]
    public async Task Start_menu_should_read_current_history_on_each_opening()
    {
        using AppSettingsTestScope settings = new();
        using SplitSettingsScope splitSettings = new();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.GetService(typeof(IRepositoryHistoryUIService)).Returns(_service);
        using StartToolStripMenuItem menu = new();
        menu.Initialize(() => commands);
        MenuItem recent = menu.GetTestAccessor().RecentRepositoriesMenuItem;

        foreach (string name in new[] { "first", "second" })
        {
            string path = Path.Join(Path.GetTempPath(), "repository-menu-" + name);
            await RepositoryHistoryManager.Locals.SaveRecentHistoryAsync([new Repository(path)]);

            recent.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));

            recent.Items.OfType<ToolStripMenuItem>().Should().ContainSingle()
                .Which.Tag.Should().BeOfType<Repository>().Which.Path.Should().Be(path);
        }
    }

    [AvaloniaTest]
    public void AddRecentRepositories_should_preserve_number_caption_branch_pin_and_tooltip()
    {
        const string path = "/repos/project";
        const string caption = "Project";
        _branchNameCache.GetCachedBranchName(path).Returns("feature/native-menu");
        ToolStripDropDownItem container = new();

        _service.GetTestAccessor().AddRecentRepositories(container, new Repository(path), caption, number: 10);

        ToolStripMenuItem item = container.Items.OfType<ToolStripMenuItem>().Single();
        item.Header.Should().Be("1_0: Project");
        WinFormsToolStripMenuSizer.GetShortcutDisplayString(item).Should().Be("feature/native-menu");
        ToolTip.GetTip(item).Should().Be(path);
    }

    [AvaloniaTest]
    public void Repository_item_should_change_to_a_valid_working_directory()
    {
        string path = Path.Join(Path.GetTempPath(), $"GitExtensions.RepositoryHistory-{Guid.NewGuid():N}");
        string gitDirectory = Path.Join(path, ".git");
        Directory.CreateDirectory(gitDirectory);
        IGitExecutor executor = Substitute.For<IGitExecutor>();
        executor.WorkingDir.Returns(path);
        executor.GetGitDirectory().Returns(gitDirectory);
        _executorProvider.GetExecutor(path).Returns(executor);
        ToolStripDropDownItem container = new();
        GitModuleEventArgs? transition = null;
        _service.GitModuleChanged += (_, e) => transition = e;
        _service.GetTestAccessor().AddRecentRepositories(container, new Repository(path), "Valid", number: 1);

        try
        {
            container.Items.OfType<ToolStripMenuItem>().Single()
                .RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            transition.Should().NotBeNull();
            transition!.GitModule.WorkingDir.Should().Be(path);
            _invalidRepositoryRemover.DidNotReceiveWithAnyArgs().ShowDeleteInvalidRepositoryDialog(default!);
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
    }

    [AvaloniaTest]
    public void Repository_item_should_offer_to_remove_an_invalid_working_directory()
    {
        const string path = "/repos/missing";
        ToolStripDropDownItem container = new();
        _service.GetTestAccessor().AddRecentRepositories(container, new Repository(path), "Missing", number: 1);

        container.Items.OfType<ToolStripMenuItem>().Single()
            .RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        _invalidRepositoryRemover.Received(1).ShowDeleteInvalidRepositoryDialog(path);
    }

    [AvaloniaTest]
    public async Task PopulateFavouriteRepositoriesMenu_should_order_categories_and_number_each_category()
    {
        using AppSettingsTestScope settings = new();
        using SplitSettingsScope splitSettings = new();
        ToolStripDropDownItem container = new();
        Repository[] repositories =
        [
            new("D repo") { Category = "D" },
            new("A second") { Category = "A" },
            new("C repo") { Category = "C" },
            new("A first") { Category = "A" },
        ];
        await RepositoryHistoryManager.Locals.SaveFavouriteHistoryAsync(repositories);

        _service.PopulateFavouriteRepositoriesMenu(container);

        ToolStripMenuItem[] categories = [.. container.Items.OfType<ToolStripMenuItem>()];
        categories.Select(item => item.Header).Should().Equal("A", "C", "D");
        categories[0].Items.OfType<ToolStripMenuItem>()
            .Select(item => item.Header)
            .Should().Equal("_1: A second", "_2: A first");
    }

    [AvaloniaTest]
    [TestCase(1, "work_tree&&name", "_1: work__tree&name")]
    [TestCase(9, "A&B", "_9: A_B")]
    [TestCase(10, "Project", "1_0: Project")]
    [TestCase(11, "Project", "11: Project")]
    public void AddRecentRepositories_should_convert_the_complete_source_mnemonic_caption(
        int number, string caption, string expectedHeader)
    {
        ToolStripDropDownItem container = new();
        _service.GetTestAccessor().AddRecentRepositories(container, new Repository("/repos/project"), caption, number);

        ToolStripMenuItem item = container.Items.OfType<ToolStripMenuItem>().Single();
        item.Header.Should().BeOfType<string>().Which.Should().Be(expectedHeader);
        item.Tag.Should().BeOfType<Repository>();
    }

    [AvaloniaTest]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("main")]
    [TestCase("(no branch)")]
    public void AddRecentRepositories_should_keep_the_cached_branch_in_the_source_shortcut_column(string? branch)
    {
        const string path = "/repos/branch-caption";
        _branchNameCache.GetCachedBranchName(path).Returns(branch);
        ToolStripDropDownItem container = new();
        _service.GetTestAccessor().AddRecentRepositories(container, new Repository(path), "Caption", number: 1);

        ToolStripMenuItem item = container.Items.OfType<ToolStripMenuItem>().Single();
        item.Header.Should().Be("_1: Caption");
        WinFormsToolStripMenuSizer.GetShortcutDisplayString(item)
            .Should().Be(string.IsNullOrEmpty(branch) ? null : branch);
        item.InputGesture.Should().BeNull("a cached branch is display text, not a keyboard command");
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task AddRecentRepositories_should_show_a_pin_only_for_an_anchored_recent_entry(bool anchored)
    {
        using AppSettingsTestScope settings = new();
        using SplitSettingsScope splitSettings = new();
        Repository repository = new(Path.Join(Path.GetTempPath(), "source-anchor"))
        {
            Anchor = anchored ? Repository.RepositoryAnchor.AnchoredInRecent : Repository.RepositoryAnchor.None,
        };
        await RepositoryHistoryManager.Locals.SaveRecentHistoryAsync([repository]);
        ToolStripDropDownItem container = new();
        _service.PopulateRecentRepositoriesMenu(container);
        ToolStripMenuItem item = container.Items.OfType<ToolStripMenuItem>().Single();
        if (anchored)
        {
            item.Icon.Should().BeOfType<Image>().Which.Source.Should().BeSameAs(GitUI.Properties.Images.Pin);
        }
        else
        {
            item.Icon.Should().BeNull();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Menu_population_should_retain_the_source_top_count_and_recent_Concat_but_favourite_Union(bool hideTop)
    {
        using AppSettingsTestScope settings = new();
        using SplitSettingsScope splitSettings = new();
        using WorkingDirectoryTestSupport.HistoryStorageScope storage = new();
        AppSettings.HideTopRepositoriesFromRecentList.Value = hideTop;
        Repository top = new(Path.Join(Path.GetTempPath(), "source-top"));
        Repository anchoredRecent = new(Path.Join(Path.GetTempPath(), "source-pinned-recent"))
        {
            Anchor = Repository.RepositoryAnchor.AnchoredInRecent,
        };
        Repository tail = new(Path.Join(Path.GetTempPath(), "source-tail"));
        Repository[] repositories = [top, anchoredRecent, tail];
        storage.Storage.Load(Arg.Any<string>()).Returns(repositories);
        ToolStripDropDownItem recent = new();
        ToolStripDropDownItem favourites = new();
        _service.PopulateRecentRepositoriesMenu(recent);
        _service.GetTestAccessor().PopulateFavouriteRepositoriesMenu(favourites, repositories);
        Repository[] expectedRecent = hideTop ? [top, anchoredRecent, tail] : [top, top, anchoredRecent, tail];
        ToolStripMenuItem[] recentItems = [.. recent.Items.OfType<ToolStripMenuItem>()];
        ToolStripMenuItem[] favouriteItems = [.. favourites.Items.OfType<ToolStripMenuItem>()
            .SelectMany(category => category.Items.OfType<ToolStripMenuItem>())];

        recent.Items.OfType<Separator>().Should().ContainSingle();
        recent.Items[1].Should().BeOfType<GitUI.Compat.WinFormsControls.ToolStripSeparator>();
        recentItems.Select(item => item.Tag).Should().Equal(expectedRecent);
        favouriteItems.Select(item => item.Tag).Should().Equal(top, anchoredRecent, tail);
        recentItems[0].Icon.Should().BeNull("the source top group is not synonymous with pinned repositories");
        recentItems.Should().Contain(item => ReferenceEquals(item.Tag, anchoredRecent) && typeof(Image).IsInstanceOfType(item.Icon));
        recentItems.Should().OnlyContain(item => item.Items.Count == 0);
        favourites.Items.OfType<ToolStripMenuItem>().Should().OnlyContain(category => ReferenceEquals(category.Tag, null) && category.Items.Count > 0);
    }

    [AvaloniaTest]
    public void Menu_population_should_Union_source_info_identity_without_deduplicating_equal_payloads()
    {
        using AppSettingsTestScope settings = new();
        using SplitSettingsScope splitSettings = new();
        using WorkingDirectoryTestSupport.HistoryStorageScope storage = new();
        AppSettings.HideTopRepositoriesFromRecentList.Value = false;
        string root = Path.GetPathRoot(Path.GetFullPath(Path.GetTempPath()))
            ?? throw new InvalidOperationException("The portable fixture path has no filesystem root.");
        Repository repository = new(root) { Category = "Team" };
        Repository[] repositories = [repository, repository];
        storage.Storage.Load(Arg.Any<string>()).Returns(repositories);
        ToolStripDropDownItem recent = new();
        ToolStripDropDownItem favourites = new();
        _service.PopulateRecentRepositoriesMenu(recent);
        _service.GetTestAccessor().PopulateFavouriteRepositoriesMenu(favourites, repositories);
        ToolStripMenuItem[] favouriteItems = [.. favourites.Items.OfType<ToolStripMenuItem>()
            .SelectMany(category => category.Items.OfType<ToolStripMenuItem>())];

        recent.Items[1].Should().BeOfType<GitUI.Compat.WinFormsControls.ToolStripSeparator>();
        recent.Items.OfType<ToolStripMenuItem>().Should().HaveCount(3);
        favouriteItems.Should().HaveCount(2,
            "Union removes only a shared RecentRepoInfo across top/recent, not separate source info instances with equal values");
        favouriteItems.Should().OnlyContain(item => ReferenceEquals(item.Tag, repository));
        favouriteItems[0].Should().NotBeSameAs(favouriteItems[1]);
        favouriteItems[0].Tag.Should().BeSameAs(favouriteItems[1].Tag,
            "equal payloads must not retroactively alter source reference-based Union");
    }

    [AvaloniaTest]
    [TestCase(0, 0)]
    [TestCase(0, 3)]
    [TestCase(1, 3)]
    [TestCase(3, 3)]
    public void PopulateRecentRepositoriesMenu_should_insert_one_separator_only_at_the_exact_source_group_boundary(
        int topCount, int count)
    {
        using AppSettingsTestScope settings = new();
        using SplitSettingsScope splitSettings = new();
        using WorkingDirectoryTestSupport.HistoryStorageScope storage = new();
        AppSettings.MaxTopRepositories = topCount;
        Repository[] repositories = [.. Enumerable.Range(0, count).Select(index => new Repository($"Group {index}"))];
        storage.Storage.Load(Arg.Any<string>()).Returns(repositories);
        ToolStripDropDownItem container = new();

        _service.PopulateRecentRepositoriesMenu(container);

        ToolStripMenuItem[] items = [.. container.Items.OfType<ToolStripMenuItem>()];
        items.Select(item => item.Header)
            .Should().Equal(Enumerable.Range(0, count).Select(index => $"_{index + 1}: Group {index}"));
        int expectedSeparators = topCount > 0 && topCount < count ? 1 : 0;
        container.Items.OfType<Separator>().Should().HaveCount(expectedSeparators);
        if (expectedSeparators > 0)
        {
            container.Items[topCount].Should().BeOfType<GitUI.Compat.WinFormsControls.ToolStripSeparator>();
        }
    }

    [AvaloniaTest]
    public void Repository_history_input_fixture_should_not_infer_a_group_from_an_anchor()
    {
        RepositoryHistoryEntry anchored = new(new Repository("/repos/legacy-pinned"), "Pinned", null,
            IsFavourite: false, IsAnchored: true);
        RepositoryHistoryEntry[] entries = [anchored];
        RepositoryHistorySnapshot snapshot = new(entries, []);
        (IReadOnlyList<RepositoryHistoryEntry> recent, IReadOnlyList<RepositoryHistoryEntry> favourites) = snapshot;
        ToolStripDropDownItem container = new();

        RepositoryHistoryTestHelper.PopulateRecentFixture(_service, container, snapshot);

        snapshot.TopCount.Should().Be(0);
        recent.Should().BeSameAs(entries);
        favourites.Should().BeEmpty();
        container.Items.OfType<Separator>().Should().BeEmpty(
            "existing snapshot constructors supplied no splitter boundary, so pin state must not invent one");
    }

    [AvaloniaTest]
    [TestCase(KeyModifiers.None, false)]
    [TestCase(KeyModifiers.Control, true)]
    [TestCase(KeyModifiers.Control | KeyModifiers.Shift, false)]
    [TestCase(KeyModifiers.Control | KeyModifiers.Alt, false)]
    [TestCase(KeyModifiers.Control | KeyModifiers.Meta, false)]
    [TestCase(KeyModifiers.Shift, false)]
    [TestCase(KeyModifiers.Alt, false)]
    [TestCase(KeyModifiers.Meta, false)]
    public void Repository_item_should_capture_new_instance_routing_only_for_the_exact_source_Control_modifier(
        KeyModifiers modifiers, bool expected)
    {
        string path = Path.Join(Path.GetTempPath(), $"GitExtensions.RepositoryModifiers-{Guid.NewGuid():N}");
        string gitDirectory = Path.Join(path, ".git");
        Directory.CreateDirectory(gitDirectory);
        IGitExecutor executor = Substitute.For<IGitExecutor>();
        executor.WorkingDir.Returns(path);
        executor.GetGitDirectory().Returns(gitDirectory);
        _executorProvider.GetExecutor(path).Returns(executor);
        ToolStripDropDownItem container = new();
        _service.GetTestAccessor().AddRecentRepositories(container, new Repository(path), "Route", number: 1);
        ToolStripMenuItem item = container.Items.OfType<ToolStripMenuItem>().Single();
        GitModuleEventArgs? transition = null;
        _service.GitModuleChanged += (_, e) => transition = e;
        string? launchedPath = null;
        Func<string, string, IProcess>? previousStart = GitExtensionsProcess.StartForTesting;
        GitExtensionsProcess.StartForTesting = (_, workingDirectory) =>
        {
            launchedPath = workingDirectory;
            return Substitute.For<IProcess>();
        };
        try
        {
            item.RaiseEvent(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Key = Key.A,
                KeyModifiers = modifiers,
            });
            item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            (launchedPath is not null).Should().Be(expected,
                "source Control.ModifierKeys == Keys.Control excludes every additional modifier");
            if (expected)
            {
                launchedPath.Should().Be(path);
                transition.Should().BeNull();
            }
            else
            {
                transition.Should().NotBeNull();
                transition!.GitModule.WorkingDir.Should().Be(path);
            }
        }
        finally
        {
            GitExtensionsProcess.StartForTesting = previousStart;
            Directory.Delete(path, recursive: true);
        }
    }

    private sealed class SplitSettingsScope : IDisposable
    {
        private readonly int _maximum = AppSettings.MaxTopRepositories;
        private readonly bool _hideTop = AppSettings.HideTopRepositoriesFromRecentList.Value;
        private readonly bool _sortTop = AppSettings.SortTopRepos;
        private readonly bool _sortRecent = AppSettings.SortRecentRepos;
        private readonly ShorteningRecentRepoPathStrategy _shortening = AppSettings.ShorteningRecentRepoPathStrategy;

        public SplitSettingsScope()
        {
            AppSettings.MaxTopRepositories = 1;
            AppSettings.HideTopRepositoriesFromRecentList.Value = true;
            AppSettings.SortTopRepos = false;
            AppSettings.SortRecentRepos = false;
            AppSettings.ShorteningRecentRepoPathStrategy = ShorteningRecentRepoPathStrategy.None;
        }

        public void Dispose()
        {
            AppSettings.MaxTopRepositories = _maximum;
            AppSettings.HideTopRepositoriesFromRecentList.Value = _hideTop;
            AppSettings.SortTopRepos = _sortTop;
            AppSettings.SortRecentRepos = _sortRecent;
            AppSettings.ShorteningRecentRepoPathStrategy = _shortening;
        }
    }
}
