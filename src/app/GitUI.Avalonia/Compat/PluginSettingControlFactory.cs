using Avalonia.Controls;
using Avalonia.Media;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

internal enum PluginSettingPlaceholderKind
{
    None,
    Number,
    String,
}

internal abstract class PluginSettingBinding(
    Control control,
    string? caption,
    PluginSettingPlaceholderKind placeholderKind = PluginSettingPlaceholderKind.None)
{
    internal Control Control { get; } = control;

    internal string? Caption { get; } = caption;

    internal void SetPlaceholder(string numberPlaceholder, string stringPlaceholder)
    {
        string? placeholder = placeholderKind switch
        {
            PluginSettingPlaceholderKind.Number => numberPlaceholder,
            PluginSettingPlaceholderKind.String => string.Format(stringPlaceholder, PluginSettingControlFactory.EmptyStringValue),
            _ => null,
        };

        switch (Control)
        {
            case NumericUpDown numericUpDown when placeholder is not null:
                numericUpDown.PlaceholderText = placeholder;
                break;
            case TextBox textBox when placeholder is not null:
                textBox.PlaceholderText = placeholder;
                break;
        }
    }

    internal abstract void Load(SettingsSource settings);

    internal abstract void Save(SettingsSource settings);
}

internal static class PluginSettingControlFactory
{
    internal const string EmptyStringValue = "<empty string>";
    internal const string NumberPlaceholder = "no value set";
    internal const string StringPlaceholder = "no value set; for empty string, enter \"{0}\" without the double quotes";

    internal static PluginSettingBinding Create(ISetting setting)
    {
        ArgumentNullException.ThrowIfNull(setting);

        if (setting.CreateControlBinding() is ISettingControlBinding customBinding)
        {
            return CreateCustomBinding(customBinding);
        }

        return setting switch
        {
            BoolSetting boolSetting => CreateBoolBinding(boolSetting),
            PasswordSetting passwordSetting => CreatePasswordBinding(passwordSetting),
            StringSetting stringSetting => CreateStringBinding(stringSetting),
            ChoiceSetting choiceSetting => CreateChoiceBinding(choiceSetting),
            NumberSetting<int> intSetting => CreateIntBinding(intSetting),
            NumberSetting<float> floatSetting => CreateNumberTextBinding(floatSetting),
            NumberSetting<double> doubleSetting => CreateNumberTextBinding(doubleSetting),
            NumberSetting<long> longSetting => CreateNumberTextBinding(longSetting),
            PseudoSetting pseudoSetting => CreatePseudoBinding(pseudoSetting),
            _ => throw new NotSupportedException(
                $"No Avalonia plugin-setting renderer is registered for {setting.GetType().Name}."),
        };
    }

    private static PluginSettingBinding CreateBoolBinding(BoolSetting setting)
    {
        CheckBox control = new() { IsThreeState = true };
        ApplyModel(setting.CustomControl, control);
        return new DelegateBinding(
            control,
            setting.Caption,
            settings => control.IsChecked = settings.SettingLevel == SettingLevel.Effective
                ? setting.ValueOrDefault(settings)
                : setting[settings],
            settings =>
            {
                bool? value = control.IsChecked;
                if (settings.SettingLevel != SettingLevel.Effective || setting.ValueOrDefault(settings) != value)
                {
                    setting[settings] = value;
                }
            });
    }

    private static PluginSettingBinding CreateStringBinding(StringSetting setting)
    {
        TextBox control = CreateTextBox(setting.CustomControl);
        control.PlaceholderText ??= string.Format(StringPlaceholder, EmptyStringValue);
        return new DelegateBinding(
            control,
            setting.Caption,
            settings =>
            {
                string? value = settings.SettingLevel == SettingLevel.Effective
                    ? setting.ValueOrDefault(settings)
                    : setting[settings];
                control.Text = value is { Length: 0 }
                    ? EmptyStringValue
                    : NormalizeMultiline(value, control.AcceptsReturn);
            },
            settings =>
            {
                string? value = NormalizeStringValue(control);
                if (settings.SettingLevel != SettingLevel.Effective || setting.ValueOrDefault(settings) != value)
                {
                    setting[settings] = value;
                }
            },
            PluginSettingPlaceholderKind.String);
    }

    private static PluginSettingBinding CreatePasswordBinding(PasswordSetting setting)
    {
        TextBox control = CreateTextBox(setting.CustomControl);
        control.PasswordChar = '\u25CF';
        control.PlaceholderText ??= string.Format(StringPlaceholder, EmptyStringValue);
        return new DelegateBinding(
            control,
            setting.Caption,
            settings =>
            {
                string? value = settings.SettingLevel == SettingLevel.Effective
                    ? setting.ValueOrDefault(settings)
                    : setting[settings];
                control.Text = value is { Length: 0 } ? EmptyStringValue : value;
            },
            settings =>
            {
                string? value = NormalizeStringValue(control);
                if (settings.SettingLevel != SettingLevel.Effective || setting.ValueOrDefault(settings) != value)
                {
                    setting[settings] = value;
                }
            },
            PluginSettingPlaceholderKind.String);
    }

