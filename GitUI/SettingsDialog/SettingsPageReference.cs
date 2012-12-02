using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.SettingsDialog
{
    /// <summary>
    /// to jump to a specific page
    /// 
    /// TODO: extend with attributes to jump to specific control
    /// </summary>
    public class SettingsPageReference
    {
        Type _settingsPageType;

        public SettingsPageReference(Type settingsPageType)
        {
            _settingsPageType = settingsPageType;
        }

        public Type SettingsPageType { get { return _settingsPageType; } }
    }
}
