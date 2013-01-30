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
        public virtual string GetTitle()
        {
            return Text;
        }

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

        IList<string> childrenText;

        /// <summary>
        /// override to provide search keywords
        /// </summary>
        public virtual IEnumerable<string> GetSearchKeywords()
        {
            return childrenText ?? (childrenText = GetChildrenText(this));
        }

        /// <summary>Recursively gets the text from all <see cref="Control"/>s within the specified <paramref name="control"/>.</summary>
        static IList<string> GetChildrenText(Control control)
        {
            if (control.HasChildren == false) { return new string[0]; }

            List<string> texts = new List<string>();
            foreach (Control child in control.Controls)
            {
                if (!child.Visible || child is TextBox || 
                    child is ComboBox || child is NumericUpDown)
                {// skip: invisible; input controls
                    continue;
                }
                if (child.Enabled && !string.IsNullOrWhiteSpace(child.Text))
                {// enabled AND not whitespace -> add
                    texts.Add(child.Text.Trim().ToLowerInvariant());
                }
                texts.AddRange(GetChildrenText(child));// recurse
            }
            return texts;
        }

        protected virtual string GetCommaSeparatedKeywordList()
        {
            return "";
        }

        public virtual bool IsInstantSavePage
        {
            get { return false; }
        }

        public virtual SettingsPageReference PageReference
        {
            get { return new SettingsPageReferenceByType(GetType()); }
        }
    }
}
