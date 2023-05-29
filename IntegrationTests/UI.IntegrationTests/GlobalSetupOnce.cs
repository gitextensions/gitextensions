[SetUpFixture]
public class GlobalSetupOnce
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
