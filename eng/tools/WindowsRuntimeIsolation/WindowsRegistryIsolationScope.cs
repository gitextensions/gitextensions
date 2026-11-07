using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Win32;

namespace GitExtensions.WindowsRuntimeIsolation;

// parity-scaffolding: owns the disposable registry boundary of a real Windows runtime smoke.
public sealed class WindowsRegistryIsolationScope : IDisposable
{
    private const string OwnerPath = @"Software\GitExtensions";
    private readonly RegistrySnapshot _ownerBefore;
    private readonly List<(Process Process, string Launch)> _applications = [];
    private bool _disposed;
    private string? _runtimeConfigurationHash;

    public WindowsRegistryIsolationScope()
    {
        _ownerBefore = RegistrySnapshot.Capture(OwnerPath);
        RegistryPath = $"{RegistryRootContract.TestRoot}\\{Environment.ProcessId}-{Guid.NewGuid():N}";
        Nonce = Guid.NewGuid().ToString("N");
        using RegistryKey root = Registry.CurrentUser.CreateSubKey(RegistryPath, writable: true);
        root.SetValue("Nonce", Nonce);
    }

    public string RegistryPath { get; }

    public bool Proven { get; private set; }

    public bool OwnerUnchanged { get; private set; }

    public bool ApplicationsProved { get; private set; }

    public int ProbeProcessId { get; private set; }

    public string OwnerBeforeHash => _ownerBefore.Hash;

    public string? OwnerAfterHash { get; private set; }

    internal string Nonce { get; }

    internal string? ProbeLaunch { get; private set; }

