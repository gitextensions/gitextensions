using GitExtensions.Extensibility.Settings;

namespace GitUI.SettingControlBindings;

internal class PasswordSettingControlBinding : SettingControlBinding<PasswordSetting, TextBox>
{
    public PasswordSettingControlBinding(PasswordSetting setting, TextBox? customControl)
        : base(setting, customControl)
    {
    }

    public override TextBox CreateControl()
    {
        Setting.CustomControl = new TextBox { PasswordChar = '\u25CF' };
        return Setting.CustomControl;
    }

    public override void LoadSetting(SettingsSource settings, TextBox control)
    {
        if (control.PlaceholderText.Length == 0 && StringSettingControlBinding.PlaceholderText.Length > 0)
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
        string? controlValue = control.Text.Trim();
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
