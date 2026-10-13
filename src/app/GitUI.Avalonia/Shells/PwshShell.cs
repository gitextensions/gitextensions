using GitCommands;
using GitUI.Properties;

namespace GitUI.Shells;

public class PwshShell : ShellDescriptor
{
    public PwshShell()
    {
        Name = "pwsh";
        Icon = Images.pwsh;

        // Cross-platform constraint: PowerShell installs without the .exe suffix on Linux and macOS.
        ExecutableName = OperatingSystem.IsWindows() ? "pwsh.exe" : "pwsh";
        if (PathUtil.TryFindShellPath(ExecutableName, out string? exePath))
        {
            ExecutablePath = exePath;
            ExecutableCommandLine = exePath.Quote();
        }
    }

    public override string GetChangeDirCommand(string path) => $"cd {path.QuoteNE()}";
}
