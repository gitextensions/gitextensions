using System;
using System.IO;
using GitCommands;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming
{
    public interface IThemePathProvider
    {
        string GetThemePath(ThemeId id);

        string AppThemesDirectory { get; }

        string? UserThemesDirectory { get; }

        string ThemeExtension { get; }
    }

    public class ThemePathProvider : IThemePathProvider
    {
        private const string Subdirectory = "Themes";

        public ThemePathProvider()
        {
            string appDirectory = AppSettings.GetGitExtensionsDirectory() ??
                throw new DirectoryNotFoundException("Application directory not found");
            AppThemesDirectory = Path.Combine(appDirectory, Subdirectory);

            string? userDirectory = AppSettings.ApplicationDataPath.Value;

            // in portable version appDirectory and userDirectory are same,
            // hence we don't have a separate directory for user themes
            UserThemesDirectory = string.Equals(appDirectory, userDirectory, StringComparison.OrdinalIgnoreCase)
                ? null
                : Path.Combine(userDirectory, Subdirectory);

            ThemeExtension = ".css";
        }

        public string AppThemesDirectory { get; }

        public string? UserThemesDirectory { get; }

        public string ThemeExtension { get; }

        /// <exception cref="InvalidOperationException">
        /// Attempt to resolve a custom theme from a %UserAppData% folder in a portable version.
        /// </exception>
        /// <exception cref="FileNotFoundException">Theme does not exist.</exception>
        public string GetThemePath(ThemeId id)
        {
            string path;
            if (id.IsBuiltin)
            {
                path = Path.Combine(AppThemesDirectory, id.Name + ThemeExtension);
            }
            else
            {
                if (UserThemesDirectory is null)
                {
                    throw new InvalidOperationException("Portable mode only supports local themes");
                }

                path = Path.Combine(UserThemesDirectory, id.Name + ThemeExtension);
            }

            return path;
        }
    }
}
