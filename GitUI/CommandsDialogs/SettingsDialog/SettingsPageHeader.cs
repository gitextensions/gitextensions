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

    public interface IGlobalSettingsPage : ISettingsPage
    {
        void SetGlobalSettings();
    }

    public interface ILocalSettingsPage : IGlobalSettingsPage
    {
        void SetLocalSettings();

        void SetEffectiveSettings();
    }

    public interface IRepoDistSettingsPage : ILocalSettingsPage
    {
        void SetRepoDistSettings();
    }

    public partial class SettingsPageHeader : GitExtensionsControl
    {
        private readonly SettingsPageWithHeader _Page;

        public SettingsPageHeader(SettingsPageWithHeader aPage)
        {
            InitializeComponent();
            Translate();

            if (aPage != null)
            {
                settingsPagePanel.Controls.Add(aPage);
                aPage.Dock = DockStyle.Fill;
                _Page = aPage;
                ConfigureHeader();
            }
        }

        private void ConfigureHeader()
        {
            ILocalSettingsPage localSettings = _Page as ILocalSettingsPage;

            if (localSettings == null)
            {
                GlobalRB.Checked = true;

                EffectiveRB.Visible = false;
                DistributedRB.Visible = false;
                LocalRB.Visible = false;
                arrows1.Visible = false;
                arrows2.Visible = false;
                arrow3.Visible = false;
                tableLayoutPanel2.RowStyles[2].Height = 0;
            }
            else
            {
                LocalRB.CheckedChanged += (a, b) =>
                {
                    if (LocalRB.Checked)
                    {
                        localSettings.SetLocalSettings();
                    }
                };

                EffectiveRB.CheckedChanged += (a, b) =>
                {
                    if (EffectiveRB.Checked)
                    {
                        arrows1.ForeColor = EffectiveRB.ForeColor;
                        localSettings.SetEffectiveSettings();
                    }
                    else
                    {
                        arrows1.ForeColor = arrows1.BackColor;
                    }

                    arrows2.ForeColor = arrows1.ForeColor;
                    arrow3.ForeColor = arrows1.ForeColor;
                };

                EffectiveRB.Checked = true;

                IRepoDistSettingsPage repoDistPage = localSettings as IRepoDistSettingsPage;

                if (repoDistPage == null)
                {
                    DistributedRB.Visible = false;
                    arrow3.Visible = false;
                }
                else
                {
                    DistributedRB.CheckedChanged += (a, b) =>
                    {
                        if (DistributedRB.Checked)
                        {
                            repoDistPage.SetRepoDistSettings();
                        }
                    };                
                }

            }

        }

        private void GlobalRB_CheckedChanged(object sender, EventArgs e)
        {
            if (GlobalRB.Checked)
            {
                _Page.SetGlobalSettings();
            }
        }
    }
}
