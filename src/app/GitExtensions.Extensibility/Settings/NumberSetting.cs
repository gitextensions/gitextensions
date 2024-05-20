namespace GitExtensions.Extensibility.Settings;

public class NumberSetting<T> : ISetting
{
    public NumberSetting(string name, T defaultValue)
        : this(name, name, defaultValue)
    {
    }

    public NumberSetting(string name, string caption, T defaultValue)
    {
        Name = name;
        Caption = caption;
        DefaultValue = defaultValue;
    }

    public string Name { get; }
    public string Caption { get; }
    public T DefaultValue { get; }
    public Control? CustomControl { get; set; }

    public ISettingControlBinding CreateControlBinding()
    {
        if (typeof(T) == typeof(int))
        {
            // It'd be great to write new NumericUpDownBinding((NumberSetting<int>)this, ...) but the compiler doesn't like that,
            // so we are forced to use the !-operator.
            return new NumericUpDownBinding((this as NumberSetting<int>)!, CustomControl as NumericUpDown);
        }
        else
        {
            return new TextBoxBinding(this, CustomControl as TextBox);
        }
    }

    // TODO: honestly, NumericUpDownBinding might be a better choice than TextBox in general since its internal type is `decimal`.
    //       We would just need to appropriately choose an increment based on NumberSetting's type.
    private class NumericUpDownBinding : SettingControlBinding<NumberSetting<int>, NumericUpDown>
    {
        public NumericUpDownBinding(NumberSetting<int> setting, NumericUpDown? customControl)
            : base(setting, customControl)
        {
        }

        public override NumericUpDown CreateControl()
        {
            NumericUpDown numericUpDown = new();

            // TODO: if we need negative values, int.MinValue should be the Minimum.
            //       Or, we can attempt to introduce a NumberSetting<int> constructor that accepts a min and max value parameter.
            numericUpDown.Minimum = 0;
            numericUpDown.Maximum = int.MaxValue;

            Setting.CustomControl = numericUpDown;
            return (NumericUpDown)Setting.CustomControl;
        }

        public override void LoadSetting(SettingsSource settings, NumericUpDown control)
        {
            control.Value = Setting.ValueOrDefault(settings);
        }

        public override void SaveSetting(SettingsSource settings, NumericUpDown control)
        {
            decimal controlValue = control.Value;

            if (Setting.ValueOrDefault(settings) == controlValue)
            {
                return;
            }

            Setting[settings] = controlValue;
        }
    }

    private class TextBoxBinding : SettingControlBinding<NumberSetting<T>, TextBox>
    {
        public TextBoxBinding(NumberSetting<T> setting, TextBox? customControl)
            : base(setting, customControl)
        {
        }

        public override TextBox CreateControl()
        {
            Setting.CustomControl = new TextBox();
            return (TextBox)Setting.CustomControl;
        }

        public override void LoadSetting(SettingsSource settings, TextBox control)
        {
            object? settingVal = settings.SettingLevel == SettingLevel.Effective
                ? Setting.ValueOrDefault(settings)
                : Setting[settings];

            control.Text = ConvertToString(settingVal);
        }

        public override void SaveSetting(SettingsSource settings, TextBox control)
        {
            string controlValue = control.Text;

            if (settings.SettingLevel == SettingLevel.Effective)
            {
                if (ConvertToString(Setting.ValueOrDefault(settings)) == controlValue)
                {
                    return;
                }
            }

            Setting[settings] = ConvertFromString(controlValue);
        }
    }

    private static string ConvertToString(object? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        return value.ToString()!;
    }

    private static object? ConvertFromString(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        Type type = typeof(T);
        if (type == typeof(int))
        {
            return int.Parse(value);
        }

        if (type == typeof(float))
        {
            return float.Parse(value);
        }

        if (type == typeof(double))
        {
            return double.Parse(value);
        }

        if (type == typeof(long))
        {
            return long.Parse(value);
        }

        return null;
    }

    public object? this[SettingsSource settings]
    {
        get
        {
            string? stringValue = settings.GetValue(Name);

            return ConvertFromString(stringValue);
        }

        set
        {
            string? stringValue = ConvertToString(value);

            settings.SetValue(Name, stringValue);
        }
    }

    public T ValueOrDefault(SettingsSource settings)
    {
        object? settingVal = this[settings];
        if (settingVal is null)
        {
            return DefaultValue;
        }
        else
        {
            return (T)settingVal;
        }
    }
}
