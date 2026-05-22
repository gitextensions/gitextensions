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

    public string Name { get; } = "PseudoSetting";
    public string Caption { get; }
    public Control? CustomControl { get; set; }
    public Func<TextBox>? TextBoxCreator => _textBoxCreator;
}
