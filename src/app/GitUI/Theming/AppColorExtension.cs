using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming;

public static class AppColorExtension
{
    public static Color GetThemeColor(this AppColor name)
    {
        Color themeColor = ThemeModule.Settings.Theme.GetColor(name);

        return themeColor is { IsEmpty: false }
            ? themeColor
            : ThemeModule.Settings.InvariantTheme.GetColor(name);
    }
}
