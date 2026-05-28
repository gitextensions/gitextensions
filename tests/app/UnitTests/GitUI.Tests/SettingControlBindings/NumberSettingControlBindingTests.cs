using GitExtensions.Extensibility.Settings;
using GitExtUtils.GitUI.Theming;
using GitUI.SettingControlBindings;
using NSubstitute;

namespace GitUITests.SettingControlBindings;

[TestFixture]
public sealed class NumberSettingControlBindingTests
{
    private const string SettingName = "TestSetting";
    private const int DefaultValue = 42;

    [Test]
    public void CreateControlBinding_TextBox_SaveSetting_should_set_null_when_TextBox_is_empty()
    {
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
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
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
        using TextBox textBox = new();
        setting.CustomControl = textBox;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();
        settingsSource.SettingLevel.Returns(SettingLevel.Effective);
        settingsSource.GetValue(SettingName).Returns((string?)null);

        binding.LoadSetting(settingsSource);

        textBox.Text.Should().Be(DefaultValue.ToString());
    }

    [Test]
    public void CreateControlBinding_TextBox_LoadSetting_should_show_empty_when_nothing_stored_at_non_effective_level(
        [Values(SettingLevel.Global, SettingLevel.Local, SettingLevel.Distributed)] SettingLevel settingLevel)
    {
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
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
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
        using TextBox textBox = new();
        setting.CustomControl = textBox;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();
        settingsSource.SettingLevel.Returns(SettingLevel.Effective);
        settingsSource.GetValue(SettingName).Returns((string?)null);

        textBox.Text = DefaultValue.ToString();
        binding.SaveSetting(settingsSource);

        settingsSource.DidNotReceive().SetValue(Arg.Any<string>(), Arg.Any<string?>());
    }

    [Test]
    public void CreateControlBinding_TextBox_LoadSetting_should_show_stored_value_at_effective_level(
        [Values(0, 1, 99, int.MaxValue)] int storedValue)
    {
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
        using TextBox textBox = new();
        setting.CustomControl = textBox;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();
        settingsSource.SettingLevel.Returns(SettingLevel.Effective);
        settingsSource.GetValue(SettingName).Returns(storedValue.ToString());

        binding.LoadSetting(settingsSource);

        textBox.Text.Should().Be(storedValue.ToString());
    }

    [Test]
    public void CreateControlBinding_TextBox_SaveSetting_should_write_valid_number(
        [Values(0, 1, 99, int.MaxValue)] int value)
    {
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
        using TextBox textBox = new();
        setting.CustomControl = textBox;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();

        textBox.Text = value.ToString();
        binding.SaveSetting(settingsSource);

        settingsSource.Received(1).SetValue(SettingName, value.ToString());
    }

    [Test]
    public void CreateControlBinding_TextBox_should_set_red_background_when_text_changes_to_invalid(
        [Values("abc", "1.5", "1e3")] string invalidText)
    {
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
        using TextBox textBox = new();
        setting.CustomControl = textBox;
        SettingControlBindingsProvider.CreateControlBinding(setting);

        textBox.Text = invalidText;

        textBox.BackColor.Should().Be(OtherColors.BrightRed);
        textBox.ForeColor.Should().Be(ColorHelper.GetTextColor(OtherColors.BrightRed));
    }

    [Test]
    public void CreateControlBinding_TextBox_SaveSetting_should_store_null_on_invalid_input(
        [Values("abc", "1.5", "1e3")] string invalidText)
    {
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
        using TextBox textBox = new();
        setting.CustomControl = textBox;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();

        textBox.Text = invalidText;
        binding.SaveSetting(settingsSource);

        settingsSource.Received(1).SetValue(SettingName, null);
    }

    [Test]
    public void CreateControlBinding_TextBox_should_restore_background_when_text_changes_to_valid(
        [Values(0, 1, 99, int.MaxValue)] int value)
    {
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
        using TextBox textBox = new() { BackColor = Color.Red, ForeColor = Color.WhiteSmoke };
        setting.CustomControl = textBox;
        SettingControlBindingsProvider.CreateControlBinding(setting);

        textBox.Text = value.ToString();

        textBox.BackColor.Should().Be(SystemColors.Window);
        textBox.ForeColor.Should().Be(SystemColors.WindowText);
    }

    [Test]
    public void CreateControlBinding_TextBox_LoadSetting_should_restore_background(
        [Values(SettingLevel.Global, SettingLevel.Local, SettingLevel.Distributed, SettingLevel.Effective)] SettingLevel settingLevel)
    {
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
        using TextBox textBox = new() { BackColor = Color.Red, ForeColor = Color.WhiteSmoke };
        setting.CustomControl = textBox;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();
        settingsSource.SettingLevel.Returns(settingLevel);
        settingsSource.GetValue(SettingName).Returns(DefaultValue.ToString());

        binding.LoadSetting(settingsSource);

        textBox.BackColor.Should().Be(SystemColors.Window);
        textBox.ForeColor.Should().Be(SystemColors.WindowText);
    }

