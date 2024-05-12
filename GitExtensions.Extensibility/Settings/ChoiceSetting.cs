namespace GitExtensions.Extensibility.Settings;

public class ChoiceSetting : ISetting
{
    public ChoiceSetting(string name, IList<string> values, string? defaultValue = null)
        : this(name, name, values, defaultValue)
    {
    }

    public ChoiceSetting(string name, string caption, IList<string> values, string? defaultValue = null)
    {
        Name = name;
        Caption = caption;
        DefaultValue = defaultValue;
        Values = values;
        if (DefaultValue is null && values.Any())
        {
            DefaultValue = values[0];
        }
    }

    public string Name { get; }
    public string Caption { get; }
    public string? DefaultValue { get; }
    public IList<string> Values { get; }
    public ComboBox? CustomControl { get; set; }

    public ISettingControlBinding CreateControlBinding()
    {
        return new ComboBoxBinding(this, CustomControl);
    }

    private class ComboBoxBinding : SettingControlBinding<ChoiceSetting, ComboBox>
    {
        public ComboBoxBinding(ChoiceSetting setting, ComboBox? customControl)
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
            string controlValue = control.SelectedItem?.ToString();
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

    public string? ValueOrDefault(SettingsSource settings)
    {
        return this[settings] ?? DefaultValue;
    }

    public string? this[SettingsSource settings]
    {
        get => settings.GetString(Name, null);
        set => settings.SetString(Name, value);
    }
}
