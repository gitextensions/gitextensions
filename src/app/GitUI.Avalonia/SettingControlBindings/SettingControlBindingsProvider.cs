using Avalonia.Controls;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Settings.UserControls;
using GitUI.Compat;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.SettingControlBindings;

/// <summary>
///  Provides factory methods for creating <see cref="PluginSettingBinding"/> instances
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
    public static PluginSettingBinding CreateControlBinding(BoolSetting setting, CheckBox? control)
        => new BoolSettingControlBinding(setting, control);

    /// <summary>
    ///  Creates a control binding for the given <see cref="StringSetting"/>.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    /// <param name="control">An optional pre-existing control to bind to.</param>
    public static PluginSettingBinding CreateControlBinding(StringSetting setting, TextBox? control)
        => new StringSettingControlBinding(setting, control);

    /// <summary>
    ///  Creates a control binding for the given <see cref="PasswordSetting"/>.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    /// <param name="control">An optional pre-existing control to bind to.</param>
    public static PluginSettingBinding CreateControlBinding(PasswordSetting setting, TextBox? control)
        => new PasswordSettingControlBinding(setting, control);

    /// <summary>
    ///  Creates a control binding for the given <see cref="ChoiceSetting"/>.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    /// <param name="control">An optional pre-existing control to bind to.</param>
    public static PluginSettingBinding CreateControlBinding(ChoiceSetting setting, ComboBox? control)
        => new ChoiceSettingControlBinding(setting, control);

    /// <summary>
    ///  Creates a control binding for the given <see cref="CredentialsSetting"/>.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    /// <param name="control">An optional pre-existing control model to bind to.</param>
    public static PluginSettingBinding CreateControlBinding(CredentialsSetting setting, CredentialsControl? control)
        => new CredentialsSettingControlBinding(setting, control);

    /// <summary>
    ///  Creates a control binding for the given <see cref="PseudoSetting"/>.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    public static PluginSettingBinding CreateControlBinding(PseudoSetting setting)
        => new PseudoSettingControlBinding(setting, customControl: null);

    /// <summary>
    ///  Creates a control binding for the given <see cref="NumberSetting{T}"/> where T is <see cref="int"/>.
    ///  Uses a <see cref="NumericUpDown"/> binding when <paramref name="control"/> is a <see cref="NumericUpDown"/>,
    ///  or a <see cref="TextBox"/> binding otherwise.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    /// <param name="control">An optional pre-existing control to bind to.</param>
    public static PluginSettingBinding CreateControlBinding(NumberSetting<int> setting, Control? control)
        => control is TextBox
            ? new NumberSettingTextBoxBinding<int>(setting, (TextBox)control)
            : new NumberSettingNumericUpDownBinding(setting, control as NumericUpDown);

    /// <summary>
    ///  Creates a <see cref="TextBox"/>-backed control binding for the given <see cref="NumberSetting{T}"/>.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    /// <param name="control">An optional pre-existing text box to bind to.</param>
    public static PluginSettingBinding CreateControlBinding<T>(NumberSetting<T> setting, TextBox? control)
        => new NumberSettingTextBoxBinding<T>(setting, control);

    /// <summary>
    ///  Creates a control binding for the given <see cref="ISetting"/>, dispatching to the appropriate
    ///  typed overload based on the runtime type of <paramref name="setting"/>.
    /// </summary>
    /// <param name="setting">The setting to bind.</param>
    /// <exception cref="NotSupportedException">
    ///  Thrown when <paramref name="setting"/> is not a known setting type.
    /// </exception>
    public static PluginSettingBinding CreateControlBinding(ISetting setting)
    {
        if (setting.CreateControlBinding() is { } customBinding)
        {
            return PluginSettingControlFactory.Create(customBinding);
        }

        return setting switch
        {
            BoolSetting s => CreateControlBinding(s, control: null),
            CredentialsSetting s => CreateControlBinding(s, s.CustomControl),
            PasswordSetting s => CreateControlBinding(s, control: null),
            StringSetting s => CreateControlBinding(s, control: null),
            ChoiceSetting s => CreateControlBinding(s, control: null),
            PseudoSetting s => CreateControlBinding(s),
            NumberSetting<int> s when s.CustomControl is WinFormsShims.TextBox =>
                new NumberSettingTextBoxBinding<int>(s, customControl: null),
            NumberSetting<int> s => CreateControlBinding(s, (Control?)null),
            NumberSetting<float> s => CreateControlBinding(s, control: null),
            NumberSetting<double> s => CreateControlBinding(s, control: null),
            NumberSetting<long> s => CreateControlBinding(s, control: null),
            _ => throw new NotSupportedException($"""
                No control binding registered for {setting.GetType().Name}.
                Consider implementing ISetting.CreateControlBinding and provide your own control binding in your plugin.
                """)
        };
    }
}
