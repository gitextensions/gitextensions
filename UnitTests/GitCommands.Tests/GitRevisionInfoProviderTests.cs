using System;
using System.IO;
using System.Text;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class GitRevisionInfoProviderTests
    {
        private IGitModule _module;
        private GitRevisionInfoProvider _provider;

        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();
            _provider = new GitRevisionInfoProvider(() => _module);
        }

        [Test]
        public void LoadChildren_should_throw_if_null()
        {
            ((Action)(() => _provider.LoadChildren(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void LoadChildren_should_throw_if_ObjectId_is_null()
        {
            var item = Substitute.For<IGitItem>();

            // ObjectId checks input, use Try to get an illegal value
            ObjectId objectId;
            var sourceBytes = Encoding.ASCII.GetBytes("");
            ObjectId.TryParseAsciiHexBytes(sourceBytes, 0, out objectId);
            item.ObjectId.Returns(objectId);

            ((Action)(() => _provider.LoadChildren(item))).Should().Throw<ArgumentException>();
        }

        [Test]
        public void LoadChildren_should_return_if_gitmodule_not_supplied()
        {
            var objectId = ObjectId.Random().ToString();
            var item = Substitute.For<IGitItem>();
            item.Guid.Returns(objectId);

            _provider = new GitRevisionInfoProvider(() => null);

            ((Action)(() => _provider.LoadChildren(item))).Should().Throw<ArgumentException>();
        }

        [Test]
        public void LoadChildren_should_return_shallow_tree_for_non_GitItem()
        {
            var objectId = ObjectId.Random();
            var item = Substitute.For<IGitItem>();
            item.ObjectId.Returns(objectId);
            item.Guid.Returns(objectId.ToString());

            var items = new[] { Substitute.For<IGitItem>(), Substitute.For<IGitItem>(), Substitute.For<IGitItem>() };
            _module.GetTree(objectId, full: false).Returns(items);

            var children = _provider.LoadChildren(item);

            children.Should().BeEquivalentTo(items);
            _module.Received(1).GetTree(objectId, false);
        }

        [Test]
        public void LoadChildren_should_return_shallow_tree_for_GitItem_with_updated_FileName()
        {
            var commitId = ObjectId.Random();
            var item = new GitItem(0, GitObjectType.Tree, commitId, "folder");

            var items = new[] { Substitute.For<IGitItem>(), new GitItem(0, GitObjectType.Blob, ObjectId.Random(), "file2"), new GitItem(0, GitObjectType.Blob, ObjectId.Random(), "file3") };
            _module.GetTree(commitId, false).Returns(items);

            var children = _provider.LoadChildren(item);

            children.Should().BeEquivalentTo(items);
            ((GitItem)items[1]).FileName.Should().Be(Path.Combine(item.FileName, "file2"));
            ((GitItem)items[2]).FileName.Should().Be(Path.Combine(item.FileName, "file3"));
            _module.Received(1).GetTree(commitId, false);
        }
    }
}
