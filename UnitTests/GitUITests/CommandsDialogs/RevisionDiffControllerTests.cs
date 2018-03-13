using System.IO.Abstractions;
using FluentAssertions;
using GitCommands;
using GitUI.CommandsDialogs;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [TestFixture]
    public class RevisionDiffControllerTests
    {
        private FileBase _file;
        private IFileSystem _fileSystem;
        private IFullPathResolver _fullPathResolver;
        private RevisionDiffController _controller;

        [SetUp]
        public void Setup()
        {
            _file = Substitute.For<FileBase>();
            _fileSystem = Substitute.For<IFileSystem>();
            _fileSystem.File.Returns(_file);

            _fullPathResolver = Substitute.For<IFullPathResolver>();

            _controller = new RevisionDiffController(_fullPathResolver, TODO, _fileSystem);
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
            var rev = new GitRevision(null);
            var selectionInfo = new ContextMenuSelectionInfo(rev, isAnyItemSelected: t);
            _controller.ShouldShowDifftoolMenus(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_DifftoolMenu_Tracked(bool t)
        {
            var rev = new GitRevision(null);
            var selectionInfo = new ContextMenuSelectionInfo(rev, isAnyTracked: t);
            _controller.ShouldShowDifftoolMenus(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_DifftoolMenu_BareRepo(bool t)
        {
            var rev = new GitRevision(null);
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
            var rev = new GitRevision(null);
            var selectionInfo = new ContextMenuSelectionInfo(rev, isAnyItemSelected: t);
            _controller.ShouldShowResetFileMenus(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_ResetMenu_Tracked(bool t)
        {
            var rev = new GitRevision(null);
            var selectionInfo = new ContextMenuSelectionInfo(rev, isAnyTracked: t);
            _controller.ShouldShowResetFileMenus(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_ResetMenu_BareRepo(bool t)
        {
            var rev = new GitRevision(null);
            var selectionInfo = new ContextMenuSelectionInfo(rev, isBareRepository: t);
            _controller.ShouldShowResetFileMenus(selectionInfo).Should().Be(!t);
        }
        #endregion

        #region main menu
        [Test]
        public void BrowseDiff_MainMenus_Default()
        {
            var rev = new GitRevision("1234567890");
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
            var rev = new GitRevision("1234567890");
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
        public void BrowseDiff_StageMenus_Unstaged(bool t)
        {
            var rev = new GitRevision(GitRevision.UnstagedGuid);
            var selectionInfo = new ContextMenuSelectionInfo(rev, firstIsParent: t);
            _controller.ShouldShowMenuStage(selectionInfo).Should().Be(t);
            _controller.ShouldShowMenuUnstage(selectionInfo).Should().BeFalse();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_StageMenus_Index(bool t)
        {
            var rev = new GitRevision(GitRevision.UnstagedGuid);
            var selectionInfo = new ContextMenuSelectionInfo(rev, firstIsParent: t);
            _controller.ShouldShowMenuStage(selectionInfo).Should().Be(t);
            _controller.ShouldShowMenuUnstage(selectionInfo).Should().BeFalse();
        }
        #endregion

        #region difftool
        public void BrowseDiff_SuppressDiffToLocalWhenNoSelectedRevision()
        {
            var selectionInfo = new ContextMenuDiffToolInfo();
            _controller.ShouldShowMenuFirstToSelected(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuFirstToLocal(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuSelectedToLocal(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuFirstParentToLocal(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuSelectedParentToLocal(selectionInfo).Should().BeFalse();
        }

        [Test]
        public void BrowseDiff_SuppressDiffToLocalWhenNoLocalExists()
        {
            var rev = new GitRevision("1234567890");
            var selectionInfo = new ContextMenuDiffToolInfo(selectedRevision: rev, localExists: false);
            _controller.ShouldShowMenuFirstToSelected(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuFirstToLocal(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuSelectedToLocal(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuFirstParentToLocal(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuSelectedParentToLocal(selectionInfo).Should().BeFalse();
        }

        [Test]
        public void BrowseDiff_ShowContextDiffToolForUnstaged()
        {
            var rev = new GitRevision(GitRevision.UnstagedGuid);
            var selectionInfo = new ContextMenuDiffToolInfo(selectedRevision: rev);
            _controller.ShouldShowMenuFirstToSelected(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuFirstToLocal(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuSelectedToLocal(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuFirstParentToLocal(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuSelectedParentToLocal(selectionInfo).Should().BeTrue();
        }

        [Test]
        public void BrowseDiff_ShowContextDiffToolForUnstagedParent()
        {
            var rev = new GitRevision("1234567890");
            var selectionInfo = new ContextMenuDiffToolInfo(selectedRevision: rev, selectedItemParentRevs: new string[] { GitRevision.UnstagedGuid });
            _controller.ShouldShowMenuFirstToSelected(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuFirstToLocal(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuSelectedToLocal(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuFirstParentToLocal(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuSelectedParentToLocal(selectionInfo).Should().BeTrue();
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void BrowseDiff_ShowContextDiffToolForDeletedAndNew(bool d, bool n)
        {
            var rev = new GitRevision("1234567890");
            var selectionInfo = new ContextMenuDiffToolInfo(selectedRevision: rev, allAreDeleted: d, allAreNew: n);
            _controller.ShouldShowMenuFirstToSelected(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuFirstToLocal(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuSelectedToLocal(selectionInfo).Should().Be(!d);
            _controller.ShouldShowMenuFirstParentToLocal(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuSelectedParentToLocal(selectionInfo).Should().Be(!n);
        }
        #endregion

        [Test]
        public void IsFirstParent_should_return_false_if_parents_null()
        {
            _controller.IsFirstParent(null, null).Should().BeFalse();
        }

        [Test]
        public void IsFirstParent_should_return_false_if_no_parents_contains_any_of_selected_items()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void IsFirstParent_should_return_true_if_all_parents_contains_all_of_selected_items()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void LocalExists_should_should_return_false_if_items_null()
        {
            _controller.LocalExists(null).Should().BeFalse();
        }

        [Test]
        public void LocalExists_should_return_true_if_no_items_tracked()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void LocalExists_should_return_true_if_file_exists()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void LocalExists_should_return_false_if_none_of_locally_tracked_items_have_files()
        {
            Assert.Inconclusive();
        }
    }
}
