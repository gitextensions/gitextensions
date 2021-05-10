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
            GitRevision rev = new(ObjectId.Random());
            FileStatusItem item = new(
                firstRev: rev,
                secondRev: new GitRevision(ObjectId.WorkTreeId),
                item: new GitItemStatus("file1")
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
            GitRevision rev = new(ObjectId.Random());
            FileStatusItem item = new(
                firstRev: rev,
                secondRev: rev,
                item: new GitItemStatus(name: "file1")
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
            GitRevision rev = new(ObjectId.Random());
            FileStatusItem item = new(
                firstRev: rev,
                secondRev: new GitRevision(ObjectId.WorkTreeId),
                item: new GitItemStatus("file1")
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
            GitRevision rev = new(ObjectId.Random());
            FileStatusItem item = new(
                firstRev: rev,
                secondRev: rev,
                item: new GitItemStatus("file1")
                {
                    IsSubmodule = isSubmodule,
                    IsDeleted = isDeleted
                });
            _rememberFileContextMenuController.ShouldEnableSecondItemDiff(item, isSecondRev).Should().Be(result);
        }

        [Test]
        public void RememberFile_GetGitCommit_null()
        {
            GitRevision rev = new(ObjectId.Random());
            GitRevision workTree = new(ObjectId.WorkTreeId);

            _rememberFileContextMenuController.GetGitCommit(null, null, false).Should().BeNull();

            FileStatusItem item = new(
                firstRev: null,
                secondRev: rev,
                item: new GitItemStatus("file"));
            _rememberFileContextMenuController.GetGitCommit(null, item, false).Should().BeNull();
        }

        [Test]
        public void RememberFile_GetGitCommit_FirstWorkTree()
        {
            GitRevision rev = new(ObjectId.Random());
            GitRevision workTree = new(ObjectId.WorkTreeId);
            var name = "WorkTreeFile";
            FileStatusItem item = new(
                firstRev: workTree,
                secondRev: rev,
                item: new GitItemStatus(name));
            _rememberFileContextMenuController.GetGitCommit(null, item, false).Should().Be(name);
        }

        [Test]
        public void RememberFile_GetGitCommit_SecondWorkTree()
        {
            GitRevision rev = new(ObjectId.Random());
            GitRevision workTree = new(ObjectId.WorkTreeId);
            var name = "WorkTreeFile";
            FileStatusItem item = new(
                firstRev: rev,
                secondRev: workTree,
                item: new GitItemStatus(name));
            _rememberFileContextMenuController.GetGitCommit(null, item, true).Should().Be(name);
        }

        [Test]
        public void RememberFile_GetGitCommit_Index_Tree()
        {
            GitRevision rev = new(ObjectId.Random());
            GitRevision index = new(ObjectId.IndexId);
            const string name = "File";
            FileStatusItem item = new(
                firstRev: rev,
                secondRev: index,
                item: new GitItemStatus(name) { TreeGuid = ObjectId.Random() });
            _rememberFileContextMenuController.GetGitCommit(null, item, true).Should().Be(item.Item.TreeGuid?.ToString());
        }

        [Test]
        public void RememberFile_GetGitCommit_Index_GetBlob()
        {
            GitRevision rev = new(ObjectId.Random());
            GitRevision index = new(ObjectId.IndexId);
            const string name = "File";
            FileStatusItem item = new(
                firstRev: rev,
                secondRev: index,
                item: new GitItemStatus(name) { TreeGuid = null });
            _rememberFileContextMenuController.GetGitCommit(GetFileBlobHash, item, true).Should().Be(ObjectId.IndexId.ToString());
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RememberFile_GetGitCommit_Commit(bool isSecondRev)
        {
            var id = ObjectId.Random();
            GitRevision rev = new(id);
            const string newName = "newName";
            const string oldName = "oldName";
            FileStatusItem item = new(
                firstRev: rev,
                secondRev: rev,
                item: new GitItemStatus(name: newName)
                {
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
            GitRevision rev = new(id);
            const string newName = "newName";
            FileStatusItem item = new(
                firstRev: rev,
                secondRev: rev,
                item: new GitItemStatus(name: newName));
            var expected = $"{id}:{newName}";
            _rememberFileContextMenuController.GetGitCommit(null, item, isSecondRev).Should().Be(expected);
        }
    }
}
