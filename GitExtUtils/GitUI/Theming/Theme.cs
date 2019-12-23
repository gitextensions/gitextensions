using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GitExtUtils.GitUI.Theming
{
    /// <summary>
    /// Interface and common utility methods to get color values by
    /// .Net system color identifiers or GitExtensions app-specific color identifiers
    /// </summary>
    public abstract class Theme
    {
        private static readonly IReadOnlyDictionary<KnownColor, KnownColor> Duplicates =
            new Dictionary<KnownColor, KnownColor>
            {
                [KnownColor.ButtonFace] = KnownColor.Control,
                [KnownColor.ButtonShadow] = KnownColor.ControlDark,
                [KnownColor.ButtonHighlight] = KnownColor.ControlLight
            };

        /// <summary>
        /// GitExtension app-specific color identifiers
        /// </summary>
        public static HashSet<AppColor> AppColors { get; } =
            new HashSet<AppColor>(Enum.GetValues(typeof(AppColor)).Cast<AppColor>());

        /// <summary>
        /// .Net system color identifiers
        /// </summary>
        public static HashSet<KnownColor> SysColors { get; } =
            new HashSet<KnownColor>(
                Enum.GetValues(typeof(KnownColor))
                    .Cast<KnownColor>()
                    .Where(c => IsSystemColor(c) && !Duplicates.ContainsKey(c)));

        /// <summary>
        /// Get GitExtensions app-specific color value as defined by this instance. If not defined,
        /// returns <see cref="Color.Empty"/>
        /// </summary>
        public abstract Color GetColor(AppColor name);

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

        /// <summary>
        /// Get .Net system color value as defined by this instance
        /// </summary>
        protected abstract Color GetSysColor(KnownColor name);

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
        protected static Color GetFixedColor(KnownColor systemColor) =>
            Color.FromArgb(Color.FromKnownColor(systemColor).ToArgb());

        /// <summary>
        /// Whether <see cref="KnownColor"/> represents Windows theme - defined color,
        /// produces same result as <see cref="Color.IsSystemColor"/> without the need to
        /// create <see cref="Color"/> instance
        /// </summary>
        protected static bool IsSystemColor(KnownColor name) =>
            name < KnownColor.Transparent || name > KnownColor.YellowGreen;
    }
}
