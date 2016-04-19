using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.SettingsDialog.Plugins
{
    public partial class PluginSettingsPage : SettingsPageWithHeader
    {
        private IGitPlugin _gitPlugin;
        private readonly IList<string> _autoGenKeywords = new List<string>();
        private bool pageInited = false;

        public PluginSettingsPage()
        {
            InitializeComponent();
            Translate();
        }

        public static PluginSettingsPage CreateSettingsPageFromPlugin(ISettingsPageHost aPageHost, IGitPlugin gitPlugin)
        {
            var result = SettingsPageBase.Create<PluginSettingsPage>(aPageHost);
            result._gitPlugin = gitPlugin;
            return result;
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

        protected override void SettingsToPage()
        {
            if (_gitPlugin == null)
                throw new ApplicationException();

            foreach (var setting in _gitPlugin.GetSettings())
            {
                setting.ControlBinding.GetControl();
                setting.ControlBinding.LoadSetting(_gitPlugin.Settings);
            }
        }

        protected override void PageToSettings()
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

            var settings = _gitPlugin.GetSettings();

            bool hasSettings = settings.Any();

            labelNoSettings.Visible = !hasSettings;

            _autoGenKeywords.Clear();

            LoadSettings();

            foreach (var setting in settings)
            {                
                _autoGenKeywords.Add(setting.Caption);

                var label =
                    new Label
                    {
                        Text = setting.Caption,
                        Location = new Point(xLabelStart, yStart),
                        Size = new Size(xEditStart - 30, 20)
                    };
                panelAutoGenControls.Controls.Add(label);

                var controlBinding = setting.ControlBinding;
                var control = controlBinding.UserControl;
                control.Location = new Point(xEditStart, yStart);
                control.Size = new Size(panelAutoGenControls.Width - xEditStart - 20, 20);
                //TODO associate control with controlBinding

                panelAutoGenControls.Controls.Add(control);

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

            foreach (var setting in _gitPlugin.GetSettings())
            {
                setting.ControlBinding.SaveSetting(_gitPlugin.Settings);
            }
        }
    }
}
