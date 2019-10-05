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
        private readonly Dictionary<KnownColor, Color> _sysColors;

        public ThemeManager(Theme defaultTheme)
        {
            _defaultTheme = defaultTheme;
            _sysColors = new Dictionary<KnownColor, Color>();

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

        /// <summary>
        /// Set current theme colors
        /// </summary>
        /// <param name="appColors">GitExtensions app-specific colors</param>
        /// <param name="systemColors">.Net system colors</param>
        public void SetColors(
            IReadOnlyDictionary<AppColor, Color> appColors,
            IReadOnlyDictionary<KnownColor, Color> systemColors)
        {
            _sysColors.Clear();
            foreach (var (name, value) in systemColors)
            {
                _sysColors.Add(name, value);
            }

            foreach (var (name, value) in appColors)
            {
                AppSettings.SetColor(name, value);
            }

            ResetGdiCaches();
            ColorChanged?.Invoke();
        }

        /// <summary>
        /// Get current theme colors
        /// <param name="appColors">GitExtensions application-specific colors</param>
        /// <param name="sysColors">.Net system colors</param>
        /// </summary>
        public void GetColors(
            out IReadOnlyDictionary<AppColor, Color> appColors,
            out IReadOnlyDictionary<KnownColor, Color> sysColors)
        {
            appColors = AppColors.ToDictionary(c => c, GetColor);
            sysColors = SysColors.ToDictionary(c => c, GetSysColor);
        }

        /// <summary>
        /// Reset current theme colors to default values.
        /// GitExtensions app-specific colors are reset to <see cref="AppColorDefaults"/>
        /// .Net system colors are reset to values defined by Windows theme
        /// </summary>
        public void ResetAllColors()
        {
            SysColors.ForEach(ResetColor);
            AppColors.ForEach(ResetInternal);

            ResetGdiCaches();
            ColorChanged?.Invoke();
        }

        /// <summary>
        /// Reset specific .Net system color to default values defined by Windows theme
        /// </summary>
        public void ResetColor(KnownColor name)
        {
            _sysColors.Remove(name);

            ResetGdiCaches();
            ColorChanged?.Invoke();
        }

        /// <summary>
        /// Reset GitExtensions app-specific color to <see cref="AppColorDefaults"/>
        /// </summary>
        public void ResetColor(AppColor name)
        {
            ResetInternal(name);
            ColorChanged?.Invoke();
        }

        /// <summary>
        /// <inheritdoc cref="Theme"/>
        /// </summary>
        public override Color GetColor(AppColor name) =>
            AppSettings.GetColor(name);

        /// <summary>
        /// Define color value for GitExtensions app-specific color
        /// </summary>
        public void SetColor(AppColor name, Color color)
        {
            AppSettings.SetColor(name, color);
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

            _sysColors[name] = value;
            ResetGdiCaches();
            ColorChanged?.Invoke();
        }

        protected override Color GetSysColor(KnownColor name) =>
            _sysColors.TryGetValue(name, out var result)
                ? result
                : _defaultTheme.GetColor(name);

        private void ResetInternal(AppColor name) =>
            AppSettings.SetColor(name, _defaultTheme.GetColor(name));

        private void ResetGdiCaches()
        {
            _colorTableField.SetValue(null, null);
            ThreadData[_systemBrushesKey] = null;
            ThreadData[_systemPensKey] = null;

            foreach (Form form in Application.OpenForms)
            {
                NativeMethods.SendMessageInt(form.Handle, NativeConstants.WM_SYSCOLORCHANGE,
                    IntPtr.Zero, IntPtr.Zero);
            }
        }
    }
}
