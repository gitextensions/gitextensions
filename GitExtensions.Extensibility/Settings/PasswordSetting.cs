namespace GitExtensions.Extensibility.Settings;

public class PasswordSetting : ISetting
{
    public PasswordSetting(string name, string defaultValue)
        : this(name, name, defaultValue)
    {
    }

    public PasswordSetting(string name, string caption, string defaultValue)
    {
        Name = name;
        Caption = caption;
        DefaultValue = defaultValue;
    }

    public string Name { get; }
    public string Caption { get; }
    public string DefaultValue { get; }
    public TextBox? CustomControl { get; set; }

    public ISettingControlBinding CreateControlBinding()
    {
        return new TextBoxBinding(this, CustomControl);
    }

    private class TextBoxBinding : SettingControlBinding<PasswordSetting, TextBox>
    {
        public TextBoxBinding(PasswordSetting setting, TextBox? customControl)
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
            string? settingVal = settings.SettingLevel == SettingLevel.Effective
                ? Setting.ValueOrDefault(settings)
                : Setting[settings];

            control.Text = settingVal;
        }

        public override void SaveSetting(SettingsSource settings, TextBox control)
        {
            string controlValue = control.Text;
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

    public string? this[SettingsSource settings]
    {
        get => settings.GetString(Name, null);
        set => settings.SetString(Name, value);
    }

    public string ValueOrDefault(SettingsSource settings)
    {
        return this[settings] ?? DefaultValue;
    }
}
