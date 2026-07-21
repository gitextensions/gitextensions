using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

internal sealed class GitConfigSettingsPageController
{
    public string GetInitialDirectory(string path, string toolPreferredPath)
        => CalculateInitialDirectory(path)
            ?? CalculateInitialDirectory(toolPreferredPath)
            ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    private static string? CalculateInitialDirectory(string suppliedPath)
    {
        if (string.IsNullOrWhiteSpace(suppliedPath))
        {
            return null;
        }

        if (Directory.Exists(suppliedPath))
        {
            suppliedPath = suppliedPath.EnsureTrailingPathSeparator();
        }

        string initialDirectory = Path.GetDirectoryName(suppliedPath) ?? suppliedPath;
        return !string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory)
            ? initialDirectory.EnsureTrailingPathSeparator()
            : null;
    }
}
