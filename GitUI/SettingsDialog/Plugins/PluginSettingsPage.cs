using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitUI.SettingsDialog.Plugins
{
    public partial class PluginSettingsPage : SettingsPageBase
    {
        private readonly IGitPlugin _gitPlugin;

        public PluginSettingsPage(IGitPlugin gitPlugin)
        {
            InitializeComponent();
            Translate();
            _gitPlugin = gitPlugin;
        }

        public override bool IsInstantSavePage
        {
            get { return true; }
        }

        public static PluginSettingsPage CreateSettingsPageFromPlugin(IGitPlugin gitPlugin)
        {
            return new PluginSettingsPage(gitPlugin);
        }

        public override string Text
        {
            get { return _gitPlugin.Description; }
            set
            {
                base.Text = value;
            }
        }

        public override void OnPageShown()
        {
            CreateAndInitPluginSettingsControls();
        }

        protected override void OnLoadSettings()
        {
            // not here
        }

        public override void SaveSettings()
        {
            // done via separate button
        }

        /// <summary>
        /// from FormPluginSettings.LoadSettings()
        /// </summary>
        private void CreateAndInitPluginSettingsControls()
        {
            const int xLabelStart = 20;
            const int xEditStart = 300;

            var yStart = 20;

            panelAutoGenControls.Controls.Clear();

            if (_gitPlugin == null)
                throw new ApplicationException();

            var settings = _gitPlugin.Settings.GetAvailableSettings();

            bool hasSettings = settings.Any();
            labelNoSettings.Visible = !hasSettings;
            buttonSave.Visible = hasSettings;
            buttonSave.Enabled = false;

            foreach (var setting in settings)
            {
                var label =
                    new Label
                    {
                        Text = setting,
                        Location = new Point(xLabelStart, yStart),
                        Size = new Size(xEditStart - 30, 20)
                    };
                panelAutoGenControls.Controls.Add(label);

                var textBox =
                    new TextBox
                    {
                        Name = setting,
                        Text = _gitPlugin.Settings.GetSetting(setting),
                        Location = new Point(xEditStart, yStart),
                        Size = new Size(panelAutoGenControls.Width - xEditStart - 20, 20)
                    };

                if (setting.ToLower().Contains("password"))
                    textBox.PasswordChar = '*';

                panelAutoGenControls.Controls.Add(textBox);

                yStart += 25;
            }
        }

        /// <summary>
        /// from FormPluginSettings.SaveSettings()
        /// </summary>
        private void SavePluginSettingsFromGeneratedControls()
        {
            if (_gitPlugin == null)
                throw new ApplicationException();

            foreach (Control control in panelAutoGenControls.Controls)
            {
                var textBox = control as TextBox;

                if (textBox != null)
                    if (_gitPlugin.Settings.GetAvailableSettings().Contains(textBox.Name))
                        _gitPlugin.Settings.SetSetting(textBox.Name, textBox.Text);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SavePluginSettingsFromGeneratedControls();
        }

        private void panelAutoGenControls_Enter(object sender, EventArgs e)
        {
            buttonSave.Enabled = true;
        }
    }
}
