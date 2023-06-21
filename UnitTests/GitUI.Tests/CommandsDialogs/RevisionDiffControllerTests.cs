using FluentAssertions;
using GitCommands;
using GitUI.CommandsDialogs;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitUITests.CommandsDialogs
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]
    public class RevisionDiffControllerTests
    {
        private IGitModule _module;
        private IFullPathResolver _fullPathResolver;
        private RevisionDiffController _controller;

        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();
            _fullPathResolver = Substitute.For<IFullPathResolver>();
            _controller = new(() => _module, _fullPathResolver);
        }

        [Test]
        public void SaveFiles_should_throw_if_files_null()
        {
            ((Action)(() => _controller.SaveFiles(files: null, userSelection: null))).Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'files')");
        }

        [Test]
        public void SaveFiles_should_not_throw_if_userSelection_null_when_files_empty()
        {
            List<FileStatusItem> files = new();

            ((Action)(() => _controller.SaveFiles(files, userSelection: null))).Should().NotThrow();
        }

        [Test]
        public void SaveFiles_should_throw_if_userSelection_null_when_files_notnull()
        {
            List<FileStatusItem> files = new()
            {
                new(default, new(ObjectId.Random()), new(""))
            };

            ((Action)(() => _controller.SaveFiles(files, userSelection: null))).Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'userSelection')");
        }

        [Test]
        public void SaveFiles_should_not_save_single_file_if_selection_cancelled()
        {
            FileStatusItem item = new(default, new(ObjectId.Random()), new(""));
            List<FileStatusItem> files = new()
            {
                item
            };

            // User cancelled the dialog
            Func<string, string?> userSelection = (_) => null;

            _controller.SaveFiles(files, userSelection);

            _fullPathResolver.Received(1).Resolve(item.Item.Name);
            _module.Received(0).SaveBlobAs(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void SaveFiles_should_save_single_file()
        {
            FileStatusItem item = new(default, new(ObjectId.Random()), new(""));
            List<FileStatusItem> files = new()
            {
                item
            };

            Func<string, string?> userSelection = (_) => "c:\\temp\\file.txt";

            _controller.SaveFiles(files, userSelection);

            _fullPathResolver.Received(1).Resolve(item.Item.Name);
            _module.Received(1).SaveBlobAs("c:\\temp\\file.txt", Arg.Any<string>());
        }

        [Test]
        public void SaveFiles_should_not_save_multi_files_if_selection_cancelled()
        {
            FileStatusItem item1 = new(default, new(ObjectId.Random()), new("item1"));
            FileStatusItem item2 = new(default, new(ObjectId.Random()), new("item2"));
            List<FileStatusItem> files = new()
            {
                item1,
                item2
            };

            // User cancelled the dialog
            Func<string, string?> userSelection = (_) => null;

            _controller.SaveFiles(files, userSelection);

            _fullPathResolver.Received(1).Resolve(item1.Item.Name);
            _fullPathResolver.Received(0).Resolve(item2.Item.Name);
            _module.Received(0).SaveBlobAs(Arg.Any<string>(), Arg.Any<string>());
        }

        [TestCase("c:\\temp")]
        [TestCase("c:\\temp\\")]
        public void SaveFiles_should_save_multi_files_same_folder(string targetFolder)
        {
            FileStatusItem item1 = new(default, new(ObjectId.Random()), new("item1"));
            FileStatusItem item2 = new(default, new(ObjectId.Random()), new("item2"));
            FileStatusItem item3 = new(default, new(ObjectId.Random()), new("item3"));
            List<FileStatusItem> files = new()
            {
                item1,
                item2,
                item3,
            };

            _fullPathResolver.Resolve(item1.Item.Name).Returns(x => "c:\\temp\\item1.txt");
            _fullPathResolver.Resolve(item2.Item.Name).Returns(x => "c:\\temp\\folder1\\item2.txt");
            _fullPathResolver.Resolve(item3.Item.Name).Returns(x => "c:\\temp\\folder1\\folder2\\item3.txt");

            Func<string, string?> userSelection = (_) => targetFolder;

            _controller.SaveFiles(files, userSelection);

            _fullPathResolver.Received(2).Resolve(item1.Item.Name);
            _fullPathResolver.Received(1).Resolve(item2.Item.Name);
            _fullPathResolver.Received(1).Resolve(item3.Item.Name);
            _module.ReceivedWithAnyArgs(3).SaveBlobAs(default, default);
            _module.Received(1).SaveBlobAs("c:\\temp\\item1.txt", Arg.Any<string>());
            _module.Received(1).SaveBlobAs("c:\\temp\\folder1\\item2.txt", Arg.Any<string>());
            _module.Received(1).SaveBlobAs("c:\\temp\\folder1\\folder2\\item3.txt", Arg.Any<string>());
        }

        [TestCase("c:\\myproject\\src")]
        [TestCase("c:\\myproject\\src\\")]
        public void SaveFiles_should_save_multi_files_different_folder(string targetFolder)
        {
            FileStatusItem item1 = new(default, new(ObjectId.Random()), new("item1"));
            FileStatusItem item2 = new(default, new(ObjectId.Random()), new("item2"));
            FileStatusItem item3 = new(default, new(ObjectId.Random()), new("item3"));
            List<FileStatusItem> files = new()
            {
                item1,
                item2,
                item3,
            };

            _fullPathResolver.Resolve(item1.Item.Name).Returns(x => "c:\\temp\\item1.txt");
            _fullPathResolver.Resolve(item2.Item.Name).Returns(x => "c:\\temp\\folder1\\item2.txt");
            _fullPathResolver.Resolve(item3.Item.Name).Returns(x => "c:\\temp\\folder1\\folder2\\item3.txt");

            Func<string, string?> userSelection = (_) => targetFolder;

            _controller.SaveFiles(files, userSelection);

            _fullPathResolver.Received(2).Resolve(item1.Item.Name);
            _fullPathResolver.Received(1).Resolve(item2.Item.Name);
            _fullPathResolver.Received(1).Resolve(item3.Item.Name);
            _module.ReceivedWithAnyArgs(3).SaveBlobAs(default, default);
            _module.Received(1).SaveBlobAs("c:\\myproject\\src\\item1.txt", Arg.Any<string>());
            _module.Received(1).SaveBlobAs("c:\\myproject\\src\\folder1\\item2.txt", Arg.Any<string>());
            _module.Received(1).SaveBlobAs("c:\\myproject\\src\\folder1\\folder2\\item3.txt", Arg.Any<string>());
        }

        private static ContextMenuSelectionInfo CreateContextMenuSelectionInfo(GitRevision selectedRevision = null,
            bool isDisplayOnlyDiff = false,
            bool isStatusOnly = false,
            int selectedGitItemCount = 1,
            bool isAnyItemIndex = false,
            bool isAnyItemWorkTree = false,
            bool isBareRepository = false,
            bool allFilesExist = true,
            bool allDirectoriesExist = false,
            bool allFilesOrUntrackedDirectoriesExist = false,
            bool isAnyTracked = true,
            bool supportPatches = true,
            bool isDeleted = false,
            bool isAnySubmodule = false)
        {
            return new ContextMenuSelectionInfo(selectedRevision,
                isDisplayOnlyDiff,
                isStatusOnly,
                selectedGitItemCount,
                isAnyItemIndex,
                isAnyItemWorkTree,
                isBareRepository,
                allFilesExist,
                allDirectoriesExist,
                allFilesOrUntrackedDirectoriesExist,
                isAnyTracked,
                supportPatches,
                isDeleted,
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
            GitRevision rev = new(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, selectedGitItemCount: t);
            _controller.ShouldShowDifftoolMenus(selectionInfo).Should().Be(t > 0);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_DifftoolMenu_BareRepo(bool t)
        {
            GitRevision rev = new(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isBareRepository: t);
            _controller.ShouldShowDifftoolMenus(selectionInfo).Should().BeTrue();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_DifftoolMenu_DisplayOnly(bool t)
        {
            GitRevision rev = new(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isDisplayOnlyDiff: t);
            _controller.ShouldShowDifftoolMenus(selectionInfo).Should().Be(!t);
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
            GitRevision rev = new(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, selectedGitItemCount: t);
            _controller.ShouldShowResetFileMenus(selectionInfo).Should().Be(t > 0);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_ResetMenu_Tracked(bool t)
        {
            GitRevision rev = new(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isAnyTracked: t);
            _controller.ShouldShowResetFileMenus(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_ResetMenu_BareRepo(bool t)
        {
            GitRevision rev = new(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isBareRepository: t);
            _controller.ShouldShowResetFileMenus(selectionInfo).Should().Be(!t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_ResetMenu_DisplayOnly(bool t)
        {
            GitRevision rev = new(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isDisplayOnlyDiff: t);
            _controller.ShouldShowResetFileMenus(selectionInfo).Should().Be(!t);
        }

        #endregion

        #region main menu

        [Test]
        public void BrowseDiff_MainMenus_NoSelection()
        {
            var selectionInfo = CreateContextMenuSelectionInfo(selectedGitItemCount: 0, allFilesExist: false, isAnyTracked: false, supportPatches: false);
            _controller.ShouldShowMenuSaveAs(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuCherryPick(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuStage(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuUnstage(selectionInfo).Should().BeFalse();
            _controller.ShouldShowSubmoduleMenus(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuOpenRevision(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuDeleteFile(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuCopyFileName(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuShowInFolder(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuShowInFileTree(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuFileHistory(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuBlame(selectionInfo).Should().BeFalse();
        }

        [Test]
        public void BrowseDiff_MainMenus_Default()
        {
            GitRevision rev = new(ObjectId.Random());
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
            _controller.ShouldShowMenuShowInFolder(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuShowInFileTree(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuFileHistory(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuBlame(selectionInfo).Should().BeTrue();
        }

        [TestCase(0)]
        [TestCase(1)]
        public void BrowseDiff_MainMenus_SingleSelected(int t)
        {
            GitRevision rev = new(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(selectedRevision: rev, selectedGitItemCount: t);
            _controller.ShouldShowMenuSaveAs(selectionInfo).Should().Be(t != 0);
            _controller.ShouldShowMenuStage(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuUnstage(selectionInfo).Should().BeFalse();
            _controller.ShouldShowSubmoduleMenus(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo).Should().Be(t != 0);
            _controller.ShouldShowMenuOpenRevision(selectionInfo).Should().Be(t != 0);
            _controller.ShouldShowMenuDeleteFile(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuCopyFileName(selectionInfo).Should().Be(t != 0);
            _controller.ShouldShowMenuShowInFolder(selectionInfo).Should().Be(t != 0);
            _controller.ShouldShowMenuShowInFileTree(selectionInfo).Should().Be(t != 0);
            _controller.ShouldShowMenuFileHistory(selectionInfo).Should().Be(t != 0);
            _controller.ShouldShowMenuBlame(selectionInfo).Should().Be(t != 0);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_StageMenus_WorkTree(bool t)
        {
            GitRevision rev = new(ObjectId.WorkTreeId);
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isAnyItemIndex: t);
            _controller.ShouldShowMenuUnstage(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_StageMenus_Index(bool t)
        {
            GitRevision rev = new(ObjectId.WorkTreeId);
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isAnyItemWorkTree: t);
            _controller.ShouldShowMenuStage(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_EditOpen_IsAnySubmodule(bool t)
        {
            GitRevision rev = new(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(selectedRevision: rev, isAnySubmodule: t);
            _controller.ShouldShowMenuOpenRevision(selectionInfo).Should().Be(!t);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void BrowseDiff_OpenRevisionFile_Commit(int t)
        {
            GitRevision rev = new(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, selectedGitItemCount: t);
            _controller.ShouldShowMenuOpenRevision(selectionInfo).Should().Be(t == 1);
        }

        [Test]
        public void BrowseDiff_OpenRevisionFile_WorkTree()
        {
            GitRevision rev = new(ObjectId.WorkTreeId);
            var selectionInfo = CreateContextMenuSelectionInfo(rev);
            _controller.ShouldShowMenuOpenRevision(selectionInfo).Should().BeFalse();
        }

        [Test]
        public void BrowseDiff_OpenRevisionFile_Index()
        {
            GitRevision rev = new(ObjectId.IndexId);
            var selectionInfo = CreateContextMenuSelectionInfo(rev);
            _controller.ShouldShowMenuOpenRevision(selectionInfo).Should().BeFalse();
        }

        [Test]
        public void BrowseDiff_OpenRevisionFile_DisplayOnly()
        {
            GitRevision rev = new(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isDisplayOnlyDiff: true);
            _controller.ShouldShowMenuOpenRevision(selectionInfo).Should().BeFalse();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_SupportLinePatches(bool t)
        {
            GitRevision rev = new(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, supportPatches: t);
            _controller.ShouldShowMenuSaveAs(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuCherryPick(selectionInfo).Should().Be(t);
            _controller.ShouldShowMenuOpenRevision(selectionInfo).Should().BeTrue();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_DisplayOnlyDiff(bool t)
        {
            GitRevision rev = new(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isDisplayOnlyDiff: t);
            _controller.ShouldShowMenuSaveAs(selectionInfo).Should().Be(!t);
            _controller.ShouldShowMenuOpenRevision(selectionInfo).Should().Be(!t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_StatusOnlyDiff(bool t)
        {
            GitRevision rev = new(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isStatusOnly: t);
            _controller.ShouldShowMenuCopyFileName(selectionInfo).Should().Be(!t);
            _controller.ShouldShowMenuShowInFolder(selectionInfo).Should().Be(!t);
            _controller.ShouldShowMenuShowInFileTree(selectionInfo).Should().Be(!t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_ShowInFileTree(bool t)
        {
            GitRevision rev = new(ObjectId.Random());
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isDeleted: t);
            _controller.ShouldShowMenuCopyFileName(selectionInfo).Should().Be(true);
            _controller.ShouldShowMenuShowInFolder(selectionInfo).Should().Be(true);
            _controller.ShouldShowMenuShowInFileTree(selectionInfo).Should().Be(!t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_DeleteFile_WorkTree(bool t)
        {
            GitRevision rev = new(ObjectId.WorkTreeId);
            var selectionInfo = CreateContextMenuSelectionInfo(rev, allFilesOrUntrackedDirectoriesExist: t);
            _controller.ShouldShowMenuDeleteFile(selectionInfo).Should().Be(t);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrowseDiff_DeleteFile_Index(bool t)
        {
            GitRevision rev = new(ObjectId.IndexId);
            var selectionInfo = CreateContextMenuSelectionInfo(rev, allFilesOrUntrackedDirectoriesExist: t);
            _controller.ShouldShowMenuDeleteFile(selectionInfo).Should().Be(t);
        }

        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(false, false, false)]
        public void BrowseDiff_Submodules_WorkTree(bool isAnySubmodule, bool submodulesExist, bool expected)
        {
            GitRevision rev = new(ObjectId.WorkTreeId);
            var selectionInfo = CreateContextMenuSelectionInfo(rev, isAnySubmodule: isAnySubmodule, allDirectoriesExist: submodulesExist);
            _controller.ShouldShowSubmoduleMenus(selectionInfo).Should().Be(expected);
        }

        #endregion
    }
}
