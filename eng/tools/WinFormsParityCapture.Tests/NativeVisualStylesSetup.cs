using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[SetUpFixture]
public sealed class NativeVisualStylesSetup
{
    [OneTimeSetUp]
    public void EnableVisualStylesBeforeCreatingNativeHandles()
    {
        // Match the reference worker before any fixture creates a native handle. Setting
        // process DPI awareness in an individual fixture is too late after an earlier HWND.
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
    }
}
