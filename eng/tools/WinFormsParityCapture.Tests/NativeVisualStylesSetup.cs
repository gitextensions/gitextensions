using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[SetUpFixture]
public sealed class NativeVisualStylesSetup
{
    [OneTimeSetUp]
    public void EnableVisualStylesBeforeCreatingNativeHandles()
    {
        // The reference worker enables styles before constructing any control. Calling this
        // in an individual fixture is too late once another fixture creates an EDIT handle.
        Application.EnableVisualStyles();
    }
}
