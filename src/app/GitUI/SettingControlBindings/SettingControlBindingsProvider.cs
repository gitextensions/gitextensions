using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Settings.UserControls;

namespace GitUI.SettingControlBindings;

/// <summary>
///  Provides factory methods for creating <see cref="ISettingControlBinding"/> instances
///  for all known <see cref="ISetting"/> implementations, keeping UI and control binding
///  code out of the Extensibility layer.
/// </summary>
public static class SettingControlBindingsProvider
{
    /// <summary>
    ///  Creates a control binding for the given <see cref="BoolSetting"/>.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    /// <param name="control">An optional pre-existing control to bind to.</param>
    public static ISettingControlBinding CreateControlBinding(BoolSetting setting, CheckBox? control)
        => new BoolSettingControlBinding(setting, control);

    /// <summary>
    ///  Creates a control binding for the given <see cref="StringSetting"/>.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    /// <param name="control">An optional pre-existing control to bind to.</param>
    public static ISettingControlBinding CreateControlBinding(StringSetting setting, TextBox? control)
        => new StringSettingControlBinding(setting, control);

    /// <summary>
    ///  Creates a control binding for the given <see cref="PasswordSetting"/>.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    /// <param name="control">An optional pre-existing control to bind to.</param>
    public static ISettingControlBinding CreateControlBinding(PasswordSetting setting, TextBox? control)
        => new PasswordSettingControlBinding(setting, control);

    /// <summary>
    ///  Creates a control binding for the given <see cref="ChoiceSetting"/>.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    /// <param name="control">An optional pre-existing control to bind to.</param>
    public static ISettingControlBinding CreateControlBinding(ChoiceSetting setting, ComboBox? control)
        => new ChoiceSettingControlBinding(setting, control);

    /// <summary>
    ///  Creates a control binding for the given <see cref="CredentialsSetting"/>.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    /// <param name="control">An optional pre-existing control to bind to.</param>
    public static ISettingControlBinding CreateControlBinding(CredentialsSetting setting, CredentialsControl? control)
        => new CredentialsSettingControlBinding(setting, control);

    /// <summary>
    ///  Creates a control binding for the given <see cref="PseudoSetting"/>.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    public static ISettingControlBinding CreateControlBinding(PseudoSetting setting)
        => new PseudoSettingControlBinding(setting, setting.CustomControl);

    /// <summary>
    ///  Creates a control binding for the given <see cref="NumberSetting{T}"/> where T is <see cref="int"/>.
    ///  Uses a <see cref="NumericUpDown"/> binding when <paramref name="control"/> is a <see cref="NumericUpDown"/>,
    ///  or a <see cref="TextBox"/> binding otherwise.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    /// <param name="control">An optional pre-existing control to bind to.</param>
    public static ISettingControlBinding CreateControlBinding(NumberSetting<int> setting, Control? control)
        => control is not TextBox
            ? new NumberSettingNumericUpDownBinding(setting, control as NumericUpDown)
            : new NumberSettingTextBoxBinding<int>(setting, control as TextBox);

    /// <summary>
    ///  Creates a <see cref="TextBox"/>-backed control binding for the given <see cref="NumberSetting{T}"/>.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    /// <param name="control">An optional pre-existing text box to bind to.</param>
    public static ISettingControlBinding CreateControlBinding<T>(NumberSetting<T> setting, TextBox? control)
        => new NumberSettingTextBoxBinding<T>(setting, control);

    /// <summary>
    ///  Creates a control binding for the given <see cref="ISetting"/>, dispatching to the appropriate
    ///  typed overload based on the runtime type of <paramref name="setting"/>.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    /// <exception cref="NotSupportedException">
    ///  Thrown when <paramref name="setting"/> is not a known setting type.
    /// </exception>
    public static ISettingControlBinding CreateControlBinding(ISetting setting)
    {
        if (setting.CreateControlBinding() is { } customBinding)
        {
            return customBinding;
        }

        return setting switch
        {
            BoolSetting s => CreateControlBinding(s, s.CustomControl),
            CredentialsSetting s => CreateControlBinding(s, s.CustomControl),
            PasswordSetting s => CreateControlBinding(s, s.CustomControl),
            StringSetting s => CreateControlBinding(s, s.CustomControl),
            ChoiceSetting s => CreateControlBinding(s, s.CustomControl),
            PseudoSetting s => CreateControlBinding(s),
            NumberSetting<int> s => CreateControlBinding(s, s.CustomControl),
            NumberSetting<float> s => CreateControlBinding(s, s.CustomControl as TextBox),
            NumberSetting<double> s => CreateControlBinding(s, s.CustomControl as TextBox),
            NumberSetting<long> s => CreateControlBinding(s, s.CustomControl as TextBox),
            _ => throw new NotSupportedException($"""
                No control binding registered for {setting.GetType().Name}.
                Consider implementing ISetting.CreateControlBinding and provide your own control binding in your plugin.
                """)
        };
    }
}
