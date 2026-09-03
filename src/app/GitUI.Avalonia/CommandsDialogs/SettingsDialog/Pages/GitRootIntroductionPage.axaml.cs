using GitCommands;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class GitRootIntroductionPage : SettingsPageBase
{
    public GitRootIntroductionPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public GitRootIntroductionPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();
    }

    protected override void SettingsToPage()
    {
        // This informational page has no editable settings to load.
    }

    protected override void PageToSettings()
    {
        // This informational page has no editable settings to save.
    }

    protected override SettingsSource GetCurrentSettings()
        => AppSettings.SettingsContainer;
}
