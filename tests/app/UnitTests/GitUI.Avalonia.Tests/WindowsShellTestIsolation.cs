using System.Runtime.CompilerServices;
using GitUI.Compat;

/// <summary>
/// Keeps the test host away from the owner's Windows shell. The taskbar and jump list would otherwise
/// change the owner's Recent items, and the close-all broadcast would close every running Git Extensions
/// window, including the WinForms application. Tests that exercise these paths install their own
/// recording substitutes and restore these defaults.
/// </summary>
internal static class WindowsShellTestIsolation
{
    internal static readonly ITaskbarShell DefaultShell = new UnsupportedTaskbarShell();

    internal static readonly Action<uint> DefaultBroadcast = message =>
        throw new InvalidOperationException($"A test broadcast the close-all message 0x{message:X} without installing a substitute.");

    [ModuleInitializer]
    internal static void Initialize()
    {
        TaskbarManager.TestAccessor.Shell = DefaultShell;
        WindowsCloseRequests.TestAccessor.Broadcast = DefaultBroadcast;
    }
}
