using System.Diagnostics;

namespace GitExtensionsTests;

internal static class ProcessTestHelper
{
    public static bool HasExited(int processId)
    {
        if (OperatingSystem.IsLinux())
        {
            string statPath = $"/proc/{processId}/stat";
            try
            {
                string stat = File.ReadAllText(statPath);
                int commandEnd = stat.LastIndexOf(')');
                if (commandEnd >= 0 && commandEnd + 2 < stat.Length
                    && stat[commandEnd + 2] is 'Z' or 'X')
                {
                    return true;
                }
            }
            catch (IOException exception)
                when (exception is FileNotFoundException or DirectoryNotFoundException)
            {
                return true;
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
