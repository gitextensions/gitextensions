using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GitCommands.UserRepositoryHistory;
using GitUI.CommandsDialogs;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [TestFixture]
    public class FormRemotesControllerTests
    {
        private FormRemotesController _controller;

        [SetUp]
        public void Setup()
        {
            _controller = new FormRemotesController();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void RemoteDelete_should_not_throw_or_mutate_list_of_remotes_if_oldRemoteUrl_null_or_empty(string oldRemoteUrl)
        {
            var remotes = new List<Repository>
            {
                new Repository("a"),
                new Repository("b")
            };

            _controller.RemoteDelete(remotes, oldRemoteUrl);

            remotes.Count.Should().Be(2);
        }

        [Test]
        public void RemoteDelete_should_not_throw_or_mutate_list_of_remotes_if_oldRemoteUrl_not_in_list()
        {
            var remotes = new List<Repository>
            {
                new Repository("a"),
                new Repository("b")
            };

            _controller.RemoteDelete(remotes, "foo");

            remotes.Count.Should().Be(2);
        }

        [Test]
        public void RemoteDelete_should_remove_remotes_matching_oldRemoteUrl()
        {
            var remotes = new List<Repository>
            {
                new Repository("a"),
                new Repository("b")
            };

            _controller.RemoteDelete(remotes, "b");

            remotes.Count.Should().Be(1);
            remotes[0].Path.Should().Be("a");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void RemoteUpdate_should_not_throw_or_mutate_list_of_remotes_if_newRemoteUrl_null_or_empty(string newRemoteUrl)
        {
            var remotes = new List<Repository>
            {
                new Repository("a"),
                new Repository("b")
            };

            _controller.RemoteUpdate(remotes, "who cares", newRemoteUrl);

            remotes.Count.Should().Be(2);
            remotes[0].Path.Should().Be("a");
            remotes[1].Path.Should().Be("b");
        }

        [Test]
        public void RemoteDelete_should_replace_matching_oldRemoteUrl()
        {
            var remotes = new List<Repository>
            {
                new Repository("a"),
                new Repository("b")
            };

            _controller.RemoteUpdate(remotes, "a", "a1");

            remotes.Count.Should().Be(2);
            remotes.Select(r => r.Path).Should().BeEquivalentTo(new[] { "a1", "b" });
        }

        [Test]
        public void RemoteDelete_should_place_newRemoteUrl_as_first()
        {
            var remotes = new List<Repository>
            {
                new Repository("a"),
                new Repository("b"),
                new Repository("c")
            };

            _controller.RemoteUpdate(remotes, "c", "a1");

            remotes.Count.Should().Be(3);
            remotes[0].Path.Should().Be("a1");
            remotes[1].Path.Should().Be("a");
            remotes[2].Path.Should().Be("b");

            _controller.RemoteUpdate(remotes, null, "q");

            remotes.Count.Should().Be(4);
            remotes[0].Path.Should().Be("q");
            remotes[1].Path.Should().Be("a1");
            remotes[2].Path.Should().Be("a");
            remotes[3].Path.Should().Be("b");
        }
    }
}
