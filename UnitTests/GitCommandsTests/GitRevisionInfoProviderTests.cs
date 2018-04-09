using System;
using System.IO;
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

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void LoadChildren_should_throw_if_guid_not_supplied(string guid)
        {
            var item = Substitute.For<IGitItem>();
            item.Guid.Returns(guid);

            ((Action)(() => _provider.LoadChildren(item))).Should().Throw<ArgumentException>();
        }

        [Test]
        public void LoadChildren_should_return_if_gitmodule_not_supplied()
        {
            var guid = Guid.NewGuid().ToString("N");
            var item = Substitute.For<IGitItem>();
            item.Guid.Returns(guid);

            _provider = new GitRevisionInfoProvider(() => null);

            ((Action)(() => _provider.LoadChildren(item))).Should().Throw<ArgumentException>();
        }

        [Test]
        public void LoadChildren_should_return_shallow_tree_for_non_GitItem()
        {
            var guid = Guid.NewGuid().ToString("N");
            var item = Substitute.For<IGitItem>();
            item.Guid.Returns(guid);

            var items = new[] { Substitute.For<IGitItem>(), Substitute.For<IGitItem>(), Substitute.For<IGitItem>() };
            _module.GetTree(guid, false).Returns(items);

            var children = _provider.LoadChildren(item);

            children.Should().BeEquivalentTo(items);
            _module.Received(1).GetTree(guid, false);
        }

        [Test]
        public void LoadChildren_should_return_shallow_tree_for_GitItem_with_updated_FileName()
        {
            var guid = ObjectId.Random();
            var item = new GitItem(0, GitObjectType.Tree, guid, "folder");

            var items = new[] { Substitute.For<IGitItem>(), new GitItem(0, GitObjectType.Blob, ObjectId.Random(), "file2"), new GitItem(0, GitObjectType.Blob, ObjectId.Random(), "file3") };
            _module.GetTree(guid.ToString(), false).Returns(items);

            var children = _provider.LoadChildren(item);

            children.Should().BeEquivalentTo(items);
            ((GitItem)items[1]).FileName.Should().Be(Path.Combine(item.FileName, "file2"));
            ((GitItem)items[2]).FileName.Should().Be(Path.Combine(item.FileName, "file3"));
            _module.Received(1).GetTree(guid.ToString(), false);
        }
    }
}