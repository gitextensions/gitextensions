using GitCommands;
using GitCommands.Git;
using GitUIPluginInterfaces;

namespace GitUI.Script
{
    public static class PowerShellHelper
    {
        internal static void RunPowerShell(string command, string argument, string workingDir, bool runInBackground)
        {
            const string filename = "powershell.exe";
            var arguments = (runInBackground ? "" : "-NoExit") + " -ExecutionPolicy Unrestricted -Command \"" + command + " " + argument + "\"";
            EnvironmentConfiguration.SetEnvironmentVariables();
            ExecutableFactory.Default.Create(filename, workingDir).Start(arguments);
        }
    }
}
