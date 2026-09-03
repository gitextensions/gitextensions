using Avalonia.Controls;
using GitExtensions.Extensibility.Settings;
using GitUI.Compat;

namespace GitUI.SettingControlBindings;

internal sealed class BoolSettingControlBinding : SettingControlBinding<BoolSetting, CheckBox>
{
    public BoolSettingControlBinding(BoolSetting setting, CheckBox? customControl)
        : base(setting, customControl)
    {
    }

    public override CheckBox CreateControl()
    {
        // Avalonia renders the portable custom-control model through a native control.
        CheckBox control = new() { IsThreeState = true };
        PluginSettingControlFactory.ApplyModel(Setting.CustomControl, control);
        return control;
    }

    public override void LoadSetting(SettingsSource settings, CheckBox control)
    {
        bool? settingVal = settings.SettingLevel == SettingLevel.Effective
            ? Setting.ValueOrDefault(settings)
            : Setting[settings];

        control.IsChecked = settingVal;
    }

    public override void SaveSetting(SettingsSource settings, CheckBox control)
    {
        bool? controlValue = control.IsChecked;
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
