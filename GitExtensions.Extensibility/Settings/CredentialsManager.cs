using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using AdysTech.CredentialManager;

namespace GitExtensions.Extensibility.Settings;

public interface ICredentialsManager
{
    void Save();
}

internal class CredentialsManager : ICredentialsManager
{
    private static ConcurrentDictionary<string, NetworkCredential?> Credentials { get; } = new ConcurrentDictionary<string, NetworkCredential?>();
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
        List<KeyValuePair<string, NetworkCredential?>> credentials = Credentials.ToList();
        if (credentials.Count < 1)
        {
            return;
        }

        Credentials.Clear();

        foreach (KeyValuePair<string, NetworkCredential?> networkCredentials in credentials)
        {
            if (networkCredentials.Value is null)
            {
                continue;
            }

            AdysTechCredentialManagerWrapper.UpdateCredentials(networkCredentials.Key,
                networkCredentials.Value.UserName,
                networkCredentials.Value.Password);
        }
    }

    protected internal NetworkCredential GetCredentialOrDefault(SettingLevel settingLevel, string name, NetworkCredential defaultValue)
    {
        string? targetName = GetWindowsCredentialsTarget(name, settingLevel);
        if (string.IsNullOrWhiteSpace(targetName))
        {
            return defaultValue;
        }

        if (Credentials.TryGetValue(targetName, out NetworkCredential? result) || AdysTechCredentialManagerWrapper.TryGetCredentials(targetName, out result))
        {
            return result ?? defaultValue;
        }

        return defaultValue;
    }

    protected internal void SetCredentials(SettingLevel settingLevel, string name, NetworkCredential? value)
    {
        string? targetName = GetWindowsCredentialsTarget(name, settingLevel);
        ArgumentNullException.ThrowIfNull(targetName);
        Credentials.AddOrUpdate(targetName, value, (s, credential) => value);
    }

    private string? GetWindowsCredentialsTarget(string name, SettingLevel settingLevel)
    {
        if (settingLevel == SettingLevel.Global)
        {
            return $"{name}";
        }

        ArgumentNullException.ThrowIfNull(_getWorkingDir);
        string? suffix = _getWorkingDir();
        return string.IsNullOrWhiteSpace(suffix) ? null : $"{name}_{suffix}";
    }

    private static class AdysTechCredentialManagerWrapper
    {
        private const string TargetPrefix = "GitExtensions_";

        private static string GetTarget(string rawTarget)
        {
            if (string.IsNullOrWhiteSpace(rawTarget))
            {
                throw new ArgumentNullException(nameof(rawTarget));
            }

            return $"{TargetPrefix}{rawTarget}";
        }

        public static bool TryGetCredentials(string target, [NotNullWhen(true)] out NetworkCredential? credentials)
        {
            credentials = CredentialManager.GetCredentials(GetTarget(target));
            return credentials is not null;
        }

        public static bool SaveCredentials(string target, string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            return CredentialManager.SaveCredentials(GetTarget(target), new NetworkCredential(userName.Trim(), password)) != null;
        }

        private static bool RemoveCredentials(string target)
        {
            if (string.IsNullOrWhiteSpace(target) || CredentialManager.GetCredentials(GetTarget(target)) is null)
            {
                return false;
            }

            return CredentialManager.RemoveCredentials(GetTarget(target));
        }

        public static bool UpdateCredentials(string target, string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return RemoveCredentials(target);
            }

            return SaveCredentials(target, userName, password);
        }
    }
}
