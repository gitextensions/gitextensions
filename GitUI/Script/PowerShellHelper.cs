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
            var psarguments = (runInBackground ? "" : "-NoExit") + " -ExecutionPolicy Unrestricted -Command \"" + command + " " + argument + "\"";
            EnvironmentConfiguration.SetEnvironmentVariables();

            var startInfo = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = psarguments,
                WorkingDirectory = workingDir,
                UseShellExecute = false
            };

            var operation = CommandLog.LogProcessStart(filename, psarguments);
            var startProcess = Process.Start(startInfo);

            startProcess.Exited += (s, e) => operation.LogProcessEnd();
        }
    }
}
