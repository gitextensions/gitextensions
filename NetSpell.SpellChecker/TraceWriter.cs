using System.Diagnostics;
using System.Reflection;

/// <summary>
/// Summary description for TraceWriter.
/// </summary>
internal static class TraceWriter
{
    public static TraceSwitch Switch { get; } = new TraceSwitch(Assembly.GetAssembly(typeof(TraceWriter)).GetName().Name, "Trace Helper Switch");

    public static void TraceError(string format, params object[] args)
    {
        if (Switch.TraceError)
        {
            Trace.WriteLine(string.Format(format, args), GetCallingMethod(new StackTrace(1)));
        }
    }

    public static void TraceError(string value)
    {
        if (Switch.TraceError)
        {
            Trace.WriteLine(value, GetCallingMethod(new StackTrace(1)));
        }
    }

    public static void TraceInfo(string format, params object[] args)
    {
        if (Switch.TraceInfo)
        {
            Trace.WriteLine(string.Format(format, args), GetCallingMethod(new StackTrace(1)));
        }
    }

    public static void TraceInfo(string value)
    {
        if (Switch.TraceInfo)
        {
            Trace.WriteLine(value, GetCallingMethod(new StackTrace(1)));
        }
    }

    public static void TraceWarning(string format, params object[] args)
    {
        if (Switch.TraceWarning)
        {
            Trace.WriteLine(string.Format(format, args), GetCallingMethod(new StackTrace(1)));
        }
    }

    public static void TraceWarning(string value)
    {
        if (Switch.TraceWarning)
        {
            Trace.WriteLine(value, GetCallingMethod(new StackTrace(1)));
        }
    }

    public static void TraceVerbose(string format, params object[] args)
    {
        if (Switch.TraceVerbose)
        {
            Trace.WriteLine(string.Format(format, args), GetCallingMethod(new StackTrace(1)));
        }
    }

    public static void TraceVerbose(string value)
    {
        if (Switch.TraceVerbose)
        {
            Trace.WriteLine(value, GetCallingMethod(new StackTrace(1)));
        }
    }

    private static string GetCallingMethod(StackTrace stack)
    {
        StackFrame frame = stack.GetFrame(0);
        string className = frame.GetMethod().DeclaringType.ToString();
        string functionName = frame.GetMethod().Name;
        return string.Format("{0}.{1}", className, functionName);
    }
}
