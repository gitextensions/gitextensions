using Avalonia.Controls;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.Compat;

namespace GitExtensionsTests;

// parity-scaffolding: inspect actual Dashboard controls without a shipping TestAccessor.
internal static class DashboardTestSupport
{
    public static Accessor GetTestAccessor(this Dashboard dashboard) => new(dashboard);

    internal readonly struct Accessor(Dashboard dashboard)
    {
        public UserRepositoriesList Repositories => dashboard.FindControl<UserRepositoriesList>("userRepositoriesList")!;

        public Button Open => ((StackPanel)dashboard.FindControl<Border>("flpnlStart")!.Child!).Children
            .OfType<DashboardLinkLabel>().ElementAt(1);
    }
}