    private static PluginSettingBinding CreateChoiceBinding(ChoiceSetting setting)
    {
        ComboBox control = new()
        {
            ItemsSource = setting.CustomControl?.Items.Count > 0
                ? setting.CustomControl.Items.Cast<object>().ToArray()
                : setting.Values.Cast<object>().ToArray(),
        };
        return new DelegateBinding(
            control,
            setting.Caption,
            settings =>
            {
                string? value = settings.SettingLevel == SettingLevel.Effective
                    ? setting.ValueOrDefault(settings)
                    : setting[settings];
                object? match = control.Items.OfType<object>()
                    .FirstOrDefault(item => string.Equals(item.ToString(), value, StringComparison.Ordinal));
                control.SelectedItem = match;
            },
            settings =>
            {
                string? value = control.SelectedItem?.ToString();
                if (settings.SettingLevel != SettingLevel.Effective || setting.ValueOrDefault(settings) != value)
                {
                    setting[settings] = value;
                }
            });
    }

    private static PluginSettingBinding CreateIntBinding(NumberSetting<int> setting)
    {
        if (setting.CustomControl is WinFormsShims.TextBox customTextBox)
        {
            return CreateNumberTextBinding(setting, customTextBox);
        }

        NumericUpDown control = new()
        {
            Minimum = 0,
            Maximum = int.MaxValue,
            Increment = 1,
            PlaceholderText = NumberPlaceholder,
        };
        return new DelegateBinding(
            control,
            setting.Caption,
            settings =>
            {
                object? value = settings.SettingLevel == SettingLevel.Effective
                    ? setting.ValueOrDefault(settings)
                    : setting[settings];
                control.Value = value is null ? null : Convert.ToDecimal(value);
            },
            settings =>
            {
                int? value = control.Value is decimal number ? decimal.ToInt32(number) : null;
                if (settings.SettingLevel != SettingLevel.Effective || value is null || setting.ValueOrDefault(settings) != value)
                {
                    setting[settings] = value;
                }
            },
            PluginSettingPlaceholderKind.Number);
    }

    private static PluginSettingBinding CreateNumberTextBinding<T>(
        NumberSetting<T> setting,
        WinFormsShims.TextBox? model = null)
    {
        TextBox control = CreateTextBox(model ?? setting.CustomControl as WinFormsShims.TextBox);
        control.PlaceholderText ??= NumberPlaceholder;
        void Validate()
        {
            bool valid = string.IsNullOrEmpty(control.Text) || TryParseNumber(control.Text, out T? _);
            control.Classes.Set("plugin-setting-invalid", !valid);
        }

        control.TextChanged += (_, _) => Validate();
        return new DelegateBinding(
            control,
            setting.Caption,
            settings =>
            {
                object? value = settings.SettingLevel == SettingLevel.Effective
                    ? setting.ValueOrDefault(settings)
                    : setting[settings];
                control.Text = value?.ToString() ?? string.Empty;
                Validate();
            },
            settings =>
            {
                object? value = TryParseNumber(control.Text, out T? parsed) ? parsed : null;
                if (settings.SettingLevel != SettingLevel.Effective
                    || value is null
                    || !Equals(setting.ValueOrDefault(settings), value))
                {
                    setting[settings] = value;
                }
            },
            PluginSettingPlaceholderKind.Number);
    }

    private static PluginSettingBinding CreatePseudoBinding(PseudoSetting setting)
    {
        WinFormsShims.Control model = setting.CustomControl
            ?? setting.TextBoxCreator?.Invoke()
            ?? throw new InvalidOperationException("Pseudo setting did not supply a control model.");
        ShimControlAdapter adapter = ShimControlAdapter.Create(model);
        return new DelegateBinding(
            adapter.Control,
            setting.Caption,
            _ => adapter.Load(),
            _ => adapter.Save());
    }

    private static PluginSettingBinding CreateCustomBinding(ISettingControlBinding binding)
    {
        ShimControlAdapter adapter = ShimControlAdapter.Create(binding.GetControl());
        return new DelegateBinding(
            adapter.Control,
            binding.Caption(),
            settings =>
            {
                binding.LoadSetting(settings);
                adapter.Load();
            },
            settings =>
            {
                adapter.Save();
                binding.SaveSetting(settings);
            });
    }

