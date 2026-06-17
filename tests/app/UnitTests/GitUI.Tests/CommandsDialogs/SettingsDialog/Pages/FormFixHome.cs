using GitUI.CommandsDialogs.SettingsDialog.Pages;

namespace GitUITests.CommandsDialogs.SettingsDialog.Pages;

public static class FormFixHomeTests
{
    [Test]
    public static void HasGlobalConfig()
    {
        string tempDirectory = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());
        DirectoryInfo dir = Directory.CreateDirectory(tempDirectory);

        FormFixHome.TestAccessor.HasGlobalGitConfig(tempDirectory).Should().BeFalse("No config files in empty directory");

        // Create XDG config file for Git
        File.Create(Path.Join(dir.CreateSubdirectory(".config/git").ToString(), "config")).Dispose();

        Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", null);
        FormFixHome.TestAccessor.HasGlobalGitConfig(tempDirectory).Should().BeTrue("XDG config with default environment");
        Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", Path.Join(tempDirectory, ".config"));
        FormFixHome.TestAccessor.HasGlobalGitConfig(tempDirectory).Should().BeTrue("XDG config with compatible setting");

        Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", Path.Join(tempDirectory, "otherpath"));
        FormFixHome.TestAccessor.HasGlobalGitConfig(tempDirectory).Should().BeFalse("XDG config with incompatible setting");

        // Create primary Git config file
        File.Create(Path.Join(dir.ToString(), ".gitconfig")).Dispose();

        FormFixHome.TestAccessor.HasGlobalGitConfig(tempDirectory).Should().BeTrue("classic Git config present");

        // Clean up temporary directory
        DeleteRecursive(dir);

        static void DeleteRecursive(DirectoryInfo path)
        {
            foreach (DirectoryInfo dir in path.EnumerateDirectories())
            {
                DeleteRecursive(dir);
            }

            foreach (FileInfo file in path.GetFiles())
            {
                file.Delete();
            }

            path.Delete();
        }
    }
}
