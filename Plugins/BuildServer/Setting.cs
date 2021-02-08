using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitExtensions.Plugins.BuildServer
{
    internal sealed class Setting : ISetting, ISettingControlBinding
    {
        public Setting(string name, IGitModule module)
        {
            Name = name;
            Control = new SettingsPage(module);
        }

        public string Name { get; }

        public string Caption
            => string.Empty;

        public SettingsPage Control { get; }

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
