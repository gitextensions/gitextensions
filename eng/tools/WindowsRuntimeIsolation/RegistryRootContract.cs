using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace GitExtensions.WindowsRuntimeIsolation;

internal static class RegistryRootContract
{
    internal const string TestRoot = @"Software\GitExtensions.Avalonia.Tests";
    internal const string RootVariable = "GITEXT_SMOKE_REGISTRY_ROOT";
    internal const string NonceVariable = "GITEXT_SMOKE_REGISTRY_NONCE";
    internal const string LaunchVariable = "GITEXT_SMOKE_REGISTRY_LAUNCH";

    internal static void ValidatePath(string path)
    {
        if (!Regex.IsMatch(path, @"\ASoftware\\GitExtensions\.Avalonia\.Tests\\[1-9][0-9]*-[0-9a-f]{32}\z", RegexOptions.CultureInvariant))
        {
            throw new InvalidOperationException("Runtime isolation requires an exact owned PID/GUID test subkey.");
        }
    }

    internal static RegistryKey OpenValidatedRoot(string path, string nonce)
    {
        ValidatePath(path);
        if (!Regex.IsMatch(nonce, @"\A[0-9a-f]{32}\z", RegexOptions.CultureInvariant))
        {
            throw new InvalidOperationException("Runtime isolation nonce is malformed.");
        }

        RegistryKey root = Registry.CurrentUser.OpenSubKey(path, writable: true)
            ?? throw new InvalidOperationException("The controller's runtime isolation key is missing.");
        if (!string.Equals(root.GetValue("Nonce") as string, nonce, StringComparison.Ordinal))
        {
            root.Dispose();
            throw new InvalidOperationException("Runtime isolation nonce does not match the controller's key.");
        }

        return root;
    }
}
