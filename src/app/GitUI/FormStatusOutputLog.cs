using System.Text;
using GitExtensions.Extensibility;

namespace GitUI
{
    public class FormStatusOutputLog
    {
        private readonly Lock _outputStringSync = new();
        private readonly StringBuilder _outputString = new();

        public void Append(string text)
        {
            ArgumentNullException.ThrowIfNull(text);

            text = text.Replace(Delimiters.VerticalFeed, Delimiters.LineFeed).ReplaceLineEndings();
            lock (_outputStringSync)
            {
                _outputString.Append(text);
            }
        }

        public void Clear()
        {
            lock (_outputStringSync)
            {
                _outputString.Clear();
            }
        }

        public string GetString()
        {
            lock (_outputStringSync)
            {
                return _outputString.ToString();
            }
        }
    }
}
