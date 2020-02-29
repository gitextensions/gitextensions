using JetBrains.Annotations;

namespace GitExtUtils.GitUI.Theming
{
    public class ThemeSettings
    {
        private static ThemeSettings _default;

        public static ThemeSettings Default =>
            _default ?? (_default = new ThemeSettings(Theme.Default, Theme.Default, true));

        public ThemeSettings(
            [NotNull] Theme theme,
            [NotNull] Theme invariantTheme,
            bool useSystemVisualStyle)
        {
            Theme = theme;
            InvariantTheme = invariantTheme;
            UseSystemVisualStyle = useSystemVisualStyle;
        }

        [NotNull]
        public Theme Theme { get; }

        [NotNull]
        public Theme InvariantTheme { get; }

        public bool UseSystemVisualStyle { get; }
    }
}
