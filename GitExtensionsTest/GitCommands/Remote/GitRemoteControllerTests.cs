using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GitCommands.Config;
using GitCommands.Remote;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitExtensionsTest.GitCommands.Remote
{
    [SetCulture("")]
    [SetUICulture("")]
    [TestFixture]
    class GitRemoteControllerTests
    {
        private IGitModule _module;
        private IConfigFileSettings _configFile;
        private IGitRemoteController _controller;


        [SetUp]
        public void Setup()
        {
            _configFile = Substitute.For<IConfigFileSettings>();

            _module = Substitute.For<IGitModule>();
            _module.LocalConfigFile.Returns(_configFile);

            _controller = new GitRemoteController(_module);
        }


        [Test]
        public void LoadRemotes_should_not_throw_if_module_is_null()
        {
            _module = null;

            ((Action)(() => _controller.LoadRemotes(true))).ShouldNotThrow();
        }

        [Test]
        public void LoadRemotes_should_not_populate_remotes_if_none()
        {
            _module.GetRemotes().Returns(x => Enumerable.Empty<string>());

            _controller.LoadRemotes(true);

            _controller.Remotes.Count.Should().Be(0);
            _module.Received(1).GetRemotes();
            _module.DidNotReceive().GetSetting(Arg.Any<string>());
            _module.DidNotReceive().GetSettings(Arg.Any<string>());
        }

        [Test]
        public void LoadRemotes_should_not_populate_remotes_if_those_are_null_or_whitespace()
        {
            _module.GetRemotes().Returns(x => new[] { null, "", " ", "    ", "\t" });

            _controller.LoadRemotes(true);

            _controller.Remotes.Count.Should().Be(0);
            _module.Received(1).GetRemotes();
            _module.DidNotReceive().GetSetting(Arg.Any<string>());
            _module.DidNotReceive().GetSettings(Arg.Any<string>());
        }

        [TestCase(false)]
        [TestCase(true)]
        public void LoadRemotes_should_populate_remotes_if_any(bool loadDisabled)
        {
            const string remoteName1 = "name1";
            const string remoteName2 = "name2";
            _module.GetRemotes().Returns(x => new[] { null, "", " ", "    ", remoteName1, "\t" });
            var sections = new List<IConfigSection> { new ConfigSection($"{GitRemoteController.DisabledSectionPrefix}{GitRemoteController.SectionRemote}.{remoteName2}", true) };
            _configFile.GetConfigSections().Returns(x => sections);

            _controller.LoadRemotes(loadDisabled);

            _controller.Remotes.Count.Should().Be(loadDisabled ? 2 : 1);

            _module.Received(1).GetRemotes();
            _module.Received(1).GetSetting(string.Format(SettingKeyString.RemoteUrl, remoteName1));
            _module.Received(1).GetSetting(string.Format(SettingKeyString.RemotePushUrl, remoteName1));
            _module.Received(1).GetSetting(string.Format(SettingKeyString.RemotePuttySshKey, remoteName1));
            _module.Received(1).GetSettings(string.Format(SettingKeyString.RemotePush, remoteName1));

            var count = loadDisabled ? 1 : 0;
            _configFile.Received(count).GetConfigSections();
            _module.Received(count).GetSetting(GitRemoteController.DisabledSectionPrefix + string.Format(SettingKeyString.RemoteUrl, remoteName2));
            _module.Received(count).GetSetting(GitRemoteController.DisabledSectionPrefix + string.Format(SettingKeyString.RemotePushUrl, remoteName2));
            _module.Received(count).GetSetting(GitRemoteController.DisabledSectionPrefix + string.Format(SettingKeyString.RemotePuttySshKey, remoteName2));
            _module.Received(count).GetSettings(GitRemoteController.DisabledSectionPrefix + string.Format(SettingKeyString.RemotePush, remoteName2));
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
        public void SaveRemote_should_throw_if_remoteName_is_null_or_empty()
        {
            ((Action)(() => _controller.SaveRemote(null, null, "b", "c", "d"))).ShouldThrow<ArgumentNullException>()
                .WithMessage("Value cannot be null.\r\nParameter name: remoteName");
            ((Action)(() => _controller.SaveRemote(null, "", "b", "c", "d"))).ShouldThrow<ArgumentNullException>()
                .WithMessage("Value cannot be null.\r\nParameter name: remoteName");
            ((Action)(() => _controller.SaveRemote(null, "  ", "b", "c", "d"))).ShouldThrow<ArgumentNullException>()
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
        public void SaveRemote_null_remote_should_set_settings()
        {
            const string remoteName = "a";
            const string remoteUrl = "b";
            const string remotePushUrl = "c";
            const string remotePuttySshKey = "";
            const string output = "yes!";
            _module.AddRemote(Arg.Any<string>(), Arg.Any<string>()).Returns(x => output);

            var result = _controller.SaveRemote(null, remoteName, remoteUrl, remotePushUrl, remotePuttySshKey);

            result.UserMessage.Should().Be(output);
            result.ShouldUpdateRemote.Should().BeTrue();
            _module.Received(1).SetSetting(string.Format(SettingKeyString.RemoteUrl, remoteName), remoteUrl);
            _module.Received(1).SetSetting(string.Format(SettingKeyString.RemotePushUrl, remoteName), remotePushUrl);
            _module.Received(1).UnsetSetting(string.Format(SettingKeyString.RemotePuttySshKey, remoteName));
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

        [Test]
        public void SetRemoteState_should_throw_if_remote_is_null()
        {
            ((Action)(() => _controller.ToggleRemoteState(null, false))).ShouldThrow<ArgumentNullException>()
                .WithMessage("Value cannot be null.\r\nParameter name: remoteName");
        }

        [Test]
        public void SetRemoteState_should_do_nothing_if_section_not_found()
        {
            _configFile.GetConfigSections().Returns(x => new List<IConfigSection>());

            _controller.ToggleRemoteState("boo", false);

            _configFile.Received(1).GetConfigSections();
            _module.DidNotReceive().RemoveRemote(Arg.Any<string>());
            _configFile.DidNotReceive().RemoveConfigSection(Arg.Any<string>());
        }

        [TestCase("name1", false)]
        [TestCase("name2", true)]
        public void SetRemoteState_should_call_ToggleRemoteState(string remoteName, bool remoteDisabled)
        {
            var sections = new List<IConfigSection> { new ConfigSection("-remote.name1", true), new ConfigSection("remote.name2", true) };
            _configFile.GetConfigSections().Returns(x => sections);

            _controller.ToggleRemoteState(remoteName, remoteDisabled);

            _configFile.Received(1).GetConfigSections();
            _module.Received(remoteDisabled ? 1 : 0).RemoveRemote(remoteName);
            _configFile.Received(remoteDisabled ? 0 : 1).RemoveConfigSection($"{GitRemoteController.DisabledSectionPrefix}{GitRemoteController.SectionRemote}.{remoteName}");

            _configFile.Received(1).AddConfigSection(sections[remoteDisabled ? 1 : 0]);
            _configFile.Received(1).Save();
        }
    }
}
