namespace GitCommands.Settings;

/// <summary>
///  Internal interface for reading and writing setting values from storage.
///  Implemented by concrete setting types such as <c>SettingOf&lt;T&gt;</c>.
/// </summary>
/// <typeparam name="T">The type of the setting value.</typeparam>
internal interface ISettingAccessor<T>
{
    /// <summary>
    ///  Reads the current value from storage, applying any type conversion.
    ///  For non-nullable types, returns the setting's default when no value is stored.
    ///  For nullable types (e.g. <c>bool?</c>), may return <see langword="null"/>.
    /// </summary>
    T GetValue();

    /// <summary>
    ///  Writes a value to storage, applying any type conversion.
    /// </summary>
    void SetValue(T? value);

    /// <summary>
    ///  Gets whether the setting has no value in storage.
    ///  For nullable types (except string), always returns <see langword="false"/> because null is a valid value.
    ///  For string and non-nullable types, returns <see langword="true"/> when no value is stored.
    /// </summary>
    bool IsUnset { get; }
}
