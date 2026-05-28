using GitExtensions.Extensibility.Settings;

namespace GitUI.SettingControlBindings;

internal class PseudoSettingControlBinding : SettingControlBinding<PseudoSetting, Control>
{
    public PseudoSettingControlBinding(PseudoSetting setting, Control? customControl)
        : base(setting, customControl)
    {
    }

    public override Control CreateControl()
    {
        ArgumentNullException.ThrowIfNull(Setting.TextBoxCreator);
        Setting.CustomControl = Setting.TextBoxCreator();
        return Setting.CustomControl;
    }

    public override void LoadSetting(SettingsSource settings, Control control)
    {
    }

    public override void SaveSetting(SettingsSource settings, Control control)
    {
    }
}
