using System;
using System.Collections.Generic;
using System.Linq;
using CommonTestUtils;
using FluentAssertions;
using GitCommands.Config;
using GitCommands.Remotes;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests.Remote
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]
    internal class GitRemoteManagerTests
    {
        private IGitModule _module;
        private IConfigFileSettings _configFile;
        private IGitRemoteManager _controller;

        [SetUp]
        public void Setup()
        {
            _configFile = Substitute.For<IConfigFileSettings>();

            _module = Substitute.For<IGitModule>();
            _module.LocalConfigFile.Returns(_configFile);

            _controller = new GitRemoteManager(() => _module);
        }

        [Test]
        public void LoadRemotes_should_not_throw_if_module_is_null()
        {
            _module = null;

            ((Action)(() => _controller.LoadRemotes(true))).Should().NotThrow();
        }

        [Test]
        public void LoadRemotes_should_not_populate_remotes_if_none()
        {
            _module.GetRemoteNames().Returns(x => Enumerable.Empty<string>());

            var remotes = _controller.LoadRemotes(true);

            remotes.Count().Should().Be(0);
            _module.Received(1).GetRemoteNames();
            _module.DidNotReceive().GetSetting(Arg.Any<string>());
            _module.DidNotReceive().GetSettings(Arg.Any<string>());
        }

        [Test]
        public void LoadRemotes_should_not_populate_remotes_if_those_are_null_or_whitespace()
        {
            _module.GetRemoteNames().Returns(x => new[] { null, "", " ", "    ", "\t" });

            var remotes = _controller.LoadRemotes(true);

            remotes.Count().Should().Be(0);
            _module.Received(1).GetRemoteNames();
            _module.DidNotReceive().GetSetting(Arg.Any<string>());
            _module.DidNotReceive().GetSettings(Arg.Any<string>());
        }

        [TestCase(false)]
        [TestCase(true)]
        public void LoadRemotes_should_populate_remotes_if_any(bool loadDisabled)
        {
            const string remoteName1 = "name1";
            const string remoteName2 = "name2";
            _module.GetRemoteNames().Returns(x => new[] { null, "", " ", "    ", remoteName1, "\t" });
            var sections = new List<IConfigSection> { new ConfigSection($"{GitRemoteManager.DisabledSectionPrefix}{GitRemoteManager.SectionRemote}.{remoteName2}", true) };
            _configFile.GetConfigSections().Returns(x => sections);

            var remotes = _controller.LoadRemotes(loadDisabled);

            remotes.Count().Should().Be(loadDisabled ? 2 : 1);

            _module.Received(1).GetRemoteNames();
            _module.Received(1).GetSetting(string.Format(SettingKeyString.RemoteUrl, remoteName1));
            _module.Received(1).GetSetting(string.Format(SettingKeyString.RemotePushUrl, remoteName1));
            _module.Received(1).GetSetting(string.Format(SettingKeyString.RemotePuttySshKey, remoteName1));
            _module.Received(1).GetSettings(string.Format(SettingKeyString.RemotePush, remoteName1));

            var count = loadDisabled ? 1 : 0;
            _configFile.Received(count).GetConfigSections();
            _module.Received(count).GetSetting(GitRemoteManager.DisabledSectionPrefix + string.Format(SettingKeyString.RemoteUrl, remoteName2));
            _module.Received(count).GetSetting(GitRemoteManager.DisabledSectionPrefix + string.Format(SettingKeyString.RemotePushUrl, remoteName2));
            _module.Received(count).GetSetting(GitRemoteManager.DisabledSectionPrefix + string.Format(SettingKeyString.RemotePuttySshKey, remoteName2));
            _module.Received(count).GetSettings(GitRemoteManager.DisabledSectionPrefix + string.Format(SettingKeyString.RemotePush, remoteName2));
        }

        [Test]
        public void RemoveRemote_should_throw_if_remote_is_null()
        {
            ((Action)(() => _controller.RemoveRemote(null))).Should().Throw<ArgumentNullException>()
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
            ((Action)(() => _controller.SaveRemote(null, null, "b", "c", "d"))).Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null.\r\nParameter name: remoteName");
            ((Action)(() => _controller.SaveRemote(null, "", "b", "c", "d"))).Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null.\r\nParameter name: remoteName");
            ((Action)(() => _controller.SaveRemote(null, "  ", "b", "c", "d"))).Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null.\r\nParameter name: remoteName");
        }

        [Test]
        public void SaveRemote_null_remote_should_invoke_AddRemote_and_require_update()
        {
            const string remoteName = "a";
            const string remoteUrl = "b";
            const string output = "";
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
            const string output = "";
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

            void Ensure(string setting, string value)
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
            }

            Ensure(SettingKeyString.RemoteUrl, remoteUrl);
            Ensure(SettingKeyString.RemotePushUrl, remotePushUrl);
            Ensure(SettingKeyString.RemotePuttySshKey, remotePuttySshKey);
        }

        [Test]
        public void SetRemoteState_should_throw_if_remote_is_null()
        {
            ((Action)(() => _controller.ToggleRemoteState(null, false))).Should().Throw<ArgumentNullException>()
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
            _configFile.Received(remoteDisabled ? 0 : 1).RemoveConfigSection($"{GitRemoteManager.DisabledSectionPrefix}{GitRemoteManager.SectionRemote}.{remoteName}");

            _configFile.Received(1).AddConfigSection(sections[remoteDisabled ? 1 : 0]);
            _configFile.Received(1).Save();
        }

        [Test]
        public void ConfigureRemotes_Should_not_update_localHead_if_remoteHead_is_local()
        {
            var refs = new[]
            {
                CreateSubstituteRef("f6323b8e80f96dff017dd14bdb28a576556adab4", "refs/heads/local", ""),
            };

            _module.GetRefs().ReturnsForAnyArgs(refs);

            _controller.ConfigureRemotes("origin");

            var mergeWith = "";
            Assert.AreEqual(mergeWith, refs[0].MergeWith);
            refs[0].Received(0).MergeWith = mergeWith;
        }

        [Test]
        public void ConfigureRemotes_Should_not_update_localHead_if_localHead_is_remote()
        {
            var refs = new[]
            {
                CreateSubstituteRef("02e10a13e06e7562f7c3c516abb2a0e1a0c0dd90", "refs/remotes/origin/develop", "origin"),
            };
            _module.GetRefs().ReturnsForAnyArgs(refs);

            _controller.ConfigureRemotes("origin");

            var mergeWith = "";
            Assert.AreEqual(mergeWith, refs[0].MergeWith);
            refs[0].Received(0).MergeWith = mergeWith;
        }

        [Test]
        public void ConfigureRemotes_Should_not_update_localHead_if_remoteHead_is_not_the_remote_origin_of_the_localHead()
        {
            var refs = new[]
            {
                CreateSubstituteRef("f6323b8e80f96dff017dd14bdb28a576556adab4", "refs/heads/develop", ""),
                CreateSubstituteRef("ddca5a9cdc3ab10e042ae6cf5f8da2dd25c4b75f", "refs/remotes/origin/master", "origin"),
            };
            _module.GetRefs().ReturnsForAnyArgs(refs);

            _controller.ConfigureRemotes("origin");

            var mergeWith = "";

            Assert.AreEqual(mergeWith, refs[0].MergeWith);
            refs[0].Received(0).MergeWith = mergeWith;
        }

        [Test]
        public void ConfigureRemotes_Should_not_update_localHead_if_remoteHead_is_Tag()
        {
            var refs = new[]
            {
                CreateSubstituteRef("02e10a13e06e7562f7c3c516abb2a0e1a0c0dd90", "refs/tags/local-tag", ""),
            };
            _module.GetRefs().ReturnsForAnyArgs(refs);

            _controller.ConfigureRemotes("origin");

            var mergeWith = "";

            Assert.AreEqual(mergeWith, refs[0].MergeWith);
            refs[0].Received(0).MergeWith = mergeWith;
        }

        [Test]
        public void ConfigureRemotes_Should_update_localHead_if_remoteHead_is_the_remote_origin_of_the_localHead()
        {
            var refs = new[]
            {
                CreateSubstituteRef("f6323b8e80f96dff017dd14bdb28a576556adab4", "refs/heads/develop", ""),
                CreateSubstituteRef("02e10a13e06e7562f7c3c516abb2a0e1a0c0dd90", "refs/remotes/origin/develop", "origin"),
            };
            _module.GetRefs().ReturnsForAnyArgs(refs);

            _controller.ConfigureRemotes("origin");
            var mergeWith = "develop";
            Assert.AreEqual(mergeWith, refs[0].MergeWith);
            refs[0].Received(1).MergeWith = mergeWith;
        }

        private IGitRef CreateSubstituteRef(string guid, string completeName, string remote)
        {
            var isRemote = !string.IsNullOrEmpty(remote);
            var name = (isRemote ? remote + "/" : "") + completeName.Split('/').LastOrDefault();
            var isTag = completeName.StartsWith("refs/tags/", StringComparison.InvariantCultureIgnoreCase);
            var gitRef = Substitute.For<IGitRef>();
            gitRef.Module.Returns(_module);
            gitRef.Guid.Returns(guid);
            gitRef.CompleteName.Returns(completeName);
            gitRef.Name.Returns(name);
            gitRef.Remote.Returns(remote);
            gitRef.IsRemote.Returns(isRemote);
            gitRef.IsTag.Returns(isTag);
            return gitRef;
        }

        [Test]
        public void GetDisabledRemotes_returns_disabled_remotes_only()
        {
            string enabledRemoteName = "enabledRemote";
            string disabledRemoteName = "disabledRemote";

            _module.GetRemoteNames().Returns(x => new[] { enabledRemoteName, });

            var sections = new List<IConfigSection> { new ConfigSection($"{GitRemoteManager.DisabledSectionPrefix}{GitRemoteManager.SectionRemote}.{disabledRemoteName}", true) };
            _configFile.GetConfigSections().Returns(x => sections);

            var disabledRemotes = _controller.GetDisabledRemotes();
            Assert.AreEqual(1, disabledRemotes.Count);
            Assert.AreEqual(disabledRemoteName, disabledRemotes[0].Name);

            var disabledRemoteNames = _controller.GetDisabledRemoteNames();
            Assert.AreEqual(1, disabledRemoteNames.Count);
            Assert.AreEqual(disabledRemoteName, disabledRemoteNames[0]);
        }

        [Test]
        public void GetEnabledRemoteNames_returns_enabled_remotes_only()
        {
            string enabledRemoteName = "enabledRemote";
            string disabledRemoteName = "disabledRemote";

            _module.GetRemoteNames().Returns(x => new[] { enabledRemoteName, });

            var sections = new List<IConfigSection> { new ConfigSection($"{GitRemoteManager.DisabledSectionPrefix}{GitRemoteManager.SectionRemote}.{disabledRemoteName}", true) };
            _configFile.GetConfigSections().Returns(x => sections);

            var enabledRemoteNames = _controller.GetEnabledRemoteNames();
            Assert.AreEqual(1, enabledRemoteNames.Count);
            Assert.AreEqual(enabledRemoteName, enabledRemoteNames[0]);
        }

        [Test]
        public void GetEnabledRemotesNameWithoutBranches_returns_enabled_remotes_without_branches_only()
        {
            string enabledRemoteNameWithBranches = "enabledRemote1";
            string enabledRemoteNameNoBranches = "enabledRemote2";
            string disabledRemoteName = "disabledRemote";

            _module.GetRemoteNames().Returns(x => new[] { enabledRemoteNameWithBranches, enabledRemoteNameNoBranches });

            var refs = new[]
            {
                CreateSubstituteRef("02e10a13e06e7562f7c3c516abb2a0e1a0c0dd90", $"refs/remotes/{enabledRemoteNameWithBranches}/develop", $"{enabledRemoteNameWithBranches}"),
            };

            _module.GetRefs().ReturnsForAnyArgs(refs);

            var sections = new List<IConfigSection> { new ConfigSection($"{GitRemoteManager.DisabledSectionPrefix}{GitRemoteManager.SectionRemote}.{disabledRemoteName}", true) };
            _configFile.GetConfigSections().Returns(x => sections);

            var enabledRemotesNoBranches = _controller.GetEnabledRemoteNamesWithoutBranches();
            Assert.AreEqual(1, enabledRemotesNoBranches.Count);
            Assert.AreEqual(enabledRemoteNameNoBranches, enabledRemotesNoBranches[0]);
        }

        [Test]
        public void EnabledRemoteExists_returns_true_for_enabled_remotes_only()
        {
            string enabledRemoteName = "enabledRemote";
            string disabledRemoteName = "disabledRemote";

            _module.GetRemoteNames().Returns(x => new[] { enabledRemoteName, });

            var sections = new List<IConfigSection> { new ConfigSection($"{GitRemoteManager.DisabledSectionPrefix}{GitRemoteManager.SectionRemote}.{disabledRemoteName}", true) };
            _configFile.GetConfigSections().Returns(x => sections);

            Assert.IsTrue(_controller.EnabledRemoteExists(enabledRemoteName));
            Assert.IsFalse(_controller.EnabledRemoteExists(disabledRemoteName));
        }

        [Test]
        public void DisabledRemoteExists_returns_true_for_disabled_remotes_only()
        {
            string enabledRemoteName = "enabledRemote";
            string disabledRemoteName = "disabledRemote";

            _module.GetRemoteNames().Returns(x => new[] { enabledRemoteName, });

            var sections = new List<IConfigSection> { new ConfigSection($"{GitRemoteManager.DisabledSectionPrefix}{GitRemoteManager.SectionRemote}.{disabledRemoteName}", true) };
            _configFile.GetConfigSections().Returns(x => sections);

            Assert.IsTrue(_controller.DisabledRemoteExists(disabledRemoteName));
            Assert.IsFalse(_controller.DisabledRemoteExists(enabledRemoteName));
        }

        public class IntegrationTests
        {
            [Test]
            public void ToggleRemoteState_should_not_fail_if_activate_repeatedly()
            {
                using (var helper = new GitModuleTestHelper())
                {
                    var manager = new GitRemoteManager(() => helper.Module);

                    const string remoteName = "active";
                    helper.Module.AddRemote(remoteName, "http://localhost/remote/repo.git");
                    manager.ToggleRemoteState(remoteName, true);

                    helper.Module.AddRemote(remoteName, "http://localhost/remote/repo.git");
                    manager.ToggleRemoteState(remoteName, false);
                }
            }

            [Test]
            public void ToggleRemoteState_should_not_fail_if_deactivate_repeatedly()
            {
                using (var helper = new GitModuleTestHelper())
                {
                    var manager = new GitRemoteManager(() => helper.Module);

                    const string remoteName = "active";
                    helper.Module.AddRemote(remoteName, "http://localhost/remote/repo.git");
                    manager.ToggleRemoteState(remoteName, true);

                    helper.Module.AddRemote(remoteName, "http://localhost/remote/repo.git");
                    manager.ToggleRemoteState(remoteName, true);
                }
            }
        }
    }
}
