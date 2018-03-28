using System;
using System.Diagnostics;
using GitCommands;

namespace GitUI.Script
{
    public static class PowerShellHelper
    {
        internal static void RunPowerShell(string command, string argument, string workingDir, bool runInBackground)
        {
            const string filename = "powershell.exe";
            var psarguments = (runInBackground ? "" : "-NoExit") + " -ExecutionPolicy Unrestricted -Command \"" + command + " " + argument + "\"";
            EnvironmentConfiguration.SetEnvironmentVariables();

            var executionStartTimestamp = DateTime.Now;

            var startInfo = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = psarguments,
                WorkingDirectory = workingDir,
                UseShellExecute = false
            };

            var startProcess = Process.Start(startInfo);

            startProcess.Exited += (sender, args) =>
            {
                var executionEndTimestamp = DateTime.Now;
                AppSettings.GitLog.Log(filename + " " + psarguments, executionStartTimestamp, executionEndTimestamp);
            };
        }
    }
}
