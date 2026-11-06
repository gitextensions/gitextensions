using System.Collections.Concurrent;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[SetUpFixture]
public sealed class NativeVisualStylesSetup
{
    private static readonly ConcurrentQueue<Exception> _threadExceptions = new();

    [OneTimeSetUp]
    public void EnableVisualStylesBeforeCreatingNativeHandles()
    {
        // Match the reference worker before any fixture creates a native handle. Setting
        // process DPI awareness in an individual fixture is too late after an earlier HWND.
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();

        // The consumer owns this test host, not the user's application. Keep this
        // handler alive until process exit so late native callbacks cannot open a
        // debugger dialog; every captured exception still fails the assembly gate.
        Application.ThreadException += (_, args) => _threadExceptions.Enqueue(args.Exception);
    }

    [OneTimeTearDown]
    public void UnexpectedNativeThreadExceptionsShouldFailTheTestHost()
    {
        Exception[] exceptions = _threadExceptions.ToArray();
        if (exceptions.Length != 0)
        {
            Assert.Fail("Unexpected WinForms UI-thread exceptions in the owned native test host:" + Environment.NewLine
                + string.Join(Environment.NewLine, exceptions.Select(exception => exception.ToString())));
        }
    }
}
