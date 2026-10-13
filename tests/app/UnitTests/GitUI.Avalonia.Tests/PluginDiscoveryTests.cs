using System.Reflection;
using GitExtensions.Compat;
using GitUIPluginInterfaces;

namespace GitExtensionsTests;

[TestFixture]
public sealed class PluginDiscoveryTests
{
    [Test]
    public void Managed_extensibility_should_resolve_a_renamed_dependency_by_assembly_metadata()
    {
        string directory = Path.Join(
            Path.GetTempPath(),
            $"GitExtensions.Avalonia.AssemblyResolution-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        try
        {
            File.WriteAllText(Path.Join(directory, "000-native.dll"), "not a managed assembly");
            string dependencyPath = Path.Join(directory, "renamed-dependency.dll");
            File.Copy(typeof(ManagedExtensibility).Assembly.Location, dependencyPath);

            string? resolvedPath = FindAssemblyPath(directory, typeof(ManagedExtensibility).Assembly.GetName());

            resolvedPath.Should().Be(dependencyPath);
        }
        finally
        {
            TestDirectory.Delete(directory);
        }
    }

    [Test]
    public void Managed_extensibility_should_ignore_a_missing_localized_runtime_resource()
    {
        string directory = Path.Join(
            Path.GetTempPath(),
            $"GitExtensions.Avalonia.AssemblyResolution-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        try
        {
            File.WriteAllText(Path.Join(directory, "000-native.dll"), "not a managed assembly");
            AssemblyName requestedAssembly = new(
                "System.Private.CoreLib.resources, Version=10.0.0.0, Culture=fr-FR, PublicKeyToken=7cec85d7bea7798e");

            string? resolvedPath = FindAssemblyPath(directory, requestedAssembly);

            resolvedPath.Should().BeNull();
        }
        finally
        {
            TestDirectory.Delete(directory);
        }
    }

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

    private static string? FindAssemblyPath(string directory, AssemblyName requestedAssembly)
    {
        MethodInfo method = typeof(ManagedExtensibility).GetMethod(
            "FindAssemblyPath",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        return (string?)method.Invoke(null, [directory, requestedAssembly]);
    }
}
