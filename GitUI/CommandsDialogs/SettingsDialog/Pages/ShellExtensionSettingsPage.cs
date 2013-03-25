﻿using System;
using GitCommands;
using GitCommands.Properties;

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
            for (int i = 0; i < Settings.Default.CascadeShellMenuItems.Length; i++)
            {
                chlMenuEntries.SetItemChecked(i, Settings.Default.CascadeShellMenuItems[i] == '1');
            }            
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

            Settings.Default.CascadeShellMenuItems = l_CascadeShellMenuItems;            
        }
    }
}
