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

    // TODO: honestly, NumericUpDownBinding might be a better choice than TextBox in general since its internal type is `decimal`.
    //       We would just need to appropriately choose an increment based on NumberSetting's type.
    internal static bool TryConvertFromString(string value, out object? result)
    {
        Type type = typeof(T);
        if (type == typeof(int) && int.TryParse(value, out int intResult))
        {
            result = intResult;
            return true;
        }

        if (type == typeof(float) && float.TryParse(value, out float floatResult))
        {
            result = floatResult;
            return true;
        }

        if (type == typeof(double) && double.TryParse(value, out double doubleResult))
        {
            result = doubleResult;
            return true;
        }

        if (type == typeof(long) && long.TryParse(value, out long longResult))
        {
            result = longResult;
            return true;
        }

        result = null;
        return false;
    }

    public object? this[SettingsSource settings]
    {
        get
        {
            string? stringValue = settings.GetValue(Name);

            if (string.IsNullOrEmpty(stringValue))
            {
                return null;
            }

            _ = TryConvertFromString(stringValue, out object? result);
            return result;
        }

        set => settings.SetValue(Name, value?.ToString());
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
