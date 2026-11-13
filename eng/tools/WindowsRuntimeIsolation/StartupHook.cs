using System.ComponentModel;
using System.Runtime.InteropServices;
using GitExtensions.WindowsRuntimeIsolation;
using Microsoft.Win32;

// parity-scaffolding: confines all managed runtime-smoke HKCU access before application startup.
internal static class StartupHook
{
    private static RegistryKey? _redirectedHive;

    public static void Initialize()
    {
        string path = Environment.GetEnvironmentVariable(RegistryRootContract.RootVariable) ?? string.Empty;
        string nonce = Environment.GetEnvironmentVariable(RegistryRootContract.NonceVariable) ?? string.Empty;
        string launch = Environment.GetEnvironmentVariable(RegistryRootContract.LaunchVariable) ?? string.Empty;
        if (!Guid.TryParseExact(launch, "N", out _))
        {
            throw new InvalidOperationException("Runtime isolation launch identity is malformed.");
        }

        RegistryKey root = RegistryRootContract.OpenValidatedRoot(path, nonce);
        int error = RegOverridePredefKey(unchecked((nint)(int)0x80000001), root.Handle.DangerousGetHandle());
        if (error != 0)
        {
            root.Dispose();
            throw new Win32Exception(error);
        }

        // Keep the override and handle alive through every ProcessExit callback; only OS teardown releases them.
        _redirectedHive = root;
        using RegistryKey initialized = _redirectedHive.CreateSubKey("InitializedProcesses", writable: true);
        initialized.SetValue(launch, Environment.ProcessId, RegistryValueKind.DWord);
    }

    [DllImport("advapi32.dll", ExactSpelling = true)]
    private static extern int RegOverridePredefKey(nint key, nint replacement);
}
