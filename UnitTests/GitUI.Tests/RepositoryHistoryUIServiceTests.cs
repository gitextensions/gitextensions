using System.Windows.Forms;
using FluentAssertions;
using GitCommands.UserRepositoryHistory;
using GitUI;
using GitUI.CommandsDialogs;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests
{
    [Apartment(ApartmentState.STA)]
    [TestFixture]
    public sealed class RepositoryHistoryUIServiceTests
    {
        private RepositoryHistoryUIService _service;
        private IRepositoryCurrentBranchNameProvider _repositoryCurrentBranchNameProvider;
        private IInvalidRepositoryRemover _invalidRepositoryRemover;

        [SetUp]
        public void Setup()
        {
            _repositoryCurrentBranchNameProvider = Substitute.For<IRepositoryCurrentBranchNameProvider>();
            _invalidRepositoryRemover = Substitute.For<IInvalidRepositoryRemover>();

            _service = new RepositoryHistoryUIService(_repositoryCurrentBranchNameProvider, _invalidRepositoryRemover);
        }

        [Test]
        public void PopulateRecentRepositoriesMenu_should_add_new_item()
        {
            ToolStripMenuItem containerMenu = new();

            const string path = "";
            const string caption = "CAPTION";
            Repository repository = new(path);

            _service.GetTestAccessor().AddRecentRepositories(containerMenu, repository, caption);

            containerMenu.DropDownItems.Count.Should().Be(1);
        }

        [Test]
        public void AddRecentRepositories_should_set_properties_correctly()
        {
            ToolStripMenuItem containerMenu = new();

            const string path = "";
            const string caption = "CAPTION";
            Repository repository = new(path);

            _service.GetTestAccessor().AddRecentRepositories(containerMenu, repository, caption);

            ToolStripMenuItem item = (ToolStripMenuItem)containerMenu.DropDownItems[0];
            item.Text.Should().Be(caption);
            item.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.ImageAndText);
            item.ToolTipText.Should().BeEmpty();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("master")]
        [TestCase("(no branch)")]
        public void AddRecentRepositories_should_show_branch_correctly(string branch)
        {
            _repositoryCurrentBranchNameProvider.GetCurrentBranchName(Arg.Any<string>()).Returns(x => branch);

            ToolStripMenuItem containerMenu = new();

            const string path = "somepath";
            const string caption = "CAPTION";
            Repository repository = new(path);

            _service.GetTestAccessor().AddRecentRepositories(containerMenu, repository, caption);

            // await adding branch name in ShortcutKeyDisplayString, done async
            ThreadHelper.JoinableTaskContext.Factory.Run(() => ThreadHelper.JoinPendingOperationsAsync(default));

            ToolStripMenuItem item = (ToolStripMenuItem)containerMenu.DropDownItems[0];
            item.ShortcutKeyDisplayString.Should().Be(branch);
        }

        [Test]
        public void ChangeWorkingDir_should_promt_user_to_delete_invalid_repo()
        {
            ToolStripMenuItem containerMenu = new();

            const string path = "";
            const string caption = "CAPTION";
            Repository repository = new(path);

            _service.GetTestAccessor().AddRecentRepositories(containerMenu, repository, caption);

            ToolStripMenuItem item = (ToolStripMenuItem)containerMenu.DropDownItems[0];
            item.PerformClick();

            _invalidRepositoryRemover.Received(1).ShowDeleteInvalidRepositoryDialog(path);
        }

        [Test]
        public void PopulateFavouriteRepositoriesMenu_should_order_favourites_alphabetically()
        {
            ToolStripMenuItem tsmiFavouriteRepositories = new();
            List<Repository> repositoryHistory = new()
            {
                new Repository(@"c:\") { Category = "D" },
                new Repository(@"c:\") { Category = "A" },
                new Repository(@"c:\") { Category = "C" },
                new Repository(@"c:\") { Category = "B" }
            };

            using Form form = new();
            form.Show();

            _service.GetTestAccessor().PopulateFavouriteRepositoriesMenu(tsmiFavouriteRepositories, repositoryHistory);

            // assert
            List<string> categories = tsmiFavouriteRepositories.DropDownItems.Cast<ToolStripMenuItem>().Select(x => x.Text).ToList();
            categories.Should().BeInAscendingOrder();
        }
    }
}
