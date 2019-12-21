using System;
using System.IO;
using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    internal class GitConfigSettingsPageController
    {
        public string GetInitialDirectory(string path, string toolPreferredPath) =>
            CalculateInitialDirectory(path) ??
            CalculateInitialDirectory(toolPreferredPath) ??
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        private static string CalculateInitialDirectory(string suppliedPath)
        {
            if (string.IsNullOrWhiteSpace(suppliedPath))
            {
                return null;
            }

            try
            {
                // the path can be either a folder or a file
                // if the path is a folder but lacks the trailing slash, Path.GetDirectoryName will return the parent directory
                // so we want to ensure the supplied directory is slash-terminated
                if (Directory.Exists(suppliedPath))
                {
                    suppliedPath = suppliedPath.EnsureTrailingPathSeparator();
                }

                // Path.GetDirectoryName returns directory information for path, or null if path denotes a root directory or is null.
                // Returns Empty if path does not contain directory information.
                string initialDirectory = Path.GetDirectoryName(suppliedPath) ?? suppliedPath;
                if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
                {
                    return initialDirectory.EnsureTrailingPathSeparator();
                }
            }
            catch (ArgumentException)
            {
                // likely there was a problem with a path
            }

            return null;
        }
    }
}
