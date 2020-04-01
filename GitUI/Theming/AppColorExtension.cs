using System.Drawing;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming
{
    public static class AppColorExtension
    {
        public static Color GetThemeColor(this AppColor name) =>
            ThemeModule.Settings.Theme.GetColor(name);
    }
}
