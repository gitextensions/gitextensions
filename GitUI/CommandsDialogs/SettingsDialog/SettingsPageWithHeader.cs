using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class SettingsPageWithHeader : SettingsPageBase, IGlobalSettingsPage
    {
        public SettingsPageWithHeader()
        {
            header = new SettingsPageHeader(this);
        }

        private SettingsPageHeader header;

        public override Control GuiControl
        {
            get
            {
                return header;
            }
        }

        public virtual void SetGlobalSettings()
        {        
        }
    }
}