    [Test]
    public void CreateControlBinding_TextBox_SaveSetting_should_write_default_value_at_non_effective_level(
        [Values(SettingLevel.Global, SettingLevel.Local, SettingLevel.Distributed)] SettingLevel settingLevel)
    {
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
        using TextBox textBox = new();
        setting.CustomControl = textBox;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();
        settingsSource.SettingLevel.Returns(settingLevel);

        textBox.Text = DefaultValue.ToString();
        binding.SaveSetting(settingsSource);

        settingsSource.Received(1).SetValue(SettingName, DefaultValue.ToString());
    }

    [Test]
    public void CreateControlBinding_NumericUpDown_LoadSetting_should_show_default_value_when_nothing_stored_at_effective_level()
    {
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
        using NumericUpDown numericUpDown = new() { Minimum = 0, Maximum = int.MaxValue };
        setting.CustomControl = numericUpDown;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();
        settingsSource.SettingLevel.Returns(SettingLevel.Effective);
        settingsSource.GetValue(SettingName).Returns((string?)null);

        binding.LoadSetting(settingsSource);

        numericUpDown.Value.Should().Be(DefaultValue);
    }

    [Test]
    public void CreateControlBinding_NumericUpDown_LoadSetting_should_show_empty_when_nothing_stored_at_non_effective_level(
        [Values(SettingLevel.Global, SettingLevel.Local, SettingLevel.Distributed)] SettingLevel settingLevel)
    {
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
        using NumericUpDown numericUpDown = new() { Minimum = 0, Maximum = int.MaxValue };
        setting.CustomControl = numericUpDown;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();
        settingsSource.SettingLevel.Returns(settingLevel);
        settingsSource.GetValue(SettingName).Returns((string?)null);

        binding.LoadSetting(settingsSource);

        numericUpDown.Text.Should().BeEmpty();
    }

    [Test]
    public void CreateControlBinding_NumericUpDown_LoadSetting_should_show_stored_value(
        [Values(SettingLevel.Global, SettingLevel.Local, SettingLevel.Distributed, SettingLevel.Effective)] SettingLevel settingLevel)
    {
        const int StoredValue = 99;
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
        using NumericUpDown numericUpDown = new() { Minimum = 0, Maximum = int.MaxValue };
        setting.CustomControl = numericUpDown;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();
        settingsSource.SettingLevel.Returns(settingLevel);
        settingsSource.GetValue(SettingName).Returns(StoredValue.ToString());

        binding.LoadSetting(settingsSource);

        numericUpDown.Value.Should().Be(StoredValue);
    }

    [Test]
    public void CreateControlBinding_NumericUpDown_SaveSetting_should_set_null_when_control_is_empty()
    {
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
        using NumericUpDown numericUpDown = new() { Minimum = 0, Maximum = int.MaxValue };
        setting.CustomControl = numericUpDown;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();

        numericUpDown.ResetText();
        binding.SaveSetting(settingsSource);

        settingsSource.Received(1).SetValue(SettingName, null);
    }

    [Test]
    public void CreateControlBinding_NumericUpDown_SaveSetting_should_not_write_when_control_shows_default_value_at_effective_level()
    {
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
        using NumericUpDown numericUpDown = new() { Minimum = 0, Maximum = int.MaxValue };
        setting.CustomControl = numericUpDown;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();
        settingsSource.SettingLevel.Returns(SettingLevel.Effective);
        settingsSource.GetValue(SettingName).Returns((string?)null);

        numericUpDown.Value = DefaultValue;
        binding.SaveSetting(settingsSource);

        settingsSource.DidNotReceive().SetValue(Arg.Any<string>(), Arg.Any<string?>());
    }

    [Test]
    public void CreateControlBinding_NumericUpDown_SaveSetting_should_write_value_at_effective_level(
        [Values(0, 1, 99, int.MaxValue)] int value)
    {
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
        using NumericUpDown numericUpDown = new() { Minimum = 0, Maximum = int.MaxValue };
        setting.CustomControl = numericUpDown;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();
        settingsSource.SettingLevel.Returns(SettingLevel.Effective);
        // Store a different value so the effective-level comparison does not skip the write.
        int storedValue = value == DefaultValue ? DefaultValue + 1 : DefaultValue;
        settingsSource.GetValue(SettingName).Returns(storedValue.ToString());

        numericUpDown.Value = value;
        binding.SaveSetting(settingsSource);

        settingsSource.Received(1).SetValue(SettingName, value.ToString());
    }

    [Test]
    public void CreateControlBinding_NumericUpDown_SaveSetting_should_write_default_value_at_non_effective_level(
        [Values(SettingLevel.Global, SettingLevel.Local, SettingLevel.Distributed)] SettingLevel settingLevel)
    {
        NumberSetting<int> setting = new(SettingName, defaultValue: DefaultValue);
        using NumericUpDown numericUpDown = new() { Minimum = 0, Maximum = int.MaxValue };
        setting.CustomControl = numericUpDown;
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        SettingsSource settingsSource = Substitute.For<SettingsSource>();
        settingsSource.SettingLevel.Returns(settingLevel);
        settingsSource.GetValue(SettingName).Returns((string?)null);

        numericUpDown.Value = DefaultValue;
        binding.SaveSetting(settingsSource);

        settingsSource.Received(1).SetValue(SettingName, DefaultValue.ToString());
    }
}
