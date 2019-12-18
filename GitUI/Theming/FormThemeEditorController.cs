using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using ResourceManager;

namespace GitUI.Theming
{
    public class FormThemeEditorController
    {
        private const string Subdirectory = "Themes";
        public const string Extension = ".colors";
        private const string CurrentThemeName = "current";
        private readonly TranslationString _saveDialogTitle = new TranslationString("Save theme");
        private readonly TranslationString _loadDialogTitle = new TranslationString("Load theme");
        private readonly TranslationString _filter = new TranslationString("GitExtensions theme (*{0})|*{0}");

        private readonly string _currentThemePath;
        private readonly string _invariantThemePath;
        private readonly ThemeManager _manager;
        private readonly Theme _defaultTheme;
        private readonly ThemePersistence _persistence;

        public FormThemeEditorController(
            ThemeManager manager,
            Theme defaultTheme,
            ThemePersistence persistence)
        {
            _manager = manager;
            _defaultTheme = defaultTheme;
            _persistence = persistence;
            UserDirectory = Path.Combine(AppSettings.ApplicationDataPath.Value, Subdirectory);
            AppDirectory = Path.Combine(AppSettings.GetGitExtensionsDirectory(), Subdirectory);
            _currentThemePath = Path.Combine(UserDirectory, CurrentThemeName + Extension);
            _invariantThemePath = Path.Combine(AppDirectory, "win10default" + Extension);
        }

        public event Action ColorChanged
        {
            add => _manager.ColorChanged += value;
            remove => _manager.ColorChanged -= value;
        }

        public event ThemeChangedHandler ThemeChanged;

        public string UserDirectory { get; }
        public string AppDirectory { get; }

        public bool TryLoadInvariantTheme(out StaticTheme theme)
        {
            if (_persistence.TryLoadFile(_invariantThemePath, out var appColors, out var sysColors))
            {
                theme = new StaticTheme(appColors, sysColors);
                return true;
            }

            theme = null;
            return false;
        }

        public bool IsCurrentThemeFile(string filename) =>
            string.Equals(Path.GetFileNameWithoutExtension(filename), CurrentThemeName,
                StringComparison.InvariantCultureIgnoreCase);

        public IEnumerable<string> GetSavedThemeNames() =>
            Directory
                .GetFiles(UserDirectory, "*" + Extension, SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileNameWithoutExtension)
                .Where(n => !string.Equals(n, CurrentThemeName, StringComparison.InvariantCultureIgnoreCase));

        public bool ApplySavedTheme(string name, bool quiet = true)
        {
            string fileName = Path.Combine(UserDirectory, name + Extension);
            if (ApplyThemeFile(fileName, quiet))
            {
                OnThemeChanged(true, fileName);
                return true;
            }

            return false;
        }

        public bool ApplyCurrentTheme(string name)
        {
            if (!string.IsNullOrEmpty(name) && ApplySavedTheme(name, quiet: false))
            {
                return true;
            }

            return ApplyThemeFile(_currentThemePath, quiet: false);
        }

        public void SaveCurrentTheme()
        {
            string file = _currentThemePath;
            _manager.GetColors(out var appColors, out var sysColors);
            _persistence.SaveToFile(appColors, sysColors, file, quiet: true);
        }

        public void SetColor(AppColor name, Color value)
        {
            _manager.SetColor(name, value);
            OnThemeChanged(true, null);
        }

        public void SetColor(KnownColor name, Color value)
        {
            _manager.SetColor(name, value);
            OnThemeChanged(true, null);
        }

        public void Reset(AppColor name)
        {
            _manager.ResetColor(name);
            OnThemeChanged(true, null);
        }

        public void Reset(KnownColor name)
        {
            _manager.ResetColor(name);
            OnThemeChanged(true, null);
        }

        public void ResetAllColors()
        {
            _manager.ResetAllColors();
            OnThemeChanged(true, null);
        }

        public void SaveToFileDialog()
        {
            if (!SaveDialog(out var selectedFile))
            {
                return;
            }

            _manager.GetColors(out var appColors, out var sysColors);
            if (!_persistence.SaveToFile(appColors, sysColors, selectedFile, quiet: false))
            {
                return;
            }

            OnThemeChanged(false, selectedFile);

            bool SaveDialog(out string filename)
            {
                var dlg = new SaveFileDialog
                {
                    DefaultExt = Extension,
                    InitialDirectory = UserDirectory,
                    AddExtension = true,
                    Filter = string.Format(_filter.Text, Extension),
                    Title = _saveDialogTitle.Text,
                    CheckPathExists = true
                };

                using (dlg)
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        filename = dlg.FileName;
                        return true;
                    }
                }

                filename = null;
                return false;
            }
        }

        public void ApplyThemeFromFileDialog()
        {
            if (TrySelectFile(out string selectedFile) && ApplyThemeFile(selectedFile))
            {
                OnThemeChanged(true, selectedFile);
            }

            bool TrySelectFile(out string result)
            {
                result = null;

                var dlg = new OpenFileDialog
                {
                    DefaultExt = Extension,
                    InitialDirectory = UserDirectory,
                    AddExtension = true,
                    Filter = string.Format(_filter.Text, Extension),
                    Title = _loadDialogTitle.Text,
                    CheckFileExists = true
                };

                using (dlg)
                {
                    if (dlg.ShowDialog() != DialogResult.OK)
                    {
                        return false;
                    }

                    result = dlg.FileName;
                    return true;
                }
            }
        }

        public Color GetColor(KnownColor name) =>
            _manager.GetColor(name);

        public Color GetColor(AppColor name) =>
            _manager.GetColor(name);

        public Color GetDefaultColor(KnownColor name) =>
            _defaultTheme.GetColor(name);

        public Color GetDefaultColor(AppColor name) =>
            _defaultTheme.GetColor(name);

        private bool ApplyThemeFile(string file, bool quiet = false)
        {
            if (_persistence.TryLoadFile(file, out var appColors, out var sysColors, quiet))
            {
                _manager.SetColors(appColors, sysColors);
                return true;
            }

            return false;
        }

        private void OnThemeChanged(bool colorsChanged, string file)
        {
            string schemaName = string.IsNullOrEmpty(file) || file.Equals(_currentThemePath, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : Path.GetFileNameWithoutExtension(file);
            ThemeChanged?.Invoke(colorsChanged, schemaName);
        }
    }
}
