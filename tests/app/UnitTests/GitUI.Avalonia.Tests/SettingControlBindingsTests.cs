using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitExtensions.Extensibility.Settings;
using GitUI.Compat;
using GitUI.SettingControlBindings;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[NonParallelizable]
public sealed class SettingControlBindingsTests
{
    [AvaloniaTest]
    public void Provider_should_restore_the_original_binding_class_boundary()
    {
        (ISetting Setting, string BindingType)[] cases =
        [
            (new BoolSetting("Bool", false), "BoolSettingControlBinding"),
            (new ChoiceSetting("Choice", ["one"], "one"), "ChoiceSettingControlBinding"),
            (new CredentialsSetting("Credentials", "Credentials", () => null), "CredentialsSettingControlBinding"),
            (new NumberSetting<int>("Int", 1), "NumberSettingNumericUpDownBinding"),
            (new NumberSetting<float>("Float", 1), "NumberSettingTextBoxBinding`1"),
            (new NumberSetting<double>("Double", 1), "NumberSettingTextBoxBinding`1"),
            (new NumberSetting<long>("Long", 1), "NumberSettingTextBoxBinding`1"),
            (new PasswordSetting("Password", "default"), "PasswordSettingControlBinding"),
            (new PseudoSetting("Pseudo", height: 40), "PseudoSettingControlBinding"),
            (new StringSetting("String", "default"), "StringSettingControlBinding"),
        ];

        foreach ((ISetting setting, string bindingType) in cases)
        {
            SettingControlBindingsProvider.CreateControlBinding(setting).GetType().Name.Should().Be(bindingType);
        }
    }

    [AvaloniaTest]
    public void Bool_binding_should_preserve_effective_and_explicit_value_semantics()
    {
        BoolSetting setting = new("Enabled", defaultValue: true);
        TestSettingsSource effective = new() { SettingLevel = SettingLevel.Effective };
        PluginSettingBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);

        binding.LoadSetting(effective);
        CheckBox control = binding.GetControl().Should().BeOfType<CheckBox>().Subject;
        control.IsThreeState.Should().BeTrue();
        control.IsChecked.Should().BeTrue();
        binding.SaveSetting(effective);
        effective.SetCount.Should().Be(0);

