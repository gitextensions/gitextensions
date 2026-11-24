using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace GitExtensions.WindowsRuntimeIsolation;

// parity-scaffolding: retains each actual managed process identity independently of its inherited launch identity.
internal sealed record ManagedProcessRecord(int ProcessId, long StartTimeUtcTicks, string Launch);

// parity-scaffolding: keeps owned registry evidence available until every inheriting managed process exits.
[SupportedOSPlatform("windows")]
internal static class ManagedProcessProof
{
    private const string ProcessesKey = "ManagedProcesses";

    internal static void Record(RegistryKey root, string launch)
    {
        using Process current = Process.GetCurrentProcess();
        long startTimeUtcTicks = current.StartTime.ToUniversalTime().Ticks;
        using (RegistryKey initialized = root.CreateSubKey("InitializedProcesses", writable: true))
        {
            // An inherited child must not replace its parent's original startup proof.
            if (initialized.GetValue(launch) is null)
            {
                initialized.SetValue(launch, current.Id, RegistryValueKind.DWord);
            }
        }

        string identity = string.Create(CultureInfo.InvariantCulture, $"{current.Id}-{startTimeUtcTicks}");
        using RegistryKey process = root.CreateSubKey($@"{ProcessesKey}\{identity}", writable: true);
        process.SetValue("Launch", launch, RegistryValueKind.String);
    }

    internal static IReadOnlyList<ManagedProcessRecord> Read(RegistryKey root)
    {
        using RegistryKey? processes = root.OpenSubKey(ProcessesKey, writable: false);
        if (processes is null)
        {
            return [];
        }

        List<ManagedProcessRecord> records = [];
        foreach (string name in processes.GetSubKeyNames())
        {
            int separator = name.IndexOf('-');
            if (separator < 1
                || !int.TryParse(name.AsSpan(0, separator), NumberStyles.None, CultureInfo.InvariantCulture, out int processId)
                || processId <= 0
                || !long.TryParse(name.AsSpan(separator + 1), NumberStyles.None, CultureInfo.InvariantCulture, out long startTimeUtcTicks)
                || startTimeUtcTicks <= 0 || startTimeUtcTicks > DateTime.MaxValue.Ticks)
            {
                throw new InvalidDataException("A managed registry-isolation process identity is malformed.");
            }

            using RegistryKey process = processes.OpenSubKey(name, writable: false)
                ?? throw new InvalidDataException("A managed registry-isolation process proof disappeared.");
            string? launch = process.GetValue("Launch") as string;
            if (launch is null || !Guid.TryParseExact(launch, "N", out _))
            {
                throw new InvalidDataException("A managed registry-isolation launch proof is malformed.");
            }

            records.Add(new ManagedProcessRecord(processId, startTimeUtcTicks, launch));
        }

        return records;
    }

    internal static bool IsAlive(ManagedProcessRecord record)
    {
        try
        {
            using Process process = Process.GetProcessById(record.ProcessId);
            return !process.HasExited && process.StartTime.ToUniversalTime().Ticks == record.StartTimeUtcTicks;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException or NotSupportedException)
        {
            // An inaccessible process may still be using the redirected registry hive.
            return true;
        }
    }
}
