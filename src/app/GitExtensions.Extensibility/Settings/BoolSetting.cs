namespace GitExtensions.Extensibility.Settings;

public class BoolSetting : ISetting
{
    public BoolSetting(string name, bool defaultValue)
        : this(name, name, defaultValue)
    {
    }

    public BoolSetting(string name, string caption, bool defaultValue)
    {
        Name = name;
        Caption = caption;
        DefaultValue = defaultValue;
    }

    public string Name { get; }
    public string Caption { get; }
    public bool DefaultValue { get; }
    public CheckBox? CustomControl { get; set; }

    public bool? this[SettingsSource settings]
    {
        get => settings.GetBool(Name);

        set => settings.SetBool(Name, value);
    }

    public bool ValueOrDefault(SettingsSource settings)
    {
        return this[settings] ?? DefaultValue;
    }
}
