using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.SettingsDialog
{
    public interface ISettingsPageHost
    {
        void GotoPage(SettingsPageReference settingsPageReference);
    }
}
