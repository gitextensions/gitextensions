using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming
{
    /// <summary>
    /// Defines current theme's customizable values for .Net system colors and GitExtensions
    /// app-specific colors.
    /// </summary>
    public class ThemeManager : Theme
    {
        private readonly Theme _defaultTheme;
        private readonly FieldInfo _colorTableField;
        private readonly PropertyInfo _threadDataProperty;
        private readonly object _systemBrushesKey;
        private readonly object _systemPensKey;

        private readonly Dictionary<KnownColor, Color> _sysColorValues =
            new Dictionary<KnownColor, Color>();
        private readonly Dictionary<AppColor, Color> _appColorValues =
            new Dictionary<AppColor, Color>();
        private bool _useInitialTheme = true;

        private StaticTheme InitialTheme { get; set; }
        public StaticTheme CurrentTheme { get; private set; }

        /// <summary>
        /// if true: use colors from theme loaded at application startup, otherwise use colors
        /// possibly changed in <see cref="FormThemeEditor"/>
        /// </summary>
        public bool UseInitialTheme
        {
            get => _useInitialTheme;
            set
            {
                if (_useInitialTheme != value)
                {
                    _useInitialTheme = value;
                    UpdateAppColors();
                    ResetGdiCaches();
                    ColorChanged?.Invoke();
                }
            }
        }

        public ThemeManager(Theme defaultTheme)
        {
            _defaultTheme = defaultTheme;
            var systemDrawingAssembly = typeof(Color).Assembly;

            _colorTableField = systemDrawingAssembly.GetType("System.Drawing.KnownColorTable")
                .GetField("colorTable", BindingFlags.Static | BindingFlags.NonPublic);

            _threadDataProperty = systemDrawingAssembly.GetType("System.Drawing.SafeNativeMethods")
                .GetNestedType("Gdip", BindingFlags.NonPublic)
                .GetProperty("ThreadData", BindingFlags.Static | BindingFlags.NonPublic);

            _systemBrushesKey = typeof(SystemBrushes)
                .GetField("SystemBrushesKey", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null);

            _systemPensKey = typeof(SystemPens)
                .GetField("SystemPensKey", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null);
        }

        public event Action ColorChanged;

        private IDictionary ThreadData =>
            (IDictionary)_threadDataProperty.GetValue(null, null);

        public void SetInitialTheme(StaticTheme theme)
        {
            InitialTheme = theme;
            SetTheme(theme);
        }

        /// <summary>
        /// Set current theme colors
        /// </summary>
        public void SetTheme(StaticTheme theme)
        {
            _sysColorValues.Clear();
            foreach (var (name, value) in theme.SysColorValues)
            {
                _sysColorValues.Add(name, value);
            }

            _appColorValues.Clear();
            foreach (var (name, value) in theme.AppColorValues)
            {
                _appColorValues.Add(name, value);
            }

            CurrentTheme = theme;
            UpdateAppColors();
            ResetGdiCaches();
            ColorChanged?.Invoke();
        }

        public void ResetTheme()
        {
            CurrentTheme = null;
            ResetAllColors();
        }

        /// <summary>
        /// Get current theme colors
        /// </summary>
        public StaticTheme GetTheme()
        {
            return new StaticTheme(
                AppColors.ToDictionary(c => c, GetModifiedColor),
                SysColors.ToDictionary(c => c, GetModifiedColor));
        }

        /// <summary>
        /// Reset current theme colors to default values.
        /// GitExtensions app-specific colors are reset to <see cref="AppColorDefaults"/>
        /// .Net system colors are reset to values defined by Windows theme
        /// </summary>
        public void ResetAllColors()
        {
            SysColors.ForEach(ResetInternal);
            AppColors.ForEach(ResetInternal);

            ResetGdiCaches();
            ColorChanged?.Invoke();
        }

        /// <summary>
        /// Reset specific .Net system color to default values defined by Windows theme
        /// </summary>
        public void ResetColor(KnownColor name)
        {
            ResetInternal(name);
            ResetGdiCaches();
            ColorChanged?.Invoke();
        }

        /// <summary>
        /// Reset GitExtensions app-specific color to <see cref="AppColorDefaults"/>
        /// </summary>
        public void ResetColor(AppColor name)
        {
            ResetInternal(name);
            UpdateAppColors(name);
            ColorChanged?.Invoke();
        }

        public Color GetThemeColor(AppColor name) =>
            (CurrentTheme ?? _defaultTheme).GetColor(name);

        public Color GetThemeColor(KnownColor name) =>
            (CurrentTheme ?? _defaultTheme).GetColor(name);

        private Color GetInitialColor(AppColor name) =>
            (InitialTheme ?? _defaultTheme).GetColor(name);

        private Color GetInitialColor(KnownColor name) =>
            (InitialTheme ?? _defaultTheme).GetColor(name);

        /// <summary>
        /// Define color value for GitExtensions app-specific color
        /// </summary>
        public void SetColor(AppColor name, Color value)
        {
            _appColorValues[name] = value;
            UpdateAppColors(name);
            ColorChanged?.Invoke();
        }

        /// <summary>
        /// Define color value for .Net system color
        /// </summary>
        public void SetColor(KnownColor name, Color value)
        {
            if (!IsSystemColor(name))
            {
                throw new ArgumentException($"{name} is not system color");
            }

            _sysColorValues[name] = value;
            ResetGdiCaches();
            ColorChanged?.Invoke();
        }

        public bool IsCurrentThemeModified() =>
            AppColors.Any(c => GetModifiedColor(c) != GetThemeColor(c)) ||
            SysColors.Any(c => GetModifiedColor(c) != GetThemeColor(c));

        public bool IsCurrentThemeInitial() =>
            ReferenceEquals(CurrentTheme, InitialTheme) || (
                CurrentTheme?.Path != null &&
                StringComparer.OrdinalIgnoreCase.Equals(CurrentTheme.Path, InitialTheme.Path));

        /// <summary>
        /// <inheritdoc cref="Theme"/>
        /// </summary>
        public override Color GetColor(AppColor name) =>
            UseInitialTheme
                ? GetInitialColor(name)
                : GetModifiedColor(name);

        protected override Color GetSysColor(KnownColor name) =>
            UseInitialTheme
                ? GetInitialColor(name)
                : GetModifiedColor(name);

        private Color GetModifiedColor(AppColor name) =>
            _appColorValues.TryGetValue(name, out var result)
                ? result
                : GetThemeColor(name);

        private Color GetModifiedColor(KnownColor name) =>
            _sysColorValues.TryGetValue(name, out var result)
                ? result
                : GetThemeColor(name);

        private void ResetInternal(KnownColor name) =>
            _sysColorValues.Remove(name);

        private void ResetInternal(AppColor name) =>
            _appColorValues.Remove(name);

        private void UpdateAppColors(AppColor? nameToUpdate = null)
        {
            if (nameToUpdate.HasValue)
            {
                var color = GetColor(nameToUpdate.Value);
                AppSettings.SetColor(nameToUpdate.Value, color);
            }
            else
            {
                foreach (var name in AppColors)
                {
                    var color = GetColor(name);
                    AppSettings.SetColor(name, color);
                }
            }
        }

        private void ResetGdiCaches()
        {
            _colorTableField.SetValue(null, null);
            ThreadData[_systemBrushesKey] = null;
            ThreadData[_systemPensKey] = null;

            foreach (Form form in Application.OpenForms)
            {
                NativeMethods.SendMessageW(form.Handle, NativeMethods.WM_SYSCOLORCHANGE);
            }
        }
    }
}
