﻿using System;
using System.Collections.Generic;
using GitExtensions.Core.Settings;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.SettingsDialog.Plugins
{
    public partial class PluginSettingsPage : AutoLayoutSettingsPage
    {
        private IGitPlugin _gitPlugin;
        private GitPluginSettingsContainer _settingsContainer;

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
            _gitPlugin = gitPlugin;
            _settingsContainer = new GitPluginSettingsContainer(gitPlugin.Name);
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
            _settingsContainer.SetSettingsSource(base.GetCurrentSettings());
            return _settingsContainer;
        }

        public override string GetTitle()
        {
            return _gitPlugin == null ? string.Empty : _gitPlugin.Description;
        }

        private IEnumerable<ISetting> GetSettings()
        {
            if (_gitPlugin == null)
            {
                throw new ApplicationException();
            }

            return _gitPlugin.HasSettings ? _gitPlugin.GetSettings() : Array.Empty<ISetting>();
        }

        public override SettingsPageReference PageReference => new SettingsPageReferenceByType(_gitPlugin.GetType());

        protected override ISettingsLayout CreateSettingsLayout()
        {
            labelNoSettings.Visible = !_gitPlugin.HasSettings;

            var layout = base.CreateSettingsLayout();

            tableLayoutPanel1.Controls.Add(layout.GetControl(), 0, 1);

            return layout;
        }
    }
}
