using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public partial class ConfigFileSettingsPageHeader : GitExtensionsControl
    {
        private readonly ConfigFileSettingsPage _Page;

        public ConfigFileSettingsPageHeader(ConfigFileSettingsPage aPage)
        {
            InitializeComponent();
            Translate();

            settingsPagePanel.Controls.Add(aPage);
            aPage.Dock = DockStyle.Fill;
            _Page = aPage;
            
            EffectiveRB.Checked = true;
        }

        private void EffectiveRB_CheckedChanged(object sender, EventArgs e)
        {
            if (EffectiveRB.Checked)
            {
                arrows1.ForeColor = EffectiveRB.ForeColor;
                _Page.SetEffectiveSettings();
            }
            else
            {
                arrows1.ForeColor = arrows1.BackColor;
            }

            arrows2.ForeColor = arrows1.ForeColor;
        }

        private void GlobalRB_CheckedChanged(object sender, EventArgs e)
        {
            if (GlobalRB.Checked)
            {
                _Page.SetGlobalSettings();
            }
        }

        private void LocalRB_CheckedChanged(object sender, EventArgs e)
        {
            if (LocalRB.Checked)
            {
                _Page.SetLocalSettings();
            }
        }
    }
}
