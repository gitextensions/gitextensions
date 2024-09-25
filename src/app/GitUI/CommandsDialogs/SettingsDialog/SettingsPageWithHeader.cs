using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public partial class SettingsPageWithHeader : SettingsPageBase, IGlobalSettingsPage
    {
        private SettingsPageHeader? _header;
        private bool _canSaveInsideRepo;

        public SettingsPageWithHeader(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            if (serviceProvider is IGitUICommands uiCommands)
            {
                _canSaveInsideRepo = uiCommands.Module.IsValidGitWorkingDir();
            }
        }

        public override Control GuiControl => _header ??= new SettingsPageHeader(this, _canSaveInsideRepo);

        public virtual void SetGlobalSettings()
        {
        }

        protected override SettingsSource GetCurrentSettings()
        {
            return AppSettings.SettingsContainer;
        }
    }
}
