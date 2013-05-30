using System;
using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ShellExtensionSettingsPage : SettingsPageBase
    {
        public ShellExtensionSettingsPage()
        {
            InitializeComponent();
            Text = "Shell extension";
            Translate();
        }

        protected override void OnLoadSettings()
        {
            for (int i = 0; i < Settings.CascadeShellMenuItems.Length; i++)
            {
                chlMenuEntries.SetItemChecked(i, Settings.CascadeShellMenuItems[i] == '1');
            }

            UpdatePreview();
        }

        public override void SaveSettings()
        {
            String l_CascadeShellMenuItems = "";

            for (int i = 0; i < chlMenuEntries.Items.Count; i++)
            {
                if (chlMenuEntries.GetItemChecked(i))
                {
                    l_CascadeShellMenuItems += "1";
                }
                else
                {
                    l_CascadeShellMenuItems += "0";
                }
            }

            Settings.CascadeShellMenuItems = l_CascadeShellMenuItems;            
        }

        private void chlMenuEntries_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            string topLevel = "";
            string cascaded = "";

            for (int i = 0; i < chlMenuEntries.Items.Count; i++)
            {
                if (chlMenuEntries.GetItemChecked(i))
                {
                    cascaded += "       " + chlMenuEntries.Items[i].ToString() + "\r\n";
                }
                else
                {
                    topLevel += "GitEx " + chlMenuEntries.Items[i].ToString() + "\r\n";
                }
            }

            labelPreview.Text = topLevel + "Git Extensions > \r\n" + cascaded;
        }
    }
}
