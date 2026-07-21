using GitCommands;
using GitCommands.Git;

namespace GitUI.CommandsDialogs.SettingsDialog;

/// <summary>Portable settings validation used by settings pages as they are ported.</summary>
public class CheckSettingsLogic(CommonLogic commonLogic)
{
    public readonly CommonLogic CommonLogic = commonLogic;

    public static bool SolveEditor(CommonLogic commonLogic)
    {
        if (string.IsNullOrEmpty(commonLogic.GetGlobalEditor()))
        {
            Environment.SetEnvironmentVariable(CommonLogic.AmbientGitEditorEnvVariableName, AppSettings.FileEditorCommand);
        }

        return true;
    }

    public static bool SolveGitExtensionsDir()
    {
        string? directory = AppSettings.GetGitExtensionsDirectory();
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
                // Finding Git is deliberately best-effort.
            }
        }

        return false;
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

    public static bool CheckIfFileIsInPath(string fileName)
        => PathUtil.TryFindFullPath(fileName, out _);

    public bool CanFindGitCmd()
        => !string.IsNullOrEmpty(CommonLogic.Module.GitExecutable.GetOutput(arguments: "--version"));

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

        string? programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string? programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        foreach (string root in new[] { programFiles, programFilesX86 }.Where(path => !string.IsNullOrWhiteSpace(path)))
        {
            yield return Path.Combine(root!, "Git", "cmd", "git.exe");
            yield return Path.Combine(root!, "Git", "bin", "git.exe");
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
