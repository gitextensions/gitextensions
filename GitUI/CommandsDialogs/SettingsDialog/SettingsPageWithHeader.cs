using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class SettingsPageWithHeader : SettingsPageBase, IGlobalSettingsPage
    {
        private SettingsPageHeader _header;

        public override Control GuiControl
        {
            get
            {
                if (_header == null)
                {
                    _header = new SettingsPageHeader(this);
                }

                return _header;
            }
        }

        public virtual void SetGlobalSettings()
        {
        }

        protected override bool AreEffectiveSettings => true;
        protected override ISettingsSource GetCurrentSettings()
        {
            return AppSettings.SettingsContainer;
        }
    }
}
