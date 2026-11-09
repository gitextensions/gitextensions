using System.Diagnostics;
using System.Text.Json;
using Microsoft.Win32;

namespace GitExtensions.WindowsRuntimeIsolation;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args is ["--registry-inherited-child"])
        {
            return 0;
        }

        if (args is ["--registry-inherited-parent"])
        {
            ProcessStartInfo start = new(Environment.ProcessPath
                ?? throw new InvalidOperationException("The owned registry helper has no executable path."))
            {
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            start.ArgumentList.Add("--registry-inherited-child");
            using Process child = Process.Start(start)
                ?? throw new InvalidOperationException("Cannot start the owned registry descendant.");
            if (!child.WaitForExit(30000))
            {
                child.Kill(entireProcessTree: true);
                child.WaitForExit();
                throw new TimeoutException("The owned registry descendant did not exit.");
            }

            return child.ExitCode;
        }

        if (args is ["--registry-inherited-child-wait", string childDirectory])
        {
            using Process current = Process.GetCurrentProcess();
            File.WriteAllText(Path.Combine(childDirectory, "child-ready.json"), JsonSerializer.Serialize(new
            {
                processId = current.Id,
                startTimeUtcTicks = current.StartTime.ToUniversalTime().Ticks,
                launch = Environment.GetEnvironmentVariable(RegistryRootContract.LaunchVariable),
            }));
            Stopwatch timeout = Stopwatch.StartNew();
            while (!File.Exists(Path.Combine(childDirectory, "stop-child")) && timeout.Elapsed < TimeSpan.FromSeconds(30))
            {
                Thread.Sleep(25);
            }

            return File.Exists(Path.Combine(childDirectory, "stop-child")) ? 0 : 8;
        }

        if (args is ["--registry-inherited-parent-wait", string parentDirectory])
        {
            ProcessStartInfo start = new(Environment.ProcessPath
                ?? throw new InvalidOperationException("The owned registry helper has no executable path."))
            {
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            start.ArgumentList.Add("--registry-inherited-child-wait");
            start.ArgumentList.Add(parentDirectory);
            using Process child = Process.Start(start)
                ?? throw new InvalidOperationException("Cannot start the owned registry descendant.");
            Stopwatch timeout = Stopwatch.StartNew();
            while (!File.Exists(Path.Combine(parentDirectory, "child-ready.json")) && !child.HasExited
                && timeout.Elapsed < TimeSpan.FromSeconds(20))
            {
                Thread.Sleep(25);
            }

            if (File.Exists(Path.Combine(parentDirectory, "child-ready.json")))
            {
                return 0;
            }

            if (!child.HasExited)
            {
                child.Kill(entireProcessTree: true);
                child.WaitForExit();
            }

            Console.Error.WriteLine("The owned registry descendant did not initialize.");
            return 9;
        }

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
