using System.Drawing;
using GitCommands;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming
{
    public static class AppColorExtension
    {
        public static Color GetThemeColor(this AppColor name)
        {
            if (AppSettings.IsDesignMode)
            {
                return Color.Transparent;
            }

            return ThemeModule.Settings.Theme.GetColor(name);
        }
    }
}
