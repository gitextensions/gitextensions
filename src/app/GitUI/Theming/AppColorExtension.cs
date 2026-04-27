using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming;

public static class AppColorExtension
{
    public static Color GetThemeColor(this AppColor name)
    {
        Color themeColor = ThemeModule.Settings.Theme.GetColor(name);
        if (themeColor is { IsEmpty: false })
        {
            return themeColor;
        }

        themeColor = ThemeModule.Settings.InvariantTheme.GetColor(name);
        if (themeColor is { IsEmpty: false })
        {
            return themeColor;
        }

        return AppColorDefaults.GetBy(name);
    }

    public static Color GetThemeColor(this AppColor name, Color fallback)
    {
        Color themeColor = name.GetThemeColor();
        return themeColor.IsEmpty ? fallback : themeColor;
    }
}
