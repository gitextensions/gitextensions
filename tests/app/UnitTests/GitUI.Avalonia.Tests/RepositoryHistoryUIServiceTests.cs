using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
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
    public void PopulateFavouriteRepositoriesMenu_should_order_categories_and_number_each_category()
    {
        ToolStripDropDownItem container = new();
        RepositoryHistoryEntry[] repositories =
        [
            Entry("/repos/d", "D repo", "D"),
            Entry("/repos/a2", "A second", "A"),
            Entry("/repos/c", "C repo", "C"),
            Entry("/repos/a1", "A first", "A"),
        ];

        _service.GetTestAccessor().PopulateFavouriteRepositoriesMenu(container, repositories);

        ToolStripMenuItem[] categories = [.. container.Items.OfType<ToolStripMenuItem>()];
        categories.Select(item => item.Header).Should().Equal("A", "C", "D");
        categories[0].Items.OfType<ToolStripMenuItem>()
            .Select(item => item.Header)
            .Should().Equal("_1: A second", "_2: A first");

        return;

        static RepositoryHistoryEntry Entry(string path, string caption, string category)
            => new(new Repository(path) { Category = category }, caption, null, IsFavourite: true, IsAnchored: false);
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
    public void AddRecentRepositories_should_show_a_pin_only_for_an_anchored_recent_entry(bool anchored)
    {
        ToolStripDropDownItem container = new();
        _service.GetTestAccessor().AddRecentRepositories(
            container, new Repository("/repos/pin"), "Pin", number: 1, anchored: anchored);

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
    public void CreateSnapshot_should_retain_the_source_top_count_and_recent_Concat_but_favourite_Union(bool hideTop)
    {
        using SplitSettingsScope settings = new();
        AppSettings.HideTopRepositoriesFromRecentList.Value = hideTop;
        Repository top = new(Path.Join(Path.GetTempPath(), "source-top"));
        Repository anchoredRecent = new(Path.Join(Path.GetTempPath(), "source-pinned-recent"))
        {
            Anchor = Repository.RepositoryAnchor.AnchoredInRecent,
        };
        Repository tail = new(Path.Join(Path.GetTempPath(), "source-tail"));
        Repository[] repositories = [top, anchoredRecent, tail];

        RepositoryHistorySnapshot snapshot = _service.GetTestAccessor().CreateSnapshot(repositories, repositories);
        Repository[] expectedRecent = hideTop ? [top, anchoredRecent, tail] : [top, top, anchoredRecent, tail];

        snapshot.TopCount.Should().Be(1);
        snapshot.Recent.Select(entry => entry.Repository).Should().Equal(expectedRecent);
        snapshot.Favourites.Select(entry => entry.Repository).Should().Equal(top, anchoredRecent, tail);
        snapshot.Recent[0].IsAnchored.Should().BeFalse(
            "the source top group is not synonymous with pinned repositories");
        snapshot.Recent.Should().Contain(entry => ReferenceEquals(entry.Repository, anchoredRecent) && entry.IsAnchored);
        snapshot.Recent.Should().OnlyContain(entry => !entry.IsFavourite);
        snapshot.Favourites.Should().OnlyContain(entry => entry.IsFavourite);
    }

    [AvaloniaTest]
    public void CreateSnapshot_should_Union_source_info_identity_before_projecting_equal_entry_values()
    {
        using SplitSettingsScope settings = new();
        AppSettings.HideTopRepositoriesFromRecentList.Value = false;
        string root = Path.GetPathRoot(Path.GetFullPath(Path.GetTempPath()))
            ?? throw new InvalidOperationException("The portable fixture path has no filesystem root.");
        Repository repository = new(root) { Category = "Team" };

        RepositoryHistorySnapshot snapshot = _service.GetTestAccessor()
            .CreateSnapshot([repository, repository], [repository, repository]);

        snapshot.TopCount.Should().Be(1);
        snapshot.Recent.Should().HaveCount(3);
        snapshot.Favourites.Should().HaveCount(2,
            "Union removes only a shared RecentRepoInfo across top/recent, not separate source info instances with equal values");
        snapshot.Favourites.Should().OnlyContain(entry => ReferenceEquals(entry.Repository, repository));
        snapshot.Favourites[0].Should().Be(snapshot.Favourites[1],
            "equal projected records must not retroactively alter source reference-based Union");
    }

    [AvaloniaTest]
    [TestCase(0, 0)]
    [TestCase(0, 3)]
    [TestCase(1, 3)]
    [TestCase(3, 3)]
    public void PopulateRecentRepositoriesMenu_should_insert_one_separator_only_at_the_exact_source_group_boundary(
        int topCount, int count)
    {
        RepositoryHistoryEntry[] entries = [.. Enumerable.Range(0, count).Select(index => new RepositoryHistoryEntry(
            new Repository($"/repos/group-{index}"), $"Group {index}", null,
            IsFavourite: false, IsAnchored: index % 2 != 0))];
        RepositoryHistorySnapshot snapshot = new(entries, []) { TopCount = topCount };
        ToolStripDropDownItem container = new();

        _service.GetTestAccessor().PopulateRecentRepositoriesMenu(container, snapshot);

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
    public void RepositoryHistorySnapshot_should_preserve_two_argument_construction_and_deconstruction_with_no_inferred_group()
    {
        RepositoryHistoryEntry anchored = new(new Repository("/repos/legacy-pinned"), "Pinned", null,
            IsFavourite: false, IsAnchored: true);
        RepositoryHistoryEntry[] entries = [anchored];
        RepositoryHistorySnapshot snapshot = new(entries, []);
        (IReadOnlyList<RepositoryHistoryEntry> recent, IReadOnlyList<RepositoryHistoryEntry> favourites) = snapshot;
        ToolStripDropDownItem container = new();

        _service.GetTestAccessor().PopulateRecentRepositoriesMenu(container, snapshot);

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
        ToolStripDropDownItem container = new();
        _service.GetTestAccessor().AddRecentRepositories(container, new Repository("/repos/key-route"), "Route", number: 1);
        ToolStripMenuItem item = container.Items.OfType<ToolStripMenuItem>().Single();

        item.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.A,
            KeyModifiers = modifiers,
        });

        _service.GetTestAccessor().GetOpenInNewInstance(item).Should().Be(expected,
            "source Control.ModifierKeys == Keys.Control excludes every additional modifier");
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
