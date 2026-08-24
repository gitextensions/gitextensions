using Avalonia.Controls;
using GitExtensions.Extensibility.Settings;
using GitUI.Compat;

namespace GitUI.SettingControlBindings;

internal class NumberSettingTextBoxBinding<T> : SettingControlBinding<NumberSetting<T>, TextBox>
{
    public NumberSettingTextBoxBinding(NumberSetting<T> setting, TextBox? customControl)
        : base(setting, customControl)
    {
        if (customControl is not null)
        {
            customControl.TextChanged += OnTextChanged;
        }
    }

    private static string ConvertToString(object? value)
    {
        return value?.ToString() ?? "";
    }

    public override TextBox CreateControl()
    {
        // Avalonia renders the portable custom-control model through a native control.
        TextBox textBox = PluginSettingControlFactory.CreateTextBox(Setting.CustomControl as GitExtensions.Shims.WinForms.TextBox);
        textBox.TextChanged += OnTextChanged;
        return textBox;
    }

    public override void LoadSetting(SettingsSource settings, TextBox control)
    {
        if (string.IsNullOrEmpty(control.PlaceholderText) && NumberSettingControlBinding.PlaceholderText.Length > 0)
        {
            control.PlaceholderText = NumberSettingControlBinding.PlaceholderText;
        }

        object? settingVal = settings.SettingLevel == SettingLevel.Effective
            ? Setting.ValueOrDefault(settings)
            : Setting[settings];

        control.Text = ConvertToString(settingVal);
    }

    public override void SaveSetting(SettingsSource settings, TextBox control)
    {
        string controlValue = control.Text ?? "";

        if (string.IsNullOrEmpty(controlValue) || !PluginSettingControlFactory.TryConvertNumberFromString<T>(controlValue, out object? parsedValue))
        {
            Setting[settings] = null;
            return;
        }

        if (settings.SettingLevel == SettingLevel.Effective)
        {
            if (ConvertToString(Setting.ValueOrDefault(settings)) == controlValue)
            {
                return;
            }
        }

        Setting[settings] = parsedValue;
    }

    private static void OnTextChanged(object? sender, EventArgs e)
    {
        if (sender is TextBox textBox)
        {
            bool isValid = string.IsNullOrEmpty(textBox.Text) || PluginSettingControlFactory.TryConvertNumberFromString<T>(textBox.Text, out _);
            textBox.Classes.Set("plugin-setting-invalid", !isValid);
        }
    }
}
