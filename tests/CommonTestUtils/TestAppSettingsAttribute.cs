using GitCommands;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace CommonTestUtils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class TestAppSettingsAttribute : Attribute, ITestAction
    {
        public ActionTargets Targets => ActionTargets.Suite;

        public void BeforeTest(ITest test)
        {
            for (int i = 0; i < 5; ++i)
            {
                try
                {
                    File.Delete(AppSettings.SettingsContainer.SettingsCache.SettingsFilePath);
                    break;
                }
                catch (IOException ex)
                {
                    Console.WriteLine(@$"Failed to delete settings file ""{AppSettings.SettingsContainer.SettingsCache.SettingsFilePath}"": {ex}");
                    Thread.Sleep(1000);
                }
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
        }
    }
}
