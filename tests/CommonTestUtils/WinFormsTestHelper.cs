using System.Diagnostics;
using NUnit.Framework;

namespace CommonTestUtils;

/// <summary>
///  Helpers for WinForms-based tests that need to pump messages on the UI thread.
/// </summary>
public static class WinFormsTestHelper
{
    // Same delay as RevisionDataGridView.BackgroundThreadUpdatePeriod
    private const int _processDelayMilliseconds = 25;

    /// <summary>
    ///  Pumps WinForms events in a loop until <paramref name="condition"/> returns <see langword="true"/>,
    ///  failing the test if the condition is not met within <paramref name="maxMilliseconds"/>.
    /// </summary>
    /// <param name="processName">A descriptive name used in the failure message.</param>
    /// <param name="condition">The condition to wait for.</param>
    /// <param name="maxMilliseconds">Maximum time to wait in milliseconds.</param>
    public static void ProcessUntil(string processName, Func<bool> condition, int maxMilliseconds = 1500)
    {
        int maxIterations = (maxMilliseconds + _processDelayMilliseconds - 1) / _processDelayMilliseconds;
        for (int iteration = 0; iteration < maxIterations; ++iteration)
        {
            if (condition())
            {
                Debug.WriteLine($"'{processName}' successfully finished in iteration {iteration}");
                return;
            }

            Application.DoEvents();
            Thread.Sleep(_processDelayMilliseconds);
        }

        Assert.Fail($"'{processName}' didn't finish in {maxIterations} iterations");
    }

    /// <summary>
    ///  Pumps WinForms events for at least <paramref name="milliseconds"/> milliseconds.
    /// </summary>
    /// <param name="milliseconds">The minimum duration to process events.</param>
    public static void ProcessEventsFor(int milliseconds)
    {
        int maxIterations = (milliseconds + _processDelayMilliseconds - 1) / _processDelayMilliseconds;
        for (int iteration = 0; iteration < maxIterations; ++iteration)
        {
            Application.DoEvents();
            Thread.Sleep(_processDelayMilliseconds);
        }
    }
}
