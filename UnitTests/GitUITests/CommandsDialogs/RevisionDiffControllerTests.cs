using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [TestFixture]
    public class RevisionDiffControllerTests
    {
        private IGitRevisionTester _tester;
        private RevisionDiffController _controller;

        [SetUp]
        public void Setup()
        {
            _tester = Substitute.For<IGitRevisionTester>();

            _controller = new RevisionDiffController(_tester);
        }

        #region difftool menu

        [Test]
        public void BrowseDiff_DifftoolMenu_Default()
        {
            var selectionInfo = new ContextMenuSelectionInfo();
            _controller.ShouldShowDifftoolMenus(selectionInfo).Should().BeTrue();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_DifftoolMenu_Selected(bool t)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = new ContextMenuSelectionInfo(rev, isAnyItemSelected: t);
            _controller.ShouldShowDifftoolMenus(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_DifftoolMenu_Tracked(bool t)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = new ContextMenuSelectionInfo(rev, isAnyTracked: t);
            _controller.ShouldShowDifftoolMenus(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_DifftoolMenu_BareRepo(bool t)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = new ContextMenuSelectionInfo(rev, isBareRepository: t);
            _controller.ShouldShowDifftoolMenus(selectionInfo).Should().BeTrue();
        }

        #endregion

        #region reset menu

        [Test]
        public void BrowseDiff_ResetMenu_Default()
        {
            var selectionInfo = new ContextMenuSelectionInfo();
            _controller.ShouldShowResetFileMenus(selectionInfo).Should().BeTrue();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_ResetMenu_Selected(bool t)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = new ContextMenuSelectionInfo(rev, isAnyItemSelected: t);
            _controller.ShouldShowResetFileMenus(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_ResetMenu_Tracked(bool t)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = new ContextMenuSelectionInfo(rev, isAnyTracked: t);
            _controller.ShouldShowResetFileMenus(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_ResetMenu_BareRepo(bool t)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = new ContextMenuSelectionInfo(rev, isBareRepository: t);
            _controller.ShouldShowResetFileMenus(selectionInfo).Should().Be(!t);
        }

        #endregion

        #region main menu

        [Test]
        public void BrowseDiff_MainMenus_Default()
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = new ContextMenuSelectionInfo(selectedRevision: rev);
            _controller.ShouldShowMenuSaveAs(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuCherryPick(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuStage(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuUnstage(selectionInfo).Should().BeFalse();
            _controller.ShouldShowSubmoduleMenus(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuEditFile(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuCopyFileName(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuShowInFileTree(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuFileHistory(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuBlame(selectionInfo).Should().BeTrue();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_MainMenus_SingleSelected(bool t)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = new ContextMenuSelectionInfo(selectedRevision: rev, isSingleGitItemSelected: t);
            _controller.ShouldShowMenuSaveAs(selectionInfo).Should().Be(t);
            _controller.ShouldShowMenuCherryPick(selectionInfo).Should().Be(t);
            _controller.ShouldShowMenuStage(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuUnstage(selectionInfo).Should().BeFalse();
            _controller.ShouldShowSubmoduleMenus(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuEditFile(selectionInfo).Should().Be(t);
            _controller.ShouldShowMenuCopyFileName(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuShowInFileTree(selectionInfo).Should().Be(t);
            _controller.ShouldShowMenuFileHistory(selectionInfo).Should().Be(t);
            _controller.ShouldShowMenuBlame(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_StageMenus_WorkTree(bool t)
        {
            var rev = new GitRevision(ObjectId.WorkTreeId);
            var selectionInfo = new ContextMenuSelectionInfo(rev, isAnyItemIndex: t);
            _controller.ShouldShowMenuUnstage(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_StageMenus_Index(bool t)
        {
            var rev = new GitRevision(ObjectId.WorkTreeId);
            var selectionInfo = new ContextMenuSelectionInfo(rev, isAnyItemWorkTree: t);
            _controller.ShouldShowMenuStage(selectionInfo).Should().Be(t);
        }

        #endregion
    }
}
