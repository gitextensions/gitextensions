using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.SettingsDialog.Pages
{
    public partial class ShellExtensionSettingsPage : SettingsPageBase
    {
        public ShellExtensionSettingsPage()
        {
            InitializeComponent();

            Text = "Shell Extension";
        }

        protected override void OnLoadSettings()
        {
            chkCascadedContextMenu.Checked = Settings.ShellCascadeContextMenu;

            for (int i = 0; i < Settings.ShellVisibleMenuItems.Length; i++)
            {
                chlMenuEntries.SetItemChecked(i, Settings.ShellVisibleMenuItems[i] == '1');
            }            
        }

        public override void SaveSettings()
        {
            Settings.ShellCascadeContextMenu = chkCascadedContextMenu.Checked;

            String l_ShellVisibleMenuItems = "";

            for (int i = 0; i < chlMenuEntries.Items.Count; i++)
            {
                if (chlMenuEntries.GetItemChecked(i))
                {
                    l_ShellVisibleMenuItems += "1";
                }
                else
                {
                    l_ShellVisibleMenuItems += "0";
                }
            }

            Settings.ShellVisibleMenuItems = l_ShellVisibleMenuItems;            
        }
    }
}
