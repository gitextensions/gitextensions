using System.Net;
using GitUIPluginInterfaces.UserControls;

namespace GitUIPluginInterfaces
{
    public class CredentialsSetting : ISetting
    {
        public CredentialsSetting(string name, string caption)
        {
            Name = name;
            Caption = caption;
        }

        private readonly NetworkCredential _defaultValue = new NetworkCredential();
        public string Name { get; }
        public string Caption { get; }
        public CredentialsControl CustomControl { get; set; }

        public NetworkCredential GetValueOrDefault(ISettingsSource settings, IGitModule gitModule)
        {
            return settings.GetCredentials(Name, gitModule, _defaultValue);
        }

        public void SaveValue(string userName, string password, ISettingsSource settings, IGitModule gitModule)
        {
            if (settings.SettingLevel == SettingLevel.Effective)
            {
                var currentCredentials = GetValueOrDefault(settings, gitModule);
                if (currentCredentials.UserName == userName && currentCredentials.Password == password)
                {
                    return;
                }
            }

            settings.SetCredentials(Name, gitModule, new NetworkCredential(userName, password));
        }

        public ISettingControlBinding CreateControlBinding()
        {
            return new CredentialsControlBinding(this, CustomControl);
        }

        private class CredentialsControlBinding : SettingControlBinding<CredentialsSetting, CredentialsControl>
        {
            public CredentialsControlBinding(CredentialsSetting setting, CredentialsControl control)
                : base(setting, control)
            {
            }

            public override CredentialsControl CreateControl()
            {
                return new CredentialsControl();
            }

            public override void LoadSetting(ISettingsSource settings, CredentialsControl control, IGitModule gitModule)
            {
                var credentials = Setting.GetValueOrDefault(settings, gitModule);
                control.UserName = credentials.UserName;
                control.Password = credentials.Password;
            }

            public override void SaveSetting(ISettingsSource settings, CredentialsControl control, IGitModule gitModule)
            {
                Setting.SaveValue(control.UserName, control.Password, settings, gitModule);
            }
        }
    }
}