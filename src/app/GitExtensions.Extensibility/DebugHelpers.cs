using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace GitExtensions.Extensibility;

/// <summary>
///  Set of DEBUG-only helpers.
/// </summary>
public static class DebugHelpers
{
    [Conditional("DEBUG")]
    public static void Assert([DoesNotReturnIf(false)] bool condition, string message)
    {
        if (!condition)
        {
            Fail(message);
        }
    }

#pragma warning disable CS8763 // A method marked [DoesNotReturn] should not return.
    [Conditional("DEBUG")]
    [DoesNotReturn]
    public static void Fail(string message)
    {
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
#pragma warning restore CS8763 // A method marked [DoesNotReturn] should not return.

    [Conditional("DEBUG")]
    public static void Trace(string message, [CallerMemberName] string caller = "")
    {
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

    private static bool IsTestRunning
        => Application.ExecutablePath.EndsWith("testhost.exe");
}
