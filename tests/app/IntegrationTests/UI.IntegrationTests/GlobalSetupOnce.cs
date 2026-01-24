[SetUpFixture]
// No namespace, so it runs for all tests.
#pragma warning disable CA1050 // Declare types in namespaces
public class GlobalSetupOnce
#pragma warning restore CA1050 // Declare types in namespaces
{
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests()
    {
    }
}
