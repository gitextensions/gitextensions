using System.Diagnostics;
using GitCommands;
using GitCommands.Logging;

namespace GitUI.Script
{
    public static class PowerShellHelper
    {
        internal static void RunPowerShell(string command, string argument, string workingDir, bool runInBackground)
        {
            const string filename = "powershell.exe";
            var arguments = (runInBackground ? "" : "-NoExit") + " -ExecutionPolicy Unrestricted -Command \"" + command + " " + argument + "\"";
            EnvironmentConfiguration.SetEnvironmentVariables();

            var startInfo = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = arguments,
                WorkingDirectory = workingDir,
                UseShellExecute = false
            };

            var operation = CommandLog.LogProcessStart(filename, arguments, workingDir);
            var process = Process.Start(startInfo);
            operation.SetProcessId(process.Id);
            process.Exited += (s, e) => operation.LogProcessEnd();
        }
    }
}
