using GitUI.UserControls;
using GitUIPluginInterfaces;

namespace JiraCommitHintPlugin
{
    internal class CredentialsControlBinding : SettingControlBinding<CredentialSetting, CredentialControl>
    {
        public CredentialsControlBinding(CredentialSetting setting, CredentialControl control)
            : base(setting, control)
        {
        }

        public override CredentialControl CreateControl()
        {
            return new CredentialControl();
        }

        public override void LoadSetting(ISettingsSource settings, CredentialControl control, IGitModule gitModule)
        {
            var credential = Setting.GetValueOrDefault(settings, gitModule);
            control.UserName = credential.UserName;
            control.Password = credential.Password;
        }

        public override void SaveSetting(ISettingsSource settings, CredentialControl control, IGitModule gitModule)
        {
            Setting.SaveValue(control.UserName, control.Password, settings, gitModule);
        }
    }
}
