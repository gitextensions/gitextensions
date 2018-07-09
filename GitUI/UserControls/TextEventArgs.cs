using System;

using JetBrains.Annotations;

namespace GitUI.UserControls
{
    public sealed class TextEventArgs : EventArgs
    {
        public TextEventArgs([NotNull] string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        [NotNull]
        public string Text { get; }
    }
}