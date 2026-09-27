using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using NSubstitute;
using ToolStripDropDownItem = GitUI.Compat.WinFormsControls.ToolStripDropDownItem;
using ToolStripMenuItem = GitUI.Compat.WinFormsControls.ToolStripMenuItem;

namespace GitExtensionsTests;

[TestFixture]
[Category("P8.6i")]
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
        Grid header = item.Header.Should().BeOfType<Grid>().Subject;
        header.Children.OfType<TextBlock>().Select(text => text.Text)
            .Should().Equal("1_0: Project", "feature/native-menu");
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
            .Select(item => ((Grid)item.Header!).Children.OfType<TextBlock>().First().Text)
            .Should().Equal("_1: A second", "_2: A first");

        return;

        static RepositoryHistoryEntry Entry(string path, string caption, string category)
            => new(new Repository(path) { Category = category }, caption, null, IsFavourite: true, IsAnchored: false);
    }
}
