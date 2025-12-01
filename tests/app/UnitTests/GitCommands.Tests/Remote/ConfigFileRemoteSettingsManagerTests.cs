using CommonTestUtils;
using FluentAssertions;
using GitCommands.Config;
using GitCommands.Remotes;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using NSubstitute;

namespace GitCommandsTests.Remote;

[SetCulture("en-US")]
[SetUICulture("en-US")]
[TestFixture]
internal class ConfigFileRemoteSettingsManagerTests
{
    private IGitModule _module;
    private IConfigFileRemoteSettingsManager _remotesManager;

    [SetUp]
    public void Setup()
    {
        _module = Substitute.For<IGitModule>();

        _remotesManager = new ConfigFileRemoteSettingsManager(() => _module);
    }

    [Test]
    public void LoadRemotes_should_not_throw_if_module_is_null()
    {
        _module = null;

        ((Action)(() => _remotesManager.LoadRemotes(true))).Should().NotThrow();
    }

    [Test]
    public void LoadRemotes_should_not_populate_remotes_if_none()
    {
        _module.GetRemoteNames().Returns(x => Array.Empty<string>());

        IEnumerable<ConfigFileRemote> remotes = _remotesManager.LoadRemotes(true);

        remotes.Should().BeEmpty();
        _module.Received(1).GetRemoteNames();
        _module.DidNotReceive().GetSetting(Arg.Any<string>());
        _module.DidNotReceive().GetSettings(Arg.Any<string>());
    }

    [Test]
    public void LoadRemotes_should_not_populate_remotes_if_those_are_null_or_whitespace()
    {
        _module.GetRemoteNames().Returns(x => new[] { null, "", " ", "    ", "\t" });

        IEnumerable<ConfigFileRemote> remotes = _remotesManager.LoadRemotes(true);

        remotes.Should().BeEmpty();
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
        (string Setting, string Value)[] settings = [($"{ConfigFileRemoteSettingsManager.DisabledSectionPrefix}{ConfigFileRemoteSettingsManager.SectionRemote}.{remoteName2}.dummy-name", "dummy value")];
        _module.GetAllLocalSettings().Returns(x => settings);

        IEnumerable<ConfigFileRemote> remotes = _remotesManager.LoadRemotes(loadDisabled);

        remotes.Should().HaveCount(loadDisabled ? 2 : 1);

        _module.Received(1).GetRemoteNames();
        _module.Received(1).GetSetting(string.Format(SettingKeyString.RemoteUrl, remoteName1));
        _module.Received(1).GetSetting(string.Format(SettingKeyString.RemotePushUrl, remoteName1));
        _module.Received(1).GetSetting(string.Format(SettingKeyString.RemotePuttySshKey, remoteName1));
        _module.Received(1).GetSettings(string.Format(SettingKeyString.RemotePush, remoteName1));

        int count = loadDisabled ? 1 : 0;
        _module.Received(count).GetAllLocalSettings();
        _module.Received(count).GetSetting(ConfigFileRemoteSettingsManager.DisabledSectionPrefix + string.Format(SettingKeyString.RemoteUrl, remoteName2));
        _module.Received(count).GetSetting(ConfigFileRemoteSettingsManager.DisabledSectionPrefix + string.Format(SettingKeyString.RemotePushUrl, remoteName2));
        _module.Received(count).GetSetting(ConfigFileRemoteSettingsManager.DisabledSectionPrefix + string.Format(SettingKeyString.RemotePuttySshKey, remoteName2));
        _module.Received(count).GetSettings(ConfigFileRemoteSettingsManager.DisabledSectionPrefix + string.Format(SettingKeyString.RemotePush, remoteName2));
    }

