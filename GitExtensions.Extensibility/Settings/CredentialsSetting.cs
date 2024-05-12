using System.Net;
using GitExtensions.Extensibility.Settings.UserControls;

namespace GitExtensions.Extensibility.Settings;

public class CredentialsSetting : ICredentialsManager, ISetting
{
    private readonly NetworkCredential _defaultValue = new();
    private readonly CredentialsManager _credentialsManager;

    public CredentialsSetting(string name, string caption, Func<string?> getWorkingDir)
    {
        Name = name;
        Caption = caption;

        _credentialsManager = new(getWorkingDir);
    }

    public string Name { get; }
    public string Caption { get; }
    public CredentialsControl? CustomControl { get; set; }

    public NetworkCredential GetValueOrDefault(SettingsSource settings)
    {
        return _credentialsManager.GetCredentialOrDefault(settings.SettingLevel, Name, _defaultValue);
    }

    public void SaveValue(SettingsSource settings, string userName, string password)
    {
        if (settings.SettingLevel == SettingLevel.Effective)
        {
            NetworkCredential currentCredentials = GetValueOrDefault(settings);
            if (currentCredentials.UserName == userName && currentCredentials.Password == password)
            {
                return;
            }
        }

        NetworkCredential? newCredentials = string.IsNullOrWhiteSpace(userName)
            ? null
            : new NetworkCredential(userName, password);
        _credentialsManager.SetCredentials(settings.SettingLevel, Name, newCredentials);
    }

    public ISettingControlBinding CreateControlBinding()
    {
        return new CredentialsControlBinding(this, CustomControl);
    }

    public void Save() => _credentialsManager.Save();

    private class CredentialsControlBinding : SettingControlBinding<CredentialsSetting, CredentialsControl>
    {
        public CredentialsControlBinding(CredentialsSetting setting, CredentialsControl? control)
            : base(setting, control)
        {
        }

        public override CredentialsControl CreateControl()
        {
            Setting.CustomControl = new CredentialsControl();
            return Setting.CustomControl;
        }

        public override void LoadSetting(SettingsSource settings, CredentialsControl control)
        {
            if (SettingLevelSupported(settings.SettingLevel))
            {
                NetworkCredential credentials = Setting.GetValueOrDefault(settings);
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

        public override void SaveSetting(SettingsSource settings, CredentialsControl control)
        {
            if (SettingLevelSupported(settings.SettingLevel))
            {
                Setting.SaveValue(settings, control.UserName, control.Password);

                // Reload actual settings.
                LoadSetting(settings, control);
            }
        }

        private static bool SettingLevelSupported(SettingLevel settingLevel)
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
