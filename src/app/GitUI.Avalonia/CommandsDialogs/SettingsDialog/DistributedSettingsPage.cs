using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog;

public partial class DistributedSettingsPage : SettingsPageWithHeader, IDistributedSettingsPage
{
    public DistributedSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    protected DistributedSettings? CurrentSettings { get; private set; }

    protected override void Init(ISettingsPageHost pageHost)
    {
        base.Init(pageHost);
        if (ServiceProvider is GitExtensions.Extensibility.Git.IGitUICommands)
        {
            CurrentSettings = CommonLogic.DistributedSettingsSet.EffectiveSettings;
        }
    }

    protected override SettingsSource GetCurrentSettings()
        => CurrentSettings ?? AppSettings.SettingsContainer;

    public void SetEffectiveSettings()
    {
        if (CurrentSettings is not null)
        {
            SetCurrentSettings(CommonLogic.DistributedSettingsSet.EffectiveSettings);
        }
    }

    public void SetLocalSettings()
    {
        if (CurrentSettings is not null)
        {
            SetCurrentSettings(CommonLogic.DistributedSettingsSet.LocalSettings);
        }
    }

    public void SetDistributedSettings()
    {
        if (CurrentSettings is not null)
        {
            SetCurrentSettings(CommonLogic.DistributedSettingsSet.DistributedSettings);
        }
    }

    public override void SetGlobalSettings()
    {
        if (CurrentSettings is null)
        {
            LoadSettings();
            return;
        }

        SetCurrentSettings(CommonLogic.DistributedSettingsSet.GlobalSettings);
    }

    private void SetCurrentSettings(DistributedSettings settings)
    {
        if (CurrentSettings is not null)
        {
            SaveSettings();
        }

        CurrentSettings = settings;
        LoadSettings();
    }
}
