using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.SettingsDialog
{
    /// <summary>
    /// set Text property to set the title
    /// </summary>
    public class SettingsPageBase : UserControl
    {
        /// <summary>
        /// called when SettingsPage is shown (again)
        /// </summary>
        public virtual void RefreshView()
        {
            // to be overridden
        }

        /// <summary>
        /// use GitCommands.Settings to load settings
        /// </summary>
        /// <param name="settings"></param>
        public virtual void LoadSettings()
        {
            // to be overridden
        }

        /// <summary>
        /// use GitCommands.Settings to save settings
        /// </summary>
        /// <param name="settings"></param>
        public virtual void SaveSettings()
        {
            // to be overridden
        }
    }
}
