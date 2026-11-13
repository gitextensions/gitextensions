using System.Net;
using System.Text.Json;
using GitExtensions.Extensibility.Settings;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitUI.BuildServerIntegration;

internal sealed class BuildServerCredentialStore(Func<string?> getWorkingDir)
{
    private const string CredentialUserName = "GitExtensions.BuildServer";
    private static readonly SettingsSource GlobalSettings = new GlobalSettingsSource();

    internal BuildServerCredentials? Get(string buildServerUniqueKey)
    {
        CredentialsSetting setting = CreateSetting(buildServerUniqueKey);
        NetworkCredential credential = setting.GetValueOrDefault(GlobalSettings);
        return Deserialize(credential);
    }

    internal void Save(string buildServerUniqueKey, IBuildServerCredentials credentials)
    {
        CredentialsSetting setting = CreateSetting(buildServerUniqueKey);
        NetworkCredential credential = Serialize(credentials);
        setting.SaveValue(GlobalSettings, credential.UserName, credential.Password);
        setting.Save();
    }

    internal static BuildServerCredentials? Deserialize(NetworkCredential credential)
    {
        if (credential.UserName != CredentialUserName || string.IsNullOrWhiteSpace(credential.Password))
        {
            return null;
        }

        try
        {
            StoredBuildServerCredentials? stored = JsonSerializer.Deserialize<StoredBuildServerCredentials>(credential.Password);
            if (stored is null
                || !Enum.TryParse(stored.CredentialType, ignoreCase: false, out BuildServerCredentialsType credentialType)
                || !Enum.IsDefined(credentialType))
            {
                return null;
            }

            return new BuildServerCredentials
            {
                BuildServerCredentialsType = credentialType,
                Username = stored.Username,
                Password = stored.Password,
                BearerToken = stored.BearerToken,
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    internal static NetworkCredential Serialize(IBuildServerCredentials credentials)
    {
        StoredBuildServerCredentials storedCredentials = new()
        {
            CredentialType = credentials.BuildServerCredentialsType.ToString(),
            Username = credentials.Username,
            Password = credentials.Password,
            BearerToken = credentials.BearerToken,
        };
        return new NetworkCredential(CredentialUserName, JsonSerializer.Serialize(storedCredentials));
    }

    private CredentialsSetting CreateSetting(string buildServerUniqueKey)
        => new($"BuildServer.{buildServerUniqueKey}", string.Empty, getWorkingDir);

    private sealed class StoredBuildServerCredentials
    {
        public string CredentialType { get; set; } = string.Empty;

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? BearerToken { get; set; }
    }

    private sealed class GlobalSettingsSource : SettingsSource
    {
        public override SettingLevel SettingLevel { get; init; } = SettingLevel.Global;

        public override string? GetValue(string name)
            => throw new NotSupportedException("The credential store uses only the setting level.");

        public override void SetValue(string name, string? value)
            => throw new NotSupportedException("The credential store uses only the setting level.");
    }
}
