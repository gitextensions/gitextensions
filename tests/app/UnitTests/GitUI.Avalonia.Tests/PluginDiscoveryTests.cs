using GitExtensions.Compat;

namespace GitExtensionsTests;

[TestFixture]
public sealed class PluginDiscoveryTests
{
    [Test]
    public void User_plugins_directory_should_use_the_Avalonia_sibling_under_local_application_data()
    {
        string localApplicationData = Path.Join(
            Path.GetTempPath(),
            $"GitExtensions.Avalonia.PluginDiscovery-{Guid.NewGuid():N}");
        try
        {
            string? path = UserPluginsDirectory.GetPath(localApplicationData);

            path.Should().Be(Path.Join(localApplicationData, "UserPlugins.Avalonia"));
            Directory.Exists(path).Should().BeTrue();
            path.Should().NotBe(Path.Join(localApplicationData, "UserPlugins"));
        }
        finally
        {
            TestDirectory.Delete(localApplicationData);
        }
    }

    [Test]
    public void User_plugins_directory_should_disable_user_plugins_when_the_directory_is_inaccessible()
    {
        List<string> diagnostics = [];
        string expectedPath = Path.Join("local-data", "UserPlugins.Avalonia");

        string? path = UserPluginsDirectory.GetTestAccessor().GetPath(
            "local-data",
            flatpakId: null,
            xdgDataHome: null,
            path => throw new UnauthorizedAccessException($"Denied {path}"),
            diagnostics.Add);

        path.Should().BeNull();
        diagnostics.Should().ContainSingle()
            .Which.Should().Contain(expectedPath)
            .And.Contain("disabled")
            .And.Contain("inaccessible");
    }

    [Test]
    public void User_plugins_directory_should_disable_user_plugins_when_local_application_data_is_unavailable()
    {
        List<string> diagnostics = [];

        string? path = UserPluginsDirectory.GetTestAccessor().GetPath(
            null,
            flatpakId: null,
            xdgDataHome: null,
            _ => throw new AssertionException("No directory should be accessed."),
            diagnostics.Add);

        path.Should().BeNull();
        diagnostics.Should().ContainSingle()
            .Which.Should().Contain("disabled")
            .And.Contain("unavailable");
    }

    [Test]
    public void User_plugins_directory_should_use_XDG_data_inside_Flatpak()
    {
        List<string> accessedPaths = [];

        string? path = UserPluginsDirectory.GetTestAccessor().GetPath(
            localApplicationDataPath: "portable-install",
            flatpakId: "com.github.gitextensions.GitExtensions.Avalonia.Devel",
            xdgDataHome: "/sandbox/data",
            accessedPaths.Add,
            _ => throw new AssertionException("No diagnostic should be written."));

        path.Should().Be(Path.Join("/sandbox/data", "GitExtensions", "UserPlugins.Avalonia"));
        accessedPaths.Should().ContainSingle().Which.Should().Be(path);
        path.Should().NotContain("portable-install");
    }

    [Test]
    public void User_plugins_directory_should_not_fall_back_when_XDG_data_is_unavailable_inside_Flatpak()
    {
        List<string> diagnostics = [];

        string? path = UserPluginsDirectory.GetTestAccessor().GetPath(
            localApplicationDataPath: "host-local-data",
            flatpakId: "com.github.gitextensions.GitExtensions.Avalonia.Devel",
            xdgDataHome: null,
            _ => throw new AssertionException("No directory should be accessed."),
            diagnostics.Add);

        path.Should().BeNull();
        diagnostics.Should().ContainSingle()
            .Which.Should().Contain("disabled")
            .And.Contain("XDG_DATA_HOME")
            .And.Contain("Flatpak");
    }
}