    private static TextBox CreateTextBox(WinFormsShims.TextBox? model)
    {
        TextBox control = new();
        if (model is null)
        {
            return control;
        }

        control.Text = model.Text;
        control.IsReadOnly = model.ReadOnly;
        control.AcceptsReturn = model.Multiline;
        control.TextWrapping = model.Multiline ? TextWrapping.Wrap : TextWrapping.NoWrap;
        if (model.Height > 0)
        {
            control.Height = model.Height;
        }

        if (model.BorderStyle == WinFormsShims.BorderStyle.None)
        {
            control.BorderThickness = new Avalonia.Thickness(0);
        }

        return control;
    }

    private static void ApplyModel(WinFormsShims.CheckBox? model, CheckBox control)
    {
        if (model is null)
        {
            return;
        }

        control.Content = string.IsNullOrEmpty(model.Text) ? null : model.Text;
        control.IsChecked = model.CheckState switch
        {
            WinFormsShims.CheckState.Checked => true,
            WinFormsShims.CheckState.Indeterminate => null,
            _ => false,
        };
    }

    private static string? NormalizeStringValue(TextBox control)
    {
        string value = (control.Text ?? string.Empty).Trim();
        control.Text = value;
        return value switch
        {
            "" => null,
            EmptyStringValue => string.Empty,
            _ => value,
        };
    }

    private static string? NormalizeMultiline(string? value, bool multiline)
        => multiline
            ? value?.Replace(Environment.NewLine, "\n").Replace("\n", Environment.NewLine)
            : value;

    private static bool TryParseNumber<T>(string? value, out T? result)
    {
        object? parsed;
        if (typeof(T) == typeof(int) && int.TryParse(value, out int intValue))
        {
            parsed = intValue;
        }
        else if (typeof(T) == typeof(float) && float.TryParse(value, out float floatValue))
        {
            parsed = floatValue;
        }
        else if (typeof(T) == typeof(double) && double.TryParse(value, out double doubleValue))
        {
            parsed = doubleValue;
        }
        else if (typeof(T) == typeof(long) && long.TryParse(value, out long longValue))
        {
            parsed = longValue;
        }
        else
        {
            result = default;
            return false;
        }

        result = (T)parsed;
        return true;
    }

    private sealed class DelegateBinding(
        Control control,
        string? caption,
        Action<SettingsSource> load,
        Action<SettingsSource> save,
        PluginSettingPlaceholderKind placeholderKind = PluginSettingPlaceholderKind.None)
        : PluginSettingBinding(control, caption, placeholderKind)
    {
        internal override void Load(SettingsSource settings) => load(settings);

        internal override void Save(SettingsSource settings) => save(settings);
    }

    private abstract class ShimControlAdapter(Control control)
    {
        internal Control Control { get; } = control;

        internal abstract void Load();

        internal abstract void Save();

        internal static ShimControlAdapter Create(WinFormsShims.Control model)
            => model switch
            {
                WinFormsShims.TextBox textBox => new TextBoxAdapter(textBox),
                WinFormsShims.CheckBox checkBox => new CheckBoxAdapter(checkBox),
                WinFormsShims.ComboBox comboBox => new ComboBoxAdapter(comboBox),
                _ => throw new NotSupportedException(
                    $"No Avalonia plugin-setting control adapter is registered for {model.GetType().Name}."),
            };
    }

    private sealed class TextBoxAdapter(WinFormsShims.TextBox model)
        : ShimControlAdapter(CreateTextBox(model))
    {
        private TextBox TextBox => (TextBox)Control;

        internal override void Load() => TextBox.Text = model.Text;

        internal override void Save() => model.Text = TextBox.Text ?? string.Empty;
    }

    private sealed class CheckBoxAdapter(WinFormsShims.CheckBox model)
        : ShimControlAdapter(new CheckBox { IsThreeState = true })
    {
        private CheckBox CheckBox => (CheckBox)Control;

        internal override void Load()
        {
            ApplyModel(model, CheckBox);
        }

        internal override void Save()
        {
            model.CheckState = CheckBox.IsChecked switch
            {
                true => WinFormsShims.CheckState.Checked,
                false => WinFormsShims.CheckState.Unchecked,
                null => WinFormsShims.CheckState.Indeterminate,
            };
        }
    }

    private sealed class ComboBoxAdapter(WinFormsShims.ComboBox model)
        : ShimControlAdapter(new ComboBox { ItemsSource = model.Items.ToArray() })
    {
        private ComboBox ComboBox => (ComboBox)Control;

        internal override void Load() => ComboBox.SelectedIndex = model.SelectedIndex;

        internal override void Save() => model.SelectedIndex = ComboBox.SelectedIndex;
    }
}
