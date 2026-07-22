using GitCommands;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class PluginRootIntroductionPage : SettingsPageBase
{
    public PluginRootIntroductionPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public PluginRootIntroductionPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();
    }

    protected override SettingsSource GetCurrentSettings()
        => AppSettings.SettingsContainer;
}
