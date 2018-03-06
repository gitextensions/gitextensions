using System;

using JetBrains.Annotations;

namespace GitUI.UserControls
{
    public sealed class TextEventArgs : EventArgs
    {
        public TextEventArgs([NotNull] string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            Text = text;
        }

        [NotNull]
        public string Text { get; }
    }
}