using CommonTestUtils;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;

namespace GitCommandsTests.Settings;

[TestFixture]
public sealed class GitConfigSettingsTests
{
    private const string _globalConfigList = "config list --global --includes --null";

    private MockExecutable _executable = null!;

    [SetUp]
    public void SetUp()
    {
        _executable = new MockExecutable();
    }

    [TearDown]
    public void TearDown()
    {
        _executable.Verify();
    }

    [Test]
    public void GetValue_returns_the_value_of_a_unique_setting()
    {
        GitConfigSettings settings = CreateGlobalSettings($"{SettingKeyString.UserName}\nBase Name\0");

        settings.GetValue(SettingKeyString.UserName).Should().Be("Base Name");
    }

    [Test]
    public void GetValue_returns_the_last_value_when_a_conditional_include_overrides_it()
    {
        // "git config list --global --includes" yields the value of the including config file first
        // and the value of the file pulled in by "includeIf" afterwards.
        // Issue #13215: this used to be classified as a multi-value setting, so GetValue returned null
        // and the settings checklist falsely reported the user identity as missing.
        GitConfigSettings settings = CreateGlobalSettings(
            $"{SettingKeyString.UserName}\nBase Name\0{SettingKeyString.UserEmail}\nbase@example.com\0" +
            $"{SettingKeyString.UserName}\nWork Name\0{SettingKeyString.UserEmail}\nwork@example.com\0");

        settings.GetValue(SettingKeyString.UserName).Should().Be("Work Name");
        settings.GetValue(SettingKeyString.UserEmail).Should().Be("work@example.com");
    }

    [Test]
    public void GetValue_returns_null_for_an_unset_setting()
    {
        GitConfigSettings settings = CreateGlobalSettings($"{SettingKeyString.UserName}\nBase Name\0");

        settings.GetValue(SettingKeyString.UserEmail).Should().BeNull();
    }

    [Test]
    public void GetValues_still_returns_all_values_of_a_multi_value_setting()
    {
        GitConfigSettings settings = CreateGlobalSettings(
            $"{SettingKeyString.CredentialHelper}\nmanager\0{SettingKeyString.CredentialHelper}\ncache\0");

        settings.GetValues(SettingKeyString.CredentialHelper).Should().Equal("manager", "cache");
        settings.GetValue(SettingKeyString.CredentialHelper).Should().Be("cache");
    }

    [Test]
    public void SetValue_to_the_effective_value_of_a_multi_value_setting_is_a_no_op()
    {
        GitConfigSettings settings = CreateGlobalSettings(
            $"{SettingKeyString.UserName}\nBase Name\0{SettingKeyString.UserName}\nWork Name\0");

        // Must not throw: writing the value that is already in effect changes nothing.
        settings.SetValue(SettingKeyString.UserName, "Work Name");
    }

    [Test]
    public void SetValue_to_a_different_value_of_a_multi_value_setting_throws()
    {
        GitConfigSettings settings = CreateGlobalSettings(
            $"{SettingKeyString.UserName}\nBase Name\0{SettingKeyString.UserName}\nWork Name\0");

        Action setValue = () => settings.SetValue(SettingKeyString.UserName, "Other Name");

        setValue.Should().Throw<UserExternalOperationException>();
    }

    private GitConfigSettings CreateGlobalSettings(string configListOutput)
    {
        using IDisposable gitVersion = _executable.StageOutput("--version", "git version 2.46.0");
        GitVersion.ResetVersion();

        GitConfigSettings settings = new(_executable, GitSettingLevel.Global);

        using IDisposable configList = _executable.StageOutput(_globalConfigList, configListOutput);

        // Force the (lazy) load while the output is staged.
        settings.GetValue("force.load");

        return settings;
    }
}
