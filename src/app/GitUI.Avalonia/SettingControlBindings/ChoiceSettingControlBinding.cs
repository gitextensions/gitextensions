using Avalonia.Controls;
using GitExtensions.Extensibility.Settings;

namespace GitUI.SettingControlBindings;

internal sealed class ChoiceSettingControlBinding : SettingControlBinding<ChoiceSetting, ComboBox>
{
    public ChoiceSettingControlBinding(ChoiceSetting setting, ComboBox? customControl)
        : base(setting, customControl)
    {
    }

    public override ComboBox CreateControl()
    {
        // Avalonia's non-editable ComboBox is the native DropDownList equivalent.
        ComboBox control = new()
        {
            ItemsSource = Setting.CustomControl?.Items.Count > 0
                ? Setting.CustomControl.Items.Cast<object>().ToArray()
                : Setting.Values.Cast<object>().ToArray(),
        };
        return control;
    }

    public override void LoadSetting(SettingsSource settings, ComboBox control)
    {
        string? settingVal = settings.SettingLevel == SettingLevel.Effective
            ? Setting.ValueOrDefault(settings)
            : Setting[settings];

        control.SelectedIndex = settingVal is null
            ? -1
            : control.Items.Cast<object>().Select(item => item.ToString()).ToList().IndexOf(settingVal);

        if (control.SelectedIndex == -1)
        {
            // Avalonia exposes unmatched non-editable text as placeholder content.
            control.PlaceholderText = settingVal;
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
