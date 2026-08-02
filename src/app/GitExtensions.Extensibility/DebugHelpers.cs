using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace GitExtensions.Extensibility;

/// <summary>
///  Set of DEBUG-only helpers.
/// </summary>
public static class DebugHelpers
{
    private const string FailFastEnvironmentVariable = "GITEXTENSIONS_DEBUG_FAIL_FAST";

    [Conditional("DEBUG")]
    public static void Assert([DoesNotReturnIf(false)] bool condition, string message)
    {
        if (!condition)
        {
            Fail(message);
        }
    }

    [Conditional("DEBUG")]
    public static void Fail(string message)
    {
        if (string.Equals(Environment.GetEnvironmentVariable(FailFastEnvironmentVariable), "1", StringComparison.Ordinal))
        {
            Console.Error.WriteLine($"Debug assertion failed: {message}");
            throw new InvalidOperationException(message);
        }

        if (Debugger.IsAttached || IsTestRunning)
        {
            Debug.Fail(message);
        }
        else
        {
            Debugger.Launch();

            if (!Debugger.IsAttached)
            {
                throw new InvalidOperationException(message);
            }
        }
    }

    [Conditional("DEBUG")]
    public static void Trace(string message, [CallerMemberName] string caller = "")
    {
        // colon and noBreakSpace are used to detect such messages in order to show them in the Output History
        const char noBreakSpace = '\u00a0';
        Debug.WriteLine($"{caller}:{noBreakSpace}{message}");
    }

    [Conditional("DEBUG")]
    public static void TraceIf(bool condition, string message, [CallerMemberName] string caller = "")
    {
        if (condition)
        {
            Trace(message, caller);
        }
    }

    // The test host is testhost.exe on Windows but testhost.dll started by the dotnet host
    // elsewhere, so the process path does not identify it on every platform; the entry
    // assembly does.
    private static bool IsTestRunning
        => string.Equals(Assembly.GetEntryAssembly()?.GetName().Name, "testhost", StringComparison.OrdinalIgnoreCase);
}
