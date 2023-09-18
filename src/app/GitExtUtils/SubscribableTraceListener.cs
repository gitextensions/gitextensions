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

public class SubscribableTraceListener : TraceListener, ISubscribableTraceListener
{
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

    public override void Write(string? message)
    {
        lock (this)
        {
            _trace.Append(message);
        }
    }

    public override void WriteLine(string? message)
    {
        lock (this)
        {
            _trace.AppendLine(message);
            Flush();
        }
    }

    public override void Flush()
    {
        lock (this)
        {
            base.Flush();
            try
            {
                if (_trace.Length == 0)
                {
                    return;
                }

                string message = _trace.ToString();
#if DEBUG
                if (message.Contains("Exception")
#endif
                {
                    TraceReceived?.Invoke(message);
                }

                _trace.Clear();
            }
            catch
            {
                // Do not worsen things
            }
        }
    }
}
