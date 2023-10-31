using System.Diagnostics;
using System.Text;

namespace GitUI.Models;

public interface IProcessHistoryModel
{
    bool Enabled { get; }
    string History { get; }

    event EventHandler HistoryChanged;

    void Clear();
    void Trace(in RunProcessInfo runProcess);
    void Trace(in Exception exception);
    void Trace(in string message);
}

public sealed class ProcessHistoryModel : IProcessHistoryModel
{
    private const string _endMark = "###";
    private const string _noExecutable = "---";

    private List<StringBuilder> _processHistory;

    public ProcessHistoryModel(in int historyDepth)
    {
        _processHistory = new List<StringBuilder>(capacity: historyDepth);
    }

    public bool Enabled => _processHistory.Capacity > 0;

    public string History
    {
        get
        {
            StringBuilder sb = new();
            lock (_processHistory)
            {
                foreach (StringBuilder entry in _processHistory)
                {
                    sb.Append(entry);
                }
            }

            sb.AppendLine(_endMark);

            return sb.ToString();
        }
    }

    public event EventHandler HistoryChanged;

    public void Clear()
    {
        lock (_processHistory)
        {
            _processHistory.Clear();
        }

        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Trace(in string message)
    {
        Add(new StringBuilder().Append(DateTime.Now.ToShortTimeString()).Append(' ').AppendLine(message));
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

            return sb.AppendLine(runProcess.Output.Trim()).AppendLine();
        }
    }

    private void Add(in StringBuilder entry)
    {
        if (!Enabled)
        {
            return;
        }

        lock (_processHistory)
        {
            if (_processHistory.Count == _processHistory.Capacity)
            {
                _processHistory.RemoveAt(0);
            }

            _processHistory.Add(entry);
        }

        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }
}
