using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using NSubstitute;

namespace GitUITests;

[Apartment(ApartmentState.STA)]
public sealed class RepositoryHistoryUIServiceTests
{
    private RepositoryHistoryUIService _service = null!;
    private IRepositoryCurrentBranchNameCache _branchNameCache = null!;
    private IInvalidRepositoryRemover _invalidRepositoryRemover = null!;

    [SetUp]
    public void Setup()
    {
        _branchNameCache = Substitute.For<IRepositoryCurrentBranchNameCache>();
        _invalidRepositoryRemover = Substitute.For<IInvalidRepositoryRemover>();

        _service = new RepositoryHistoryUIService(Substitute.For<IGitExecutorProvider>(), _branchNameCache, _invalidRepositoryRemover);
    }

    [Test]
    public void PopulateRecentRepositoriesMenu_should_add_new_item()
    {
        using ToolStripMenuItem containerMenu = new();

        const string path = "";
        const string caption = "CAPTION";
        Repository repository = new(path);

        _service.GetTestAccessor().AddRecentRepositories(containerMenu, repository, caption, number: 1);

        containerMenu.DropDownItems.Count.Should().Be(1);
    }

    [Test]
    public void AddRecentRepositories_should_set_properties_correctly()
    {
        using ToolStripMenuItem containerMenu = new();

        const string path = "";
        const string caption = "CAPTION";
        Repository repository = new(path);

        _service.GetTestAccessor().AddRecentRepositories(containerMenu, repository, caption, number: 1);

        ToolStripMenuItem item = (ToolStripMenuItem)containerMenu.DropDownItems[0];
        item.Text.Should().Be($"&1: {caption}");
        item.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.ImageAndText);
        item.ToolTipText.Should().BeEmpty();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("master")]
    [TestCase("(no branch)")]
    public void AddRecentRepositories_should_show_branch_correctly(string? branch)
    {
        _branchNameCache.GetCachedBranchName(Arg.Any<string>()).Returns(string.IsNullOrWhiteSpace(branch) ? null : branch);

        using ToolStripMenuItem containerMenu = new();

        const string path = "somepath";
        const string caption = "CAPTION";
        Repository repository = new(path);

        _service.GetTestAccessor().AddRecentRepositories(containerMenu, repository, caption, number: 1);

        ToolStripMenuItem item = (ToolStripMenuItem)containerMenu.DropDownItems[0];
        if (string.IsNullOrWhiteSpace(branch))
        {
            item.ShortcutKeyDisplayString.Should().BeNullOrEmpty();
        }
        else
        {
            item.ShortcutKeyDisplayString.Should().Be(branch);
        }
    }

    [Test]
    public void ChangeWorkingDir_should_promt_user_to_delete_invalid_repo()
    {
        using ToolStripMenuItem containerMenu = new();

        const string path = "";
        const string caption = "CAPTION";
        Repository repository = new(path);

        _service.GetTestAccessor().AddRecentRepositories(containerMenu, repository, caption, number: 1);

        ToolStripMenuItem item = (ToolStripMenuItem)containerMenu.DropDownItems[0];
        item.PerformClick();

        _invalidRepositoryRemover.Received(1).ShowDeleteInvalidRepositoryDialog(path);
    }

    [Test]
    public void PopulateFavouriteRepositoriesMenu_should_order_favourites_alphabetically()
    {
        using ToolStripMenuItem tsmiFavouriteRepositories = new();
        List<Repository> repositoryHistory =
        [
            new Repository(@"c:\") { Category = "D" },
            new Repository(@"c:\") { Category = "A" },
            new Repository(@"c:\") { Category = "C" },
            new Repository(@"c:\") { Category = "B" }
        ];

        using Form form = new();
        form.Show();

        _service.GetTestAccessor().PopulateFavouriteRepositoriesMenu(tsmiFavouriteRepositories, repositoryHistory);

        // assert
        List<string> categories = [.. tsmiFavouriteRepositories.DropDownItems.Cast<ToolStripMenuItem>().Select(x => x.Text!)];
        categories.Should().BeInAscendingOrder();
    }
}
