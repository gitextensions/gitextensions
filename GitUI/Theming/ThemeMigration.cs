using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using GitCommands;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming
{
    internal class ThemeMigration
    {
        private readonly ThemeRepository _repository;
        private readonly ThemeId _migratedThemeId = new ThemeId("migrated", false);
        private readonly ThemeId[] _themeIds;

        public ThemeMigration(ThemeRepository repository)
        {
            _repository = repository;
            _themeIds = _repository.GetThemeIds().ToArray();
        }

        public void Migrate()
        {
            if (!MigrationAlreadyHappened())
            {
                MigrateColorSettings();
                MigratePrebuiltThemes();
            }
        }

        /// <summary>
        /// Before <see cref="ThemeModule"/>, <see cref="AppSettings"/> was responsible for colors.
        /// This method saves custom colors (if any) from <see cref="AppSettings"/> to a user-defined theme named 'migrated'.
        /// </summary>
        private void MigrateColorSettings()
        {
            if (!string.IsNullOrEmpty(AppSettings.ThemeId.Name))
            {
                // migration must have happened already
                return;
            }

#pragma warning disable 618
            if (Theme.AppColorNames.All(name => AppSettings.GetColor(name).ToArgb() == AppColorDefaults.GetBy(name).ToArgb()))
            {
                // only default color values were used
                return;
            }

            var migratedTheme = new Theme(
                Theme.AppColorNames.ToDictionary(name => name, AppSettings.GetColor),
                new Dictionary<KnownColor, Color>(),
                _migratedThemeId);
#pragma warning restore 618

            try
            {
                _repository.Save(migratedTheme);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to migrate color settings: {ex}");
                return;
            }

            AppSettings.ThemeId = _migratedThemeId;
        }

        /// <summary>
        /// Previously all themes were deployed to user data directory.
        /// Now we deploy builtin themes to application directory, leaving user directory for
        /// custom user-defined themes, so built-in themes must be purged from there.
        /// </summary>
        private void MigratePrebuiltThemes()
        {
            // TODO remove this method after 4.0 is released
            foreach (ThemeId id in _themeIds)
            {
                if (!id.IsBuiltin)
                {
                    _repository.Delete(id);
                }
            }

            ThemeId themeId = new ThemeId(AppSettings.ThemeId.Name, isBuiltin: true);
            AppSettings.ThemeId = _themeIds.Contains(themeId)
                ? themeId
                : ThemeId.Default;
        }

        private bool MigrationAlreadyHappened() =>
            _themeIds.Contains(_migratedThemeId);
    }
}
