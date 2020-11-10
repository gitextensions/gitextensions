using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Settings;
using ProxySwitcher.Properties;

namespace ProxySwitcher
{
    [Export(typeof(IGitPlugin))]
    public sealed class Plugin : IGitPlugin,
        IGitPluginConfigurable,
        IGitPluginExecutable
    {
        public readonly StringSetting Username = new StringSetting("Username", Strings.Username, string.Empty);
        public readonly StringSetting Password = new StringSetting("Password", Strings.Password, string.Empty);
        public readonly StringSetting HttpProxy = new StringSetting("HTTP proxy", Strings.HttpProxy, string.Empty);
        public readonly StringSetting HttpProxyPort = new StringSetting("HTTP proxy port", Strings.HttpProxyPort, "8080");

        public string Name => "Proxy Switcher";

        public string Description => Strings.Description;

        public Image Icon => Images.IconProxySwitcher;

        public IGitPluginSettingsContainer SettingsContainer { get; set; }

        public IEnumerable<ISetting> GetSettings()
        {
            yield return Username;
            yield return Password;
            yield return HttpProxy;
            yield return HttpProxyPort;
        }

        public bool Execute(GitUIEventArgs args)
        {
            using var form = new ProxySwitcherForm(this, SettingsContainer.GetSettingsSource(), args);

            form.ShowDialog(args.OwnerForm);

            return false;
        }
    }
}
