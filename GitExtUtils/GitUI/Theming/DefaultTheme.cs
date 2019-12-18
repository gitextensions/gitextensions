using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GitExtUtils.GitUI.Theming
{
    /// <summary>
    /// Defines default values for .Net system colors and GitExtensions app-specific colors
    /// </summary>
    public class DefaultTheme : Theme
    {
        private readonly Dictionary<KnownColor, Color> _sysColors;

        public DefaultTheme() =>
            _sysColors = ReadDefaultColors();

        public override Color GetColor(AppColor name) =>
            AppColorDefaults.GetBy(name);

        private Dictionary<KnownColor, Color> ReadDefaultColors() =>
            SysColors.ToDictionary(name => name, GetFixedColor);

        protected override Color GetSysColor(KnownColor name) =>
            _sysColors.TryGetValue(name, out var result)
                ? result
                : Color.Empty;
    }
}
