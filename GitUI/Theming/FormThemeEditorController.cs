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
        private const string InvariantThemeName = "win10default";

        private readonly TranslationString _saveDialogTitle = new TranslationString("Save theme");
        private readonly TranslationString _loadDialogTitle = new TranslationString("Load theme");
        private readonly TranslationString _filter = new TranslationString("GitExtensions theme (*{0})|*{0}");

        private readonly ThemeManager _manager;
        private readonly ThemePersistence _persistence;
        private bool _useSystemVisualStyle = true;

        public FormThemeEditorController(ThemeManager manager, ThemePersistence persistence)
        {
            _manager = manager;
            _persistence = persistence;
            UserDirectory = Path.Combine(AppSettings.ApplicationDataPath.Value, Subdirectory);
            AppDirectory = Path.Combine(AppSettings.GetGitExtensionsDirectory(), Subdirectory);
        }

        public event Action ColorChanged
        {
            add => _manager.ColorChanged += value;
            remove => _manager.ColorChanged -= value;
        }

        public event ThemeChangedHandler ThemeChanged;

        public bool UseSystemVisualStyleInitial { get; private set; } = true;

        public bool UseSystemVisualStyle
        {
            get => UseInitialTheme ? UseSystemVisualStyleInitial : _useSystemVisualStyle;
            set => _useSystemVisualStyle = value;
        }

        public string UserDirectory { get; }
        public string AppDirectory { get; }

        public string ThemeName =>
            GetThemeName(_manager.CurrentTheme?.Path);

        public bool IsThemeModified() =>
            _manager.IsCurrentThemeModified();

        public bool IsThemeInitial() =>
            _manager.IsCurrentThemeInitial() &&
            _useSystemVisualStyle == UseSystemVisualStyleInitial;

        /// <summary>
        /// <inheritdoc cref="ThemeManager.UseInitialTheme"/>
        /// </summary>
        public bool UseInitialTheme
        {
            private get => _manager.UseInitialTheme;
            set => _manager.UseInitialTheme = value;
        }

        public StaticTheme LoadInvariantTheme(bool quiet = false) =>
            _persistence.LoadFile(GetOriginalThemePath(InvariantThemeName), quiet);

        public bool IsCurrentThemeFile(string path) =>
            StringComparer.OrdinalIgnoreCase.Equals(path, GetThemePath(CurrentThemeName));

        public IEnumerable<string> GetSavedThemeNames() =>
            Directory
                .GetFiles(UserDirectory, "*" + Extension, SearchOption.TopDirectoryOnly)
                .Where(path => !IsCurrentThemeFile(path))
                .Select(GetThemeName);

        public bool SetInitialTheme(string name, bool useSystemVisualStyle)
        {
            UseSystemVisualStyleInitial = useSystemVisualStyle;
            UseSystemVisualStyle = useSystemVisualStyle;

            var path = GetThemePath(string.IsNullOrEmpty(name) ? CurrentThemeName : name);
            var theme = _persistence.LoadFile(path, quiet: false);
            if (theme == null)
            {
                return false;
            }

            _manager.SetInitialTheme(theme);
            return true;
        }

        public void SetTheme(string name, bool quiet = true)
        {
            string path = GetThemePath(name);
            SetThemeFile(path, quiet);
        }

        public void SaveCurrentTheme()
        {
            string path = GetThemePath(CurrentThemeName);
            var theme = _manager.GetTheme();
            _persistence.SaveToFile(theme, path, quiet: true);
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

        public void ResetTheme()
        {
            _manager.ResetTheme();
            OnThemeChanged(true, _manager.CurrentTheme?.Path);
        }

        public void ResetAllColors()
        {
            _manager.ResetAllColors();
            OnThemeChanged(true, _manager.CurrentTheme?.Path);
        }

        public void SaveThemeDialog()
        {
            if (!SaveDialog(out var path))
            {
                return;
            }

            var theme = _manager.GetTheme();
            if (!_persistence.SaveToFile(theme, path, quiet: false))
            {
                return;
            }

            _manager.SetTheme(theme.WithPath(path));
            OnThemeChanged(false, path);

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

        public void SetThemeFileDialog()
        {
            if (TrySelectFile(out string path))
            {
                SetThemeFile(path, quiet: false);
            }

            bool TrySelectFile(out string filename)
            {
                filename = null;

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

                    filename = dlg.FileName;
                    return true;
                }
            }
        }

        public Color GetColor(KnownColor name) =>
            _manager.GetColor(name);

        public Color GetColor(AppColor name) =>
            _manager.GetColor(name);

        public Color GetDefaultColor(KnownColor name) =>
            _manager.GetThemeColor(name);

        public Color GetDefaultColor(AppColor name) =>
            _manager.GetThemeColor(name);

        private void SetThemeFile(string path, bool quiet = false)
        {
            var theme = _persistence.LoadFile(path, quiet);
            if (theme == null)
            {
                return;
            }

            _manager.SetTheme(theme);
            OnThemeChanged(true, path);
        }

        private void OnThemeChanged(bool colorsChanged, string path)
        {
            string name = GetThemeName(path);
            ThemeChanged?.Invoke(colorsChanged, name);
        }

        private string GetThemeName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (IsCurrentThemeFile(path))
            {
                return null;
            }

            return Path.GetFileNameWithoutExtension(path);
        }

        private string GetThemePath(string name) =>
            Path.Combine(UserDirectory, name + Extension);

        private string GetOriginalThemePath(string name) =>
            Path.Combine(AppDirectory, name + Extension);
    }
}
