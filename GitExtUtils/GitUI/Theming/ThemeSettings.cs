using System;
using JetBrains.Annotations;

namespace GitExtUtils.GitUI.Theming
{
    public class ThemeSettings
    {
        private static ThemeSettings _default;

        public static ThemeSettings Default =>
            _default ??= new ThemeSettings(Theme.Default, Theme.Default, ThemeVariations.None, useSystemVisualStyle: true);

        public ThemeSettings(
            [NotNull] Theme theme,
            [NotNull] Theme invariantTheme,
            [NotNull] string[] variations,
            bool useSystemVisualStyle)
        {
            Theme = theme;
            InvariantTheme = invariantTheme;
            UseSystemVisualStyle = useSystemVisualStyle;
            Variations = variations;
        }

        [NotNull]
        public Theme Theme { get; }

        [NotNull]
        public Theme InvariantTheme { get; }

        [NotNull]
        public string[] Variations { get; }

        public bool UseSystemVisualStyle { get; }
    }
}
