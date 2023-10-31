using System.Diagnostics;
using System.Text;
using GitUI.Editor.Diff;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Models;

public interface IOutputHistoryModel
{
    event EventHandler HistoryChanged;

    bool Enabled { get; }
    string History { get; }

    void Clear();
    void Trace(in RunProcessInfo runProcess);
    void Trace(in Exception exception);
    void Trace(in string message);
}

public sealed class OutputHistoryModel : IOutputHistoryModel
{
    private const string _endMark = "###";
    private const string _noExecutable = "---";

    private List<StringBuilder> _outputHistory;

    public event EventHandler HistoryChanged;

    public OutputHistoryModel(in int historyDepth)
    {
        _outputHistory = new List<StringBuilder>(capacity: historyDepth);
    }

    public bool Enabled => _outputHistory.Capacity > 0;

    public string History
    {
        get
        {
            StringBuilder sb = new();
            lock (_outputHistory)
            {
                foreach (StringBuilder entry in _outputHistory)
                {
                    sb.Append(entry);
                }
            }

            sb.AppendLine(_endMark);

            return sb.ToString();
        }
    }

    public void Clear()
    {
        lock (_outputHistory)
        {
            _outputHistory.Clear();
        }

        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Trace(in string message)
    {
        string time = DateTime.Now.ToShortTimeString();
        Add(new StringBuilder(time, capacity: time.Length + 1 + message.Length).Append(' ').AppendLine(message));
    }

    public void Trace(in Exception exception)
    {
        Trace(exception.ToStringDemystified());
    }

    public void Trace(in RunProcessInfo runProcess)
    {
        Add(Format(runProcess));

        return;

        static StringBuilder Format(in RunProcessInfo runProcess)
        {
            StringBuilder sb = new();
            sb.Append(runProcess.FinishTime.ToShortTimeString()).Append(' ');
            if (string.IsNullOrWhiteSpace(runProcess.Executable))
            {
                sb.AppendLine(_noExecutable);
            }
            else
            {
                sb.Append(runProcess.Executable).Append(' ').AppendLine(runProcess.Arguments);
            }

            List<TextMarker> textMarkers = [];
            AnsiEscapeUtilities.ParseEscape(runProcess.Output.Trim(), sb, textMarkers, traceErrors: false);

            return sb.AppendLine().AppendLine();
        }
    }

    private void Add(in StringBuilder entry)
    {
        if (!Enabled)
        {
            return;
        }

        lock (_outputHistory)
        {
            if (_outputHistory.Count == _outputHistory.Capacity)
            {
                _outputHistory.RemoveAt(0);
            }

            _outputHistory.Add(entry);
        }

        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }
}
