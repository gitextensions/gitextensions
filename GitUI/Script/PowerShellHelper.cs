using GitCommands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GitUI.Script
{
    public static class PowerShellHelper
    {
        internal static void RunPowerShell(string command, string argument,string workingDir, bool runInBackground)
        {
            var filename = "powershell.exe";
            var psarguments = (runInBackground ? "" : "-NoExit") + " -ExecutionPolicy Unrestricted -Command \""+command+" "+argument+"\"";
            GitCommandHelpers.SetEnvironmentVariable();

            var executionStartTimestamp = DateTime.Now;

            var startInfo = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = psarguments,
                WorkingDirectory = workingDir
            };
            startInfo.UseShellExecute = false;

            var startProcess = Process.Start(startInfo);

            startProcess.Exited += (sender, args) =>
            {
                var executionEndTimestamp = DateTime.Now;
                AppSettings.GitLog.Log(filename + " " + psarguments, executionStartTimestamp, executionEndTimestamp);
            };

        }
    }
}
