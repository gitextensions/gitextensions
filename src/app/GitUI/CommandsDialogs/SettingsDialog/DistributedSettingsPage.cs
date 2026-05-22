using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitUI.SettingControlBindings;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog;

public partial class DistributedSettingsPage : SettingsPageWithHeader, IDistributedSettingsPage
{
    private static TranslationString _numberSettingPlaceholder = new(@"no value set");
    private static TranslationString _stringSettingPlaceholder = new(@"no value set; for empty string, enter ""{0}"" without the double quotes");

    static DistributedSettingsPage()
    {
        NumberSettingControlBinding.PlaceholderText = _numberSettingPlaceholder.Text;
        StringSettingControlBinding.PlaceholderText = _stringSettingPlaceholder.Text;
    }

    public DistributedSettingsPage(IServiceProvider serviceProvider)
       : base(serviceProvider)
    {
    }

    protected DistributedSettingsSet DistributedSettingsSet => CommonLogic.DistributedSettingsSet;
    protected DistributedSettings? CurrentSettings { get; private set; }

    protected override void Init(ISettingsPageHost pageHost)
    {
        base.Init(pageHost);

        CurrentSettings = DistributedSettingsSet.EffectiveSettings;
    }

    protected override SettingsSource GetCurrentSettings()
    {
        Validates.NotNull(CurrentSettings);

        return CurrentSettings;
    }

    public void SetEffectiveSettings()
    {
        if (DistributedSettingsSet.EffectiveSettings is not null)
        {
            SetCurrentSettings(DistributedSettingsSet.EffectiveSettings);
        }
    }

    public void SetLocalSettings()
    {
        if (DistributedSettingsSet.LocalSettings is not null)
        {
            SetCurrentSettings(DistributedSettingsSet.LocalSettings);
        }
    }

    public void SetDistributedSettings()
    {
        if (DistributedSettingsSet.DistributedSettings is not null)
        {
            SetCurrentSettings(DistributedSettingsSet.DistributedSettings);
        }
    }

    public override void SetGlobalSettings()
    {
        if (DistributedSettingsSet.GlobalSettings is not null)
        {
            SetCurrentSettings(DistributedSettingsSet.GlobalSettings);
        }
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
