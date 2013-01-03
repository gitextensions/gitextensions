using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.SettingsDialog
{
    /// <summary>
    /// set Text property in derived classes to set the title
    /// </summary>
    public class SettingsPageBase : GitExtensionsControl, ISettingsPage
    {
        public virtual string Title { get { return Text; } }

        public Control GuiControl { get { return this; } }

        /// <summary>
        /// Called when SettingsPage is shown (again);
        /// e. g. after user clicked a tree item
        /// </summary>
        public virtual void OnPageShown()
        {
            // to be overridden
        }

        [Obsolete("TODO: is this needed here (per settingpage) or globally for all settings (as it was originally)?")]
        protected bool loadingSettings;

        public void LoadSettings()
        {
            loadingSettings = true;
            OnLoadSettings();
            loadingSettings = false;
        }

        ////public void SaveSettings()
        ////{
        ////    OnSaveSettings();
        ////}

        /// <summary>
        /// use GitCommands.Settings to load settings in derived classes
        /// </summary>
        protected virtual void OnLoadSettings()
        {
            // to be overridden
        }

        /// <summary>
        /// use GitCommands.Settings to save settings in derived classes
        /// </summary>
        public virtual void SaveSettings()
        {
            // to be overridden
        }

        /// <summary>
        /// override to provide search keywords
        /// </summary>
        public virtual IEnumerable<string> GetSearchKeywords()
        {
            // split at comma and return a list of trimmed strings
            return GetCommaSeparatedKeywordList()
                .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim());
        }

        protected virtual string GetCommaSeparatedKeywordList()
        {
            return "";
        }

        public virtual bool IsInstantSavePage
        {
            get { return false; }
        }
    }
}
