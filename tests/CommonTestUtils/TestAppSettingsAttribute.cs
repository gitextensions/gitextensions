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

        File.Delete(AppSettings.SettingsContainer.SettingsCache.SettingsFilePath);
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
