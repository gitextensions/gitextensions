using Microsoft.Win32;

namespace GitExtensions.WindowsRuntimeIsolation;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args is not ["--registry-probe"])
        {
            Console.Error.WriteLine("This tool only accepts --registry-probe.");
            return 2;
        }

        string path = Environment.GetEnvironmentVariable(RegistryRootContract.RootVariable) ?? string.Empty;
        RegistryRootContract.ValidatePath(path);
        string launch = Environment.GetEnvironmentVariable(RegistryRootContract.LaunchVariable) ?? string.Empty;

        // Even without a startup hook this probe writes only the authorized test path, never the owner key.
        using (RegistryKey probe = Registry.CurrentUser.CreateSubKey($"{path}\\Probe", writable: true))
        {
            probe.SetValue("Early", launch);
        }

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            using RegistryKey probe = Registry.CurrentUser.CreateSubKey($"{path}\\Probe", writable: true);
            probe.SetValue("Late", launch);
        };
        return 0;
    }
}
