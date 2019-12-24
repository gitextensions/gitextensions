using System.Collections.Generic;
using System.Drawing;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming
{
    /// <summary>
    /// A read-only set of values for .Net system colors and GitExtensions app-specific colors
    /// </summary>
    public class ReadOnlyTheme : Theme
    {
        public IReadOnlyDictionary<AppColor, Color> AppColorValues { get; }
        public IReadOnlyDictionary<KnownColor, Color> SysColorValues { get; }
        public string Path { get; }

        public ReadOnlyTheme(
            IReadOnlyDictionary<AppColor, Color> appColors,
            IReadOnlyDictionary<KnownColor, Color> sysColors,
            string path = null)
        {
            Path = path;
            AppColorValues = appColors;
            SysColorValues = sysColors;
        }

        public ReadOnlyTheme WithPath(string path) =>
            new ReadOnlyTheme(AppColorValues, SysColorValues, path);

        public override Color GetColor(AppColor name) =>
            AppColorValues.TryGetValue(name, out var result)
                ? result
                : Color.Empty;

        protected override Color GetSysColor(KnownColor name) =>
            SysColorValues.TryGetValue(name, out var result)
                ? result
                : Color.Empty;
    }
}
