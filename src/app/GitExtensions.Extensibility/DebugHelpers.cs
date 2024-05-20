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

    private static bool IsTestRunning
        => Application.ExecutablePath.EndsWith("testhost.exe");
}
