namespace GitExtensions.Extensibility.Settings;

public abstract class SettingControlBinding<TSetting, TControl> : ISettingControlBinding where TControl : Control where TSetting : ISetting
{
    private TControl? _control;
    protected readonly TSetting Setting;

    protected SettingControlBinding(TSetting setting, TControl? customControl)
    {
        Setting = setting;
        _control = customControl;
    }

    private TControl Control
    {
        get
        {
            if (_control?.IsDisposed is not false)
            {
                _control = CreateControl();
            }

            return _control;
        }
    }

    public Control GetControl()
    {
        return Control;
    }

    public void LoadSetting(SettingsSource settings)
    {
        LoadSetting(settings, Control);
    }

    /// <summary>
    /// Saves value from Control to settings
    /// </summary>
    public void SaveSetting(SettingsSource settings)
    {
        SaveSetting(settings, Control);
    }

    public virtual string Caption()
    {
        return Setting.Caption;
    }

    public ISetting GetSetting()
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
