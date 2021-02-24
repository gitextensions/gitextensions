using System;

namespace GitUI.UserControls
{
    public sealed class TextEventArgs : EventArgs
    {
        public TextEventArgs(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public string Text { get; }
    }
}
