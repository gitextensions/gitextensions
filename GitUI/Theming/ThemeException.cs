using System;

namespace GitUI.Theming
{
    public class ThemeException : Exception
    {
        public ThemeException()
        {
        }

        public ThemeException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }

        public ThemeException(string message, string path, Exception? innerException = null)
            : base($"Failed to load {path}: {message}", innerException)
        {
        }
    }
}
