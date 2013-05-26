﻿using System;
using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ShellExtensionSettingsPage : SettingsPageWithHeader
    {
        public ShellExtensionSettingsPage()
        {
            InitializeComponent();
            Text = "Shell extension";
            Translate();
        }

        protected override void SettingsToPage()
        {
            for (int i = 0; i < AppSettings.CascadeShellMenuItems.Length; i++)
            {
                chlMenuEntries.SetItemChecked(i, AppSettings.CascadeShellMenuItems[i] == '1');
            }

            UpdatePreview();
        }

        protected override void PageToSettings()
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

            AppSettings.CascadeShellMenuItems = l_CascadeShellMenuItems;            
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
