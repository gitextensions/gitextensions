namespace GitCommands.Settings;

public interface ISetting<T>
{
    /// <summary>
    /// Name of the setting.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///  Default value for setting type.
    ///  For nullable except "string" is default(T).
    ///  For "string" is the defaultValue ?? string.Empty from constructor.
    ///  For non nullable is the defaultValue from constructor.
    /// </summary>
    T? Default { get; }

    /// <summary>
    ///  Value of the setting.
    ///  For nullable except "string" is the value from storage.
    ///  For "string" is the value from storage or <see cref="Default"/>.
    ///  For non nullable is the value from storage or <see cref="Default"/>.
    /// </summary>
    T? Value { get; set; }

    /// <summary>
    ///  Full name of the setting.
    ///  Includes section name and setting name.
    /// </summary>
    string FullPath { get; }
}
