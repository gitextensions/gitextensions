using GitExtensions.Extensibility.Settings;
using GitUI.SettingControlBindings;
using NSubstitute;

namespace GitUITests.SettingControlBindings;

[TestFixture]
public sealed class PasswordSettingControlBindingTests
{
    private const string SettingName = "TestSetting";
    private const string DefaultValue = "default";

    [Test]
    public void CreateControlBinding_TextBox_SaveSetting_should_set_null_when_TextBox_is_empty()
    {
        PasswordSetting setting = new(SettingName, DefaultValue);
        using TextBox textBox = new();
        setting.CustomControl = textBox;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();

        textBox.Text = string.Empty;
        binding.SaveSetting(settingsSource);

        settingsSource.Received(1).SetValue(SettingName, null);
    }

    [Test]
    public void CreateControlBinding_TextBox_LoadSetting_should_show_default_value_when_nothing_stored_at_effective_level()
    {
        PasswordSetting setting = new(SettingName, DefaultValue);
        using TextBox textBox = new();
        setting.CustomControl = textBox;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();
        settingsSource.SettingLevel.Returns(SettingLevel.Effective);
        settingsSource.GetValue(SettingName).Returns((string?)null);

        binding.LoadSetting(settingsSource);

        textBox.Text.Should().Be(DefaultValue);
    }

    [Test]
    public void CreateControlBinding_TextBox_LoadSetting_should_show_empty_when_nothing_stored_at_non_effective_level(
        [Values(SettingLevel.Global, SettingLevel.Local, SettingLevel.Distributed)] SettingLevel settingLevel)
    {
        PasswordSetting setting = new(SettingName, DefaultValue);
        using TextBox textBox = new();
        setting.CustomControl = textBox;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();
        settingsSource.SettingLevel.Returns(settingLevel);
        settingsSource.GetValue(SettingName).Returns((string?)null);

        binding.LoadSetting(settingsSource);

        textBox.Text.Should().BeEmpty();
    }

    [Test]
    public void CreateControlBinding_TextBox_SaveSetting_should_not_write_when_TextBox_shows_default_value_at_effective_level()
    {
        PasswordSetting setting = new(SettingName, DefaultValue);
        using TextBox textBox = new();
        setting.CustomControl = textBox;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();
        settingsSource.SettingLevel.Returns(SettingLevel.Effective);
        settingsSource.GetValue(SettingName).Returns((string?)null);

        textBox.Text = DefaultValue;
        binding.SaveSetting(settingsSource);

        settingsSource.DidNotReceive().SetValue(Arg.Any<string>(), Arg.Any<string?>());
    }

    [Test]
    public void CreateControlBinding_TextBox_SaveSetting_should_write_value(
        [Values("secret", "p@ssw0rd")] string value)
    {
        PasswordSetting setting = new(SettingName, DefaultValue);
        using TextBox textBox = new();
        setting.CustomControl = textBox;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();

        textBox.Text = value;
        binding.SaveSetting(settingsSource);

        settingsSource.Received(1).SetValue(SettingName, value);
    }

    [Test]
    public void CreateControlBinding_TextBox_SaveSetting_should_write_default_value_at_non_effective_level(
        [Values(SettingLevel.Global, SettingLevel.Local, SettingLevel.Distributed)] SettingLevel settingLevel)
    {
        PasswordSetting setting = new(SettingName, DefaultValue);
        using TextBox textBox = new();
        setting.CustomControl = textBox;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();
        settingsSource.SettingLevel.Returns(settingLevel);

        textBox.Text = DefaultValue;
        binding.SaveSetting(settingsSource);

        settingsSource.Received(1).SetValue(SettingName, DefaultValue);
    }
}
