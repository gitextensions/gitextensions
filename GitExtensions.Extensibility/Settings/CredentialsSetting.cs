using System;
using System.Net;
using GitExtensions.Core.Settings;
using GitExtensions.Extensibility.Settings.UserControls;

namespace GitExtensions.Extensibility.Settings
{
    public class CredentialsSetting : CredentialsManager, ISetting
    {
        public CredentialsSetting(string name, string caption, Func<string> getWorkingDir)
            : base(getWorkingDir)
        {
            Name = name;
            Caption = caption;
        }

        private readonly NetworkCredential _defaultValue = new NetworkCredential();
        public string Name { get; }
        public string Caption { get; }
        public CredentialsControl CustomControl { get; set; }

        public NetworkCredential GetValueOrDefault(ISettingsSource settings)
        {
            return GetCredentialOrDefault(settings.SettingLevel, Name, _defaultValue);
        }

        public void SaveValue(ISettingsSource settings, string userName, string password)
        {
            if (settings.SettingLevel == SettingLevel.Effective)
            {
                var currentCredentials = GetValueOrDefault(settings);
                if (currentCredentials.UserName == userName && currentCredentials.Password == password)
                {
                    return;
                }
            }

            var newCredentials = string.IsNullOrWhiteSpace(userName)
                ? null
                : new NetworkCredential(userName, password);
            SetCredentials(settings.SettingLevel, Name, newCredentials);
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
                Setting.CustomControl = new CredentialsControl();
                return Setting.CustomControl;
            }

            public override void LoadSetting(ISettingsSource settings, CredentialsControl control)
            {
                if (SettingLevelSupported(settings.SettingLevel))
                {
                    var credentials = Setting.GetValueOrDefault(settings);
                    control.UserName = credentials.UserName;
                    control.Password = credentials.Password;
                    control.Enabled = true;
                }
                else
                {
                    control.UserName = string.Empty;
                    control.Password = string.Empty;
                    control.Enabled = false;
                }
            }

            public override void SaveSetting(ISettingsSource settings, CredentialsControl control)
            {
                if (SettingLevelSupported(settings.SettingLevel))
                {
                    Setting.SaveValue(settings, control.UserName, control.Password);

                    // Reload actual settings.
                    LoadSetting(settings, control);
                }
            }

            private bool SettingLevelSupported(SettingLevel settingLevel)
            {
                switch (settingLevel)
                {
                    case SettingLevel.Global:
                    case SettingLevel.Local:
                    case SettingLevel.Effective:
                        return true;
                }

                return false;
            }
        }
    }
}
