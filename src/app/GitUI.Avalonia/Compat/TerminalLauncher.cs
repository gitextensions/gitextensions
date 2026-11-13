using System.Diagnostics;

namespace GitUI.Compat;

internal interface ITerminalLauncher
{
    void Launch(string workingDirectory);

    void LaunchShell(string workingDirectory, string executablePath);
}

internal sealed class TerminalLauncher : ITerminalLauncher
{
    private readonly Func<string, string?> _getEnvironmentVariable;
    private readonly Func<bool> _isFlatpak;
    private readonly Func<string, string?> _resolveExecutable;
    private readonly Action<ProcessStartInfo> _startProcess;
    private readonly Func<string, LocatedTool?> _locateTool;
    private readonly TerminalPlatform _platform;

    public TerminalLauncher()
        : this(
            Environment.GetEnvironmentVariable,
            ResolveExecutable,
            startInfo => Process.Start(startInfo),
            GetCurrentPlatform(),
            FlatpakEnvironment.IsFlatpak,
            executable => PlatformToolLocator.Current.Find(executable))
    {
    }

    internal TerminalLauncher(
        Func<string, string?> getEnvironmentVariable,
        Func<string, string?> resolveExecutable,
        Action<ProcessStartInfo> startProcess,
        TerminalPlatform platform,
        Func<bool>? isFlatpak = null,
        Func<string, LocatedTool?>? locateTool = null)
    {
        _getEnvironmentVariable = getEnvironmentVariable;
        _isFlatpak = isFlatpak ?? (() => false);
        _resolveExecutable = resolveExecutable;
        _startProcess = startProcess;
        _locateTool = locateTool ?? (_ => null);
        _platform = platform;
    }

    public void Launch(string workingDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);

        ProcessStartInfo startInfo = CreateStartInfo(workingDirectory);
        _startProcess(startInfo);
    }

    public void LaunchShell(string workingDirectory, string executablePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        if (_platform != TerminalPlatform.Windows)
        {
            throw new PlatformNotSupportedException("A shell executable cannot open a graphical terminal on this platform.");
        }

        // throwOnErrorExit would redirect the output
        _startProcess(new ProcessStartInfo
        {
            FileName = executablePath,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
        });
    }

    private ProcessStartInfo CreateStartInfo(string workingDirectory)
    {
        ProcessStartInfo startInfo = new()
        {
            UseShellExecute = false,
            WorkingDirectory = workingDirectory,
        };

        switch (_platform)
        {
            case TerminalPlatform.Windows:
                startInfo.FileName = FindRequiredExecutable("wt.exe", "cmd.exe");
                if (startInfo.FileName.EndsWith("wt.exe", StringComparison.OrdinalIgnoreCase))
                {
                    startInfo.ArgumentList.Add("-d");
                    startInfo.ArgumentList.Add(workingDirectory);
                }

                break;

            case TerminalPlatform.MacOS:
                startInfo.FileName = FindRequiredExecutable("open");
                startInfo.ArgumentList.Add("-a");
                startInfo.ArgumentList.Add("Terminal");
                startInfo.ArgumentList.Add(workingDirectory);
                break;

            default:
                string? configuredTerminal = _getEnvironmentVariable("TERMINAL");
                if (!string.IsNullOrWhiteSpace(configuredTerminal))
                {
                    SetTerminal(FindRequiredTool(configuredTerminal));
                }
                else if (_resolveExecutable("xdg-terminal-exec") is string desktopTerminalLauncher)
                {
                    startInfo.FileName = desktopTerminalLauncher;
                    startInfo.ArgumentList.Add($"--dir={workingDirectory}");
                }
                else
                {
                    SetTerminal(FindRequiredTool("x-terminal-emulator", "gnome-terminal", "konsole", "xterm"));
                }

                SanitizeLinuxEnvironment(startInfo.Environment);
                if (_isFlatpak())
                {
                    FlatpakHost.ForwardToHost(startInfo);
                }

                break;
        }

        return startInfo;

        void SetTerminal(LocatedTool tool)
        {
            startInfo.FileName = tool.Executable;
            foreach (string argument in tool.PrefixArguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            if (tool.PrefixArguments.SequenceEqual(["run", "org.kde.konsole"]))
            {
                startInfo.ArgumentList.Add("--workdir");
                startInfo.ArgumentList.Add(workingDirectory);
            }
        }
    }

    internal static void SanitizeLinuxEnvironment(IDictionary<string, string?> environment)
    {
        string[] snapVariables = [.. environment.Keys.Where(key => key == "SNAP" || key.StartsWith("SNAP_", StringComparison.Ordinal))];
        foreach (string variable in snapVariables)
        {
            environment.Remove(variable);
        }

        string[] toolkitVariables =
        [
            "GDK_PIXBUF_MODULE_FILE",
            "GIO_MODULE_DIR",
            "GI_TYPELIB_PATH",
            "GTK_EXE_PREFIX",
            "GTK_IM_MODULE_FILE",
            "GTK_PATH",
            "LD_LIBRARY_PATH",
        ];
        foreach (string variable in toolkitVariables)
        {
            if (environment.TryGetValue(variable, out string? value)
                && value?.Contains("/snap/", StringComparison.Ordinal) is true)
            {
                environment.Remove(variable);
            }
        }

        RestoreOriginal("XDG_CONFIG_DIRS");
        RestoreOriginal("XDG_DATA_DIRS");

        void RestoreOriginal(string variable)
        {
            string originalVariable = $"{variable}_VSCODE_SNAP_ORIG";
            if (environment.Remove(originalVariable, out string? originalValue))
            {
                environment[variable] = originalValue;
            }
        }
    }

    private string FindRequiredExecutable(params string[] candidates)
        => FindRequiredTool(candidates).Executable;

    private LocatedTool FindRequiredTool(params string[] candidates)
    {
        foreach (string candidate in candidates)
        {
            string? executable = _resolveExecutable(candidate);
            if (executable is not null)
            {
                return new LocatedTool(executable, []);
            }
        }

        foreach (string candidate in candidates)
        {
            if (_locateTool(candidate) is LocatedTool tool)
            {
                return tool;
            }
        }

        throw new FileNotFoundException($"No supported terminal executable was found ({string.Join(", ", candidates)}).");
    }

    private static string? ResolveExecutable(string executable)
    {
        if (FlatpakEnvironment.IsFlatpak())
        {
            return FlatpakHost.Default.ResolveExecutable(executable);
        }

        if (Path.IsPathRooted(executable) || executable.Contains(Path.DirectorySeparatorChar))
        {
            return File.Exists(executable) ? executable : null;
        }

        string[] searchDirectories = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        return searchDirectories
            .Select(directory => Path.Combine(directory, executable))
            .FirstOrDefault(File.Exists);
    }

    private static TerminalPlatform GetCurrentPlatform()
        => OperatingSystem.IsWindows() ? TerminalPlatform.Windows
            : OperatingSystem.IsMacOS() ? TerminalPlatform.MacOS
            : TerminalPlatform.Linux;
}

internal enum TerminalPlatform
{
    Linux,
    Windows,
    MacOS,
}
