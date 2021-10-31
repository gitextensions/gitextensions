using System;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs.SettingsDialog.ShellExtension;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ShellExtensionSettingsPage : SettingsPageWithHeader
    {
        public ShellExtensionSettingsPage()
        {
            InitializeComponent();
            Text = "Shell extension";
            InitializeComplete();

            // when the dock is set in the designer it causes weird visual artifacts in scaled Windows environments
            chlMenuEntries.Dock = DockStyle.Fill;
        }

        protected override void SettingsToPage()
        {
            for (int i = 0; i < AppSettings.CascadeShellMenuItems.Length; i++)
            {
                chlMenuEntries.SetItemChecked(i, AppSettings.CascadeShellMenuItems[i] == '1');
            }

            cbAlwaysShowAllCommands.Checked = AppSettings.AlwaysShowAllCommands;
            CheckShellIntegrationSettings();
            UpdatePreview();
        }

        public override void OnPageShown()
        {
            CheckShellIntegrationSettings();
        }

        private void CheckShellIntegrationSettings()
        {
            if (ShellExtensionManager.CheckFilesFound())
            {
                cbEnableIntegration.Enabled = true;
                cbEnableIntegration.Checked = ShellExtensionManager.CheckIfRegistered();
            }
            else
            {
                cbEnableIntegration.Enabled = false;
                cbEnableIntegration.Checked = false;
            }
        }

        protected override void PageToSettings()
        {
            string l_CascadeShellMenuItems = "";

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

            AppSettings.CascadeShellMenuItems = l_CascadeShellMenuItems;
            AppSettings.AlwaysShowAllCommands = cbAlwaysShowAllCommands.Checked;

            if (ShellExtensionManager.CheckFilesFound() && cbEnableIntegration.Checked != ShellExtensionManager.CheckIfRegistered())
            {
                try
                {
                    if (cbEnableIntegration.Checked)
                    {
                        ShellExtensionManager.Register();
                    }
                    else
                    {
                        ShellExtensionManager.Unregister();
                    }
                }
                catch
                {
                    // ignored
                }
            }
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
                    cascaded += "       " + chlMenuEntries.Items[i] + "\r\n";
                }
                else
                {
                    topLevel += "GitExt " + chlMenuEntries.Items[i] + "\r\n";
                }
            }

            labelPreview.Text = topLevel + "Git Extensions > \r\n" + cascaded;
        }
    }
}
