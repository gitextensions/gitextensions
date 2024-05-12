using Microsoft;

namespace GitExtensions.Extensibility.Settings;

/// <summary>
/// Not a real setting (as it save no setting value). It is used to display a control that is not a setting (linklabel, text,...)
/// </summary>
public class PseudoSetting : ISetting
{
    private readonly Func<TextBox>? _textBoxCreator;

    public PseudoSetting(Control control, string caption = "")
    {
        Caption = caption;
        CustomControl = control;
    }

    public PseudoSetting(string text, string caption = "    ", int? height = null, Action<TextBox>? textboxSettings = null)
    {
        Caption = caption;

        _textBoxCreator = () =>
        {
            TextBox textbox = new() { ReadOnly = true, BorderStyle = BorderStyle.None, Text = text };

            if (height.HasValue)
            {
                textbox.Multiline = true;
                textbox.Height = height.Value;
            }

            textboxSettings?.Invoke(textbox);
            return textbox;
        };

        CustomControl = _textBoxCreator();
    }

    public string Name { get; } = "PseusoSetting";
    public string Caption { get; }
    public Control? CustomControl { get; set; }

    public ISettingControlBinding CreateControlBinding()
    {
        return new PseudoBinding(this, CustomControl, _textBoxCreator);
    }

    private class PseudoBinding : SettingControlBinding<PseudoSetting, Control>
    {
        private readonly Func<TextBox>? _textBoxCreator;

        public PseudoBinding(PseudoSetting setting, Control? customControl, Func<TextBox>? textBoxCreator)
            : base(setting, customControl)
        {
            _textBoxCreator = textBoxCreator;
        }

        public override Control CreateControl()
        {
            ArgumentNullException.ThrowIfNull(_textBoxCreator);
            Setting.CustomControl = _textBoxCreator();
            return Setting.CustomControl;
        }

        public override void LoadSetting(SettingsSource settings, Control control)
        {
        }

        public override void SaveSetting(SettingsSource settings, Control control)
        {
        }
    }
}