        TestSettingsSource global = new() { SettingLevel = SettingLevel.Global };
        binding.LoadSetting(global);
        control.IsChecked.Should().BeNull();
        control.IsChecked = false;
        binding.SaveSetting(global);
        global.GetValue("Enabled").Should().Be("false");
    }

    [AvaloniaTest]
    public void String_and_password_bindings_should_preserve_null_empty_default_and_trim_semantics()
    {
        StringSettingControlBinding.PlaceholderText = "unset; enter {0} for empty";
        TestSettingsSource effective = new() { SettingLevel = SettingLevel.Effective };
        StringSetting stringSetting = new("Command", "default");
        PluginSettingBinding stringBinding = SettingControlBindingsProvider.CreateControlBinding(stringSetting);
        stringBinding.LoadSetting(effective);
        TextBox stringControl = stringBinding.GetControl().Should().BeOfType<TextBox>().Subject;
        stringControl.Text.Should().Be("default");
        stringControl.PlaceholderText.Should().Be("unset; enter <empty string> for empty");
        stringBinding.SaveSetting(effective);
        effective.SetCount.Should().Be(0);

        TestSettingsSource global = new() { SettingLevel = SettingLevel.Global };
        global.SetValue("Command", string.Empty);
        stringBinding.LoadSetting(global);
        stringControl.Text.Should().Be("<empty string>");
        stringControl.Text = "  fetch --all  ";
        stringBinding.SaveSetting(global);
        global.GetValue("Command").Should().Be("fetch --all");

        PasswordSetting passwordSetting = new("Token", "fallback");
        PluginSettingBinding passwordBinding = SettingControlBindingsProvider.CreateControlBinding(passwordSetting);
        TextBox passwordControl = passwordBinding.GetControl().Should().BeOfType<TextBox>().Subject;
        passwordControl.PasswordChar.Should().Be('\u25CF');
        passwordControl.Text = "<empty string>";
        passwordBinding.SaveSetting(global);
        global.GetValue("Token").Should().Be(string.Empty);
    }

    [AvaloniaTest]
    public void Choice_binding_should_expose_unmatched_values_without_selecting_an_item()
    {
        ChoiceSetting setting = new("Mode", ["one", "two"], "one");
        TestSettingsSource settings = new() { SettingLevel = SettingLevel.Global };
        settings.SetValue("Mode", "legacy");
        PluginSettingBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);

        binding.LoadSetting(settings);

        ComboBox control = binding.GetControl().Should().BeOfType<ComboBox>().Subject;
        control.SelectedIndex.Should().Be(-1);
        control.PlaceholderText.Should().Be("legacy");
        binding.SaveSetting(settings);
        settings.GetValue("Mode").Should().BeNull();
    }

    [AvaloniaTest]
    public void Numeric_up_down_binding_should_preserve_effective_default_and_explicit_null_semantics()
    {
        NumberSettingControlBinding.PlaceholderText = "no value set";
        NumberSetting<int> setting = new("Interval", 42);
        PluginSettingBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        NumericUpDown control = binding.GetControl().Should().BeOfType<NumericUpDown>().Subject;
        TestSettingsSource effective = new() { SettingLevel = SettingLevel.Effective };

        binding.LoadSetting(effective);
        control.Value.Should().Be(42);
        binding.SaveSetting(effective);
        effective.SetCount.Should().Be(0);

        TestSettingsSource global = new() { SettingLevel = SettingLevel.Global };
        binding.LoadSetting(global);
        control.Value.Should().BeNull();
        ToolTip.GetTip(control).Should().Be("no value set");
        binding.SaveSetting(global);
        global.SetCount.Should().Be(1);
        global.GetValue("Interval").Should().BeNull();
    }

    [AvaloniaTest]
    public void Numeric_text_binding_should_validate_and_store_the_same_supported_types()
    {
        NumberSetting<int> setting = new("Interval", 42)
        {
            CustomControl = new WinFormsShims.TextBox(),
        };
        PluginSettingBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        TextBox control = binding.GetControl().Should().BeOfType<TextBox>().Subject;
        TestSettingsSource settings = new() { SettingLevel = SettingLevel.Global };

        control.Text = "invalid";
        Dispatcher.UIThread.RunJobs();
        control.Classes.Should().Contain("plugin-setting-invalid");
        binding.SaveSetting(settings);
        settings.GetValue("Interval").Should().BeNull();

        control.Text = "99";
        Dispatcher.UIThread.RunJobs();
        control.Classes.Should().NotContain("plugin-setting-invalid");
        binding.SaveSetting(settings);
        settings.GetValue("Interval").Should().Be("99");
    }

    [AvaloniaTest]
    public void Credentials_binding_should_disable_and_clear_unsupported_setting_levels()
    {
        CredentialsSetting setting = new("Credentials", "Credentials", () => null);
        setting.CustomControl = new GitExtensions.Extensibility.Settings.UserControls.CredentialsControl
        {
            UserName = "real-user",
            Password = "real-password",
        };
        PluginSettingBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        TestSettingsSource settings = new() { SettingLevel = SettingLevel.Distributed };

        binding.LoadSetting(settings);

        Grid control = binding.GetControl().Should().BeOfType<Grid>().Subject;
        control.IsEnabled.Should().BeFalse();
        control.Children.OfType<TextBox>().Should().OnlyContain(textBox => string.IsNullOrEmpty(textBox.Text));
        setting.CustomControl.UserName.Should().BeEmpty();
        setting.CustomControl.Password.Should().BeEmpty();
    }

    [AvaloniaTest]
    public void Credentials_binding_should_reload_the_queued_value_after_save()
    {
        CredentialsSetting setting = new($"P62-{Guid.NewGuid():N}", "Credentials", () => null);
        PluginSettingBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        TestSettingsSource settings = new() { SettingLevel = SettingLevel.Global };
        Grid control = binding.GetControl().Should().BeOfType<Grid>().Subject;
        TextBox[] fields = control.Children.OfType<TextBox>().ToArray();
        fields[0].Text = "user";
        fields[1].Text = "secret";

        binding.SaveSetting(settings);

        fields[0].Text.Should().Be("user");
        fields[1].Text.Should().Be("secret");
        setting.CustomControl!.UserName.Should().Be("user");
        setting.CustomControl.Password.Should().Be("secret");

        fields[0].Text = string.Empty;
        binding.SaveSetting(settings);
    }

    [AvaloniaTest]
    public void Unsupported_setting_should_keep_the_original_actionable_error()
    {
        Action action = () => SettingControlBindingsProvider.CreateControlBinding(new UnknownSetting());

        action.Should().Throw<NotSupportedException>()
            .WithMessage("*No control binding registered for UnknownSetting.*ISetting.CreateControlBinding*");
    }

    private sealed class TestSettingsSource : SettingsSource
    {
        private readonly Dictionary<string, string?> _values = [];

        internal int SetCount { get; private set; }

        public override string? GetValue(string name) => _values.GetValueOrDefault(name);

        public override void SetValue(string name, string? value)
        {
            SetCount++;
            _values[name] = value;
        }
    }

    private sealed class UnknownSetting : ISetting
    {
        public string Name => "Unknown";

        public string Caption => "Unknown";
    }
}
