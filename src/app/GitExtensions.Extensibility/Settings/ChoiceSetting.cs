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
