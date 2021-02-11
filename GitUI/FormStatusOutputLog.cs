using System;
using System.Text;

namespace GitUI
{
    public class FormStatusOutputLog
    {
        private readonly StringBuilder _outputString = new();

        public void Append(string text)
        {
            if (text is null)
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

        public string GetString()
        {
            lock (_outputString)
            {
                return _outputString.ToString();
            }
        }
    }
}
