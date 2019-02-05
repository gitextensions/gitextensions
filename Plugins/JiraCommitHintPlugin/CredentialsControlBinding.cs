using GitUI.UserControls;
using GitUIPluginInterfaces;

namespace JiraCommitHintPlugin
{
    internal class CredentialsControlBinding : SettingControlBinding<CredentialsSetting, CredentialsControl>
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
