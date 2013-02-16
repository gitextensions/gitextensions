using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.SettingsDialog.Plugins
{
    public partial class PluginSettingsPage : SettingsPageBase
    {
        private readonly IGitPlugin _gitPlugin;
        private readonly IList<string> _autoGenKeywords = new List<string>();
        private bool pageInited = false;

        public PluginSettingsPage(IGitPlugin gitPlugin)
        {
            InitializeComponent();
            Translate();
            _gitPlugin = gitPlugin;
        }

        public static PluginSettingsPage CreateSettingsPageFromPlugin(IGitPlugin gitPlugin)
        {
            return new PluginSettingsPage(gitPlugin);
        }

        public override string GetTitle()
        {
            return _gitPlugin == null ? string.Empty : _gitPlugin.Description;
        }

        protected override string GetCommaSeparatedKeywordList()
        {
            return string.Join(",", _autoGenKeywords);
        }

        public override void OnPageShown()
        {
            if (pageInited)
                return;

            pageInited = true;
            CreateAndInitPluginSettingsControls();
        }

        protected override void OnLoadSettings()
        {
            if (_gitPlugin == null)
                throw new ApplicationException();

            foreach (Control control in panelAutoGenControls.Controls)
            {
                var textBox = control as TextBox;

                if (textBox != null)
                    if (_gitPlugin.Settings.GetAvailableSettings().Contains(textBox.Name))
                        textBox.Text = _gitPlugin.Settings.GetSetting(textBox.Name);
            }
        }

        public override void SaveSettings()
        {
            SavePluginSettingsFromGeneratedControls();
        }

        public override SettingsPageReference PageReference
        {
            get { return new SettingsPageReferenceByType(_gitPlugin.GetType()); }
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

            _autoGenKeywords.Clear();

            foreach (var setting in settings)
            {
                _autoGenKeywords.Add(setting.ToLowerInvariant());

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
    }
}
