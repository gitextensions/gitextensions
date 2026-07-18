using System.Diagnostics;
using System.Text;
using GitUI.Editor.Diff;

namespace GitUI.Models;

// Avalonia twin of OutputHistoryModel. Its ANSI parser is the existing Avalonia diff parser;
// storage, formatting, depth and notification semantics remain the same as WinForms.
internal sealed class OutputHistoryModel : IOutputHistoryProvider, IOutputHistoryRecorder
{
    private const string _endMark = "###";
    private const string _noExecutable = "---";

    private readonly Lock _outputHistoryLock = new();
    private readonly List<StringBuilder> _outputHistory;

    public event EventHandler? HistoryChanged;

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
            lock (_outputHistoryLock)
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

    public void ClearHistory()
    {
        lock (_outputHistoryLock)
        {
            _outputHistory.Clear();
        }

        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RecordHistory(in string message)
    {
        string time = DateTime.Now.ToShortTimeString();
        Add(new StringBuilder(time, capacity: time.Length + 1 + message.Length).Append(' ').AppendLine(message));
    }

    public void RecordHistory(in Exception exception)
    {
        RecordHistory(exception.ToStringDemystified());
    }

    public void RecordHistory(in RunProcessInfo runProcess)
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

        sb.Append(AnsiDiffTextParser.Parse(runProcess.Output.Trim(), [])).AppendLine().AppendLine();
        Add(sb);
    }

    private void Add(in StringBuilder entry)
    {
        if (!Enabled)
        {
            return;
        }

        lock (_outputHistoryLock)
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
