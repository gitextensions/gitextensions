using System.Diagnostics;
using System.Text;

namespace GitExtUtils;

public delegate void TraceHandler(in string message);

/// <summary>
///  Provides an event for receiving the <see cref="Trace"/> output.
/// </summary>
public interface ISubscribableTraceListener
{
    event TraceHandler TraceReceived;
}

internal class SubscribableTraceListener : TraceListener, ISubscribableTraceListener
{
    private readonly Lock _lock = new();

    private readonly StringBuilder _trace = new();

    public event TraceHandler TraceReceived;

    public SubscribableTraceListener()
    {
        Trace.Listeners.Add(this);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Trace.Listeners.Remove(this);
        }

        base.Dispose(disposing);
    }

    public override void Flush()
    {
        lock (_lock)
        {
            base.Flush();
            try
            {
                if (_trace.Length == 0)
                {
                    return;
                }

                TraceReceived?.Invoke(_trace.ToString());
                _trace.Clear();
            }
            catch
            {
                // Do not worsen things
            }
        }
    }

    public override void Write(string? message)
    {
        lock (_lock)
        {
            _trace.Append(message);
        }
    }

    public override void WriteLine(string? message)
    {
        lock (_lock)
        {
            _trace.AppendLine(message);
            Flush();
        }
    }
}
