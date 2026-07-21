using GitCommands;
using GitExtensions.Extensibility;

namespace GitUI.ScriptsEngine;

public static class PowerShellHelper
{
    internal static void RunPowerShell(string command, string? argument, string workingDir, bool runInBackground)
    {
        string executableName = GetExecutableName();
        string arguments = (runInBackground ? string.Empty : "-NoExit")
            + " -ExecutionPolicy Unrestricted -Command \"" + command + " " + argument + "\"";
        EnvironmentConfiguration.SetEnvironmentVariables();

        IExecutable executable = new Executable(executableName, workingDir);
        executable.Start(arguments, createWindow: !runInBackground);
    }

    internal static string GetExecutableName()
        => OperatingSystem.IsWindows() ? "powershell.exe" : "pwsh";
}
