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

            var startInfo = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = psarguments,
                WorkingDirectory = workingDir,
                UseShellExecute = false
            };

            var startCmd = AppSettings.GitLog.Log(filename, psarguments);
            var startProcess = Process.Start(startInfo);

            startProcess.Exited += (sender, args) =>
            {
                startCmd.LogEnd();
            };
        }
    }
}
