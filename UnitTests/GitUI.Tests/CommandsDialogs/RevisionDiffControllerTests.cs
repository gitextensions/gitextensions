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

        private ContextMenuSelectionInfo CreateContextMenuSelectionInfo(GitRevision selectedRevision = null,
            bool isAnyCombinedDiff = false,
            int selectedGitItemCount = 1,
            bool isAnyItemIndex = false,
            bool isAnyItemWorkTree = false,
            bool isBareRepository = false,
            bool allFilesExist = true,
            bool allFilesOrUntrackedDirectoriesExist = false,
            bool isAnyTracked = true,
            bool isAnySubmodule = false)
        {
            return new ContextMenuSelectionInfo(selectedRevision,
                isAnyCombinedDiff,
                selectedGitItemCount,
                isAnyItemIndex,
                isAnyItemWorkTree,
                isBareRepository,
                allFilesExist,
                allFilesOrUntrackedDirectoriesExist,
                isAnyTracked,
                isAnySubmodule);
        }

        #region difftool menu

        [Test]
        public void BrowseDiff_DifftoolMenu_Default()
        {
            var selectionInfo = CreateContextMenuSelectionInfo();
            _controller.ShouldShowDifftoolMenus(selectionInfo).Should().BeTrue();
        }

        [TestCase(0)]
        [TestCase(1)]
        public void BrowseDiff_DifftoolMenu_Selected(int t)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, selectedGitItemCount: t);
            _controller.ShouldShowDifftoolMenus(selectionInfo).Should().Be(t != 0);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_DifftoolMenu_Tracked(bool t)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isAnyTracked: t);
            _controller.ShouldShowDifftoolMenus(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_DifftoolMenu_BareRepo(bool t)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isBareRepository: t);
            _controller.ShouldShowDifftoolMenus(selectionInfo).Should().BeTrue();
        }

        #endregion

        #region reset menu

        [Test]
        public void BrowseDiff_ResetMenu_Default()
        {
            var selectionInfo = CreateContextMenuSelectionInfo();
            _controller.ShouldShowResetFileMenus(selectionInfo).Should().BeTrue();
        }

        [TestCase(0)]
        [TestCase(1)]
        public void BrowseDiff_ResetMenu_Selected(int t)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, selectedGitItemCount: t);
            _controller.ShouldShowResetFileMenus(selectionInfo).Should().Be(t != 0);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_ResetMenu_Tracked(bool t)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isAnyTracked: t);
            _controller.ShouldShowResetFileMenus(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_ResetMenu_BareRepo(bool t)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isBareRepository: t);
            _controller.ShouldShowResetFileMenus(selectionInfo).Should().Be(!t);
        }

        #endregion

        #region main menu

        [Test]
        public void BrowseDiff_MainMenus_NoSelection()
        {
            var selectionInfo = CreateContextMenuSelectionInfo(selectedGitItemCount: 0, allFilesExist: false, isAnyTracked: false);
            _controller.ShouldShowMenuSaveAs(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuCherryPick(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuStage(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuUnstage(selectionInfo).Should().BeFalse();
            _controller.ShouldShowSubmoduleMenus(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuOpenRevision(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuDeleteFile(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuCopyFileName(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuShowInFileTree(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuFileHistory(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuBlame(selectionInfo).Should().BeFalse();
        }

        [Test]
        public void BrowseDiff_MainMenus_Default()
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(selectedRevision: rev);
            _controller.ShouldShowMenuSaveAs(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuCherryPick(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuStage(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuUnstage(selectionInfo).Should().BeFalse();
            _controller.ShouldShowSubmoduleMenus(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuOpenRevision(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuDeleteFile(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuCopyFileName(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuShowInFileTree(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuFileHistory(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuBlame(selectionInfo).Should().BeTrue();
        }

        [TestCase(0)]
        [TestCase(1)]
        public void BrowseDiff_MainMenus_SingleSelected(int t)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(selectedRevision: rev, selectedGitItemCount: t);
            _controller.ShouldShowMenuSaveAs(selectionInfo).Should().Be(t != 0);
            _controller.ShouldShowMenuCherryPick(selectionInfo).Should().Be(t != 0);
            _controller.ShouldShowMenuStage(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuUnstage(selectionInfo).Should().BeFalse();
            _controller.ShouldShowSubmoduleMenus(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo).Should().Be(t != 0);
            _controller.ShouldShowMenuOpenRevision(selectionInfo).Should().Be(t != 0);
            _controller.ShouldShowMenuDeleteFile(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuCopyFileName(selectionInfo).Should().Be(t != 0);
            _controller.ShouldShowMenuShowInFileTree(selectionInfo).Should().Be(t != 0);
            _controller.ShouldShowMenuFileHistory(selectionInfo).Should().Be(t != 0);
            _controller.ShouldShowMenuBlame(selectionInfo).Should().Be(t != 0);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_StageMenus_WorkTree(bool t)
        {
            var rev = new GitRevision(ObjectId.WorkTreeId);
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isAnyItemIndex: t);
            _controller.ShouldShowMenuUnstage(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_StageMenus_Index(bool t)
        {
            var rev = new GitRevision(ObjectId.WorkTreeId);
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isAnyItemWorkTree: t);
            _controller.ShouldShowMenuStage(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_EditOpen_IsAnySubmodule(bool t)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(selectedRevision: rev, isAnySubmodule: t);
            _controller.ShouldShowMenuOpenRevision(selectionInfo).Should().Be(!t);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void BrowseDiff_OpenRevisionFile_Commit(int t)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, selectedGitItemCount: t);
            _controller.ShouldShowMenuOpenRevision(selectionInfo).Should().Be(t == 1);
        }

        [Test]
        public void BrowseDiff_OpenRevisionFile_WorkTree()
        {
            var rev = new GitRevision(ObjectId.WorkTreeId);
            var selectionInfo = CreateContextMenuSelectionInfo(rev);
            _controller.ShouldShowMenuOpenRevision(selectionInfo).Should().BeFalse();
        }

        [Test]
        public void BrowseDiff_OpenRevisionFile_Index()
        {
            var rev = new GitRevision(ObjectId.IndexId);
            var selectionInfo = CreateContextMenuSelectionInfo(rev);
            _controller.ShouldShowMenuOpenRevision(selectionInfo).Should().BeFalse();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_DeleteFile_WorkTree(bool t)
        {
            var rev = new GitRevision(ObjectId.WorkTreeId);
            var selectionInfo = CreateContextMenuSelectionInfo(rev, allFilesOrUntrackedDirectoriesExist: t);
            _controller.ShouldShowMenuDeleteFile(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_DeleteFile_Index(bool t)
        {
            var rev = new GitRevision(ObjectId.IndexId);
            var selectionInfo = CreateContextMenuSelectionInfo(rev, allFilesOrUntrackedDirectoriesExist: t);
            _controller.ShouldShowMenuDeleteFile(selectionInfo).Should().Be(t);
        }

        #endregion
    }
}
