using System.ComponentModel.Composition;
using GitUIPluginInterfaces;
using ProxySwitcher.Properties;
using ResourceManager;

namespace ProxySwitcher
{
    [Export(typeof(IGitPlugin))]
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
            Icon = Resources.IconProxySwitcher;
        }

        public override System.Collections.Generic.IEnumerable<ISetting> GetSettings()
        {
            yield return Username;
            yield return Password;
            yield return HttpProxy;
            yield return HttpProxyPort;
        }

        public override bool Execute(GitUIEventArgs args)
        {
            using (var form = new ProxySwitcherForm(this, Settings, args))
            {
                form.ShowDialog(args.OwnerForm);
            }

            return false;
        }
    }
}
