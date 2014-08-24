using NUnit.Framework;
using GitCommands.Config;

namespace GitExtensionsTest.GitUI
{
    [TestFixture]
    public class FormUpdateFixture
    {
        private string GetReleasesConfigFileText()
        {
            ConfigFile configFile = new ConfigFile("", true);
            configFile.SetValue("Version \"2.47\".ReleaseType", "Major");
            configFile.SetValue("Version \"2.48\".ReleaseType", "Major");
            configFile.SetValue("Version \"2.49\".ReleaseType", "ReleaseCandidate");
            configFile.SetValue("RCVersion \"2.50\".ReleaseType", "ReleaseCandidate");

            return configFile.GetAsString();
        }
    }
}
