using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class SettingsPageWithHeader : SettingsPageBase, IGlobalSettingsPage
    {
        private SettingsPageHeader? _header;

        public override Control GuiControl => _header ??= new SettingsPageHeader(this);

        public virtual void SetGlobalSettings()
        {
        }

        protected override ISettingsSource GetCurrentSettings()
        {
            return AppSettings.SettingsContainer;
        }
    }
}
