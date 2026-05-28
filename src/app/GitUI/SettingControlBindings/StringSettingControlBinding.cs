using GitExtensions.Extensibility.Settings;

namespace GitUI.SettingControlBindings;

internal class StringSettingControlBinding : SettingControlBinding<StringSetting, TextBox>
{
    /// <summary>
    ///  Text to be shown / entered in the input control when the value is an empty string,
    ///  since empty string is a valid value for a StringSetting but it would be indistinguishable from null or unset value in the UI.
    /// </summary>
    /// <remarks>
    ///  Has visibility internal because used by <see cref="PasswordSettingControlBinding"/>, too.
    /// </remarks>
    internal const string EmptyStringValue = "<empty string>";

    /// <summary>
    ///  The text to be shown as a hint in the input control when the value is null.
    ///  It should contain "{0}" where the <see cref="EmptyStringValue"/> replacement text should be shown.
    /// </summary>
    public static string PlaceholderText { get; set; } = "";

    public StringSettingControlBinding(StringSetting setting, TextBox? customControl)
        : base(setting, customControl)
    {
    }

    public override TextBox CreateControl()
    {
        Setting.CustomControl = new TextBox();
        return Setting.CustomControl;
    }

    public override void LoadSetting(SettingsSource settings, TextBox control)
    {
        if (control.PlaceholderText.Length == 0 && PlaceholderText.Length > 0)
        {
            control.PlaceholderText = string.Format(PlaceholderText, EmptyStringValue);
        }

        string? settingVal = settings.SettingLevel == SettingLevel.Effective
            ? Setting.ValueOrDefault(settings)
            : Setting[settings];

        if (settingVal is { Length: 0 })
        {
            settingVal = EmptyStringValue;
        }

        // for multiline control, transform "\n" in "\r\n" but prevent "\r\n" to be transformed in "\r\r\n"
        control.Text = control.Multiline
            ? settingVal?.Replace(Environment.NewLine, "\n").Replace("\n", Environment.NewLine)
            : settingVal;
    }

    public override void SaveSetting(SettingsSource settings, TextBox control)
    {
        // Trim value because the XML serializer will trim it on load anyway.
        string? controlValue = control.Text.Trim();
        control.Text = controlValue;
        if (controlValue.Length == 0)
        {
            controlValue = null;
        }
        else if (controlValue == EmptyStringValue)
        {
            controlValue = "";
        }

        if (settings.SettingLevel == SettingLevel.Effective)
        {
            if (Setting.ValueOrDefault(settings) == controlValue)
            {
                return;
            }
        }

        Setting[settings] = controlValue;
    }
}
