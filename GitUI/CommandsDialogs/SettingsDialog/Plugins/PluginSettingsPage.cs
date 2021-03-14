using System;
using System.Collections.Generic;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog.Plugins
{
    public partial class PluginSettingsPage : AutoLayoutSettingsPage
    {
        private IGitPlugin? _gitPlugin;
        private GitPluginSettingsContainer? _settingsContainer;

        public PluginSettingsPage()
        {
            InitializeComponent();
        }

        private void CreateSettingsControls()
        {
            var settings = GetSettings();

            foreach (var setting in settings)
            {
                AddSettingControl(setting.CreateControlBinding());
            }
        }

        private void Init(IGitPlugin gitPlugin)
        {
            Validates.NotNull(gitPlugin.Description);

            _gitPlugin = gitPlugin;

            // Description for old plugin setting processing as key
            _settingsContainer = new GitPluginSettingsContainer(gitPlugin.Id, gitPlugin.Description);
            CreateSettingsControls();
            InitializeComplete();
        }

        public static PluginSettingsPage CreateSettingsPageFromPlugin(ISettingsPageHost pageHost, IGitPlugin gitPlugin)
        {
            var result = Create<PluginSettingsPage>(pageHost);
            result.Init(gitPlugin);
            return result;
        }

        protected override ISettingsSource GetCurrentSettings()
        {
            Validates.NotNull(_settingsContainer);

            _settingsContainer.SetSettingsSource(base.GetCurrentSettings());
            return _settingsContainer;
        }

        public override string GetTitle()
        {
            return _gitPlugin?.Name ?? string.Empty;
        }

        private IEnumerable<ISetting> GetSettings()
        {
            if (_gitPlugin is null)
            {
                throw new ApplicationException();
            }

            return _gitPlugin.HasSettings ? _gitPlugin.GetSettings() : Array.Empty<ISetting>();
        }

        public override SettingsPageReference PageReference
        {
            get
            {
                Validates.NotNull(_gitPlugin);

                return new SettingsPageReferenceByType(_gitPlugin.GetType());
            }
        }

        protected override ISettingsLayout CreateSettingsLayout()
        {
            Validates.NotNull(_gitPlugin);

            labelNoSettings.Visible = !_gitPlugin.HasSettings;

            var layout = base.CreateSettingsLayout();

            tableLayoutPanel1.Controls.Add(layout.GetControl(), 0, 1);

            return layout;
        }
    }
}
