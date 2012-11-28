using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.SettingsDialog
{
    public class SettingsPageRegistry
    {
        private IList<SettingsPageBase> _settingsPageCollection = new List<SettingsPageBase>();

        public void RegisterSettingsPage(SettingsPageBase settingsPage)
        {
            _settingsPageCollection.Add(settingsPage);
        }

        public IEnumerable<SettingsPageBase> GetSettingsPages()
        {
            return _settingsPageCollection;
        }
    }
}
