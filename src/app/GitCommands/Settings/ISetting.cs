namespace GitCommands.Settings;

public interface ISetting<T>
{
    /// <summary>
    /// Name of the setting.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///  Default value for the setting.
    /// </summary>
    T Default { get; }

    /// <summary>
    ///  Value of the setting.
    ///  Is the value from storage, or <see cref="Default"/> if absent.
    /// </summary>
    T Value { get; set; }

    /// <summary>
    ///  Full name of the setting.
    ///  Includes section name and setting name.
    /// </summary>
    string FullPath { get; }
}
