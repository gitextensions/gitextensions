namespace GitUI.UserControls
{
    /// <summary>
    /// Suppresses invalid cursor movement keypresses in order to avoid the "ding" sound.
    /// </summary>
    /// The end positions cannot be retrieved in the KeyDown handler
    /// because they are implemented using SendMessage calls.
    internal sealed class TextBoxSilencer
    {
        private static readonly char[] _lineEndChars = { '\r', '\n' };

        private RichTextBox _textBox;

        private bool _isAtFirstColumn;
        private bool _isAtEndColumn;
        private bool _isAtFirstLine;
        private bool _isAtLastLine;

        public TextBoxSilencer(RichTextBox textBox)
        {
            _textBox = textBox;
            _textBox.SelectionChanged += (s, e) => UpdatePositionFlags();
            _textBox.KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Up when _isAtFirstLine:
                case Keys.Down when _isAtLastLine:
                case Keys.Home when _isAtFirstColumn:
                case Keys.End when _isAtEndColumn:
                case Keys.Left or Keys.PageUp when _isAtFirstLine && _isAtFirstColumn:
                case Keys.Right or Keys.PageDown when _isAtLastLine && _isAtEndColumn:
                    e.Handled = true;
                    break;
            }
        }

        private void UpdatePositionFlags()
        {
            string text = _textBox.Text;
            int position = _textBox.SelectionStart;

            _isAtFirstColumn = position == _textBox.GetFirstCharIndexOfCurrentLine();
            _isAtEndColumn = position == GetLineEnd(text, startIndex: position);
            _isAtFirstLine = position <= GetLineEnd(text, startIndex: 0);
            _isAtLastLine = text.IndexOfAny(_lineEndChars, startIndex: position) < 0;
        }

        private static int GetLineEnd(string text, int startIndex)
        {
            int eol = text.IndexOfAny(_lineEndChars, startIndex);
            return eol >= startIndex ? eol : text.Length;
        }
    }
}
