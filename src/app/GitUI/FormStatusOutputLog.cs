using System.Text;
using GitExtensions.Extensibility;

namespace GitUI;

public class FormStatusOutputLog
{
    private readonly Lock _outputStringLock = new();
    private readonly StringBuilder _outputString = new();

    public void Append(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        text = text.Replace(Delimiters.VerticalFeed, Delimiters.LineFeed).ReplaceLineEndings();
        lock (_outputStringLock)
        {
            _outputString.Append(text);
        }
    }

    public void Clear()
    {
        lock (_outputStringLock)
        {
            _outputString.Clear();
        }
    }

    public string GetString()
    {
        lock (_outputStringLock)
        {
            return _outputString.ToString();
        }
    }
}