    public async Task ProveAsync(string runtimeConfigurationPath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Proven = false;
        _runtimeConfigurationHash = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(runtimeConfigurationPath)));
        ProcessStartInfo start = new("dotnet")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        start.ArgumentList.Add("exec");
        start.ArgumentList.Add("--runtimeconfig");
        start.ArgumentList.Add(Path.GetFullPath(runtimeConfigurationPath));
        start.ArgumentList.Add(typeof(WindowsRegistryIsolationScope).Assembly.Location);
        start.ArgumentList.Add("--registry-probe");
        string launch = Configure(start);
        ProbeLaunch = launch;
        using Process probe = Process.Start(start)
            ?? throw new InvalidOperationException("Cannot start the harmless registry-isolation probe.");
        ProbeProcessId = probe.Id;
        Task<string> stdout = probe.StandardOutput.ReadToEndAsync();
        Task<string> stderr = probe.StandardError.ReadToEndAsync();
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(30));
        try
        {
            await probe.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            if (!probe.HasExited)
            {
                probe.Kill(entireProcessTree: true);
            }

            await probe.WaitForExitAsync().ConfigureAwait(false);
            throw new InvalidOperationException("The harmless registry-isolation probe timed out; application launch refused.");
        }

        string output = await stdout.ConfigureAwait(false);
        string error = await stderr.ConfigureAwait(false);
        if (probe.ExitCode != 0)
        {
            throw new InvalidOperationException($"Registry-isolation probe failed ({probe.ExitCode}): {error}{output}. Application launch refused.");
        }

        using RegistryKey root = RegistryRootContract.OpenValidatedRoot(RegistryPath, Nonce);
        using RegistryKey? initialized = root.OpenSubKey("InitializedProcesses", writable: false);
        using RegistryKey? nested = root.OpenSubKey($"{RegistryPath}\\Probe", writable: false);
        if (!Equals(initialized?.GetValue(launch), ProbeProcessId)
            || !string.Equals(nested?.GetValue("Early") as string, launch, StringComparison.Ordinal)
            || !string.Equals(nested?.GetValue("Late") as string, launch, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Registry redirection was not proved through process exit; application launch refused.");
        }

        VerifyOwnerUnchanged();
        Proven = true;
    }

    public Process StartApplication(ProcessStartInfo start)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!Proven)
        {
            throw new InvalidOperationException("Application launch requires a successful registry-isolation probe first.");
        }

        string configuration = Path.ChangeExtension(start.FileName, ".runtimeconfig.json");
        if (!File.Exists(configuration)
            || Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(configuration))) != _runtimeConfigurationHash)
        {
            throw new InvalidOperationException("The application runtime configuration must match the proved configuration.");
        }

        string launch = Configure(start);
        OwnerUnchanged = false;
        OwnerAfterHash = null;
        Process application = Process.Start(start)
            ?? throw new InvalidOperationException("Cannot start the isolated application.");
        _applications.Add((application, launch));
        return application;
    }

    public void VerifyOwnerUnchanged()
    {
        RegistrySnapshot after = RegistrySnapshot.Capture(OwnerPath);
        OwnerAfterHash = after.Hash;
        OwnerUnchanged = false;
        _ownerBefore.AssertUnchanged(after);
        OwnerUnchanged = true;
    }

    public string GetEvidenceJson() => JsonSerializer.Serialize(new
    {
        registryAccess = "isolatedProcessHive",
        registryPath = RegistryPath,
        probeProcessId = ProbeProcessId,
        probeLaunchIdentity = ProbeLaunch,
        provenThroughProcessExit = Proven,
        ownerBeforeSha256 = OwnerBeforeHash,
        ownerAfterSha256 = OwnerAfterHash,
        ownerUnchanged = OwnerUnchanged,
        applicationProofs = _applications.Select(application => new { processId = application.Process.Id, launchIdentity = application.Launch }).ToArray(),
        applicationsProved = ApplicationsProved,
        cleaned = _disposed,
    }, new JsonSerializerOptions { WriteIndented = true });

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_applications.Any(application => !application.Process.HasExited))
        {
            throw new InvalidOperationException("Terminate and wait for isolated applications before disposing their registry scope.");
        }

        try
        {
            VerifyOwnerUnchanged();
            using RegistryKey root = RegistryRootContract.OpenValidatedRoot(RegistryPath, Nonce);
            using RegistryKey? initialized = root.OpenSubKey("InitializedProcesses", writable: false);
            foreach ((Process application, string launch) in _applications)
            {
                string id = application.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (!Equals(initialized?.GetValue(launch), application.Id))
                {
                    throw new InvalidOperationException($"The runtime-smoke child {id} has no matching startup-hook proof.");
                }
            }

            ApplicationsProved = true;
        }
        finally
        {
            using (RegistryRootContract.OpenValidatedRoot(RegistryPath, Nonce))
            {
                Registry.CurrentUser.DeleteSubKeyTree(RegistryPath, throwOnMissingSubKey: false);
            }

            _disposed = true;
        }
    }

    private string Configure(ProcessStartInfo start)
    {
        using RegistryKey root = RegistryRootContract.OpenValidatedRoot(RegistryPath, Nonce);
        if (start.UseShellExecute)
        {
            throw new InvalidOperationException("Registry isolation requires direct process creation with its private environment.");
        }

        string hook = typeof(WindowsRegistryIsolationScope).Assembly.Location;
        start.Environment.TryGetValue("DOTNET_STARTUP_HOOKS", out string? existing);
        start.Environment["DOTNET_STARTUP_HOOKS"] = string.IsNullOrEmpty(existing)
            ? hook
            : string.Join(Path.PathSeparator, new[] { hook }.Concat(existing.Split(Path.PathSeparator)
                .Where(path => !string.Equals(path, hook, StringComparison.OrdinalIgnoreCase))));
        start.Environment[RegistryRootContract.RootVariable] = RegistryPath;
        start.Environment[RegistryRootContract.NonceVariable] = Nonce;
        string launch = Guid.NewGuid().ToString("N");
        start.Environment[RegistryRootContract.LaunchVariable] = launch;
        return launch;
    }
}
