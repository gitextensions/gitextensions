using System;
using System.Drawing;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitUI.Plugin
{
    public partial class FormPluginSettings : GitExtensionsForm
    {
        private IGitPlugin _selectedGitPlugin;

        public FormPluginSettings()
        {
            InitializeComponent();
            Translate();
        }

        private void FormPluginSettingsLoad(object sender, EventArgs e)
        {
            PluginList.DataSource = LoadedPlugins.Plugins;
            PluginList.DisplayMember = "Description";
        }

        private void PluginListSelectedIndexChanged(object sender, EventArgs e)
        {
            SaveSettings();
            _selectedGitPlugin = PluginList.SelectedItem as IGitPlugin;
            LoadSettings();
        }

        private void SaveSettings()
        {
            if (_selectedGitPlugin == null)
                return;

            foreach (Control control in splitContainer1.Panel2.Controls)
            {
                var textBox = control as TextBox;

                if (textBox != null)
                    if (_selectedGitPlugin.Settings.GetAvailableSettings().Contains(textBox.Name))
                        _selectedGitPlugin.Settings.SetSetting(textBox.Name, textBox.Text);
            }
        }

        private void LoadSettings()
        {
            RestorePosition("plugin-settings");
            const int xLabelStart = 20;
            const int xEditStart = 200;

            var yStart = 20;

            splitContainer1.Panel2.Controls.Clear();

            if (_selectedGitPlugin == null)
                return;

            foreach (var setting in _selectedGitPlugin.Settings.GetAvailableSettings())
            {
                var label =
                    new Label
                        {
                            Text = setting,
                            Location = new Point(xLabelStart, yStart),
                            Size = new Size(xEditStart - 30, 20)
                        };
                splitContainer1.Panel2.Controls.Add(label);

                var textBox =
                    new TextBox
                        {
                            Name = setting,
                            Text = _selectedGitPlugin.Settings.GetSetting(setting),
                            Location = new Point(xEditStart, yStart),
                            Size = new Size(splitContainer1.Panel2.Width - xEditStart - 20, 20)
                        };
                splitContainer1.Panel2.Controls.Add(textBox);

                yStart += 25;
            }
        }

        private void FormPluginSettingsFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("plugin-settings");
            SaveSettings();
        }
    }
}