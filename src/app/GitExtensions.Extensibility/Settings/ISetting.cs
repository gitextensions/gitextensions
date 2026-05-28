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

    /// <summary>
    ///  Creates a new binding between a setting and a user interface control.
    /// </summary>
    /// <remarks>
    ///  Use this method to establish a connection between a configuration setting and a UI control,
    ///  enabling automatic synchronization of values. The returned binding can be used to manage updates between the
    ///  setting and the control.
    /// </remarks>
    /// <returns>
    ///  An <see cref="ISettingControlBinding"/> instance representing the binding,
    ///  or <see langword="null"/> if the default binding provided
    ///  by GitUI.SettingControlBindings.SettingControlBindingsProvider should be used.
    /// </returns>
    ISettingControlBinding? CreateControlBinding() => null;
}
