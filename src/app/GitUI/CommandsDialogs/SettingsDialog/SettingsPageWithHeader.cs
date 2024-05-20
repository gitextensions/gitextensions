using GitCommands;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public partial class SettingsPageWithHeader : SettingsPageBase, IGlobalSettingsPage
    {
        private SettingsPageHeader? _header;

        public SettingsPageWithHeader(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override Control GuiControl => _header ??= new SettingsPageHeader(this);

        public virtual void SetGlobalSettings()
        {
        }

        protected override SettingsSource GetCurrentSettings()
        {
            return AppSettings.SettingsContainer;
        }
    }
}
