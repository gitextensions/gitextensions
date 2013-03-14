using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs.SettingsDialog
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

        private bool _loadingSettings;

        /// <summary>
        /// True during execution of LoadSettings(). Usually derived classes
        /// apply settings to GUI controls. Some of controls trigger events - 
        /// IsLoadingSettings can be used for example to not execute the event action.
        /// </summary>
        protected bool IsLoadingSettings
        {
            get { return _loadingSettings; }
        }

        public void LoadSettings()
        {
            _loadingSettings = true;
            OnLoadSettings();
            _loadingSettings = false;
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
                if (!child.Visible || child is NumericUpDown)
                {// skip: invisible; some input controls
                    continue;
                }

                if (child.Enabled && !string.IsNullOrWhiteSpace(child.Text))
                {// enabled AND not whitespace -> add
                    // also searches text boxes and comboboxes
                    // TODO(optional): search through the drop down list of comboboxes
                    // TODO(optional): convert numeric dropdown values to text
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
