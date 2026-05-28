using GitExtensions.Extensibility.Settings;

namespace GitUI.SettingControlBindings;

internal class ChoiceSettingControlBinding : SettingControlBinding<ChoiceSetting, ComboBox>
{
    public ChoiceSettingControlBinding(ChoiceSetting setting, ComboBox? customControl)
        : base(setting, customControl)
    {
    }

    public override ComboBox CreateControl()
    {
        Setting.CustomControl = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        Setting.CustomControl.Items.AddRange(Setting.Values.ToArray());
        return Setting.CustomControl;
    }

    public override void LoadSetting(SettingsSource settings, ComboBox control)
    {
        string? settingVal = settings.SettingLevel == SettingLevel.Effective
            ? Setting.ValueOrDefault(settings)
            : Setting[settings];

        control.SelectedIndex = settingVal is null ? -1 : Setting.Values.IndexOf(settingVal);

        if (control.SelectedIndex == -1)
        {
            control.Text = settingVal;
        }
    }

    public override void SaveSetting(SettingsSource settings, ComboBox control)
    {
        string? controlValue = control.SelectedItem?.ToString();
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
