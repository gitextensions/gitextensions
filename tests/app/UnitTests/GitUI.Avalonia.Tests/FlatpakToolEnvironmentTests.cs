using System.ComponentModel;
using System.Diagnostics;
using CommonTestUtils;
using GitUI.Compat;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class FlatpakToolEnvironmentTests
{
    private string _root = null!;

    [SetUp]
    public void SetUp()
    {
        _root = Path.Combine(Path.GetTempPath(), "GitExtensions.FlatpakTools-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    [TearDown]
    public void TearDown()
    {
        Path.GetDirectoryName(_root).Should().Be(Path.TrimEndingDirectorySeparator(Path.GetTempPath()));
        Path.GetFileName(_root).Should().StartWith("GitExtensions.FlatpakTools-");
        TestDirectory.Delete(_root);
    }

    [Test]
    public async Task Initialize_should_leave_an_unconfined_environment_unchanged()
    {
        bool result = await FlatpakToolEnvironment.InitializeAsync(false,
            _ => throw new AssertionException("No sandbox metadata is needed."),
            (_, _) => throw new AssertionException("The environment must stay unchanged."),
            _ => throw new AssertionException("The host must not be queried."));

        result.Should().BeFalse();
        Directory.GetFileSystemEntries(_root).Should().BeEmpty();
    }

    [Test]
    public async Task Initialize_should_supply_private_proxies_only_for_installed_known_tools_and_host_visible_temporary_files()
    {
        Dictionary<string, string?> environment = [];
        string cache = Path.Combine(_root, "host-cache");
        bool result = await Initialize(environment, cache + "\0" + cache + "\0meld\0unknown-program\0");

        result.Should().BeTrue();
        string proxy = environment[FlatpakToolEnvironment.ProxyDirectoryVariable]!;
        Directory.GetFiles(proxy).Select(Path.GetFileName).Should().Equal("meld");
        File.ReadAllText(Path.Combine(proxy, "meld")).Should().Contain("gitextensions-host-tool").And.Contain("'meld'");
        environment["TMPDIR"].Should().StartWith(cache + Path.DirectorySeparatorChar);
        Directory.Exists(environment["TMPDIR"]).Should().BeTrue();
        Directory.GetFiles(cache, "*.probe", SearchOption.AllDirectories).Should().BeEmpty();
        environment[FlatpakToolEnvironment.ErrorVariable].Should().BeNull();
    }

    [Test]
    public async Task Initialize_should_remove_a_managed_proxy_after_the_host_tool_disappears()
    {
        Dictionary<string, string?> environment = [];
        string cache = Path.Combine(_root, "host-cache");
        (await Initialize(environment, cache + "\0" + cache + "\0meld\0")).Should().BeTrue();
        string proxy = Path.Combine(environment[FlatpakToolEnvironment.ProxyDirectoryVariable]!, "meld");
        File.Exists(proxy).Should().BeTrue();

        (await Initialize(environment, cache + "\0" + cache + "\0")).Should().BeTrue();

        File.Exists(proxy).Should().BeFalse();
    }

    [Test]
    public async Task Initialize_should_try_the_host_home_cache_if_the_XDG_cache_does_not_expose_the_same_file()
    {
        Dictionary<string, string?> environment = [];
        string privateCache = Path.Combine(_root, "host-private-cache");
        string sharedCache = Path.Combine(_root, "host-home-cache");

        bool result = await Initialize(environment, privateCache + "\0" + sharedCache + "\0",
            marker => marker.StartsWith(privateCache, StringComparison.Ordinal) ? (1, string.Empty, "private namespace") : (0, File.ReadAllText(marker), string.Empty));

        result.Should().BeTrue();
        environment["TMPDIR"].Should().StartWith(sharedCache + Path.DirectorySeparatorChar);
        Directory.GetFiles(_root, "*.probe", SearchOption.AllDirectories).Should().BeEmpty();
    }

    [Test]
    public async Task Initialize_should_report_denied_host_access_without_replacing_the_existing_temporary_directory()
    {
        Dictionary<string, string?> environment = [];
        bool result = await FlatpakToolEnvironment.InitializeAsync(true, GetEnvironment,
            (name, value) => environment[name] = value,
            _ => Task.FromResult((1, string.Empty, "permission denied")));

        result.Should().BeFalse();
        environment.Should().NotContainKey("TMPDIR");
        environment[FlatpakToolEnvironment.ErrorVariable].Should().Contain("Host tools are not available in this Flatpak installation.");
        Directory.Exists(environment[FlatpakToolEnvironment.ProxyDirectoryVariable]).Should().BeTrue();
    }

    [Test]
    public async Task Initialize_should_report_a_missing_host_launcher_instead_of_aborting_application_startup()
    {
        Dictionary<string, string?> environment = [];
        bool result = await FlatpakToolEnvironment.InitializeAsync(true, GetEnvironment,
            (name, value) => environment[name] = value,
            _ => throw new Win32Exception("The host launcher is missing."));

        result.Should().BeFalse();
        environment.Should().NotContainKey("TMPDIR");
        environment[FlatpakToolEnvironment.ErrorVariable].Should().Contain("not available in this Flatpak installation").And.Contain("launcher is missing");
    }

    [Test]
    public async Task Initialize_should_reject_a_relative_application_cache_without_writing_it()
    {
        Dictionary<string, string?> environment = [];
        bool result = await FlatpakToolEnvironment.InitializeAsync(true, _ => "relative-cache",
            (name, value) => environment[name] = value,
            _ => throw new AssertionException("The host must not be queried for an invalid application cache."));

        result.Should().BeFalse();
        environment.Should().NotContainKey(FlatpakToolEnvironment.ProxyDirectoryVariable);
        environment[FlatpakToolEnvironment.ErrorVariable].Should().Contain("not an absolute path");
    }

    [Test]
    public async Task Initialize_should_refuse_temporary_storage_that_the_host_cannot_read()
    {
        Dictionary<string, string?> environment = [];
        string cache = Path.Combine(_root, "invisible-cache");

        bool result = await Initialize(environment, cache + "\0" + cache + "\0",
            _ => (1, string.Empty, "not the same namespace"));

        result.Should().BeFalse();
        environment.Should().NotContainKey("TMPDIR");
        environment[FlatpakToolEnvironment.ErrorVariable].Should().Contain("No host cache directory exposes the same temporary files");
        Directory.GetFiles(_root, "*.probe", SearchOption.AllDirectories).Should().BeEmpty();
    }

    private Task<bool> Initialize(Dictionary<string, string?> environment, string metadata,
        Func<string, (int ExitCode, string Output, string Error)>? readMarker = null)
        => FlatpakToolEnvironment.InitializeAsync(true, GetEnvironment, (name, value) => environment[name] = value,
            request =>
            {
                request.FileName.Should().Be("/usr/bin/flatpak-spawn");
                request.ArgumentList.Should().Contain("--host").And.Contain("--watch-bus");
                if (request.ArgumentList.Contains("/bin/cat"))
                {
                    string marker = request.ArgumentList.Last();
                    return Task.FromResult(readMarker?.Invoke(marker) ?? (0, File.ReadAllText(marker), string.Empty));
                }

                request.ArgumentList.Should().Contain("gitextensions-tool-environment");
                return Task.FromResult((0, metadata, string.Empty));
            });

    private string? GetEnvironment(string name)
        => name switch
        {
            "XDG_CACHE_HOME" => Path.Combine(_root, "sandbox-cache"),
            "FLATPAK_ID" => "com.github.gitextensions.Parity.Test",
            _ => null,
        };
}
