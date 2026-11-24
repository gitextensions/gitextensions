using System.Diagnostics;

namespace GitExtensionsTests;

internal static class ProcessTestHelper
{
    public static bool HasExited(int processId)
        => HasExited(processId, File.ReadAllText);

    internal static bool HasExited(int processId, Func<string, string> readStat)
    {
        if (OperatingSystem.IsLinux())
        {
            string statPath = $"/proc/{processId}/stat";
            try
            {
                string stat = readStat(statPath);
                int commandEnd = stat.LastIndexOf(')');
                if (commandEnd >= 0 && commandEnd + 2 < stat.Length
                    && stat[commandEnd + 2] is 'Z' or 'X')
                {
                    return true;
                }
            }
            catch (IOException)
            {
                // procfs can lose an opened stat entry during exit; confirm the process lifetime below.
            }
        }

        try
        {
            using Process process = Process.GetProcessById(processId);
            return process.HasExited;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }
}
