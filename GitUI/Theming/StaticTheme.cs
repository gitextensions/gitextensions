using System.Collections.Generic;
using System.Drawing;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming
{
    /// <summary>
    /// A read-only set of values for .Net system colors and GitExtensions app-specific colors
    /// </summary>
    public class StaticTheme : Theme
    {
        private readonly IReadOnlyDictionary<AppColor, Color> _appColors;
        private readonly IReadOnlyDictionary<KnownColor, Color> _sysColors;

        public StaticTheme(
            IReadOnlyDictionary<AppColor, Color> appColors,
            IReadOnlyDictionary<KnownColor, Color> sysColors)
        {
            _appColors = appColors;
            _sysColors = sysColors;
        }

        public override Color GetColor(AppColor name) =>
            _appColors.TryGetValue(name, out var result)
                ? result
                : Color.Empty;

        protected override Color GetSysColor(KnownColor name) =>
            _sysColors.TryGetValue(name, out var result)
                ? result
                : Color.Empty;
    }
}
