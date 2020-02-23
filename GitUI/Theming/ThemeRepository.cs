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
        private const string Extension = ".css";

        private readonly ThemePersistence _persistence;

        public ThemeRepository(ThemePersistence persistence)
        {
            _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
            string appDirectory = AppSettings.GetGitExtensionsDirectory() ??
                throw new InvalidOperationException("Missing application directory");
            AppDirectory = Path.Combine(appDirectory, Subdirectory);
            DataDirectory = Path.Combine(AppSettings.ApplicationDataPath.Value, Subdirectory);
        }

        private string AppDirectory { get; }
        private string DataDirectory { get; }
        public string InvariantThemeName { get; } = "win10default";

        public Theme GetTheme(ThemeId id)
        {
            string path = GetPath(id);
            return _persistence.Load(path, id);
        }

        public void Save(Theme theme) =>
            _persistence.Save(theme, GetPath(theme.Id));

        public Theme GetInvariantTheme() =>
            GetTheme(new ThemeId(InvariantThemeName, isBuiltin: true));

        public IEnumerable<ThemeId> GetThemeIds() =>
            GetBuiltinThemeIds().Concat(GetUserCustomizedThemeIds());

        private IEnumerable<ThemeId> GetBuiltinThemeIds() =>
            new DirectoryInfo(AppDirectory)
                .EnumerateFiles("*" + Extension, SearchOption.TopDirectoryOnly)
                .Select(_ => Path.GetFileNameWithoutExtension(_.Name))
                .Where(name => !name.Equals(InvariantThemeName, StringComparison.OrdinalIgnoreCase))
                .Select(name => new ThemeId(name, true));

        public void Delete(ThemeId id)
        {
            if (id.IsBuiltin)
            {
                throw new InvalidOperationException("Only user-defined theme can be deleted");
            }

            var path = GetPath(id);
            File.Delete(path);
        }

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

        private string GetPath(ThemeId id) =>
            Path.Combine(id.IsBuiltin
                ? AppDirectory
                : DataDirectory, id.Name + Extension);
    }
}
