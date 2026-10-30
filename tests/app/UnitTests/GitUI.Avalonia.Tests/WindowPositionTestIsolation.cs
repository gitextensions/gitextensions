using System.Reflection;
using System.Security.Cryptography;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitUI;
using GitUIPluginInterfaces;

[SetUpFixture]
public sealed class WindowPositionTestIsolation
{
    private Func<string> _originalConfigFilePathProvider = null!;
    private string _settingsDirectory = null!;
    private Lazy<string?>? _originalApplicationDataPath;
    private DistributedSettings? _originalSettingsContainer;
    private GitExtSettingsCache? _isolatedSettingsCache;
    private string _originalSettingsPath = null!;
    private string? _originalSettingsHash;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _settingsDirectory = Path.Combine(
            Path.GetTempPath(),
            $"GitExtensions.Avalonia.TestSettings-{Environment.ProcessId}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_settingsDirectory);
        _originalConfigFilePathProvider = WindowPositionList.ConfigFilePathProvider;
        try
        {
            // The existing accessor first initializes AppSettings, which may read the test
            // host's normal profile. Never delete, reset, or save that profile for a test run.
            AppSettings.TestAccessor accessor = AppSettings.GetTestAccessor();
            _originalApplicationDataPath = accessor.ApplicationDataPath;
            _originalSettingsContainer = AppSettings.SettingsContainer;
            _originalSettingsPath = _originalSettingsContainer.SettingsCache.SettingsFilePath;
            _originalSettingsHash = SettingsHash(_originalSettingsPath);

            string settingsPath = Path.Combine(_settingsDirectory, AppSettings.SettingsFileName);
            _isolatedSettingsCache = GitExtSettingsCache.Create(settingsPath, useSharedCache: false);
            DistributedSettings settings = new(lowerPriority: null, _isolatedSettingsCache, SettingLevel.Unknown);
            accessor.ApplicationDataPath = new Lazy<string?>(() => _settingsDirectory);
            SetSettingsContainer(settings);
            WindowPositionList.ConfigFilePathProvider = () => Path.Combine(_settingsDirectory, "WindowPositions.xml");

            AppSettings.SettingsFilePath.Should().Be(settingsPath);
            AppSettings.SettingsContainer.SettingsCache.Should().BeSameAs(_isolatedSettingsCache);
            Path.GetDirectoryName(WindowPositionList.ConfigFilePathProvider()).Should().Be(_settingsDirectory);
            AppSettings.GetString("Detailed.ShowSplitViewLayout", null).Should().BeNull(
                "the assembly must start with its own fresh profile, not inherit a previous interactive or test session");
            AppSettings.ShowSplitViewLayout.Should().BeTrue();
            // The headless lifetime skips desktop composition setup. Initialize the base
            // catalog after settings isolation, before any Browse consumer can load plugins.
            ManagedExtensibility.Initialise();
            TestContext.Progress.WriteLine($"Isolated settings: {settingsPath}");
            TestContext.Progress.WriteLine($"Preserved profile: {_originalSettingsPath}; SHA256={_originalSettingsHash ?? "<absent>"}");
        }
        catch
        {
            RestoreSettings();
            throw;
        }
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        try
        {
            SettingsHash(_originalSettingsPath).Should().Be(_originalSettingsHash,
                "tests must not change the original test host profile while using the isolated cache");
            TestContext.Progress.WriteLine($"Preserved profile unchanged: {_originalSettingsPath}; SHA256={_originalSettingsHash ?? "<absent>"}");
        }
        finally
        {
            RestoreSettings();
        }
    }

    private void RestoreSettings()
    {
        try
        {
            if (_originalSettingsContainer is not null)
            {
                SetSettingsContainer(_originalSettingsContainer);
            }

            if (_originalApplicationDataPath is not null)
            {
                AppSettings.GetTestAccessor().ApplicationDataPath = _originalApplicationDataPath;
            }

            WindowPositionList.ConfigFilePathProvider = _originalConfigFilePathProvider;
        }
        finally
        {
            _isolatedSettingsCache?.Dispose();
            if (Directory.Exists(_settingsDirectory))
            {
                Directory.Delete(_settingsDirectory, recursive: true);
            }
        }
    }

    private static void SetSettingsContainer(DistributedSettings settings)
    {
        // Assembly-lifetime isolation cannot use the synchronous UsingContainer action:
        // that would hold the original cache lock across asynchronous UI tests. Keep the
        // private-setter reflection here in test infrastructure, not in shipping code.
        MethodInfo setter = typeof(AppSettings).GetProperty(
                nameof(AppSettings.SettingsContainer), BindingFlags.Public | BindingFlags.Static)!
            .GetSetMethod(nonPublic: true)!;
        setter.Invoke(obj: null, [settings]);
    }

    private static string? SettingsHash(string path) =>
        File.Exists(path) ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) : null;
}
