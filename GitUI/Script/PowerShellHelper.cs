using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.Script
{
    public static class PowerShellHelper
    {
        internal static void RunPowerShell(string command, string? argument, string workingDir, bool runInBackground)
        {
            const string filename = "powershell.exe";
            var arguments = (runInBackground ? "" : "-NoExit") + " -ExecutionPolicy Unrestricted -Command \"" + command + " " + argument + "\"";
            EnvironmentConfiguration.SetEnvironmentVariables();

            IExecutable executable = new Executable(filename, workingDir);
            executable.Start(arguments, createWindow: !runInBackground);
        }
    }
}
