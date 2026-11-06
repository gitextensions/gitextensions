using System.Reflection;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitUI;

namespace GitExtensionsTests;

internal sealed class AppSettingsTestScope : IDisposable
{
    private static readonly MethodInfo SettingsContainerSetter = typeof(AppSettings).GetProperty(
        nameof(AppSettings.SettingsContainer), BindingFlags.Public | BindingFlags.Static)!.GetSetMethod(nonPublic: true)!;
    private static readonly FieldInfo WindowPositionCache = typeof(WindowPositionManager).GetField(
        "_windowPositionList", BindingFlags.NonPublic | BindingFlags.Static)!;
    private readonly WindowPositionList? _previousWindowPositions;
    private readonly DistributedSettings _previousSettings;
    private readonly Lazy<string?> _previousApplicationDataPath;
    private readonly Func<string> _previousWindowPositionPath;
    private readonly GitExtSettingsCache _cache;
    private readonly string _directory;
    private bool _disposed;

    public AppSettingsTestScope()
    {
        _previousWindowPositions = (WindowPositionList?)WindowPositionCache.GetValue(null);
        _previousSettings = AppSettings.SettingsContainer;
        _previousApplicationDataPath = AppSettings.GetTestAccessor().ApplicationDataPath;
        _previousWindowPositionPath = WindowPositionList.ConfigFilePathProvider;
        _directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.FixtureSettings-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_directory);
        _cache = GitExtSettingsCache.Create(Path.Combine(_directory, AppSettings.SettingsFileName), useSharedCache: false);
        // Borrow the settings reference without holding UsingContainer's cache lock across asynchronous UI work.
        SettingsContainerSetter.Invoke(null, [new DistributedSettings(null, _cache, SettingLevel.Unknown)]);
        AppSettings.GetTestAccessor().ApplicationDataPath = new Lazy<string?>(() => _directory);
        WindowPositionList.ConfigFilePathProvider = () => WindowPositionPath;
        ReloadWindowPositions();
    }

    public string WindowPositionPath => Path.Combine(_directory, "WindowPositions.xml");

    public void ReloadWindowPositions() => WindowPositionCache.SetValue(null, null);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            SettingsContainerSetter.Invoke(null, [_previousSettings]);
            AppSettings.GetTestAccessor().ApplicationDataPath = _previousApplicationDataPath;
            WindowPositionList.ConfigFilePathProvider = _previousWindowPositionPath;
            WindowPositionCache.SetValue(null, _previousWindowPositions);
        }
        finally
        {
            _cache.Dispose();
            TestDirectory.Delete(_directory);
        }
    }
}
