using GitUIPluginInterfaces;
using ResourceManager;

namespace ProxySwitcher
{
    public class ProxySwitcherPlugin : GitPluginBase
    {
        public readonly StringSetting Username = new StringSetting("Username", string.Empty);
        public readonly StringSetting Password = new StringSetting("Password", string.Empty);
        public readonly StringSetting HttpProxy = new StringSetting("HTTP proxy", string.Empty);
        public readonly StringSetting HttpProxyPort = new StringSetting("HTTP proxy port", "8080");

        public ProxySwitcherPlugin()
        {
            SetNameAndDescription("Proxy Switcher");
            Translate();
        }
        
        private StringSetting Username = new StringSetting("Username", "");
        private StringSetting Password = new StringSetting("Password", "");
        private StringSetting HttpProxy= new StringSetting("HTTP proxy", "");
        private StringSetting HttpProxyPort = new StringSetting("HTTP proxy port", "8080");

        #region IGitPlugin Members
        
        public override System.Collections.Generic.IEnumerable<ISetting> GetSettings()
        {
            yield return Username;
            yield return Password;
            yield return HttpProxy;
            yield return HttpProxyPort;
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            using (var form = new ProxySwitcherForm(this, Settings, gitUiCommands))
            {
                form.ShowDialog(gitUiCommands.OwnerForm);
            }
            return false;
        }
        
        #endregion IGitPlugin Members
    }
}
