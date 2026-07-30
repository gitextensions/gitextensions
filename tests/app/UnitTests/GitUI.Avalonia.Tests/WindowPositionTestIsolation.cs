using GitUI;

[SetUpFixture]
public sealed class WindowPositionTestIsolation
{
    private Func<string> _originalConfigFilePathProvider = null!;
    private string _settingsDirectory = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _settingsDirectory = Path.Combine(
            Path.GetTempPath(),
            $"GitExtensions.Avalonia.WindowPositionTests-{Environment.ProcessId}");
        _originalConfigFilePathProvider = WindowPositionList.ConfigFilePathProvider;
        WindowPositionList.ConfigFilePathProvider = () => Path.Combine(_settingsDirectory, "WindowPositions.xml");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        WindowPositionList.ConfigFilePathProvider = _originalConfigFilePathProvider;
        if (Directory.Exists(_settingsDirectory))
        {
            Directory.Delete(_settingsDirectory, recursive: true);
        }
    }
}
