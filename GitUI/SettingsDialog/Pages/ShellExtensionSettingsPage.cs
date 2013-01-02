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
            Translate();

            Text = "Shell Extension";
        }

        protected override void OnLoadSettings()
        {
            for (int i = 0; i < Settings.CascadeShellMenuItems.Length; i++)
            {
                chlMenuEntries.SetItemChecked(i, Settings.CascadeShellMenuItems[i] == '1');
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

            Settings.CascadeShellMenuItems = l_CascadeShellMenuItems;            
        }
    }
}
