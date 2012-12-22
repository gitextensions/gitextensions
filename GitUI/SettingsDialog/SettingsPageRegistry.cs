using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.SettingsDialog
{
    public class SettingsPageRegistry
    {
        private IList<ISettingsPage> _settingsPageCollection = new List<ISettingsPage>();

        public void RegisterSettingsPage(ISettingsPage settingsPage)
        {
            _settingsPageCollection.Add(settingsPage);
        }

        public IEnumerable<ISettingsPage> GetSettingsPages()
        {
            return _settingsPageCollection;
        }
    }
}
