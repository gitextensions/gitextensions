using System;
using System.Text;

using JetBrains.Annotations;

namespace GitUI
{
    public class FormStatusOutputLog
    {
        [NotNull]
        private readonly StringBuilder _outputString = new StringBuilder();

        public void Append([NotNull] string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            lock (_outputString)
            {
                _outputString.Append(text);
            }
        }

        public void Clear()
        {
            lock (_outputString)
            {
                _outputString.Clear();
            }
        }

        [NotNull]
        public string GetString()
        {
            lock (_outputString)
            {
                return _outputString.ToString();
            }
        }
    }
}