using System.Net;

namespace GitExtensions.Extensibility.Settings;

// Cross-platform (net10.0) substitute for CredentialsManager.cs, which is Windows-only
// because it stores credentials in the Windows Credential Store via AdysTech.CredentialManager.
// Reading behaves like an empty store; storing fails fast until a Linux secret-service backend
// is implemented.
public interface ICredentialsManager
{
    void Save();
}

internal class CredentialsManager : ICredentialsManager
{
    private readonly Func<string?>? _getWorkingDir;

    public CredentialsManager()
    {
    }

    protected internal CredentialsManager(Func<string?> getWorkingDir)
    {
        _getWorkingDir = getWorkingDir;
    }

    public void Save()
    {
        // Nothing can have been queued because SetCredentials always throws on this platform.
    }

    protected internal NetworkCredential GetCredentialOrDefault(SettingLevel settingLevel, string name, NetworkCredential defaultValue)
    {
        return defaultValue;
    }

    protected internal void SetCredentials(SettingLevel settingLevel, string name, NetworkCredential? value)
    {
        throw new PlatformNotSupportedException("Storing credentials is not supported on this platform yet.");
    }
}
