using GitExtensions.Extensibility;

namespace GitUI.UserControls;

/// <summary>
/// Suppresses invalid cursor movement keypresses in order to avoid the "ding" sound.
/// </summary>
/// The end positions cannot be retrieved in the KeyDown handler
/// because they are implemented using SendMessage calls.
internal sealed class TextBoxSilencer
{
    private readonly RichTextBox _textBox;

    public TextBoxSilencer(RichTextBox textBox)
    {
        _textBox = textBox;
        _textBox.KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled || _textBox.SelectionLength > 0)
        {
            return;
        }

        string text = _textBox.Text;
        int position = _textBox.SelectionStart;

        bool isAtFirstColumn = position == _textBox.GetFirstCharIndexOfCurrentLine();
        bool isAtEndColumn = position == text.GetLineEnd(startIndex: position);
        bool isAtFirstLine = position <= text.GetLineEnd(startIndex: 0);
        bool isAtLastLine = text.IndexOfAny(Delimiters.LineFeedAndCarriageReturnSearchValues, position) < 0;
        bool ctrl = e.Control;

        switch (e.KeyCode)
        {
            case Keys.Up when isAtFirstLine:
            case Keys.Down when isAtLastLine:
            case Keys.Home when isAtFirstColumn && (!ctrl || isAtFirstLine):
            case Keys.End when isAtEndColumn && (!ctrl || isAtLastLine):
            case Keys.Left or Keys.PageUp when isAtFirstLine && isAtFirstColumn:
            case Keys.Right or Keys.PageDown when isAtLastLine && isAtEndColumn:
                e.Handled = true;
                break;
        }
    }
}
