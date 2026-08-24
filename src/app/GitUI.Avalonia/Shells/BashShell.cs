using GitCommands;
using GitUI.Properties;

namespace GitUI.Shells;

public class BashShell : ShellDescriptor
{
    private const string GitBashExe = "git-bash.exe"; // Bash with git in the path, should generally be in the git dir
    private const string BashExe = "bash.exe"; // Fallback to generic bash, should generally be in the git bin dir
    private const string ShExe = "sh.exe";     // Fallback to SH
    public const string ShellName = "bash";

    public BashShell()
    {
        Name = ShellName;
        Icon = Images.GitForWindows;

        // Cross-platform constraint: executable names omit the Windows suffix on Unix-like systems.
        string gitBashExecutable = OperatingSystem.IsWindows() ? GitBashExe : "bash";
        string[] shellExecutableNames = OperatingSystem.IsWindows() ? [BashExe, ShExe] : ["bash", "sh"];
        if (PathUtil.TryFindShellPath(gitBashExecutable, out string? exePath))
        {
            ExecutableName = gitBashExecutable;
            ExecutablePath = exePath;

            // Try to find bash or sh below to set ExecutableCommandLine, as git-bash.exe cannot be connected to the built-in console.
        }

        foreach (string shellExecutableName in shellExecutableNames)
        {
            if (PathUtil.TryFindShellPath(shellExecutableName, out exePath))
            {
                if (ExecutablePath is null)
                {
                    ExecutableName = shellExecutableName;
                    ExecutablePath = exePath;
                }

                ExecutableCommandLine = $"{exePath.Quote()} --login -i";

                break;
            }
        }

        ExecutableName ??= gitBashExecutable;
    }

    public override string GetChangeDirCommand(string path)
    {
        try
        {
            DirectoryInfo directoryInfo = new(path);
            if (directoryInfo.Exists)
            {
                // Cross-platform constraint: only Git Bash requires conversion from a Windows drive path.
                string posixPath = OperatingSystem.IsWindows()
                    ? "/" + directoryInfo.FullName.ToPosixPath().Remove(1, 1)
                    : directoryInfo.FullName;
                return $"cd {posixPath.QuoteNE()}";
            }
        }
        catch
        {
            // no-op
        }

        return string.Empty;
    }
}
