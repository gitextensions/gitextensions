using GitCommands;
using GitUI.Properties;

namespace GitUI.Shells;

public class PowerShellShell : ShellDescriptor
{
    public PowerShellShell()
    {
        Name = "powershell";
        Icon = Images.powershell;

        // Cross-platform constraint: retain the original choice while accepting a suffix-free executable off Windows.
        ExecutableName = OperatingSystem.IsWindows() ? "powershell.exe" : "powershell";
        if (PathUtil.TryFindShellPath(ExecutableName, out string? exePath))
        {
            ExecutablePath = exePath;
            ExecutableCommandLine = exePath.Quote();
        }
    }

    public override string GetChangeDirCommand(string path) => $"cd {path.QuoteNE()}";
}
