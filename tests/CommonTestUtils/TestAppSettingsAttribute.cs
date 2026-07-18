using GitCommands;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace CommonTestUtils;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class TestAppSettingsAttribute : Attribute, ITestAction
{
    private readonly Semaphore? _windowsSemaphore = OperatingSystem.IsWindows()
        ? new(initialCount: 1, maximumCount: 1, "GitExtensionsTestAssemblySerializer")
        : null;
    private readonly Mutex? _portableMutex = OperatingSystem.IsWindows()
        ? null
        : new(
            initiallyOwned: false,
            name: "GitExtensionsTestAssemblySerializer",
            new NamedWaitHandleOptions
            {
                CurrentUserOnly = true,
                CurrentSessionOnly = false
            });

    public ActionTargets Targets => ActionTargets.Suite;

    public void BeforeTest(ITest test)
    {
        if (_windowsSemaphore is not null)
        {
            _windowsSemaphore.WaitOne();
        }
        else
        {
            _portableMutex!.WaitOne();
        }

        // A test host may run under the shared dotnet runtime rather than a native testhost.exe apphost (e.g. on the
        // arm64 CI runner, which has no native testhost.exe). In that case Application.ExecutablePath — and therefore
        // AppSettings.GetGitExtensionsDirectory() — points at the dotnet install directory instead of the test output,
        // so app-relative content such as the Themes folder cannot be found and theme loading throws (and, worse, pops
        // a modal error dialog that hangs the headless run). Pin the path to the test's own directory so it resolves.
        AppSettings.GetTestAccessor().ApplicationExecutablePath = Path.Combine(AppContext.BaseDirectory, "GitExtensions.exe");

        string settingsFilePath = AppSettings.SettingsContainer.SettingsCache.SettingsFilePath;
        if (File.Exists(settingsFilePath))
        {
            File.Delete(settingsFilePath);
        }

        AppSettings.SettingsContainer.SettingsCache.Load();

        AppSettings.CheckForUpdates = false;
        AppSettings.ShowAvailableDiffTools = false;

        // Create the settings file so that the SettingsCache does not think it should reload the file again and again
        AppSettings.SettingsContainer.SettingsCache.Save();
    }

    public void AfterTest(ITest test)
    {
        AppSettings.SettingsContainer.SettingsCache.Dispose();

        if (_windowsSemaphore is not null)
        {
            _windowsSemaphore.Release();
        }
        else
        {
            _portableMutex!.ReleaseMutex();
        }
    }
}
