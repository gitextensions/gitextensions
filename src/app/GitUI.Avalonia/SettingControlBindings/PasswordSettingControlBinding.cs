using Avalonia.Controls;
using GitExtensions.Extensibility.Settings;
using GitUI.Compat;

namespace GitUI.SettingControlBindings;

internal class PasswordSettingControlBinding : SettingControlBinding<PasswordSetting, TextBox>
{
    public PasswordSettingControlBinding(PasswordSetting setting, TextBox? customControl)
        : base(setting, customControl)
    {
    }

    public override TextBox CreateControl()
    {
        // Avalonia renders the portable custom-control model through a native control.
        TextBox control = PluginSettingControlFactory.CreateTextBox(Setting.CustomControl);
        control.PasswordChar = '\u25CF';
        return control;
    }

    public override void LoadSetting(SettingsSource settings, TextBox control)
    {
        if (string.IsNullOrEmpty(control.PlaceholderText) && StringSettingControlBinding.PlaceholderText.Length > 0)
        {
            control.PlaceholderText = string.Format(StringSettingControlBinding.PlaceholderText, StringSettingControlBinding.EmptyStringValue);
        }

        string? settingVal = settings.SettingLevel == SettingLevel.Effective
            ? Setting.ValueOrDefault(settings)
            : Setting[settings];

        if (settingVal is { Length: 0 })
        {
            settingVal = StringSettingControlBinding.EmptyStringValue;
        }

        control.Text = settingVal;
    }

    public override void SaveSetting(SettingsSource settings, TextBox control)
    {
        // Trim value because the XML serializer will trim it on load anyway.
        string? controlValue = (control.Text ?? "").Trim();
        control.Text = controlValue;
        if (controlValue.Length == 0)
        {
            controlValue = null;
        }
        else if (controlValue == StringSettingControlBinding.EmptyStringValue)
        {
            controlValue = "";
        }

        if (settings.SettingLevel == SettingLevel.Effective)
        {
            if (Setting.ValueOrDefault(settings) == controlValue)
            {
                return;
            }
        }

        Setting[settings] = controlValue;
    }
}
