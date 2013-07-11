using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class SettingsPageWithHeader : SettingsPageBase, IGlobalSettingsPage
    {
        private SettingsPageHeader header;

        public override Control GuiControl
        {
            get
            {
                if (header == null)
                {
                    header = new SettingsPageHeader(this);
                }

                return header;
            }
        }

        public virtual void SetGlobalSettings()
        {        
        }
    }
}
