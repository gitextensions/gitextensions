#nullable enable

using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog;

public partial class SettingsPageWithHeader : SettingsPageBase, IGlobalSettingsPage
{
    private readonly bool _canSaveInsideRepo;
    private readonly Lazy<SettingsPageHeader> _header;

    public SettingsPageWithHeader(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _header = new(() => new SettingsPageHeader(this, _canSaveInsideRepo));
        if (serviceProvider is IGitUICommands uiCommands)
        {
            _canSaveInsideRepo = uiCommands.Module.IsValidGitWorkingDir();
        }
    }

    public override Control GuiControl => _header.Value;

    public override bool ReadOnly
    {
        get
        {
            // Lazy might be being initialized yet, the EffectiveSettings are current, then return true
            return TryGetHeader()?.ReadOnly ?? true;

            SettingsPageHeader? TryGetHeader()
            {
                try
                {
                    return _header.Value;
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
        }
    }

    public virtual void SetGlobalSettings()
    {
    }

    protected override SettingsSource GetCurrentSettings()
    {
        return AppSettings.SettingsContainer;
    }
}
