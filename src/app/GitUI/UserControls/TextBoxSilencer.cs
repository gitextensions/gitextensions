using GitExtensions.Extensibility;

namespace GitUI.UserControls
{
    /// <summary>
    /// Suppresses invalid cursor movement keypresses in order to avoid the "ding" sound.
    /// </summary>
    /// The end positions cannot be retrieved in the KeyDown handler
    /// because they are implemented using SendMessage calls.
    internal sealed class TextBoxSilencer
    {
        private RichTextBox _textBox;

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
            bool isAtEndColumn = position == GetLineEnd(text, startIndex: position);
            bool isAtFirstLine = position <= GetLineEnd(text, startIndex: 0);
            bool isAtLastLine = text.IndexOfAny(Delimiters.LineFeedAndCarriageReturn, startIndex: position) < 0;

            switch (e.KeyCode)
            {
                case Keys.Up when isAtFirstLine:
                case Keys.Down when isAtLastLine:
                case Keys.Home when isAtFirstColumn:
                case Keys.End when isAtEndColumn:
                case Keys.Left or Keys.PageUp when isAtFirstLine && isAtFirstColumn:
                case Keys.Right or Keys.PageDown when isAtLastLine && isAtEndColumn:
                    e.Handled = true;
                    break;
            }
        }

        private static int GetLineEnd(string text, int startIndex)
        {
            int eol = text.IndexOfAny(Delimiters.LineFeedAndCarriageReturn, startIndex);
            return eol >= startIndex ? eol : text.Length;
        }
    }
}
