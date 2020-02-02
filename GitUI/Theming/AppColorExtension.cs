using System.Drawing;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming
{
    public static class AppColorExtension
    {
        public static Color Value(this AppColor name) =>
            ThemeModule.Settings.Theme.GetColor(name);
    }
}
