using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitUI
{
    public partial class FormPluginSettings : GitExtensionsForm
    {
        public FormPluginSettings()
        {
            InitializeComponent();
        }

        private void FormPluginSettings_Load(object sender, EventArgs e)
        {
            PluginList.DataSource = GitUIPluginCollection.Plugins;
            PluginList.DisplayMember = "Description";
        }

        private IGitPlugin SelectedGitPlugin;

        private void PluginList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveSettings();
            SelectedGitPlugin = PluginList.SelectedItem as IGitPlugin;
            LoadSettings();
        }

        private void SaveSettings()
        {
            if (SelectedGitPlugin != null)
            {
                foreach (Control control in splitContainer1.Panel2.Controls)
                {
                    TextBox textBox = control as TextBox;

                    if (textBox != null)
                        if (SelectedGitPlugin.Settings.GetAvailableSettings().Contains(textBox.Name))
                            SelectedGitPlugin.Settings.SetSetting(textBox.Name, textBox.Text);
                }
            }
        }

        private void LoadSettings()
        {
            int xLabelStart = 20;
            int xEditStart = 200;

            int yStart = 20;

            splitContainer1.Panel2.Controls.Clear();

            if (SelectedGitPlugin != null)
            {
                foreach (string setting in SelectedGitPlugin.Settings.GetAvailableSettings())
                {
                    Label label = new Label();
                    label.Text = setting;
                    label.Location = new Point(xLabelStart, yStart);
                    label.Size = new Size(xEditStart - 30, 20);
                    splitContainer1.Panel2.Controls.Add(label);

                    TextBox textBox = new TextBox();
                    textBox.Name = setting;
                    textBox.Text = SelectedGitPlugin.Settings.GetSetting(setting);
                    textBox.Location = new Point(xEditStart, yStart);
                    textBox.Size = new Size(splitContainer1.Panel2.Width - xEditStart - 20, 20);
                    splitContainer1.Panel2.Controls.Add(textBox);

                    yStart += 25;
                }
            }
        }

        private void FormPluginSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }
    }
}
