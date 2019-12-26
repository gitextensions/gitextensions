using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitCommands;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming
{
    public class ThemeRepository
    {
        private const string Subdirectory = "Themes";
        private const string Extension = ".colors";
        private const string InvariantThemeName = "win10default";

        private readonly ThemePersistence _persistence;

        public ThemeRepository(ThemePersistence persistence)
        {
            _persistence = persistence;
            string appDirectory = AppSettings.GetGitExtensionsDirectory() ??
                throw new InvalidOperationException("Missing application directory");
            AppDirectory = Path.Combine(appDirectory, Subdirectory);
            DataDirectory = Path.Combine(AppSettings.ApplicationDataPath.Value, Subdirectory);
        }

        private string AppDirectory { get; }
        private string DataDirectory { get; }

        public Theme GetTheme(ThemeId id)
        {
            string directory = id.IsBuiltin
                ? AppDirectory
                : DataDirectory;
            string path = Path.Combine(directory, id.Name + Extension);
            return _persistence.LoadFile(path, id);
        }

        public Theme GetInvariantTheme() =>
            GetTheme(new ThemeId(InvariantThemeName, true));

        public IEnumerable<ThemeId> GetThemeIds() =>
            GetBuiltinThemeIds().Concat(GetUserCustomizedThemeIds());

        private IEnumerable<ThemeId> GetBuiltinThemeIds() =>
            new DirectoryInfo(AppDirectory)
                .EnumerateFiles("*" + Extension, SearchOption.TopDirectoryOnly)
                .Select(_ => Path.GetFileNameWithoutExtension(_.Name))
                .Where(name => !name.Equals(InvariantThemeName, StringComparison.OrdinalIgnoreCase))
                .Select(name => new ThemeId(name, true));

        private IEnumerable<ThemeId> GetUserCustomizedThemeIds()
        {
            var directory = new DirectoryInfo(DataDirectory);
            return directory.Exists
                ? directory
                    .EnumerateFiles("*" + Extension, SearchOption.TopDirectoryOnly)
                    .Select(_ => Path.GetFileNameWithoutExtension(_.Name))
                    .Select(name => new ThemeId(name, false))
                : Enumerable.Empty<ThemeId>();
        }
    }
}
