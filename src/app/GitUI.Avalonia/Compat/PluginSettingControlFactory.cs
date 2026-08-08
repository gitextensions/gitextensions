using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitUI.SettingControlBindings;
using ExtensibilityBinding = GitExtensions.Extensibility.Settings.ISettingControlBinding;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

/// <summary>
/// Provides the native Avalonia control boundary used by setting-control bindings.
/// </summary>
public abstract class PluginSettingBinding
{
    public abstract Control GetControl();

    public abstract void LoadSetting(SettingsSource settings);

    public abstract void SaveSetting(SettingsSource settings);

    public abstract string? Caption();

    public abstract ISetting GetSetting();

    internal Control Control => GetControl();

    internal void Load(SettingsSource settings) => LoadSetting(settings);

    internal void Save(SettingsSource settings) => SaveSetting(settings);

    internal void SetPlaceholder(string numberPlaceholder, string stringPlaceholder)
    {
        NumberSettingControlBinding.PlaceholderText = numberPlaceholder;
        StringSettingControlBinding.PlaceholderText = stringPlaceholder;

        switch (GetControl())
        {
            case NumericUpDown numericUpDown when GetSetting() is NumberSetting<int>:
                numericUpDown.PlaceholderText = numberPlaceholder;
                break;
            case TextBox textBox when GetSetting().GetType().IsGenericType
                && GetSetting().GetType().GetGenericTypeDefinition() == typeof(NumberSetting<>):
                textBox.PlaceholderText = numberPlaceholder;
                break;
            case TextBox textBox when GetSetting() is StringSetting or PasswordSetting:
                textBox.PlaceholderText = string.Format(stringPlaceholder, StringSettingControlBinding.EmptyStringValue);
                break;
        }
    }
}

internal static class PluginSettingControlFactory
{
    internal const string EmptyStringValue = StringSettingControlBinding.EmptyStringValue;
    internal const string NumberPlaceholder = "no value set";
    internal const string StringPlaceholder = "no value set; for empty string, enter \"{0}\" without the double quotes";

    internal static PluginSettingBinding Create(ISetting setting)
    {
        ArgumentNullException.ThrowIfNull(setting);
        return SettingControlBindingsProvider.CreateControlBinding(setting);
    }

    internal static PluginSettingBinding Create(ExtensibilityBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);
        return new CustomPluginSettingBinding(binding);
    }

    internal static TextBox CreateTextBox(WinFormsShims.TextBox? model)
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

    internal static void ApplyModel(WinFormsShims.CheckBox? model, CheckBox control)
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

    internal static ShimControlAdapter CreateAdapter(WinFormsShims.Control model)
        => model switch
        {
            WinFormsShims.TextBox textBox => new TextBoxAdapter(textBox),
            WinFormsShims.CheckBox checkBox => new CheckBoxAdapter(checkBox),
            WinFormsShims.ComboBox comboBox => new ComboBoxAdapter(comboBox),
            WinFormsShims.LinkLabel linkLabel => new LinkLabelAdapter(linkLabel),
            _ => throw new NotSupportedException(
                $"No Avalonia plugin-setting control adapter is registered for {model.GetType().Name}."),
        };

    internal abstract class ShimControlAdapter(Control control)
    {
        internal Control Control { get; } = control;

        internal abstract void Load();

        internal abstract void Save();
    }

    private sealed class CustomPluginSettingBinding : PluginSettingBinding
    {
        private readonly ExtensibilityBinding _binding;
        private readonly ShimControlAdapter _adapter;

        internal CustomPluginSettingBinding(ExtensibilityBinding binding)
        {
            _binding = binding;
            _adapter = CreateAdapter(binding.GetControl());
        }

        public override Control GetControl() => _adapter.Control;

        public override void LoadSetting(SettingsSource settings)
        {
            _binding.LoadSetting(settings);
            _adapter.Load();
        }

        public override void SaveSetting(SettingsSource settings)
        {
            _adapter.Save();
            _binding.SaveSetting(settings);
        }

        public override string? Caption() => _binding.Caption();

        public override ISetting GetSetting() => _binding.GetSetting();
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

    private sealed class LinkLabelAdapter(WinFormsShims.LinkLabel model)
        : ShimControlAdapter(new HyperlinkButton())
    {
        private HyperlinkButton LinkLabel => (HyperlinkButton)Control;

        internal override void Load()
        {
            LinkLabel.Content = model.Text;
            LinkLabel.Click -= LinkLabel_Click;
            LinkLabel.Click += LinkLabel_Click;
        }

        internal override void Save()
        {
        }

        private void LinkLabel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            model.PerformClick();
        }
    }
}
