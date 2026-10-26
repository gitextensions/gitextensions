namespace GitUI.Shells;

public class ShellProvider : IShellProvider
{
    private static IShellDescriptor DefaultShell = new BashShell();
    private static readonly IShellDescriptor[] Shells = [DefaultShell, new CmdShell(), new PwshShell(), new PowerShellShell()];

    public IReadOnlyList<IShellDescriptor> GetShells() => Shells;

    public IShellDescriptor GetShell(string? name) => Shells.FirstOrDefault(s => s.Name == name) ?? DefaultShell;

    public string GetShellCommandLine(string? shellType)
    {
        IShellDescriptor shell = GetShell(shellType);

        if (!shell.HasExecutable || shell.ExecutableCommandLine is null)
        {
            // Fallback to default if ExecutableCommandLine is not set
            // Cross-platform constraint: the Avalonia console does not reference ConEmu, so use the platform console shell.
            return OperatingSystem.IsWindows()
                ? "cmd.exe"
                : Environment.GetEnvironmentVariable("SHELL") ?? "/bin/sh";
        }

        return shell.ExecutableCommandLine;
    }
}
