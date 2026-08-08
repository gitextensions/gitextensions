using Avalonia.Controls;
using GitExtensions.Extensibility.Settings;

namespace GitUI.SettingControlBindings;

internal abstract class SettingControlBinding<TSetting, TControl> : GitUI.Compat.PluginSettingBinding
    where TControl : Control
    where TSetting : ISetting
{
    private TControl? _control;
    protected readonly TSetting Setting;

    protected SettingControlBinding(TSetting setting, TControl? customControl)
    {
        Setting = setting;
        _control = customControl;
    }

    private TControl BoundControl
    {
        get
        {
            // Avalonia controls do not expose WinForms' IsDisposed lifecycle flag.
            _control ??= CreateControl();
            return _control;
        }
    }

    public override Control GetControl()
    {
        return BoundControl;
    }

    public override void LoadSetting(SettingsSource settings)
    {
        LoadSetting(settings, BoundControl);
    }

    /// <summary>
    /// Saves value from Control to settings
    /// </summary>
    public override void SaveSetting(SettingsSource settings)
    {
        SaveSetting(settings, BoundControl);
    }

    public override string Caption()
    {
        return Setting.Caption;
    }

    public override ISetting GetSetting()
    {
        return Setting;
    }

    /// <summary>
    /// Creates a control to be placed on FormSettings to edit this setting value
    /// Control should take care of scalability and resizability of its sub-controls
    /// </summary>
    public abstract TControl CreateControl();

    /// <summary>
    /// Loads setting value from settings to Control
    /// </summary>
    public abstract void LoadSetting(SettingsSource settings, TControl control);

    /// <summary>
    /// Saves value from Control to settings
    /// </summary>
    public abstract void SaveSetting(SettingsSource settings, TControl control);
}
