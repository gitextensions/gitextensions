using GitExtensions.Extensibility.Settings;
using GitExtUtils.GitUI.Theming;

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
        TextBox textBox = new();
        textBox.TextChanged += OnTextChanged;
        Setting.CustomControl = textBox;
        return textBox;
    }

    public override void LoadSetting(SettingsSource settings, TextBox control)
    {
        if (control.PlaceholderText.Length == 0 && NumberSettingControlBinding.PlaceholderText.Length > 0)
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
        string controlValue = control.Text;

        if (string.IsNullOrEmpty(controlValue) || !NumberSetting<T>.TryConvertFromString(controlValue, out object? parsedValue))
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
            bool isValid = string.IsNullOrEmpty(textBox.Text) || NumberSetting<T>.TryConvertFromString(textBox.Text, out _);
            textBox.BackColor = isValid ? SystemColors.Window : OtherColors.BrightRed;
            textBox.ForeColor = isValid ? SystemColors.WindowText : ColorHelper.GetTextColor(OtherColors.BrightRed);
        }
    }
}
