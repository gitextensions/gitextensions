using System.Text;
using GitExtensions.Extensibility;

namespace GitUI
{
    public class FormStatusOutputLog
    {
        private readonly StringBuilder _outputString = new();

        public void Append(string text)
        {
            ArgumentNullException.ThrowIfNull(text);

            text = text.Replace(Delimiters.VerticalFeed, Delimiters.LineFeed).ReplaceLineEndings();
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
