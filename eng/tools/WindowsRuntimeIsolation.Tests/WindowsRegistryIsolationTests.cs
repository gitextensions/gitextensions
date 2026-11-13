using System.Diagnostics;
using System.Text.Json.Nodes;
using GitExtensions.WindowsRuntimeIsolation;
using Microsoft.Win32;

namespace GitExtensions.WindowsRuntimeIsolation.Tests;

[TestFixture]
[NonParallelizable]
public sealed class WindowsRegistryIsolationTests
{
    private string _temporaryDirectory = null!;
    private string _applicationConfiguration = null!;
    private string _helperDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        DirectoryInfo? repository = new(AppContext.BaseDirectory);
        while (repository is not null && !File.Exists(Path.Combine(repository.FullName, "Directory.Packages.props")))
        {
            repository = repository.Parent;
        }

        string root = repository?.FullName ?? throw new AssertionException("Run these tool contracts inside the repository.");
        _applicationConfiguration = Path.Combine(root, "artifacts", "Debug", "bin", "GitExtensions.Avalonia", "net10.0", "GitExtensions.Avalonia.runtimeconfig.json");
        _helperDirectory = Path.Combine(root, "artifacts", "Debug", "bin", "WindowsRuntimeIsolation", "net10.0-windows");
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"gitextensions-registry-contract-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_temporaryDirectory);
    }

    [TearDown]
    public void TearDown() => Directory.Delete(_temporaryDirectory, recursive: true);

    [Test]
    public async Task Probe_should_prove_real_runtime_configuration_and_retain_redirection_through_process_exitAsync()
    {
        using WindowsRegistryIsolationScope scope = new();
        await scope.ProveAsync(_applicationConfiguration);
        scope.Proven.Should().BeTrue();
        scope.ProbeProcessId.Should().BeGreaterThan(0);
        scope.OwnerUnchanged.Should().BeTrue();
        scope.OwnerAfterHash.Should().Be(scope.OwnerBeforeHash);
        using RegistryKey root = Registry.CurrentUser.OpenSubKey(scope.RegistryPath, writable: false)!;
        root.OpenSubKey("Probe", writable: false).Should().BeNull("the probe must never write outside the redirected process hive");
        using RegistryKey nested = root.OpenSubKey($"{scope.RegistryPath}\\Probe", writable: false)!;
        nested.GetValue("Early").Should().Be(scope.ProbeLaunch);
        nested.GetValue("Late").Should().Be(scope.ProbeLaunch, "even ProcessExit callbacks must remain isolated");
    }

    [Test]
    public async Task Disabled_startup_hooks_should_refuse_application_launch_without_touching_the_owner_keyAsync()
    {
        JsonObject configuration = JsonNode.Parse(File.ReadAllText(_applicationConfiguration))!.AsObject();
        JsonObject options = configuration["runtimeOptions"]!.AsObject();
        JsonObject properties = options["configProperties"]?.AsObject() ?? new JsonObject();
        if (options["configProperties"] is null)
        {
            options["configProperties"] = properties;
        }

        properties["System.StartupHookProvider.IsSupported"] = false;
        string path = Path.Combine(_temporaryDirectory, "disabled.runtimeconfig.json");
        File.WriteAllText(path, configuration.ToJsonString());
        using WindowsRegistryIsolationScope scope = new();
        await scope.Awaiting(value => value.ProveAsync(path)).Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not proved*application launch refused*");
        scope.Proven.Should().BeFalse();
        scope.Invoking(value => value.StartApplication(NewHelperStart())).Should().Throw<InvalidOperationException>()
            .WithMessage("*successful registry-isolation probe first*");
        using RegistryKey outer = Registry.CurrentUser.OpenSubKey($"{scope.RegistryPath}\\Probe", writable: false)!;
        outer.GetValue("Early").Should().Be(scope.ProbeLaunch, "a failed-hook probe must still write only an authorized test path");
        scope.VerifyOwnerUnchanged();
        scope.OwnerAfterHash.Should().Be(scope.OwnerBeforeHash);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Application_factory_should_supply_the_hook_and_require_each_actual_child_proofAsync(bool removeProof)
    {
        using WindowsRegistryIsolationScope scope = new();
        scope.Invoking(value => value.StartApplication(NewHelperStart())).Should().Throw<InvalidOperationException>();
        string configuration = Path.Combine(_helperDirectory, "WindowsRuntimeIsolation.runtimeconfig.json");
        await scope.ProveAsync(configuration);
        ProcessStartInfo start = NewHelperStart();
        start.Environment["GITEXT_SMOKE_EXTERNAL_INPUT"] = "preserved";
        string? parentRoot = Environment.GetEnvironmentVariable(RegistryRootContract.RootVariable);
        Process application = scope.StartApplication(start);
        try
        {
            using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(30));
            await application.WaitForExitAsync(timeout.Token);
            application.HasExited.Should().BeTrue();
            application.ExitCode.Should().Be(0);
            start.Environment[RegistryRootContract.RootVariable].Should().Be(scope.RegistryPath);
            start.Environment[RegistryRootContract.NonceVariable].Should().Be(scope.Nonce);
            start.Environment["DOTNET_STARTUP_HOOKS"]!.Split(Path.PathSeparator)[0]
                .Should().Be(typeof(WindowsRegistryIsolationScope).Assembly.Location);
            start.Environment["GITEXT_SMOKE_EXTERNAL_INPUT"].Should().Be("preserved");
            Environment.GetEnvironmentVariable(RegistryRootContract.RootVariable).Should().Be(parentRoot);
            scope.VerifyOwnerUnchanged();
            ProcessStartInfo unknown = new(Path.Combine(_temporaryDirectory, "different.exe")) { UseShellExecute = false };
            File.WriteAllText(Path.ChangeExtension(unknown.FileName, ".runtimeconfig.json"), File.ReadAllText(configuration) + Environment.NewLine);
            scope.Invoking(value => value.StartApplication(unknown)).Should().Throw<InvalidOperationException>()
                .WithMessage("*runtime configuration must match*");
            if (removeProof)
            {
                using RegistryKey initialized = Registry.CurrentUser.OpenSubKey($"{scope.RegistryPath}\\InitializedProcesses", writable: true)!;
                initialized.DeleteValue(start.Environment[RegistryRootContract.LaunchVariable]!);
                scope.Invoking(value => value.Dispose()).Should().Throw<InvalidOperationException>()
                    .WithMessage("*no matching startup-hook proof*");
                scope.ApplicationsProved.Should().BeFalse();
                scope.OwnerUnchanged.Should().BeTrue();
            }
            else
            {
                scope.Dispose();
                scope.ApplicationsProved.Should().BeTrue();
            }
        }
        finally
        {
            if (!application.HasExited)
            {
                application.Kill(entireProcessTree: true);
                await application.WaitForExitAsync();
            }

            scope.Dispose();
            application.Dispose();
        }
    }

    [Test]
    public void Root_contract_should_reject_a_nested_path_and_a_changed_nonce_before_any_override()
    {
        using WindowsRegistryIsolationScope scope = new();
        Action nested = () => RegistryRootContract.OpenValidatedRoot($"{scope.RegistryPath}\\invalid", scope.Nonce);
        nested.Should().Throw<InvalidOperationException>().WithMessage("*exact owned PID/GUID*");
        using RegistryKey root = Registry.CurrentUser.OpenSubKey(scope.RegistryPath, writable: true)!;
        root.SetValue("Nonce", "changed");
        try
        {
            Action changed = () => RegistryRootContract.OpenValidatedRoot(scope.RegistryPath, scope.Nonce);
            changed.Should().Throw<InvalidOperationException>().WithMessage("*nonce does not match*");
        }
        finally
        {
            root.SetValue("Nonce", scope.Nonce);
        }
    }

    [TestCase("value")]
    [TestCase("kind")]
    [TestCase("added")]
    [TestCase("removed")]
    [TestCase("subkey")]
    public void Snapshot_should_detect_every_changed_value_kind_and_subkey_without_restoring_it(string change)
    {
        using WindowsRegistryIsolationScope scope = new();
        string path = $"{scope.RegistryPath}\\GuardFixture";
        using RegistryKey fixture = Registry.CurrentUser.CreateSubKey(path, writable: true);
        fixture.SetValue("test", "1", RegistryValueKind.String);
        RegistrySnapshot before = RegistrySnapshot.Capture(path);
        switch (change)
        {
            case "value":
                fixture.SetValue("test", "2");
                break;
            case "kind":
                fixture.SetValue("test", 1, RegistryValueKind.DWord);
                break;
            case "added":
                fixture.SetValue("new", "1");
                break;
            case "removed":
                fixture.DeleteValue("test");
                break;
            case "subkey":
                using (fixture.CreateSubKey("child", writable: true))
                {
                }

                break;
        }

        RegistrySnapshot after = RegistrySnapshot.Capture(path);
        Action verify = () => before.AssertUnchanged(after);
        verify.Should().Throw<InvalidOperationException>().WithMessage("*registry changed*");
        RegistrySnapshot.Capture(path).Hash.Should().Be(after.Hash, "the guard must never restore values");
    }

    [Test]
    public void Cleanup_should_remove_only_its_exact_owned_key()
    {
        using WindowsRegistryIsolationScope neighbour = new();
        WindowsRegistryIsolationScope scope = new();
        string path = scope.RegistryPath;
        scope.Dispose();
        scope.Dispose();
        using RegistryKey? deleted = Registry.CurrentUser.OpenSubKey(path, writable: false);
        deleted.Should().BeNull();
        using RegistryKey? retained = Registry.CurrentUser.OpenSubKey(neighbour.RegistryPath, writable: false);
        retained.Should().NotBeNull();
        neighbour.VerifyOwnerUnchanged();
        neighbour.OwnerAfterHash.Should().Be(neighbour.OwnerBeforeHash);
    }

    private ProcessStartInfo NewHelperStart()
    {
        ProcessStartInfo start = new(Path.Combine(_helperDirectory, "WindowsRuntimeIsolation.exe"))
        {
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        start.ArgumentList.Add("--registry-probe");
        return start;
    }
}
