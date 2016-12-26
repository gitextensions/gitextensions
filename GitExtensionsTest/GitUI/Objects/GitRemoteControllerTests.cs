using System;
using System.Linq;
using FluentAssertions;
using GitCommands.Config;
using GitUI.Objects;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitExtensionsTest.GitUI.Objects
{
    [SetCulture("")]
    [SetUICulture("")]
    [TestFixture]
    class GitRemoteControllerTests
    {
        private IGitModule _module;
        private IGitRemoteController _controller;


        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();

            _controller = new GitRemoteController(_module);
        }


        [Test]
        public void LoadRemotes_should_not_throw_if_module_is_null()
        {
            _module = null;

            ((Action)(() => _controller.LoadRemotes())).ShouldNotThrow();
        }

        [Test]
        public void LoadRemotes_should_not_populate_remotes_if_none()
        {
            _module.GetRemotes().Returns(x => Enumerable.Empty<string>());

            _controller.LoadRemotes();

            _controller.Remotes.Count.Should().Be(0);
            _module.Received(1).GetRemotes();
            _module.DidNotReceive().GetSetting(Arg.Any<string>());
            _module.DidNotReceive().GetSettings(Arg.Any<string>());
        }

        [Test]
        public void LoadRemotes_should_not_populate_remotes_if_those_are_null_or_whitespace()
        {
            _module.GetRemotes().Returns(x => new[] { null, "", " ", "    ", "\t" });

            _controller.LoadRemotes();

            _controller.Remotes.Count.Should().Be(0);
            _module.Received(1).GetRemotes();
            _module.DidNotReceive().GetSetting(Arg.Any<string>());
            _module.DidNotReceive().GetSettings(Arg.Any<string>());
        }

        [Test]
        public void LoadRemotes_should_populate_remotes_if_any()
        {
            const string remote = "a";
            _module.GetRemotes().Returns(x => new[] { null, "", " ", "    ", remote, "\t" });

            _controller.LoadRemotes();

            _controller.Remotes.Count.Should().Be(1);
            _module.Received(1).GetRemotes();
            _module.Received(1).GetSetting(string.Format(SettingKeyString.RemoteUrl, remote));
            _module.Received(1).GetSetting(string.Format(SettingKeyString.RemotePushUrl, remote));
            _module.Received(1).GetSetting(string.Format(SettingKeyString.RemotePuttySshKey, remote));
            _module.Received(1).GetSettings(string.Format(SettingKeyString.RemotePush, remote));
        }

        [Test]
        public void RemoveRemote_should_throw_if_remote_is_null()
        {
            ((Action)(() => _controller.RemoveRemote(null))).ShouldThrow<ArgumentNullException>()
                .WithMessage("Value cannot be null.\r\nParameter name: remote");
        }

        [Test]
        public void RemoveRemote_success()
        {
            var remote = new GitRemote { Name = "bla" };

            _controller.RemoveRemote(remote);

            _module.Received(1).RemoveRemote(remote.Name);
        }

        [Test]
        public void SaveRemote_should_throw_if_remoteName_is_null()
        {
            ((Action)(() => _controller.SaveRemote(null, null, "b", "c", "d"))).ShouldThrow<ArgumentNullException>()
                .WithMessage("Value cannot be null.\r\nParameter name: remoteName");
        }

        [Test]
        public void SaveRemote_null_remote_should_invoke_AddRemote_and_require_update()
        {
            const string remoteName = "a";
            const string remoteUrl = "b";
            const string output = "yes!";
            _module.AddRemote(Arg.Any<string>(), Arg.Any<string>()).Returns(x => output);

            var result = _controller.SaveRemote(null, remoteName, remoteUrl, null, null);

            result.UserMessage.Should().Be(output);
            result.ShouldUpdateRemote.Should().BeTrue();
            _module.Received(1).AddRemote(remoteName, remoteUrl);
        }

        [Test]
        public void SaveRemote_populated_remote_should_invoke_RenameRemote_if_remoteName_mismatch_no_update_required()
        {
            const string remoteName = "a";
            const string remoteUrl = "b";
            const string output = "yes!";
            var gitRemote = new GitRemote { Name = "old", Url = remoteUrl };
            _module.RenameRemote(Arg.Any<string>(), Arg.Any<string>()).Returns(x => output);

            var result = _controller.SaveRemote(gitRemote, remoteName, remoteUrl, null, null);

            result.UserMessage.Should().Be(output);
            result.ShouldUpdateRemote.Should().BeFalse();
            _module.Received(1).RenameRemote(gitRemote.Name, remoteName);
        }

        [Test]
        public void SaveRemote_populated_remote_should_require_update_if_remoteUrl_mismatch()
        {
            const string remoteName = "a";
            const string remoteUrl = "b";
            const string output = "yes!";
            var gitRemote = new GitRemote { Name = "old", Url = "old" };
            _module.RenameRemote(Arg.Any<string>(), Arg.Any<string>()).Returns(x => output);

            var result = _controller.SaveRemote(gitRemote, remoteName, remoteUrl, null, null);

            result.UserMessage.Should().Be(output);
            result.ShouldUpdateRemote.Should().BeTrue();
            _module.Received(1).RenameRemote(gitRemote.Name, remoteName);
        }

        [TestCase(null, null, null)]
        [TestCase("a", null, null)]
        [TestCase("a", "b", null)]
        [TestCase("a", "b", "c")]
        public void SaveRemote_should_update_settings(string remoteUrl, string remotePushUrl, string remotePuttySshKey)
        {
            var remote = new GitRemote { Name = "bla", Url = remoteUrl };

            _controller.SaveRemote(remote, remote.Name, remoteUrl, remotePushUrl, remotePuttySshKey);

            Action<string, string> ensure = (setting, value) =>
            {
                setting = string.Format(setting, remote.Name);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _module.Received(1).SetSetting(setting, value);
                }
                else
                {
                    _module.Received(1).UnsetSetting(setting);
                }
            };
            ensure(SettingKeyString.RemoteUrl, remoteUrl);
            ensure(SettingKeyString.RemotePushUrl, remotePushUrl);
            ensure(SettingKeyString.RemotePuttySshKey, remotePuttySshKey);
        }
    }
}
