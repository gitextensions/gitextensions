using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.Json;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

/// <summary>
/// Activates only a visible native test window owned by the current STA thread.
/// </summary>
internal static class NativeTestWindowActivation
{
    /// <summary>
    /// Temporarily joins the actual foreground input thread while requesting normal activation.
    /// </summary>
    /// <param name="host">The visible test window owned by the current STA thread.</param>
    /// <param name="waitForPresentation">
    /// Wait for byte-exact desktop presentation before screen observations. PrintWindow-only
    /// consumers can disable this wait without changing the source window's desktop styles.
    /// </param>
    internal static void Activate(Form host, bool waitForPresentation = true)
    {
        nint window = ValidateOwnedWindow(host);
        uint currentThread = GetCurrentThreadId();
        nint foreground = GetForegroundWindow();
        uint foregroundThread = foreground == 0 ? 0 : GetWindowThreadProcessId(foreground, out _);
        if (foreground != 0 && foregroundThread == 0)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Cannot identify the actual foreground input thread.");
        }

        bool attached = foregroundThread != 0 && foregroundThread != currentThread;
        if (attached && !AttachThreadInput(currentThread, foregroundThread, true))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Cannot join the owned test thread to the actual foreground input thread.");
        }

        try
        {
            host.BringToFront();
            host.Activate();
            SetForegroundWindow(window);
            Application.DoEvents();
        }
        finally
        {
            if (attached && !AttachThreadInput(currentThread, foregroundThread, false))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Cannot detach the owned test thread from the foreground input thread.");
            }
        }

        AssertForeground(host);
        if (waitForPresentation)
        {
            WaitForPresentation(host);
        }
    }

    /// <summary>
    /// Verifies actual foreground ownership without changing activation or an open popup.
    /// </summary>
    internal static void AssertForeground(Form host)
    {
        nint window = ValidateOwnedWindow(host);
        nint foreground = GetForegroundWindow();
        if (foreground != window)
        {
            throw new InvalidOperationException($"The owned native test window must be foreground; expected HWND 0x{window:X}, actual HWND 0x{foreground:X}.");
        }
    }

    private static nint ValidateOwnedWindow(Form host)
    {
        ArgumentNullException.ThrowIfNull(host);
        if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
        {
            throw new InvalidOperationException("Native test activation requires the owning STA thread.");
        }

        if (host.IsDisposed || !host.IsHandleCreated || !host.Visible)
        {
            throw new InvalidOperationException("Native test activation requires an existing visible, undisposed test window.");
        }

        nint window = host.Handle;
        uint ownerThread = GetWindowThreadProcessId(window, out uint ownerProcess);
        if (ownerThread != GetCurrentThreadId() || ownerProcess != (uint)Environment.ProcessId)
        {
            throw new InvalidOperationException("The native test window must belong to the current STA thread and test process.");
        }

        return window;
    }

    private static void WaitForPresentation(Form host)
    {
        // Foreground ownership precedes completion of the desktop's show/activation
        // transition. A DWM flush alone waits only for this application's queued updates.
        // Observe byte-exact screen stability before any test pointer or menu input.
        Rectangle bounds = host.RectangleToScreen(host.ClientRectangle);
        if (bounds.Width <= 0 || bounds.Height <= 0 || !NativeMethods.IsEntirelyOnScreen(bounds))
        {
            throw new InvalidOperationException("The owned test client must be wholly on screen before presentation can be observed.");
        }

        using Bitmap frame = new(bounds.Width, bounds.Height, PixelFormat.Format32bppRgb);
        byte[] current = new byte[checked(bounds.Width * bounds.Height * 4)];
        byte[] previous = new byte[current.Length];
        bool havePrevious = false;
        int attempts = 0;
        int unownedAttempts = 0;
        Point center = new(bounds.Left + (bounds.Width / 2), bounds.Top + (bounds.Height / 2));
        nint visibleWindow = 0;
        nint visibleRoot = 0;
        Stopwatch elapsed = Stopwatch.StartNew();
        Stopwatch stable = Stopwatch.StartNew();
        while (elapsed.Elapsed < TimeSpan.FromSeconds(3))
        {
            host.Update();
            Application.DoEvents();
            AssertForeground(host);
            if (host.RectangleToScreen(host.ClientRectangle) != bounds)
            {
                throw new InvalidOperationException("The owned test client changed bounds while waiting for presentation.");
            }

            Marshal.ThrowExceptionForHR(DwmFlush());
            attempts++;
            visibleWindow = WindowFromPoint(center);
            visibleRoot = GetAncestor(visibleWindow, 2);
            if (visibleRoot != host.Handle)
            {
                // Native foreground activation can precede the screen's z-order handoff.
                // Do not sample an unrelated surface or count it toward stable presentation.
                havePrevious = false;
                stable.Restart();
                unownedAttempts++;
                if (unownedAttempts == 1)
                {
                    TestContext.Progress.WriteLine("nativeTestActivationNotReady " + DescribeScreenOwnership(host, center, visibleWindow, visibleRoot));
                }

                continue;
            }

            using (Graphics graphics = Graphics.FromImage(frame))
            {
                graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

            BitmapData data = frame.LockBits(new Rectangle(Point.Empty, frame.Size), ImageLockMode.ReadOnly, frame.PixelFormat);
            try
            {
                Marshal.Copy(data.Scan0, current, 0, current.Length);
            }
            finally
            {
                frame.UnlockBits(data);
            }

            AssertForeground(host);
            if (!havePrevious || !current.AsSpan().SequenceEqual(previous))
            {
                stable.Restart();
            }
            else if (stable.Elapsed >= TimeSpan.FromMilliseconds(100))
            {
                TestContext.Progress.WriteLine($"nativeTestActivationPresentation elapsedMs={elapsed.ElapsedMilliseconds} attempts={attempts} unownedAttempts={unownedAttempts} stableMs={stable.ElapsedMilliseconds}");
                return;
            }

            (current, previous) = (previous, current);
            havePrevious = true;
        }

        throw new TimeoutException($"The owned test client's actual screen pixels did not remain byte-exact for 100 ms within 3 seconds; attempts={attempts}, unownedAttempts={unownedAttempts}. "
            + DescribeScreenOwnership(host, center, visibleWindow, visibleRoot));
    }

    private static string DescribeScreenOwnership(Form host, Point center, nint visibleWindow, nint visibleRoot)
        => JsonSerializer.Serialize(new
        {
            center,
            host = host.Handle.ToString(),
            foreground = GetForegroundWindow().ToString(),
            visibleWindow = visibleWindow.ToString(),
            visibleRoot = visibleRoot.ToString(),
            hostAndNeighbors = NativeMethods.GetNativeWindowAndNeighborReceipts(host.Handle),
            visibleRootAndNeighbors = NativeMethods.GetNativeWindowAndNeighborReceipts(visibleRoot),
        });

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(nint window, out uint processId);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AttachThreadInput(uint sourceThread, uint targetThread, [MarshalAs(UnmanagedType.Bool)] bool attach);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(nint window);

    [DllImport("user32.dll")]
    private static extern nint WindowFromPoint(Point point);

    [DllImport("user32.dll")]
    private static extern nint GetAncestor(nint window, uint flags);

    [DllImport("dwmapi.dll")]
    private static extern int DwmFlush();
}
