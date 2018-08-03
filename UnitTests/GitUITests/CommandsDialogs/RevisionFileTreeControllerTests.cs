using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
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
    public class RevisionFileTreeControllerTests
    {
        private IFileAssociatedIconProvider _iconProvider;
        private IGitRevisionInfoProvider _revisionInfoProvider;
        private RevisionFileTreeController _controller;
        private TreeNode _rootNode;
        private ImageList _imageList;

        [SetUp]
        public void Setup()
        {
            _revisionInfoProvider = Substitute.For<IGitRevisionInfoProvider>();
            _iconProvider = Substitute.For<IFileAssociatedIconProvider>();
            _controller = new RevisionFileTreeController(() => @"c:\repo", _revisionInfoProvider, _iconProvider);

            _rootNode = new TreeNode();
            _imageList = new ImageList();
        }

        [TearDown]
        public void TearDown()
        {
            _imageList?.Dispose();
            _imageList = null;
        }

        [Test]
        public void LoadItemsInTreeView_should_not_add_nods_if_no_children()
        {
            var item = new GitItem(0, GitObjectType.Tree, ObjectId.Random(), "folder");
            _revisionInfoProvider.LoadChildren(item).Returns(x => null);

            _controller.LoadChildren(item, _rootNode.Nodes, _imageList.Images);

            _rootNode.Nodes.Count.Should().Be(0);
            _imageList.Images.Count.Should().Be(0);
        }

        [Test]
        public void LoadItemsInTreeView_should_add_all_none_GitItem_items_with_1st_level_nodes()
        {
            var items = new IGitItem[] { new MockGitItem("file1"), new MockGitItem("file2") };
            var item = new MockGitItem("folder");
            _revisionInfoProvider.LoadChildren(item).Returns(items);

            _controller.LoadChildren(item, _rootNode.Nodes, _imageList.Images);

            _rootNode.Nodes.Count.Should().Be(items.Length);
            for (int i = 0; i < items.Length - 1; i++)
            {
                _rootNode.Nodes[i].Text.Should().Be(items[i].Name);
                _rootNode.Nodes[i].ImageIndex.Should().Be(-1);
                _rootNode.Nodes[i].SelectedImageIndex.Should().Be(-1);
                _rootNode.Nodes[i].Nodes.Count.Should().Be(1);
            }

            _imageList.Images.Count.Should().Be(0);
        }

        [Test]
        public void LoadItemsInTreeView_should_add_IsTree_as_folders()
        {
            var items = new[] { new GitItem(0, GitObjectType.Tree, ObjectId.Random(), "file1"), new GitItem(0, GitObjectType.Tree, ObjectId.Random(), "file2") };
            var item = new GitItem(0, GitObjectType.Tree, ObjectId.Random(), "folder");

            _revisionInfoProvider.LoadChildren(item).Returns(items);

            _controller.LoadChildren(item, _rootNode.Nodes, _imageList.Images);

            _rootNode.Nodes.Count.Should().Be(items.Length);
            for (int i = 0; i < items.Length - 1; i++)
            {
                _rootNode.Nodes[i].Text.Should().Be(items[i].Name);
                _rootNode.Nodes[i].ImageIndex.Should().Be(RevisionFileTreeController.TreeNodeImages.Folder);
                _rootNode.Nodes[i].SelectedImageIndex.Should().Be(RevisionFileTreeController.TreeNodeImages.Folder);
                _rootNode.Nodes[i].Nodes.Count.Should().Be(1);
            }

            _imageList.Images.Count.Should().Be(0);
        }

        [Test]
        public void LoadItemsInTreeView_should_add_IsCommit_as_submodule()
        {
            var items = new[] { new GitItem(0, GitObjectType.Commit, ObjectId.Random(), "file1"), new GitItem(0, GitObjectType.Commit, ObjectId.Random(), "file2") };
            var item = new GitItem(0, GitObjectType.Tree, ObjectId.Random(), "folder");
            _revisionInfoProvider.LoadChildren(item).Returns(items);

            _controller.LoadChildren(item, _rootNode.Nodes, _imageList.Images);

            _rootNode.Nodes.Count.Should().Be(items.Length);
            for (int i = 0; i < items.Length - 1; i++)
            {
                _rootNode.Nodes[i].Text.Should().Be($"{items[i].Name} (Submodule)");
                _rootNode.Nodes[i].ImageIndex.Should().Be(RevisionFileTreeController.TreeNodeImages.Submodule);
                _rootNode.Nodes[i].SelectedImageIndex.Should().Be(RevisionFileTreeController.TreeNodeImages.Submodule);
                _rootNode.Nodes[i].Nodes.Count.Should().Be(0);
            }

            _imageList.Images.Count.Should().Be(0);
        }

        [Test]
        public void LoadItemsInTreeView_should_add_IsBlob_as_file()
        {
            var items = new[] { new GitItem(0, GitObjectType.Blob, ObjectId.Random(), "file1"), new GitItem(0, GitObjectType.Blob, ObjectId.Random(), "file2") };
            var item = new GitItem(0, GitObjectType.Tree, ObjectId.Random(), "folder");
            _revisionInfoProvider.LoadChildren(item).Returns(items);

            _controller.LoadChildren(item, _rootNode.Nodes, _imageList.Images);

            _rootNode.Nodes.Count.Should().Be(items.Length);
            for (int i = 0; i < items.Length - 1; i++)
            {
                _rootNode.Nodes[i].Text.Should().Be(items[i].Name);
                _rootNode.Nodes[i].Nodes.Count.Should().Be(0);
            }
        }

        [Test]
        public void LoadItemsInTreeView_should_not_load_icons_for_file_without_extension()
        {
            var items = new[] { new GitItem(0, GitObjectType.Blob, ObjectId.Random(), "file1."), new GitItem(0, GitObjectType.Blob, ObjectId.Random(), "file2") };
            var item = new GitItem(0, GitObjectType.Tree, ObjectId.Random(), "folder");
            _revisionInfoProvider.LoadChildren(item).Returns(items);

            _controller.LoadChildren(item, _rootNode.Nodes, _imageList.Images);

            _rootNode.Nodes.Count.Should().Be(items.Length);
            for (int i = 0; i < items.Length - 1; i++)
            {
                _rootNode.Nodes[i].Text.Should().Be(items[i].Name);
                _rootNode.Nodes[i].ImageKey.Should().BeEmpty();
                _rootNode.Nodes[i].SelectedImageKey.Should().BeEmpty();
                _rootNode.Nodes[i].Nodes.Count.Should().Be(0);
            }

            _imageList.Images.Count.Should().Be(0);
            _iconProvider.DidNotReceive().Get(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void LoadItemsInTreeView_should_not_add_icons_for_file_if_none_provided()
        {
            var items = new[] { new GitItem(0, GitObjectType.Blob, ObjectId.Random(), "file1.foo"), new GitItem(0, GitObjectType.Blob, ObjectId.Random(), "file2.txt") };
            var item = new GitItem(0, GitObjectType.Tree, ObjectId.Random(), "folder");
            _revisionInfoProvider.LoadChildren(item).Returns(items);

            _controller.LoadChildren(item, _rootNode.Nodes, _imageList.Images);

            _rootNode.Nodes.Count.Should().Be(items.Length);
            for (int i = 0; i < items.Length - 1; i++)
            {
                _rootNode.Nodes[i].Text.Should().Be(items[i].Name);
                _rootNode.Nodes[i].ImageKey.Should().BeEmpty();
                _rootNode.Nodes[i].SelectedImageKey.Should().BeEmpty();
                _rootNode.Nodes[i].Nodes.Count.Should().Be(0);
                _iconProvider.Received(1).Get(Arg.Any<string>(), items[i].Name);
            }

            _imageList.Images.Count.Should().Be(0);
        }

        [Test]
        public void LoadItemsInTreeView_should_add_icon_for_file_extension_only_once()
        {
            var items = new[] { new GitItem(0, GitObjectType.Blob, ObjectId.Random(), "file1.txt"), new GitItem(0, GitObjectType.Blob, ObjectId.Random(), "file2.txt") };
            var item = new GitItem(0, GitObjectType.Tree, ObjectId.Random(), "folder");
            _revisionInfoProvider.LoadChildren(item).Returns(items);
            using (var bitmap = new Bitmap(1, 1))
            using (var icon = Icon.FromHandle(bitmap.GetHicon()))
            {
                _iconProvider.Get(Arg.Any<string>(), Arg.Is<string>(x => x.EndsWith(".txt"))).Returns(icon);

                _controller.LoadChildren(item, _rootNode.Nodes, _imageList.Images);

                _rootNode.Nodes.Count.Should().Be(items.Length);
                for (int i = 0; i < items.Length - 1; i++)
                {
                    _rootNode.Nodes[i].Text.Should().Be(items[i].Name);
                    _rootNode.Nodes[i].ImageKey.Should().Be(".txt");
                    _rootNode.Nodes[i].SelectedImageKey.Should().Be(".txt");
                    _rootNode.Nodes[i].Nodes.Count.Should().Be(0);
                    _iconProvider.Received(1).Get(Arg.Any<string>(), items[i].Name);
                }

                _imageList.Images.Count.Should().Be(1);
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
        private class MockGitItem : IGitItem
        {
            public MockGitItem(string name)
            {
                Name = name;
                ObjectId = ObjectId.Random();
                Guid = ObjectId.ToString();
            }

            public string Guid { get; }
            public ObjectId ObjectId { get; }
            public string Name { get; }
        }
    }
}
