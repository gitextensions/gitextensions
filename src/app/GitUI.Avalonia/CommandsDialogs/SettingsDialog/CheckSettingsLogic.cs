using System.Runtime.Versioning;
using GitCommands;
using GitCommands.Git;
using Microsoft.Win32;

namespace GitUI.CommandsDialogs.SettingsDialog;

/// <summary>Portable settings validation used by settings pages as they are ported.</summary>
public class CheckSettingsLogic
{
    public readonly CommonLogic CommonLogic;

    public CheckSettingsLogic(CommonLogic commonLogic)
    {
        CommonLogic = commonLogic;
    }

    public bool AutoSolveAllSettings()
    {
        bool valid = SolveGitCommand();
        valid = SolveLinuxToolsDir() && valid;
        valid = SolveGitExtensionsDir() && valid;
        valid = SolveEditor(CommonLogic) && valid;
        CommonLogic.GitConfigSettingsSet.Save();
        CommonLogic.DistributedSettingsSet.Save();
        return valid;
    }

    public static bool SolveEditor(CommonLogic commonLogic)
    {
        if (string.IsNullOrEmpty(commonLogic.GetGlobalEditor()))
        {
            Environment.SetEnvironmentVariable(CommonLogic.AmbientGitEditorEnvVariableName, EditorHelper.GetDefaultEditor());
        }

        return true;
    }

    public static bool SolveLinuxToolsDir(string? possibleNewPath = null)
    {
        if (!OperatingSystem.IsWindows())
        {
            AppSettings.LinuxToolsDir = string.Empty;
            return true;
        }

        string candidate = string.IsNullOrWhiteSpace(possibleNewPath)
            ? AppSettings.LinuxToolsDir
            : possibleNewPath.Trim();
        if (ContainsShell(candidate))
        {
            AppSettings.LinuxToolsDir = candidate;
            return true;
        }

        string gitCommand = AppSettings.GitCommandValue;
        string[] derivedCandidates =
        [
            gitCommand.Replace(@"\cmd\git.exe", @"\usr\bin\", StringComparison.OrdinalIgnoreCase)
                .Replace(@"\cmd\git.cmd", @"\usr\bin\", StringComparison.OrdinalIgnoreCase),
            gitCommand.Replace(@"\cmd\git.exe", @"\bin\", StringComparison.OrdinalIgnoreCase)
                .Replace(@"\cmd\git.cmd", @"\bin\", StringComparison.OrdinalIgnoreCase),
        ];
        string? detected = derivedCandidates.FirstOrDefault(ContainsShell);
        if (detected is not null)
        {
            AppSettings.LinuxToolsDir = detected;
            return true;
        }

        if (CheckIfFileIsInPath("sh.exe") || CheckIfFileIsInPath("sh"))
        {
            AppSettings.LinuxToolsDir = string.Empty;
            return true;
        }

        return false;

        static bool ContainsShell(string path)
            => Directory.Exists(path)
                && (File.Exists(Path.Join(path, "sh.exe")) || File.Exists(Path.Join(path, "sh")));
    }

    [SupportedOSPlatform("windows")]
    private static IEnumerable<string> GetGitLocations()
    {
        string? envVariable = Environment.GetEnvironmentVariable("GITEXT_GIT");
        if (!string.IsNullOrEmpty(envVariable))
        {
            yield return envVariable;
        }

        yield return
            CommonLogic.GetRegistryValue(Registry.LocalMachine,
                             "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1", "InstallLocation");
        string? programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
        string? programFilesX86 = null;
        if (IntPtr.Size == 8
            || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))
        {
            programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
        }

        if (programFiles is not null)
        {
            yield return programFiles + @"\Git\";
        }

        if (programFilesX86 is not null)
        {
            yield return programFilesX86 + @"\Git\";
        }

        if (programFiles is not null)
        {
            yield return programFiles + @"\msysgit\";
        }

        if (programFilesX86 is not null)
        {
            yield return programFilesX86 + @"\msysgit\";
        }

        yield return @"C:\msysgit\";

        // cygwin has old git version on windows and bash has a lot of bugs
        yield return @"C:\cygwin\";
        yield return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs", "Git\\");
    }

    public static bool SolveGitExtensionsDir()
    {
        string directory = Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory);
        if (!Directory.Exists(directory))
        {
            return false;
        }

        AppSettings.SetInstallDir(directory);
        return true;
    }

    public static bool SolveGitCommand(string? possibleNewPath = null)
    {
        IEnumerable<string> candidates = OperatingSystem.IsWindows()
            ? GetWindowsCommandLocations(possibleNewPath)
            : GetPortableCommandLocations(possibleNewPath);

        foreach (string command in candidates)
        {
            try
            {
                // Use cached version if possible
                if (AppSettings.GitCommand == command && GitVersion.Current?.IsUnknown is false)
                {
                    return true;
                }

                if (!string.IsNullOrEmpty(new Executable(command).GetOutput(arguments: "--version")))
                {
                    AppSettings.GitCommandValue = command;
                    return true;
                }
            }
            catch (Exception)
            {
                // Ignore exception, we are trying to find a way to execute git
            }
        }

        return false;
    }

    public static bool CheckIfFileIsInPath(string fileName)
        => PathUtil.TryFindFullPath(fileName, out _);

    public bool CanFindGitCmd()
        => !string.IsNullOrEmpty(CommonLogic.Module.GitExecutable.GetOutput(arguments: "--version"));

    [SupportedOSPlatform("windows")]
    private static IEnumerable<string> GetWindowsCommandLocations(string? possibleNewPath)
    {
        if (File.Exists(possibleNewPath))
        {
            yield return possibleNewPath!;
        }

        if (File.Exists(AppSettings.GitCommandValue))
        {
            yield return AppSettings.GitCommandValue;
        }

        foreach (string path in GetGitLocations())
        {
            if (Directory.Exists(path + @"bin\"))
            {
                yield return path + @"bin\git.exe";
            }
        }

        foreach (string path in GetGitLocations())
        {
            if (Directory.Exists(path + @"cmd\"))
            {
                yield return path + @"cmd\git.exe";
                yield return path + @"cmd\git.cmd";
            }
        }

        yield return "git";
        yield return "git.cmd";
    }

    private static IEnumerable<string> GetPortableCommandLocations(string? possibleNewPath)
    {
        if (!string.IsNullOrWhiteSpace(possibleNewPath))
        {
            yield return possibleNewPath.Trim();
        }

        if (!string.IsNullOrWhiteSpace(AppSettings.GitCommandValue))
        {
            yield return AppSettings.GitCommandValue;
        }

        yield return "git";
    }
}
