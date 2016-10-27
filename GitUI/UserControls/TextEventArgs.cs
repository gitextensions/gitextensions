using System;

using JetBrains.Annotations;

namespace GitUI.UserControls
{
    public sealed class TextEventArgs : EventArgs
    {
        [NotNull]
        private readonly string _text;

        public TextEventArgs([NotNull] string text)
        {
            if(text == null)
                throw new ArgumentNullException("text");
            _text = text;
        }

        [NotNull]
        public string Text
        {
            get
            {
                return _text;
            }
        }
    }
}