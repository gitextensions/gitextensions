using System;
using System.Collections.Generic;
using System.Text;
using GitUIPluginInterfaces;
using ResourceManager.Translation;

namespace ProxySwitcher
{
    public class ProxySwitcherPlugin : GitPluginBase
    {
        #region Translation
        private readonly TranslationString _pluginDescription = new TranslationString("Proxy Switcher");
        #endregion

        public override string Description
        {
            get { return _pluginDescription.Text; }
        }

        protected override void RegisterSettings()
        {
            base.RegisterSettings();
            Settings.AddSetting(SettingsKey.Username, string.Empty);
            Settings.AddSetting(SettingsKey.Password, string.Empty);
            Settings.AddSetting(SettingsKey.HttpProxy, string.Empty);
            Settings.AddSetting(SettingsKey.HttpProxyPort, "8080");
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            using (var form = new ProxySwitcherForm(Settings, gitUiCommands))
            {
                form.ShowDialog(gitUiCommands.OwnerForm);
            }
            return false;
        }
    }
}
