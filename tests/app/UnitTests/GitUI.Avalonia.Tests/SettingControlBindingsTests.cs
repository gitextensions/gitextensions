using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using GitExtensions.Extensibility.Settings;
using GitExtensions.ParityCapture;
using GitUI.Compat;
using GitUI.SettingControlBindings;
using GitUI.Theming;
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
        control.Height.Should().Be(24);
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
        stringControl.Height.Should().Be(23);
        stringControl.Classes.Should().Contain("plugin-setting-text");
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
        ToolTip.GetTip(control).Should().BeOfType<ToolTip>().Which.Content.Should().Be("no value set");
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
        control.Classes.Should().Contain("plugin-setting-number");
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
    public void Numeric_text_bindings_should_parse_every_original_supported_type()
    {
        AssertNumberSaved(new NumberSetting<float>("Float", 1), "1.25", 1.25f);
        AssertNumberSaved(new NumberSetting<double>("Double", 1), "2.5", 2.5d);
        AssertNumberSaved(new NumberSetting<long>("Long", 1), "9223372036854775806", 9223372036854775806L);

        static void AssertNumberSaved<T>(NumberSetting<T> setting, string text, T expected)
        {
            TestSettingsSource settings = new() { SettingLevel = SettingLevel.Global };
            PluginSettingBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting, control: null);
            TextBox control = binding.GetControl().Should().BeOfType<TextBox>().Subject;
            control.Text = text;

            binding.SaveSetting(settings);

            setting[settings].Should().Be(expected);
            control.Classes.Should().NotContain("plugin-setting-invalid");
        }
    }

    [AvaloniaTest]
    public void Portable_custom_control_models_should_preserve_the_original_control_contract()
    {
        WinFormsShims.TextBox textModel = new()
        {
            BorderStyle = WinFormsShims.BorderStyle.None,
            Height = 40,
            Multiline = true,
            ReadOnly = true,
            Text = "model text",
        };
        StringSetting stringSetting = new("Custom", "default") { CustomControl = textModel };

        TextBox textControl = SettingControlBindingsProvider.CreateControlBinding(stringSetting)
            .GetControl().Should().BeOfType<TextBox>().Subject;

        textControl.Text.Should().Be("model text");
        textControl.IsReadOnly.Should().BeTrue();
        textControl.AcceptsReturn.Should().BeTrue();
        textControl.Height.Should().Be(40);
        textControl.BorderThickness.Should().Be(new Avalonia.Thickness(0));

        WinFormsShims.CheckBox checkModel = new()
        {
            CheckState = WinFormsShims.CheckState.Indeterminate,
            Text = "Tri-state",
        };
        BoolSetting boolSetting = new("TriState", defaultValue: false) { CustomControl = checkModel };
        CheckBox checkControl = SettingControlBindingsProvider.CreateControlBinding(boolSetting)
            .GetControl().Should().BeOfType<CheckBox>().Subject;

        checkControl.Content.Should().Be("Tri-state");
        checkControl.IsChecked.Should().BeNull();
        checkControl.IsThreeState.Should().BeTrue();
    }

    [AvaloniaTest]
    public void Pseudo_binding_should_round_trip_the_portable_model_through_its_native_adapter()
    {
        WinFormsShims.TextBox model = new() { Text = "initial" };
        PseudoSetting setting = new(model);
        PluginSettingBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        TextBox control = binding.GetControl().Should().BeOfType<TextBox>().Subject;
        TestSettingsSource settings = new();

        control.Text = "native edit";
        binding.SaveSetting(settings);
        model.Text.Should().Be("native edit");

        model.Text = "model update";
        binding.LoadSetting(settings);
        control.Text.Should().Be("model update");
    }

    [AvaloniaTest]
    public void Paired_capture_surfaces_should_expose_all_binding_controls_and_edge_states()
    {
        SettingControlBindingsCaptureSurface normal = new();
        SettingControlBindingsNullCaptureSurface edge = new();
        Dispatcher.UIThread.RunJobs();
        string[] names =
        [
            "boolControl",
            "choiceControl",
            "stringControl",
            "passwordControl",
            "numberControl",
            "numberTextControl",
            "credentialsControl",
            "pseudoControl",
        ];

        normal.Children.OfType<Grid>().Single().Children.OfType<TextBlock>().Should().HaveCount(8);
        names.Should().OnlyContain(name => normal.GetLogicalDescendants().OfType<Control>().Any(control => control.Name == name));
        Find<CheckBox>(normal, "boolControl").IsChecked.Should().BeTrue();
        Find<ComboBox>(normal, "choiceControl").SelectedItem.Should().Be("two");
        Find<TextBox>(normal, "numberTextControl").Text.Should().Be("1.5");

        Find<CheckBox>(edge, "boolControl").IsChecked.Should().BeNull();
        Find<NumericUpDown>(edge, "numberControl").Value.Should().BeNull();
        Find<TextBox>(edge, "numberTextControl").Text.Should().Be("invalid");
        Find<Grid>(edge, "credentialsControl").IsEnabled.Should().BeFalse();

        static T Find<T>(Grid surface, string name) where T : Control
            => surface.GetLogicalDescendants().OfType<T>().Single(control => control.Name == name);
    }

    [AvaloniaTest]
    public void Capture_reader_should_emit_the_source_setting_control_layout_contract()
    {
        AvaloniaThemeResources.Apply(Application.Current!, ThemeModule.Settings);
        SettingControlBindingsCaptureSurface surface = new();
        Window window = new() { Width = 800, Height = 320, Content = surface };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        CaptureNode root;
        try
        {
            root = new AvaloniaControlTreeReader(surface, renderScale: 1)
                .ReadPrimary(surface, new PixelSize(800, 320)).Root;
        }
        finally
        {
            window.Close();
        }

        CaptureNode layout = root.Children.Should().ContainSingle().Subject;
        IReadOnlyDictionary<string, CaptureNode> nodes = Flatten(root)
            .Where(node => node.FieldName is not null || node.Name is not null)
            .GroupBy(node => node.FieldName ?? node.Name!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        root.Padding.Dip.Should().Be(new CaptureThicknessF { Left = 12, Top = 12, Right = 12, Bottom = 12 });
        layout.FieldName.Should().Be("_layout");
        layout.Type.Should().Be("System.Windows.Forms.TableLayoutPanel");
        layout.BoundsDip.Should().Be(new CaptureRectangleF { X = 12, Y = 12, Width = 776, Height = 296 });
        nodes["boolControl"].BoundsDip.Should().Be(new CaptureRectangleF { X = 173, Y = 3, Width = 600, Height = 24 });
        nodes["choiceControl"].BoundsDip.Should().Be(new CaptureRectangleF { X = 173, Y = 33, Width = 600, Height = 23 });
        nodes["stringControl"].BoundsDip.Should().Be(new CaptureRectangleF { X = 173, Y = 62, Width = 600, Height = 23 });
        nodes["_parent"].Name.Should().Be("numberControl");
        nodes["_parent"].BoundsDip.Should().Be(new CaptureRectangleF { X = 173, Y = 120, Width = 600, Height = 23 });
        nodes["credentialsControl"].BoundsDip.Should().Be(new CaptureRectangleF { X = 173, Y = 178, Width = 600, Height = 24 });
        nodes["mainTableLayoutPanel"].Children.Select(node => node.FieldName).Should().Equal(
            "userNameLabel",
            "userNameTextBox",
            "passwordTextBox",
            "passwordLabel");
        CaptureRectangleF userNameBounds = nodes["userNameTextBox"].BoundsDip;
        userNameBounds.Y.Should().Be(0);
        userNameBounds.Height.Should().Be(23);
        (userNameBounds.X + userNameBounds.Width).Should().Be(269);
        if (OperatingSystem.IsWindows())
        {
            // The AutoSize column begins at the real WinForms TextRenderer width on Windows;
            // other platforms retain their native UI font while preserving the fill boundary.
            userNameBounds.X.Should().Be(71);
            userNameBounds.Width.Should().Be(198);
        }

        CaptureRectangleF passwordBounds = nodes["passwordTextBox"].BoundsDip;
        passwordBounds.Y.Should().Be(0);
        passwordBounds.Height.Should().Be(23);
        (passwordBounds.X + passwordBounds.Width).Should().Be(600);
        if (OperatingSystem.IsWindows())
        {
            passwordBounds.X.Should().Be(398);
            passwordBounds.Width.Should().Be(202);
        }

        nodes["pseudoControl"].BoundsDip.Should().Be(new CaptureRectangleF { X = 173, Y = 230, Width = 600, Height = 40 });
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
        control.Height.Should().Be(24);
        Grid layout = control.GetLogicalDescendants().OfType<Grid>().Single(grid => grid.Name == "mainTableLayoutPanel");
        layout.Height.Should().Be(24);
        layout.GetLogicalChildren().OfType<Control>().Select(child => child.Name).Should().Equal(
            "userNameLabel",
            "userNameTextBox",
            "passwordTextBox",
            "passwordLabel");
        control.GetLogicalDescendants().OfType<TextBox>().Should().OnlyContain(textBox => textBox.Height == 23);
        control.GetLogicalDescendants().OfType<Control>().Select(child => child.Name).Should().Contain(
            "userNameLabel",
            "userNameTextBox",
            "passwordLabel",
            "passwordTextBox");
        control.IsEnabled.Should().BeFalse();
        control.GetLogicalDescendants().OfType<TextBox>().Should().OnlyContain(textBox => string.IsNullOrEmpty(textBox.Text));
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
        TextBox[] fields = control.GetLogicalDescendants().OfType<TextBox>().ToArray();
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

    private static IEnumerable<CaptureNode> Flatten(CaptureNode root)
    {
        yield return root;
        foreach (CaptureNode child in root.Children)
        {
            foreach (CaptureNode descendant in Flatten(child))
            {
                yield return descendant;
            }
        }
    }

    private sealed class UnknownSetting : ISetting
    {
        public string Name => "Unknown";

        public string Caption => "Unknown";
    }
}
