using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GitExtUtils.GitUI.Theming
{
    /// <summary>
    /// A set of values for .Net system colors and GitExtensions app-specific colors
    /// </summary>
    public class Theme
    {
        private static readonly IReadOnlyDictionary<KnownColor, KnownColor> Duplicates =
            new Dictionary<KnownColor, KnownColor>
            {
                [KnownColor.ButtonFace] = KnownColor.Control,
                [KnownColor.ButtonShadow] = KnownColor.ControlDark,
                [KnownColor.ButtonHighlight] = KnownColor.ControlLight
            };

        private static Theme _default;
        public IReadOnlyDictionary<AppColor, Color> AppColorValues { get; }
        public IReadOnlyDictionary<KnownColor, Color> SysColorValues { get; }

        public static Theme Default =>
            _default ?? (_default = CreateDefaultTheme());

        public Theme(
            IReadOnlyDictionary<AppColor, Color> appColors,
            IReadOnlyDictionary<KnownColor, Color> sysColors,
            ThemeId id)
        {
            Id = id;
            AppColorValues = appColors;
            SysColorValues = sysColors;
        }

        public ThemeId Id { get; }

        /// <summary>
        /// Get GitExtensions app-specific color value as defined by this instance. If not defined,
        /// returns <see cref="Color.Empty"/>
        /// </summary>
        public Color GetColor(AppColor name) =>
            AppColorValues.TryGetValue(name, out var result)
                ? result
                : Color.Empty;

        /// <summary>
        /// Get .Net system color value as defined by this instance
        /// </summary>
        private Color GetSysColor(KnownColor name) =>
            SysColorValues.TryGetValue(name, out var result)
                ? result
                : Color.Empty;

        /// <summary>
        /// GitExtension app-specific color identifiers
        /// </summary>
        public static IReadOnlyCollection<AppColor> AppColorNames { get; } =
            new HashSet<AppColor>(Enum.GetValues(typeof(AppColor)).Cast<AppColor>());

        /// <summary>
        /// .Net system color identifiers
        /// </summary>
        private static IReadOnlyCollection<KnownColor> SysColorNames { get; } =
            new HashSet<KnownColor>(
                Enum.GetValues(typeof(KnownColor))
                    .Cast<KnownColor>()
                    .Where(c => IsSystemColor(c) && !Duplicates.ContainsKey(c)));

        /// <summary>
        /// Get .Net system color value as defined by this instance. If not defined, returns
        /// <see cref="Color.Empty"/>
        /// </summary>
        public Color GetColor(KnownColor name)
        {
            if (!IsSystemColor(name))
            {
                throw new ArgumentException($"{name} is not system color");
            }

            var actualName = Duplicates.TryGetValue(name, out var duplicate)
                ? duplicate
                : name;

            return GetSysColor(actualName);
        }

        /// <summary>
        /// Get .Net system color value as defined by this instance. If not defined, returns
        /// actual .Net <see cref="SystemColors"/> color
        /// </summary>
        public Color GetNonEmptyColor(KnownColor name)
        {
            var result = GetColor(name);
            if (result == Color.Empty)
            {
                return Color.FromKnownColor(name);
            }

            return result;
        }

        private static Theme CreateDefaultTheme()
        {
            var appColors = AppColorNames.ToDictionary(name => name, AppColorDefaults.GetBy);
            var sysColors = SysColorNames.ToDictionary(name => name, GetFixedColor);
            return new Theme(appColors, sysColors, ThemeId.Default);
        }

        /// <summary>
        /// Get .Net system color value as defined by system.
        /// The value is converted to fixed RGB as opposed to <see cref="SystemColors"/> color
        /// which takes value from theme colors table which may change.
        ///
        /// This method is needed because we trick .Net to assume modified <see cref="SystemColors"/>
        /// values to apply custom color scheme to GitExtensions, but still need access to values as
        /// before our modifications.
        ///
        /// This method should only be called before our modifications to .Net system colors.
        /// </summary>
        private static Color GetFixedColor(KnownColor systemColor) =>
            Color.FromArgb(Color.FromKnownColor(systemColor).ToArgb());

        /// <summary>
        /// Whether <see cref="KnownColor"/> represents Windows theme - defined color,
        /// produces same result as <see cref="Color.IsSystemColor"/> without the need to
        /// create <see cref="Color"/> instance
        /// </summary>
        private static bool IsSystemColor(KnownColor name) =>
            name < KnownColor.Transparent || name > KnownColor.YellowGreen;
    }
}
