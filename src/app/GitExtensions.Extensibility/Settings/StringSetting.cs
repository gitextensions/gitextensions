namespace GitExtensions.Extensibility.Settings;

public class StringSetting : ISetting
{
    public StringSetting(string name, string defaultValue)
        : this(name, name, defaultValue)
    {
    }

    public StringSetting(string name, string caption, string defaultValue)
    {
        Name = name;
        Caption = caption;
        DefaultValue = defaultValue;
    }

    public string Name { get; }
    public string Caption { get; }
    public string DefaultValue { get; }
    public TextBox? CustomControl { get; set; }

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
