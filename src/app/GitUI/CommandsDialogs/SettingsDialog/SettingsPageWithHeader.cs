using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public partial class SettingsPageWithHeader : SettingsPageBase, IGlobalSettingsPage
    {
        private SettingsPageHeader? _header;
        private bool _isrepoValid;

        public SettingsPageWithHeader(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            IGitModule gitModule = (serviceProvider as GitUICommands).Module;
            _isrepoValid = gitModule.IsValidGitWorkingDir();
        }

        public override Control GuiControl => _header ??= new SettingsPageHeader(this, _isrepoValid);

        public virtual void SetGlobalSettings()
        {
        }

        protected override SettingsSource GetCurrentSettings()
        {
            return AppSettings.SettingsContainer;
        }
    }
}
