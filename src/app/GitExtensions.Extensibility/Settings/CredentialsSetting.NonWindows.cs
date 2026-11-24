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

    public void Save() => _credentialsManager.Save();
}