    [Test]
    public void RemoveRemote_should_throw_if_remote_is_null()
    {
        ((Action)(() => _remotesManager.RemoveRemote(null))).Should().Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'remote')");
    }

    [Test]
    public void RemoveRemote_success()
    {
        ConfigFileRemote remote = new() { Name = "bla" };

        _remotesManager.RemoveRemote(remote);

        _module.Received(1).RemoveRemote(remote.Name);
    }

    [Test]
    public void SaveRemote_should_throw_if_remoteName_is_null_or_empty()
    {
        ((Action)(() => _remotesManager.SaveRemote(null, null, "b", "c", "d", "e"))).Should().Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'remoteName')");
        ((Action)(() => _remotesManager.SaveRemote(null, "", "b", "c", "d", "e"))).Should().Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'remoteName')");
        ((Action)(() => _remotesManager.SaveRemote(null, "  ", "b", "c", "d", "e"))).Should().Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'remoteName')");
    }

    [Test]
    public void SaveRemote_null_remote_should_invoke_AddRemote_and_require_update()
    {
        const string remoteName = "a";
        const string remoteUrl = "b";
        const string output = "";
        _module.AddRemote(Arg.Any<string>(), Arg.Any<string>()).Returns(x => output);

        ConfigFileRemoteSaveResult result = _remotesManager.SaveRemote(null, remoteName, remoteUrl, null, null, null);

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
        const string remoteColor = "";
        const string output = "";
        _module.AddRemote(Arg.Any<string>(), Arg.Any<string>()).Returns(x => output);

        ConfigFileRemoteSaveResult result = _remotesManager.SaveRemote(null, remoteName, remoteUrl, remotePushUrl, remotePuttySshKey, remoteColor);

        result.UserMessage.Should().Be(output);
        result.ShouldUpdateRemote.Should().BeTrue();
        _module.Received(1).SetSetting(string.Format(SettingKeyString.RemoteUrl, remoteName), remoteUrl);
        _module.Received(1).SetSetting(string.Format(SettingKeyString.RemotePushUrl, remoteName), remotePushUrl);
        _module.Received(1).UnsetSetting(string.Format(SettingKeyString.RemotePuttySshKey, remoteName));
        _module.Received(1).UnsetSetting(string.Format(SettingKeyString.RemoteColor, remoteName));
    }

    [Test]
    public void SaveRemote_populated_remote_should_invoke_RenameRemote_if_remoteName_mismatch_no_update_required()
    {
        const string remoteName = "a";
        const string remoteUrl = "b";
        const string output = "yes!";
        ConfigFileRemote gitRemote = new() { Name = "old", Url = remoteUrl };
        _module.RenameRemote(Arg.Any<string>(), Arg.Any<string>()).Returns(x => output);

        ConfigFileRemoteSaveResult result = _remotesManager.SaveRemote(gitRemote, remoteName, remoteUrl, null, null, null);

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
        ConfigFileRemote gitRemote = new() { Name = "old", Url = "old" };
        _module.RenameRemote(Arg.Any<string>(), Arg.Any<string>()).Returns(x => output);

        ConfigFileRemoteSaveResult result = _remotesManager.SaveRemote(gitRemote, remoteName, remoteUrl, null, null, null);

        result.UserMessage.Should().Be(output);
        result.ShouldUpdateRemote.Should().BeTrue();
        _module.Received(1).RenameRemote(gitRemote.Name, remoteName);
    }

    [TestCase(null, null, null, null)]
    [TestCase("a", null, null, null)]
    [TestCase("a", "b", null, null)]
    [TestCase("a", "b", "c", null)]
    [TestCase("a", "b", "c", "d")]
    public void SaveRemote_should_update_settings(string? remoteUrl, string? remotePushUrl, string? remotePuttySshKey, string? remoteColor)
    {
        ConfigFileRemote remote = new() { Name = "bla", Url = remoteUrl };

        _remotesManager.SaveRemote(remote, remote.Name, remoteUrl, remotePushUrl, remotePuttySshKey, remoteColor);

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
        ((Action)(() => _remotesManager.ToggleRemoteState(null, false))).Should().Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'remoteName')");
    }

    [Test]
    public void SetRemoteState_should_do_nothing_if_section_not_found()
    {
        _module.GetAllLocalSettings().Returns(x => []);

        _remotesManager.ToggleRemoteState("boo", false);

        _module.Received(1).GetAllLocalSettings();
        _module.DidNotReceive().RemoveRemote(Arg.Any<string>());
        _module.DidNotReceive().RemoveConfigSection(Arg.Any<string>());
    }

    [TestCase("name1", false)]
    [TestCase("name2", true)]
    public void SetRemoteState_should_call_ToggleRemoteState(string remoteName, bool remoteDisabled)
    {
        (string Setting, string Value)[] settings = [("-remote.name1.dummy-name1", "dummy value 1"), ("remote.name2.dummy-name2", "dummy value 2")];
        _module.GetAllLocalSettings().Returns(x => settings);

        _remotesManager.ToggleRemoteState(remoteName, remoteDisabled);

        _module.Received(1).GetAllLocalSettings();
        _module.Received(remoteDisabled ? 1 : 0).RemoveRemote(remoteName);
        _module.Received(remoteDisabled ? 0 : 1).RemoveConfigSection($"{ConfigFileRemoteSettingsManager.DisabledSectionPrefix}{ConfigFileRemoteSettingsManager.SectionRemote}", remoteName);

        (string setting, string value) = settings[remoteDisabled ? 1 : 0];
        setting = remoteDisabled
            ? $"{ConfigFileRemoteSettingsManager.DisabledSectionPrefix}{setting}"
            : setting.TrimStart(ConfigFileRemoteSettingsManager.DisabledSectionPrefix[0]);
        _module.Received(1).SetSetting(setting, value, append: true);
    }

    [Test]
    public void ConfigureRemotes_Should_not_update_localHead_if_remoteHead_is_local()
    {
        IGitRef[] refs = new[]
        {
            CreateSubstituteRef("f6323b8e80f96dff017dd14bdb28a576556adab4", "refs/heads/local", ""),
        };

        _module.GetRefs(RefsFilter.NoFilter).ReturnsForAnyArgs(refs);

        _remotesManager.ConfigureRemotes("origin");

        string mergeWith = "";
        ClassicAssert.AreEqual(mergeWith, refs[0].MergeWith);
        refs[0].Received(0).MergeWith = mergeWith;
    }

    [Test]
    public void ConfigureRemotes_Should_not_update_localHead_if_localHead_is_remote()
    {
        IGitRef[] refs = new[]
        {
            CreateSubstituteRef("02e10a13e06e7562f7c3c516abb2a0e1a0c0dd90", "refs/remotes/origin/develop", "origin"),
        };
        _module.GetRefs(RefsFilter.NoFilter).ReturnsForAnyArgs(refs);

        _remotesManager.ConfigureRemotes("origin");

        string mergeWith = "";
        ClassicAssert.AreEqual(mergeWith, refs[0].MergeWith);
        refs[0].Received(0).MergeWith = mergeWith;
    }

    [Test]
    public void ConfigureRemotes_Should_not_update_localHead_if_remoteHead_is_not_the_remote_origin_of_the_localHead()
    {
        IGitRef[] refs = new[]
        {
            CreateSubstituteRef("f6323b8e80f96dff017dd14bdb28a576556adab4", "refs/heads/develop", ""),
            CreateSubstituteRef("ddca5a9cdc3ab10e042ae6cf5f8da2dd25c4b75f", "refs/remotes/origin/master", "origin"),
        };
        _module.GetRefs(RefsFilter.NoFilter).ReturnsForAnyArgs(refs);

        _remotesManager.ConfigureRemotes("origin");

        string mergeWith = "";

        ClassicAssert.AreEqual(mergeWith, refs[0].MergeWith);
        refs[0].Received(0).MergeWith = mergeWith;
    }

    [Test]
    public void ConfigureRemotes_Should_not_update_localHead_if_remoteHead_is_Tag()
    {
        IGitRef[] refs = new[]
        {
            CreateSubstituteRef("02e10a13e06e7562f7c3c516abb2a0e1a0c0dd90", "refs/tags/local-tag", ""),
        };
        _module.GetRefs(RefsFilter.NoFilter).ReturnsForAnyArgs(refs);

        _remotesManager.ConfigureRemotes("origin");

        string mergeWith = "";

        ClassicAssert.AreEqual(mergeWith, refs[0].MergeWith);
        refs[0].Received(0).MergeWith = mergeWith;
    }

    [Test]
    public void ConfigureRemotes_Should_update_localHead_if_remoteHead_is_the_remote_origin_of_the_localHead()
    {
        IGitRef[] refs = new[]
        {
            CreateSubstituteRef("f6323b8e80f96dff017dd14bdb28a576556adab4", "refs/heads/develop", ""),
            CreateSubstituteRef("02e10a13e06e7562f7c3c516abb2a0e1a0c0dd90", "refs/remotes/origin/develop", "origin"),
        };
        _module.GetRefs(RefsFilter.NoFilter).ReturnsForAnyArgs(refs);

        _remotesManager.ConfigureRemotes("origin");
        string mergeWith = "develop";
        ClassicAssert.AreEqual(mergeWith, refs[0].MergeWith);
        refs[0].Received(1).MergeWith = mergeWith;
    }

    private IGitRef CreateSubstituteRef(string guid, string completeName, string remote)
    {
        bool isRemote = !string.IsNullOrEmpty(remote);
        string name = (isRemote ? remote + "/" : "") + completeName.LazySplit('/').LastOrDefault();
        bool isTag = completeName.StartsWith("refs/tags/", StringComparison.InvariantCultureIgnoreCase);
        IGitRef gitRef = Substitute.For<IGitRef>();
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

        (string Setting, string Value)[] settings = [($"{ConfigFileRemoteSettingsManager.DisabledSectionPrefix}{ConfigFileRemoteSettingsManager.SectionRemote}.{disabledRemoteName}.dummy-name", "dummy value")];
        _module.GetAllLocalSettings().Returns(x => settings);

        IReadOnlyList<GitExtensions.Extensibility.Git.Remote> disabledRemotes = _remotesManager.GetDisabledRemotes();
        ClassicAssert.AreEqual(1, disabledRemotes.Count);
        ClassicAssert.AreEqual(disabledRemoteName, disabledRemotes[0].Name);

        IReadOnlyList<string> disabledRemoteNames = _remotesManager.GetDisabledRemoteNames();
        ClassicAssert.AreEqual(1, disabledRemoteNames.Count);
        ClassicAssert.AreEqual(disabledRemoteName, disabledRemoteNames[0]);
    }

    [Test]
    public void GetEnabledRemoteNames_returns_enabled_remotes_only()
    {
        string enabledRemoteName = "enabledRemote";
        string disabledRemoteName = "disabledRemote";

        _module.GetRemoteNames().Returns(x => new[] { enabledRemoteName, });

        (string Setting, string Value)[] settings = [($"{ConfigFileRemoteSettingsManager.DisabledSectionPrefix}{ConfigFileRemoteSettingsManager.SectionRemote}.{disabledRemoteName}.dummy-name", "dummy value")];
        _module.GetAllLocalSettings().Returns(x => settings);

        IReadOnlyList<string> enabledRemoteNames = _remotesManager.GetEnabledRemoteNames();
        ClassicAssert.AreEqual(1, enabledRemoteNames.Count);
        ClassicAssert.AreEqual(enabledRemoteName, enabledRemoteNames[0]);
    }

    [Test]
    public void EnabledRemoteExists_returns_true_for_enabled_remotes_only()
    {
        string enabledRemoteName = "enabledRemote";
        string disabledRemoteName = "disabledRemote";

        _module.GetRemoteNames().Returns(x => new[] { enabledRemoteName, });

        (string Setting, string Value)[] settings = [($"{ConfigFileRemoteSettingsManager.DisabledSectionPrefix}{ConfigFileRemoteSettingsManager.SectionRemote}.{disabledRemoteName}.dummy-name", "dummy value")];
        _module.GetAllLocalSettings().Returns(x => settings);

        ClassicAssert.IsTrue(_remotesManager.EnabledRemoteExists(enabledRemoteName));
        ClassicAssert.IsFalse(_remotesManager.EnabledRemoteExists(disabledRemoteName));
    }

    [Test]
    public void DisabledRemoteExists_returns_true_for_disabled_remotes_only()
    {
        string enabledRemoteName = "enabledRemote";
        string disabledRemoteName = "disabledRemote";

        _module.GetRemoteNames().Returns(x => new[] { enabledRemoteName, });

        (string Setting, string Value)[] settings = [($"{ConfigFileRemoteSettingsManager.DisabledSectionPrefix}{ConfigFileRemoteSettingsManager.SectionRemote}.{disabledRemoteName}.dummy-name", "dummy value")];
        _module.GetAllLocalSettings().Returns(x => settings);

        ClassicAssert.IsTrue(_remotesManager.DisabledRemoteExists(disabledRemoteName));
        ClassicAssert.IsFalse(_remotesManager.DisabledRemoteExists(enabledRemoteName));
    }

    [Test]
    public void GetDefaultPushRemote_returns_null_for_no_remote_branch()
    {
        ConfigFileRemote remote = new() { Push = [] };
        ClassicAssert.IsNull(_remotesManager.GetDefaultPushRemote(remote, "BranchName"));
    }

    [TestCase("BranchName", "BranchName")]
    [TestCase("BranchName", "dev/me/BranchName")]
    public void GetDefaultPushRemote_returns_value_for_mapped_remote_branch(string branchName, string pushToBranch)
    {
        ConfigFileRemote remote = new() { Push = [$"refs/heads/{branchName}:refs/heads/{pushToBranch}"] };
        ClassicAssert.AreEqual(pushToBranch, _remotesManager.GetDefaultPushRemote(remote, branchName));
    }

    [TestCase("BranchName", "*")]
    [TestCase("BranchName", "dev/me/*")]
    public void GetDefaultPushRemote_returns_value_for_wildcard_mapped_remote_branch(string branchName, string pushToBranch)
    {
        ConfigFileRemote remote = new() { Push = [$"refs/heads/*:refs/heads/{pushToBranch}"] };
        ClassicAssert.AreEqual(pushToBranch.Replace("*", branchName), _remotesManager.GetDefaultPushRemote(remote, branchName));
    }

    public class IntegrationTests
    {
        [Test]
        public void ToggleRemoteState_should_not_fail_if_activate_repeatedly()
        {
            using GitModuleTestHelper helper = new();
            ConfigFileRemoteSettingsManager manager = new(() => helper.Module);

            const string remoteName = "active";
            helper.Module.AddRemote(remoteName, "http://localhost/remote/repo.git");
            manager.ToggleRemoteState(remoteName, true);

            helper.Module.AddRemote(remoteName, "http://localhost/remote/repo.git");
            manager.ToggleRemoteState(remoteName, false);

            helper.Module.GetEffectiveSetting("reload now");
            helper.Module.GetSettings("reload local settings, too");
        }

        [Test]
        public void ToggleRemoteState_should_not_fail_if_deactivate_repeatedly()
        {
            using GitModuleTestHelper helper = new();
            ConfigFileRemoteSettingsManager manager = new(() => helper.Module);

            const string remoteName = "active";
            helper.Module.AddRemote(remoteName, "http://localhost/remote/repo.git");
            manager.ToggleRemoteState(remoteName, true);

            helper.Module.AddRemote(remoteName, "http://localhost/remote/repo.git");
            manager.ToggleRemoteState(remoteName, true);

            helper.Module.GetEffectiveSetting("reload now");
            helper.Module.GetSettings("reload local settings, too");
        }
    }
}
