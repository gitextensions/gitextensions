namespace GitExtensions.Extensibility.Settings;

public interface ISetting
{
    /// <summary>
    /// Name of the setting
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Caption of the setting
    /// </summary>
    string Caption { get; }

    ISettingControlBinding CreateControlBinding();
}
