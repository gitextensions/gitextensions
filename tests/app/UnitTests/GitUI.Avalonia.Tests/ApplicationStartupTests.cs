using GitCommands;
using GitExtensions;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class ApplicationStartupTests
{
    [TestCase(true, "wayland-0", true)]
    [TestCase(true, null, false)]
    [TestCase(true, "", false)]
    [TestCase(false, "wayland-0", false)]
    [Category("P0_6")]
    public void ShouldUseWayland_should_require_a_linux_wayland_session(
        bool isLinux,
        string? waylandDisplay,
        bool expected)
    {
        Program.ShouldUseWayland(isLinux, waylandDisplay).Should().Be(expected);
    }

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
