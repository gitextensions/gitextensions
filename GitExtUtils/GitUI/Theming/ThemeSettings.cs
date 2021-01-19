namespace GitExtUtils.GitUI.Theming
{
    public class ThemeSettings
    {
        private static ThemeSettings? _default;

        public static ThemeSettings Default =>
            _default ??= new ThemeSettings(Theme.Default, Theme.Default, ThemeVariations.None, useSystemVisualStyle: true);

        public ThemeSettings(
            Theme theme,
            Theme invariantTheme,
            string[] variations,
            bool useSystemVisualStyle)
        {
            Theme = theme;
            InvariantTheme = invariantTheme;
            UseSystemVisualStyle = useSystemVisualStyle;
            Variations = variations;
        }

        public Theme Theme { get; }

        public Theme InvariantTheme { get; }

        public string[] Variations { get; }

        public bool UseSystemVisualStyle { get; }
    }
}
