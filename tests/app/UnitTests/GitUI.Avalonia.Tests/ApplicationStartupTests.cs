using GitCommands;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class ApplicationStartupTests
{
    [Test]
    public void GetGitExtensionsFullPath_should_accept_the_Avalonia_entry_point()
    {
        AppSettings.TestAccessor accessor = AppSettings.GetTestAccessor();
        string originalPath = accessor.ApplicationExecutablePath;
        string avaloniaPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "GitExtensions.Avalonia.exe");

        try
        {
            accessor.ApplicationExecutablePath = avaloniaPath;

            AppSettings.GetGitExtensionsFullPath().Should().Be(avaloniaPath);
        }
        finally
        {
            accessor.ApplicationExecutablePath = originalPath;
        }
    }
}
