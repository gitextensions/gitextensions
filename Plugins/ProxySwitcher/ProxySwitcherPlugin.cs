using GitUIPluginInterfaces;
using ResourceManager;

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

        public override System.Collections.Generic.IEnumerable<ISetting> GetSettings()
        {
            yield return SettingsKey.Username;
            yield return SettingsKey.Password;
            yield return SettingsKey.HttpProxy;
            yield return SettingsKey.HttpProxyPort;
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
