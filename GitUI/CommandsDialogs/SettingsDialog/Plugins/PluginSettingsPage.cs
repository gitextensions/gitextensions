﻿using System;
using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.SettingsDialog.Plugins
{
    public partial class PluginSettingsPage : AutoLayoutSettingsPage
    {
        private IGitPlugin _gitPlugin;
        private GitPluginSettingsContainer settingsCointainer;

        public PluginSettingsPage()
        {
            InitializeComponent();
            Translate();
        }

        private void CreateSettingsControls()
        {
            var settings = GetSettings();

            foreach (var setting in settings)
            {
               this.AddSetting(setting);
            }
        }

        private void Init(IGitPlugin _gitPlugin)
        {
            this._gitPlugin = _gitPlugin;
            settingsCointainer = new GitPluginSettingsContainer(_gitPlugin.Name);
            CreateSettingsControls();
            Translate();
        }

        public static PluginSettingsPage CreateSettingsPageFromPlugin(ISettingsPageHost aPageHost, IGitPlugin gitPlugin)
        {
            var result = Create<PluginSettingsPage>(aPageHost);
            result.Init(gitPlugin);
            return result;
        }

        protected override ISettingsSource GetCurrentSettings()
        {
            settingsCointainer.SetSettingsSource(base.GetCurrentSettings());
            return settingsCointainer;
        }

        public override string GetTitle()
        {
            return _gitPlugin == null ? string.Empty : _gitPlugin.Description;
        }

        private IEnumerable<ISetting> GetSettings()
        {
            if (_gitPlugin == null)
                throw new ApplicationException();

            return _gitPlugin.GetSettings();
        }

        public override SettingsPageReference PageReference => new SettingsPageReferenceByType(_gitPlugin.GetType());

        protected override SettingsLayout CreateSettingsLayout()
        {
            labelNoSettings.Visible = !GetSettings().Any();

            var layout = base.CreateSettingsLayout();

            tableLayoutPanel1.Controls.Add(layout.GetControl(), 0, 1);

            return layout;
        }
    }
}
