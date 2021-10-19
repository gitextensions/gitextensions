using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitExtensions.Plugins.BuildServerIntegration
{
    internal sealed class Settings : ISetting, ISettingControlBinding
    {
        public Settings(string name, IGitModule module)
        {
            Name = name;
            Control = new SettingsControl(module);
        }

        public string Name { get; }

        public string Caption
            => string.Empty;

        public SettingsControl Control { get; }

        public ISettingControlBinding CreateControlBinding()
            => this;

        string ISettingControlBinding.Caption()
            => Caption;

        public Control GetControl()
            => Control;

        public ISetting GetSetting()
            => this;

        public void LoadSetting(ISettingsSource settings)
            => Control.SettingsToPage(settings);

        public void SaveSetting(ISettingsSource settings)
            => Control.PageToSettings(settings);
    }
}
