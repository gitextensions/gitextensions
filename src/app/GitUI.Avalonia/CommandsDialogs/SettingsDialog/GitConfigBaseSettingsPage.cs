using GitExtensions.Extensibility.Settings;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog;

public partial class GitConfigBaseSettingsPage : SettingsPageWithHeader, IGitConfigSettingsPage
{
    public GitConfigBaseSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    protected GitConfigSettingsSet GitConfigSettingsSet => CommonLogic.GitConfigSettingsSet;

    protected SettingsSource? CurrentSettings { get; private set; }

    protected override void Init(ISettingsPageHost pageHost)
    {
        base.Init(pageHost);
        if (ServiceProvider is EmptyServiceProvider)
        {
            return;
        }

        CurrentSettings = GitConfigSettingsSet.EffectiveSettings;
    }

    protected override SettingsSource GetCurrentSettings()
    {
        Validates.NotNull(CurrentSettings);
        return CurrentSettings;
    }

    protected override void PageToSettings()
    {
        base.PageToSettings();
        GitConfigSettingsSet.Save();
    }

    public void SetEffectiveSettings()
    {
        if (CurrentSettings is not null)
        {
            SetCurrentSettings(GitConfigSettingsSet.EffectiveSettings);
        }
    }

    public void SetLocalSettings()
    {
        if (CurrentSettings is not null)
        {
            SetCurrentSettings(GitConfigSettingsSet.LocalSettings);
        }
    }

    public override void SetGlobalSettings()
    {
        if (CurrentSettings is not null)
        {
            SetCurrentSettings(GitConfigSettingsSet.GlobalSettings);
        }
    }

    public void SetSystemSettings()
    {
        if (CurrentSettings is not null)
        {
            SetCurrentSettings(GitConfigSettingsSet.SystemSettings);
        }
    }

    private void SetCurrentSettings(SettingsSource settings)
    {
        if (CurrentSettings is not null)
        {
            SaveSettings();
        }

        CurrentSettings = settings;
        LoadSettings();
    }
}
