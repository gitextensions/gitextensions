using FluentAssertions;
using GitCommands;
using GitUI.CommandsDialogs;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [TestFixture]
    public class RememberFileContextMenuControllerTests
    {
        private RememberFileContextMenuController _rememberFileContextMenuController;

        /// <summary>
        /// Mock of GitModule.GetFileBlobHash
        /// </summary>
        /// <param name="name">The git blob name</param>
        /// <param name="id">The commit id</param>
        /// <returns>The Git commit id for the item, to get a predictable output</returns>
        private static ObjectId GetFileBlobHash(string name, ObjectId id)
        {
            return id;
        }

        [SetUp]
        public void Setup()
        {
            _rememberFileContextMenuController = new RememberFileContextMenuController();
        }

        [TestCase(false, false, false, true)]
        [TestCase(false, false, true, false)]
        [TestCase(false, true, false, true)]
        [TestCase(false, true, true, false)]
        [TestCase(true, false, false, false)]
        [TestCase(true, false, true, false)]
        [TestCase(true, true, false, false)]
        [TestCase(true, true, true, false)]
        public void RememberFile_ShouldEnableFirstRemember_WorkTree(bool isSubmodule, bool isDeleted, bool isSecondRev, bool result)
        {
            var rev = new GitRevision(ObjectId.Random());
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: new GitRevision(ObjectId.WorkTreeId),
                item: new GitItemStatus
                {
                    IsSubmodule = isSubmodule,
                    IsDeleted = isDeleted
                });
            _rememberFileContextMenuController.ShouldEnableFirstItemDiff(item, isSecondRev).Should().Be(result);
        }

        [TestCase(false, false, false, true)]
        [TestCase(false, false, true, true)]
        [TestCase(false, true, false, true)]
        [TestCase(false, true, true, false)]
        [TestCase(true, false, false, false)]
        [TestCase(true, false, true, false)]
        [TestCase(true, true, false, false)]
        [TestCase(true, true, true, false)]
        public void RememberFile_ShouldEnableFirstRemember_Commit(bool isSubmodule, bool isDeleted, bool isSecondRev, bool result)
        {
            var rev = new GitRevision(ObjectId.Random());
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: rev,
                item: new GitItemStatus
                {
                    IsSubmodule = isSubmodule,
                    IsDeleted = isDeleted
                });
            _rememberFileContextMenuController.ShouldEnableFirstItemDiff(item, isSecondRev).Should().Be(result);
        }

        [TestCase(false, false, false, true)]
        [TestCase(false, false, true, true)]
        [TestCase(false, true, false, true)]
        [TestCase(false, true, true, false)]
        [TestCase(true, false, false, false)]
        [TestCase(true, false, true, false)]
        [TestCase(true, true, false, false)]
        [TestCase(true, true, true, false)]
        public void RememberFile_ShouldEnableSecondRemember_WorkTree(bool isSubmodule, bool isDeleted, bool isSecondRev, bool result)
        {
            var rev = new GitRevision(ObjectId.Random());
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: new GitRevision(ObjectId.WorkTreeId),
                item: new GitItemStatus
                {
                    IsSubmodule = isSubmodule,
                    IsDeleted = isDeleted
                });
            _rememberFileContextMenuController.ShouldEnableSecondItemDiff(item, isSecondRev).Should().Be(result);
        }

        [TestCase(false, false, false, true)]
        [TestCase(false, false, true, true)]
        [TestCase(false, true, false, true)]
        [TestCase(false, true, true, false)]
        [TestCase(true, false, false, false)]
        [TestCase(true, false, true, false)]
        [TestCase(true, true, false, false)]
        [TestCase(true, true, true, false)]
        public void RememberFile_ShouldEnableSecondRemember_Commit(bool isSubmodule, bool isDeleted, bool isSecondRev, bool result)
        {
            var rev = new GitRevision(ObjectId.Random());
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: rev,
                item: new GitItemStatus
                {
                    IsSubmodule = isSubmodule,
                    IsDeleted = isDeleted
                });
            _rememberFileContextMenuController.ShouldEnableSecondItemDiff(item, isSecondRev).Should().Be(result);
        }

        [Test]
        public void RememberFile_GetGitCommit_null()
        {
            var rev = new GitRevision(ObjectId.Random());
            var workTree = new GitRevision(ObjectId.WorkTreeId);

            _rememberFileContextMenuController.GetGitCommit(null, null, false).Should().BeNull();

            var item = new FileStatusItem(
                firstRev: null,
                secondRev: rev,
                item: new GitItemStatus { Name = "file" });
            _rememberFileContextMenuController.GetGitCommit(null, item, false).Should().BeNull();

            item = new FileStatusItem(
                firstRev: workTree,
                secondRev: rev,
                item: new GitItemStatus());
            _rememberFileContextMenuController.GetGitCommit(null, item, false).Should().BeNull();
        }

        [Test]
        public void RememberFile_GetGitCommit_FirstWorkTree()
        {
            var rev = new GitRevision(ObjectId.Random());
            var workTree = new GitRevision(ObjectId.WorkTreeId);
            var name = "WorkTreeFile";
            var item = new FileStatusItem(
                firstRev: workTree,
                secondRev: rev,
                item: new GitItemStatus { Name = name });
            _rememberFileContextMenuController.GetGitCommit(null, item, false).Should().Be(name);
        }

        [Test]
        public void RememberFile_GetGitCommit_SecondWorkTree()
        {
            var rev = new GitRevision(ObjectId.Random());
            var workTree = new GitRevision(ObjectId.WorkTreeId);
            var name = "WorkTreeFile";
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: workTree,
                item: new GitItemStatus { Name = name });
            _rememberFileContextMenuController.GetGitCommit(null, item, true).Should().Be(name);
        }

        [Test]
        public void RememberFile_GetGitCommit_Index_Tree()
        {
            var rev = new GitRevision(ObjectId.Random());
            var index = new GitRevision(ObjectId.IndexId);
            const string name = "File";
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: index,
                item: new GitItemStatus { Name = name, TreeGuid = ObjectId.Random() });
            _rememberFileContextMenuController.GetGitCommit(null, item, true).Should().Be(item.Item.TreeGuid?.ToString());
        }

        [Test]
        public void RememberFile_GetGitCommit_Index_GetBlob()
        {
            var rev = new GitRevision(ObjectId.Random());
            var index = new GitRevision(ObjectId.IndexId);
            const string name = "File";
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: index,
                item: new GitItemStatus { Name = name, TreeGuid = null });
            _rememberFileContextMenuController.GetGitCommit(GetFileBlobHash, item, true).Should().Be(ObjectId.IndexId.ToString());
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RememberFile_GetGitCommit_Commit(bool isSecondRev)
        {
            var id = ObjectId.Random();
            var rev = new GitRevision(id);
            const string newName = "newName";
            const string oldName = "oldName";
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: rev,
                item: new GitItemStatus
                {
                    Name = newName,
                    OldName = oldName
                });
            var expected = $"{id}:{(isSecondRev ? newName : oldName)}";
            _rememberFileContextMenuController.GetGitCommit(null, item, isSecondRev).Should().Be(expected);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RememberFile_GetGitCommit_CommitNoOld(bool isSecondRev)
        {
            var id = ObjectId.Random();
            var rev = new GitRevision(id);
            const string newName = "newName";
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: rev,
                item: new GitItemStatus
                {
                    Name = newName
                });
            var expected = $"{id}:{newName}";
            _rememberFileContextMenuController.GetGitCommit(null, item, isSecondRev).Should().Be(expected);
        }
    }
}
