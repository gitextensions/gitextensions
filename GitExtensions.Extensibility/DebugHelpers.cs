using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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

    private static bool IsTestRunning
        => Application.ExecutablePath.EndsWith("testhost.exe");
}
