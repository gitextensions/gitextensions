using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            Assert.ThrowsAsync<ArgumentNullException>(() => _provider.LoadChildren(null));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void LoadChildren_should_throw_if_guid_not_supplied(string guid)
        {
            var item = Substitute.For<IGitItem>();
            item.Guid.Returns(guid);

            Assert.ThrowsAsync<ArgumentException>(() => _provider.LoadChildren(item));
        }

        [Test]
        public void LoadChildren_should_return_if_gitmodule_not_supplied()
        {
            var guid = Guid.NewGuid().ToString("N");
            var item = Substitute.For<IGitItem>();
            item.Guid.Returns(guid);

            _provider = new GitRevisionInfoProvider(() => null);

            Assert.ThrowsAsync<ArgumentException>(() => _provider.LoadChildren(item));
        }

        [Test]
        public async Task LoadChildren_should_return_shallow_tree_for_non_GitItem()
        {
            var guid = Guid.NewGuid().ToString("N");
            var item = Substitute.For<IGitItem>();
            item.Guid.Returns(guid);

            var items = new[] { Substitute.For<IGitItem>(), Substitute.For<IGitItem>(), Substitute.For<IGitItem>() };

            _module.GetTreeAsync(guid, false).Returns(items);

            var children = await _provider.LoadChildren(item);

            children.Should().BeEquivalentTo(items);
            _module.Received(1).GetTreeAsync(guid, false);
        }

        [Test]
        public async Task LoadChildren_should_return_shallow_tree_for_GitItem_with_updated_FileName()
        {
            var item = new GitItem("", "", Guid.NewGuid().ToString("N"), "folder");
            var expectedChildren = new[] { Substitute.For<IGitItem>(), new GitItem("", "", "", "file2"), new GitItem("", "", "", "file3") };

            _module.GetTreeAsync(item.Guid, full: false).Returns(expectedChildren);

            var children = await _provider.LoadChildren(item);

            children.Count().Should().Be(3);
            children.Should().BeEquivalentTo(expectedChildren);
            ((GitItem)expectedChildren[1]).FileName.Should().Be(Path.Combine(item.FileName, "file2"));
            ((GitItem)expectedChildren[2]).FileName.Should().Be(Path.Combine(item.FileName, "file3"));
            _module.Received(1).GetTreeAsync(item.Guid, false);
        }
    }
}