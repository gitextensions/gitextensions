using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming
{
    public interface IThemeRepository
    {
        string InvariantThemeName { get; }

        Theme GetTheme(ThemeId themeId, IReadOnlyList<string> variations);

        void Save(Theme theme);

        Theme GetInvariantTheme();

        IEnumerable<ThemeId> GetThemeIds();

        void Delete(ThemeId themeId);
    }

    public class ThemeRepository : IThemeRepository
    {
        private readonly IThemePersistence _persistence;
        private readonly IThemePathProvider _themePathProvider;

        public ThemeRepository(IThemePersistence persistence, IThemePathProvider themePathProvider)
        {
            _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
            _themePathProvider = themePathProvider ?? throw new ArgumentNullException(nameof(themePathProvider));
        }

        public ThemeRepository()
            : this(new ThemePersistence(new ThemeLoader(new ThemeCssUrlResolver(new ThemePathProvider()), new ThemeFileReader())), new ThemePathProvider())
        {
        }

        public string InvariantThemeName { get; } = "invariant";

        public Theme GetTheme(ThemeId themeId, IReadOnlyList<string> variations)
        {
            string themePath = _themePathProvider.GetThemePath(themeId);
            return _persistence.Load(themePath, themeId, variations);
        }

        public void Save(Theme theme) =>
            _persistence.Save(theme, _themePathProvider.GetThemePath(theme.Id));

        public Theme GetInvariantTheme() =>
            GetTheme(new ThemeId(InvariantThemeName, isBuiltin: true), variations: Array.Empty<string>());

        public IEnumerable<ThemeId> GetThemeIds() =>
            GetBuiltinThemeIds().Concat(GetUserCustomizedThemeIds());

        public void Delete(ThemeId themeId)
        {
            if (themeId.IsBuiltin)
            {
                throw new InvalidOperationException("Only user-defined theme can be deleted");
            }

            var themePath = _themePathProvider.GetThemePath(themeId);
            File.Delete(themePath);
        }

        private IEnumerable<ThemeId> GetBuiltinThemeIds() =>
            new DirectoryInfo(_themePathProvider.AppThemesDirectory)
                .EnumerateFiles("*" + _themePathProvider.ThemeExtension, SearchOption.TopDirectoryOnly)
                .Select(fileInfo => Path.GetFileNameWithoutExtension(fileInfo.Name))
                .Where(fileName => !fileName.Equals(InvariantThemeName, StringComparison.OrdinalIgnoreCase))
                .Select(fileName => new ThemeId(fileName, true));

        private IEnumerable<ThemeId> GetUserCustomizedThemeIds()
        {
            if (_themePathProvider.UserThemesDirectory is null)
            {
                return Enumerable.Empty<ThemeId>();
            }

            DirectoryInfo directory = new(_themePathProvider.UserThemesDirectory);
            return directory.Exists
                ? directory
                    .EnumerateFiles("*" + _themePathProvider.ThemeExtension, SearchOption.TopDirectoryOnly)
                    .Select(fileInfo => Path.GetFileNameWithoutExtension(fileInfo.Name))
                    .Select(fileName => new ThemeId(fileName, false))
                : Enumerable.Empty<ThemeId>();
        }
    }
}
