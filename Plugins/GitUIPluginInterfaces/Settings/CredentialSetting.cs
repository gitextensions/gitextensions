using System;
using System.Net;
using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public class CredentialSetting : ISetting
    {
        public CredentialSetting(string name, string caption, Func<ISetting, Control, ISettingControlBinding> createControlBindingFunc)
        {
            Name = name;
            Caption = caption;
            CreateControlBindingFunc = createControlBindingFunc;
        }

        private readonly NetworkCredential _defaultValue = new NetworkCredential();
        private Func<ISetting, Control, ISettingControlBinding> CreateControlBindingFunc { get; }
        public string Name { get; }
        public string Caption { get; }
        public Control CustomControl { get; set; }

        public NetworkCredential GetValueOrDefault(ISettingsSource settings, IGitModule gitModule)
        {
            return settings.GetCredential(Name, gitModule, _defaultValue);
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

            settings.SetCredential(Name, gitModule, new NetworkCredential(userName, password));
        }

        public ISettingControlBinding CreateControlBinding()
        {
            return CreateControlBindingFunc(this, CustomControl);
        }
    }
}