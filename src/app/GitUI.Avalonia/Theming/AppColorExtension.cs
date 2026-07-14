using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming;

// Twin of GitUI/Theming/AppColorExtension.cs.
// TODO(avalonia-port): resolves default colors only; the CSS theme loader (Phase 3) will make
// this honor the selected theme like the WinForms ThemeModule does.
public static class AppColorExtension
{
    public static Color GetThemeColor(this AppColor name)
        => AppColorDefaults.GetBy(name);
}
