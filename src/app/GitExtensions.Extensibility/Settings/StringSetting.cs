namespace GitExtensions.Extensibility.Settings;

public class StringSetting : ISetting
{
    public StringSetting(string name, string defaultValue)
        : this(name, name, defaultValue)
    {
    }

    public StringSetting(string name, string caption, string defaultValue, bool useDefaultValueIfBlank = false)
    {
        Name = name;
        Caption = caption;
        DefaultValue = defaultValue;
        UseDefaultValueIfBlank = useDefaultValueIfBlank;
    }

    public string Name { get; }
    public string Caption { get; }
    public string DefaultValue { get; }
    public TextBox? CustomControl { get; set; }
    public bool UseDefaultValueIfBlank { get; }

    public ISettingControlBinding CreateControlBinding()
    {
        return new TextBoxBinding(this, CustomControl, UseDefaultValueIfBlank);
    }

    private class TextBoxBinding : SettingControlBinding<StringSetting, TextBox>
    {
        private readonly bool _useDefaultValueIfBlank;

        public TextBoxBinding(StringSetting setting, TextBox? customControl, bool useDefaultValueIfBlank)
            : base(setting, customControl)
        {
            _useDefaultValueIfBlank = useDefaultValueIfBlank;
        }

        public override TextBox CreateControl()
        {
            Setting.CustomControl = new TextBox();
            return Setting.CustomControl;
        }

        public override void LoadSetting(SettingsSource settings, TextBox control)
        {
            if (control.ReadOnly)
            {
                // readonly controls can't be changed by the user, so there is no need to load settings
                return;
            }

            string? settingVal = settings.SettingLevel == SettingLevel.Effective
                ? Setting.ValueOrDefault(settings)
                : Setting[settings];

            if (settingVal is null && _useDefaultValueIfBlank)
            {
                settingVal = Setting.ValueOrDefault(settings);
            }

            // for multiline control, transform "\n" in "\r\n" but prevent "\r\n" to be transformed in "\r\r\n"
            control.Text = control.Multiline
                ? settingVal?.Replace(Environment.NewLine, "\n").Replace("\n", Environment.NewLine)
                : settingVal;
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
